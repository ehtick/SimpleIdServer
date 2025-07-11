﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using FormBuilder;
using FormBuilder.Repositories;
using FormBuilder.Stores;
using MassTransit;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SimpleIdServer.IdServer.Captcha;
using SimpleIdServer.IdServer.Fido.Services;
using SimpleIdServer.IdServer.Fido.UI.ViewModels;
using SimpleIdServer.IdServer.Helpers;
using SimpleIdServer.IdServer.Jwt;
using SimpleIdServer.IdServer.Options;
using SimpleIdServer.IdServer.Stores;
using SimpleIdServer.IdServer.UI;
using SimpleIdServer.IdServer.UI.Infrastructures;
using SimpleIdServer.IdServer.UI.Services;
using SimpleIdServer.IdServer.UI.ViewModels;

namespace SimpleIdServer.IdServer.Fido.UI.Webauthn
{
    [Area(Constants.AMR)]
    public class AuthenticateController : BaseAuthenticationMethodController<AuthenticateWebauthnViewModel>
    {
        public AuthenticateController(
            ITemplateStore templateStore, 
            IConfiguration configuration,
            IAuthenticationHelper authenticationHelper,
            IDistributedCache distributedCache,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IWebauthnAuthenticationService userAuthenticationService,
            IOptions<IdServerHostOptions> options,
            IDataProtectionProvider dataProtectionProvider,
            ITokenRepository tokenRepository,
            ITransactionBuilder transactionBuilder,
            IJwtBuilder jwtBuilder,
            IClientRepository clientRepository,
            IAmrHelper amrHelper,
            IUserRepository userRepository,
            IUserSessionResitory userSessionRepository,
            IUserTransformer userTransformer,
            IBusControl busControl,
            IAntiforgery antiforgery,
            IAuthenticationContextClassReferenceRepository authenticationContextClassReferenceRepository, 
            ISessionManager sessionManager, 
            IWorkflowStore workflowStore,
            IFormStore formStore,
            ILanguageRepository languageRepository,
            IAcrHelper acrHelper,
            IWorkflowHelper workflowHelper,
            ICaptchaValidatorFactory captchaValidatorFactory,
            IOptions<FormBuilderOptions> formBuilderOptions) : base(templateStore, configuration, options, authenticationSchemeProvider, userAuthenticationService, dataProtectionProvider, tokenRepository, transactionBuilder, jwtBuilder, authenticationHelper, clientRepository, amrHelper, userRepository, userSessionRepository, userTransformer, busControl, antiforgery, authenticationContextClassReferenceRepository, sessionManager, workflowStore, formStore, languageRepository, acrHelper, workflowHelper, captchaValidatorFactory, formBuilderOptions)
        {
        }

        protected override string Amr => Constants.AMR;

        protected override bool IsExternalIdProvidersDisplayed => false;

        protected override bool TryGetLogin(AcrAuthInfo amr, out string login)
        {
            login = null;
            if (amr == null || string.IsNullOrWhiteSpace(amr.Login)) return false;
            login = amr.Login;
            return true;
        }

        protected override Task<UserAuthenticationResult> CustomAuthenticate(string prefix, string authenticatedUserId, AuthenticateWebauthnViewModel viewModel, CancellationToken cancellationToken)
        {
            return Task.FromResult(UserAuthenticationResult.Ok());
        }

        protected override void EnrichViewModel(AuthenticateWebauthnViewModel viewModel)
        {
            var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var realm = "/";
            if (!string.IsNullOrWhiteSpace(viewModel.Realm))
                realm = $"/{viewModel.Realm}/";
            viewModel.BeginLoginUrl = $"{issuer}{realm}{Constants.EndPoints.BeginLogin}";
            viewModel.EndLoginUrl = $"{issuer}{realm}{Constants.EndPoints.EndLogin}";
        }
    }
}
