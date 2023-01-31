﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;
using SimpleIdServer.IdServer.Api.Token.TokenBuilders;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.DTOs;
using SimpleIdServer.IdServer.Exceptions;
using SimpleIdServer.IdServer.Extensions;
using SimpleIdServer.IdServer.Options;
using SimpleIdServer.IdServer.Store;
using SimpleIdServer.IdServer.Stores;
using SimpleIdServer.IdServer.UI.Services;
using SimpleIdServer.IdServer.WsFederation.Extensions;
using System.Net;
using System.Security.Claims;

namespace SimpleIdServer.IdServer.WsFederation.Api
{
    public class SSOController : BaseWsFederationController
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserTransformer _userTransformer;
        private readonly IDataProtector _dataProtector;
        private readonly IdServerHostOptions _options;

        public SSOController(IClientRepository clientRepository, IUserRepository userRepository, IUserTransformer userTransformer, IDataProtectionProvider dataProtectionProvider, IOptions<IdServerHostOptions> opts, IOptions<IdServerWsFederationOptions> options, IKeyStore keyStore) : base(options, keyStore)
        {
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _userTransformer = userTransformer;
            _dataProtector = dataProtectionProvider.CreateProtector("Authorization");
            _options = opts.Value;
        }

        public async Task<IActionResult> Login(CancellationToken cancellationToken)
        {
            var queryStr = Request.QueryString.Value;
            var federationMessage = WsFederationMessage.FromQueryString(queryStr);
            try
            {
                if (federationMessage.IsSignInMessage)
                    return await SignIn(federationMessage, cancellationToken);

                throw new NotImplementedException();
            }
            catch(OAuthException ex)
            {
                return RedirectToAction("Index", "Errors", new { code = ex.Code, message = ex.Message });
            }
        }

        private async Task<IActionResult> SignIn(WsFederationMessage message, CancellationToken cancellationToken)
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var client = await Validate();
            if (User == null || User.Identity == null || User.Identity.IsAuthenticated == false)
                return RedirectToLoginPage();

            var tokenType = GetTokenType(client);
            var nameIdentifier = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await _userRepository.Query().Include(u => u.OAuthUserClaims).AsNoTracking().FirstAsync(u => u.Id == nameIdentifier, cancellationToken);
            var subject = BuildSubject();
            return BuildResponse();

            async Task<Domains.Client> Validate()
            {
                var client = await _clientRepository.Query().Include(c => c.Scopes).ThenInclude(s => s.Claims).AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == message.Wtrealm, cancellationToken);
                if (client == null)
                    throw new OAuthException(ErrorCodes.INVALID_RP, ErrorMessages.UNKNOWN_RP);

                if (!client.IsWsFederationEnabled())
                    throw new OAuthException(ErrorCodes.INVALID_RP, ErrorMessages.WSFEDERATION_NOT_ENABLED);

                var tokenType = GetTokenType(client);
                if (tokenType != WsFederationConstants.TokenTypes.Saml2TokenProfile11 && tokenType != WsFederationConstants.TokenTypes.Saml11TokenProfile11)
                    throw new OAuthException(ErrorCodes.INVALID_RP, ErrorMessages.UNSUPPORTED_TOKENTYPE);

                return client;
            }

            IActionResult RedirectToLoginPage()
            {
                var queryStr = Request.QueryString.Value;
                var returnUrl = $"{issuer}/{WsFederationConstants.EndPoints.SSO}{queryStr}&{AuthorizationRequestParameters.ClientId}={message.Wtrealm}";
                var url = Url.Action("Index", "Authenticate", new
                {
                    returnUrl = _dataProtector.Protect(returnUrl),
                    area = Constants.Areas.Password
                });
                return Redirect(url);
            }

            ClaimsIdentity BuildSubject()
            {
                var claims = new Dictionary<string, object>();
                IdTokenBuilder.EnrichWithScopeParameter(claims, client.Scopes, user, user.Id);
                var transformedClaims = _userTransformer.ConvertToIdentityClaims(claims).ToList();
                if (!transformedClaims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                    transformedClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

                var format = Microsoft.IdentityModel.Tokens.Saml2.ClaimProperties.SamlNameIdentifierFormat;
                if (tokenType == WsFederationConstants.TokenTypes.Saml11TokenProfile11)
                    format = Microsoft.IdentityModel.Tokens.Saml.ClaimProperties.SamlNameIdentifierFormat;

                foreach (var cl in transformedClaims)
                {
                    if (cl.Type == ClaimTypes.NameIdentifier)
                        cl.Properties[format] = Options.DefaultNameIdentifierFormat;
                }

                return new ClaimsIdentity(transformedClaims, "idserver");
            }

            IActionResult BuildResponse()
            {
                var descriptor = new SecurityTokenDescriptor
                {
                    Audience = client.ClientId,
                    IssuedAt = DateTime.UtcNow,
                    NotBefore = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddSeconds(client.TokenExpirationTimeInSeconds ?? _options.DefaultTokenExpirationTimeInSeconds),
                    Subject = subject,
                    Issuer = issuer,
                    SigningCredentials = GetSigningCredentials()
                };
                SecurityTokenHandler handler;
                if (tokenType == WsFederationConstants.TokenTypes.Saml2TokenProfile11)
                    handler = new Saml2SecurityTokenHandler();
                else
                    handler = new SamlSecurityTokenHandler();

                var securityToken = handler.CreateToken(descriptor);

                var response = new RequestSecurityTokenResponse
                {
                    CreatedAt = securityToken.ValidFrom,
                    ExpiresAt = securityToken.ValidTo,
                    AppliesTo = client.ClientId,
                    Context = message.Wctx,
                    RequestedSecurityToken = securityToken,
                    SecurityTokenHandler = handler
                };

                var responseMessage = new WsFederationMessage
                {
                    IssuerAddress = message.Wreply,
                    Wresult = response.Serialize(),
                    Wctx = message.Wctx,
                    PostTitle = message.PostTitle,
                    Script = message.Script,
                    ScriptButtonText = message.ScriptButtonText,
                    ScriptDisabledText = message.ScriptDisabledText,
                    Wa = Microsoft.IdentityModel.Protocols.WsFederation.WsFederationConstants.WsFederationActions.SignIn
                };

                if (!string.IsNullOrWhiteSpace(message.Script))
                {
                    var content = responseMessage.BuildFormPost();
                    return new ContentResult
                    {
                        ContentType = "text/html",
                        StatusCode = (int)HttpStatusCode.OK,
                        Content = responseMessage.BuildFormPost()
                    };
                }

                return Redirect(responseMessage.BuildRedirectUrl());
            }
        }

        private string GetTokenType(Client client) => client.GetWsTokenType() ?? Options.DefaultTokenType;
    }
}