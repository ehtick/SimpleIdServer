﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens.Saml2;
using SimpleIdServer.IdServer.Api;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.DTOs;
using SimpleIdServer.IdServer.Exceptions;
using SimpleIdServer.IdServer.Extractors;
using SimpleIdServer.IdServer.Helpers;
using SimpleIdServer.IdServer.Options;
using SimpleIdServer.IdServer.Saml.Idp.Apis;
using SimpleIdServer.IdServer.Saml.Idp.DTOs;
using SimpleIdServer.IdServer.Saml.Idp.Extensions;
using SimpleIdServer.IdServer.Saml.Idp.Factories;
using SimpleIdServer.IdServer.Stores;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleIdServer.IdServer.Saml2.Api;

public class SamlSSOController : Controller
{
    private readonly IClientRepository _clientRepository;
    private readonly ISaml2ConfigurationFactory _saml2ConfigurationFactory;
    private readonly IDataProtector _dataProtector;
    private readonly IScopeClaimsExtractor _scopeClaimsExtractor;
    private readonly IDistributedCache _distributedCache;
    private readonly IUserRepository _userRepository;
    private readonly ISaml2AuthResponseEnricher _saml2AuthResponseEnricher;
    private readonly IRealmStore _realmStore;
    private readonly IdServerHostOptions _options;
    private readonly ILogger<SamlSSOController> _logger;

    public SamlSSOController(
        IClientRepository clientRepository, 
        ISaml2ConfigurationFactory saml2ConfigurationFactory, 
        IDataProtectionProvider dataProtectionProvider, 
        IScopeClaimsExtractor scopeClaimsExtractor, 
        IDistributedCache distributedCache,
        IUserRepository userRepository,
        ISaml2AuthResponseEnricher saml2AuthResponseEnricher,
        IRealmStore realmStore,
        IOptions<IdServerHostOptions> options,
        ILogger<SamlSSOController> logger)
    {
        _clientRepository = clientRepository;
        _saml2ConfigurationFactory = saml2ConfigurationFactory;
        _dataProtector = dataProtectionProvider.CreateProtector("Authorization");
        _scopeClaimsExtractor = scopeClaimsExtractor;
        _distributedCache = distributedCache;
        _userRepository = userRepository;
        _saml2AuthResponseEnricher = saml2AuthResponseEnricher;
        _realmStore = realmStore;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> LoginGet(CancellationToken cancellationToken)
    {
        var issuer = Request.GetAbsoluteUriWithVirtualPath();
        var requestBinding = new Saml2RedirectBinding();
        var deserializedHttpRequest = Request.ToGenericHttpRequest();
        ClientResult clientResult = null;
        var requestedIssuer = requestBinding.ReadSamlRequest(deserializedHttpRequest, new Saml2AuthnRequest(new Saml2Configuration())).Issuer;
        try
        {
            clientResult = await GetClient(requestedIssuer, _realmStore.Realm ?? Constants.DefaultRealm, cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BuildError(ex);
        }

        if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
            return RedirectToLoginPage();
         
        Saml2AuthnRequest saml2AuthnRequest = new Saml2AuthnRequest(clientResult.SpSamlConfiguration);
        try
        {
            requestBinding.Unbind(deserializedHttpRequest, saml2AuthnRequest);
            return await BuildLoginResponse(saml2AuthnRequest, clientResult, Saml2StatusCodes.Success, requestBinding.RelayState, issuer, cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return await BuildLoginResponse(saml2AuthnRequest, clientResult, Saml2StatusCodes.Responder, requestBinding.RelayState, issuer, cancellationToken);
        }

        IActionResult RedirectToLoginPage()
        {
            var queryStr = Request.QueryString.Value.TrimStart('?');
            var realm = _realmStore.Realm;
            var prefix = string.IsNullOrWhiteSpace(realm) ? string.Empty : $"{realm}/";
            var returnUrl = $"{issuer}/{prefix}{Saml.Idp.Constants.RouteNames.SingleSignOnHttpRedirect}?{AuthorizationRequestParameters.ClientId}={clientResult.Client.ClientId}&{queryStr}";
            var url = Url.Action("Index", "Authenticate", new
            {
                returnUrl = _dataProtector.Protect(returnUrl),
                area = Constants.AreaPwd
            });
            return Redirect(url);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        ClientResult clientResult = null;
        var httpRequest = await Request.ToGenericHttpRequestAsync(validate: false);
        var binding = new Saml2PostBinding();
        var requestedIssuer = binding.ReadSamlRequest(httpRequest, new Saml2LogoutRequest(new Saml2Configuration())).Issuer;
        try
        {
            clientResult = await GetClient(requestedIssuer, _realmStore.Realm ?? Constants.DefaultRealm, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BuildError(ex);
        }

        var idpSamlConfiguration = _saml2ConfigurationFactory.BuildSamlIdpConfiguration(Request.GetAbsoluteUriWithVirtualPath(), Request.GetAbsoluteUriWithVirtualPath());
        var saml2LogoutRequest = new Saml2LogoutRequest(clientResult.SpSamlConfiguration);
        try
        {
            httpRequest.Binding.Unbind(httpRequest, saml2LogoutRequest);
            return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Success, httpRequest.Binding.RelayState, saml2LogoutRequest.SessionIndex, clientResult, idpSamlConfiguration);

        }
        catch(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Responder, httpRequest.Binding.RelayState, saml2LogoutRequest.SessionIndex, clientResult, idpSamlConfiguration);
        }
    }

    [HttpPost]
    public async Task<IActionResult> LoginArtifact(CancellationToken cancellationToken)
    {
        var soapEnvelope = new Saml2SoapEnvelope();
        var httpRequest = await Request.ToGenericHttpRequestAsync(readBodyAsString: true);
        var issuer = soapEnvelope.ReadSamlRequest(httpRequest, new Saml2ArtifactResolve(new Saml2Configuration())).Issuer;
        ClientResult clientResult = null;
        try
        {
            clientResult = await GetClient(issuer, _realmStore.Realm ?? Constants.DefaultRealm, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BuildError(ex);
        }

        var saml2ArtifactResolve = new Saml2ArtifactResolve(clientResult.SpSamlConfiguration);
        try
        {
            var idpSamlConfiguration = _saml2ConfigurationFactory.BuildSamlIdpConfiguration(Request.GetAbsoluteUriWithVirtualPath(), Request.GetAbsoluteUriWithVirtualPath());
            soapEnvelope.Unbind(httpRequest, saml2ArtifactResolve);
            var base64 = await _distributedCache.GetStringAsync(saml2ArtifactResolve.Artifact, cancellationToken);
            if (string.IsNullOrWhiteSpace(base64)) throw new OAuthException(string.Empty, $"Saml2AuthnResponse not found by Artifact '{saml2ArtifactResolve.Artifact}' in the cache.");
            await _distributedCache.RemoveAsync(saml2ArtifactResolve.Artifact, cancellationToken);
            var cachedSaml2AuthnResponse = ReadAuthnResponse(base64);
            var saml2ArtifactResponse = new Saml2ArtifactResponse(idpSamlConfiguration, cachedSaml2AuthnResponse)
            {
                InResponseTo = saml2ArtifactResolve.Id
            };
            soapEnvelope.Bind(saml2ArtifactResponse);
            return soapEnvelope.ToActionResult();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.ToString());
            return BuildError(ex);
        }

        Saml2AuthnResponse ReadAuthnResponse(string base64)
        {
            var binding = new Saml2PostBinding();
            var saml2AuthnResponse = new Saml2AuthnResponse(new Saml2Configuration
            {
                AudienceRestricted = false
            });
            var fakeHttpRequest = new ITfoxtec.Identity.Saml2.Http.HttpRequest
            {
                Method = "POST",
                Form = new System.Collections.Specialized.NameValueCollection
                {
                    { "SAMLResponse", base64 }
                }
            };
            var req = binding.ReadSamlResponse(fakeHttpRequest, saml2AuthnResponse);
            return req as Saml2AuthnResponse;
        }
    }

    private async Task<ClientResult> GetClient(string issuer, string realm, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByClientId(realm, issuer, cancellationToken);
        if (client == null) throw new OAuthException(string.Empty, $"the client '{issuer}' doesn't exist");
        var kvp = await _saml2ConfigurationFactory.BuildSamSpConfiguration(client, cancellationToken);
        return new ClientResult { Client = client, SpSamlConfiguration = kvp.Item1, EntityDescriptor = kvp.Item2 };
    }

    private Task<IActionResult> BuildLoginResponse(Saml2Request request, ClientResult clientResult, Saml2StatusCodes status, string relayState, string issuer, CancellationToken cancellationToken)
    {
        if (clientResult.Client.GetUseAcrsArtifact()) return BuildArtifactLoginResponse(request, clientResult, status, relayState, issuer, cancellationToken);
        else return BuildPostLoginResponse(request, clientResult, status, relayState, issuer, cancellationToken);
    }

    private async Task<IActionResult> BuildPostLoginResponse(Saml2Request request, ClientResult clientResult, Saml2StatusCodes status, string relayState, string issuer, CancellationToken cancellationToken)
    {
        var destination = clientResult.EntityDescriptor.SPSsoDescriptor.AssertionConsumerServices.Where(a => a.IsDefault && a.Binding == ProtocolBindings.HttpPost).OrderBy(a => a.Index).First().Location;
        var idpSamlConfiguration = _saml2ConfigurationFactory.BuildSamlIdpConfiguration(issuer, issuer);
        var responsebinding = new Saml2PostBinding
        {
            RelayState = _realmStore.Realm ?? Constants.DefaultRealm
        };
        var response = await BuildAuthnSamlResponse(request, clientResult, idpSamlConfiguration, status, destination, issuer, cancellationToken);
        return responsebinding.Bind(response).ToActionResult();
    }

    private async Task<IActionResult> BuildArtifactLoginResponse(Saml2Request request, ClientResult clientResult, Saml2StatusCodes status, string relayState, string issuer,CancellationToken cancellationToken)
    {
        var destination = clientResult.EntityDescriptor.SPSsoDescriptor.AssertionConsumerServices.Where(a => a.IsDefault && a.Binding == ProtocolBindings.HttpArtifact).OrderBy(a => a.Index).First().Location;
        var idpSamlConfiguration = _saml2ConfigurationFactory.BuildSamlIdpConfiguration(issuer, issuer);
        var responsebinding = new Saml2ArtifactBinding
        {
            RelayState = _realmStore.Realm ?? Constants.DefaultRealm
        };
        var saml2ArtifactResolve = new Saml2ArtifactResolve(idpSamlConfiguration)
        {
            Destination = destination
        };
        responsebinding.Bind(saml2ArtifactResolve);
        var response = await BuildAuthnSamlResponse(request, clientResult, idpSamlConfiguration, status, destination, issuer, cancellationToken);
        await _distributedCache.SetStringAsync(saml2ArtifactResolve.Artifact, Convert.ToBase64String(Encoding.UTF8.GetBytes(response.ToXml().OuterXml)));
        return responsebinding.ToActionResult();
    }

    private async Task<Saml2AuthnResponse> BuildAuthnSamlResponse(Saml2Request request, ClientResult clientResult, Saml2Configuration idpSamlConfiguration, Saml2StatusCodes status, Uri destination, string issuer, CancellationToken cancellationToken)
    {
        var response = new Saml2AuthnResponse(idpSamlConfiguration)
        {
            InResponseTo = request.Id,
            Status = status,
            Destination = destination
        };
        if (status == Saml2StatusCodes.Success)
        {
            var claimsIdentity = await BuildSubject(clientResult.Client, issuer, cancellationToken);
            response.SessionIndex = Guid.NewGuid().ToString();
            response.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
            response.ClaimsIdentity = claimsIdentity;
            var securityToken = response.CreateSecurityToken(clientResult.Client.ClientId, subjectConfirmationLifetime: 5, issuedTokenLifetime: 60);
            _saml2AuthResponseEnricher.Enrich(securityToken);
        }

        return response;
    }

    private async Task<ClaimsIdentity> BuildSubject(Client client, string issuer, CancellationToken cancellationToken)
    {
        var nameIdentifier = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var realm = _realmStore.Realm ?? Constants.DefaultRealm;
        var user = await _userRepository.GetBySubject(nameIdentifier, realm, cancellationToken);
        var context = new HandlerContext(new HandlerContextRequest(issuer, string.Empty, null, null, null, (X509Certificate2)null, null), realm, _options);
        context.SetUser(user, null);
        var claims = (await _scopeClaimsExtractor.ExtractClaims(context, client.Scopes, ScopeProtocols.SAML)).Select(c => new Claim(c.Key, c.Value.ToString())).ToList();
        if (claims.Count(t => t.Type == ClaimTypes.NameIdentifier) == 0)
            throw new OAuthException(string.Empty, "token cannot be generated if there is no claim, please specify one or more scope in the client");

        if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Name));

        return new ClaimsIdentity(claims, "idserver");
    }

    private IActionResult LogoutResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, string sessionIndex, ClientResult relyingParty, Saml2Configuration idpSamlConfiguration)
    {
        var responsebinding = new Saml2PostBinding
        {
            RelayState = relayState
        };
        var destination = relyingParty.EntityDescriptor.SPSsoDescriptor.SingleLogoutServices.FirstOrDefault()?.Location;
        var saml2LogoutResponse = new Saml2LogoutResponse(idpSamlConfiguration)
        {
            InResponseTo = inResponseTo,
            Status = status,
            SessionIndex = sessionIndex,
            Destination = destination
        };
        return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
    }

    private IActionResult BuildError(Exception ex) => BuildError(ex.ToString());

    private IActionResult BuildError(string errorMessage)
    {
        var error = new SamlSSOError { ErrorMessage = errorMessage };
        using (var stream = new StringWriter())
        {
            using (var writer = XmlWriter.Create(stream))
            {
                var serializer = new XmlSerializer(typeof(SamlSSOError));
                serializer.Serialize(writer, error);
                return new ContentResult
                {
                    ContentType = "text/html",
                    Content = stream.ToString(),
                };
            }
        }            
    }

    private record ClientResult
    {
        public Client Client { get; set; } = null!;
        public Saml2Configuration SpSamlConfiguration { get; set; } = null!;
        public EntityDescriptor EntityDescriptor { get; set; } = null!;
    }
}
