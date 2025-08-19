﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.Extensions.Options;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.Helpers;
using SimpleIdServer.IdServer.Layout.AuthFormLayout;
using SimpleIdServer.IdServer.Options;
using SimpleIdServer.IdServer.Stores;
using SimpleIdServer.IdServer.UI.Services;
using SimpleIdServer.IdServer.UI.ViewModels;

namespace SimpleIdServer.IdServer.Pwd.Services;

public interface IPasswordAuthenticationService : IUserAuthenticationService
{

}

public class PasswordAuthenticationService : GenericAuthenticationService<AuthenticatePasswordViewModel>, IPasswordAuthenticationService
{
    private readonly IEnumerable<IIdProviderAuthService> _authServices;

    public PasswordAuthenticationService(
        IEnumerable<IIdProviderAuthService> authServices, 
        IOptions<IdServerHostOptions> options,
        IAuthenticationHelper authenticationHelper, 
        IUserRepository userRepository) : base(authenticationHelper, userRepository)
    {
        _authServices = authServices;
    }

    public override string Amr => Constants.AreaPwd;

    protected override async Task<User> GetUser(string authenticatedUserId, AuthenticatePasswordViewModel viewModel, string realm, CancellationToken cancellationToken)
    {
        User authenticatedUser = null;
        if (string.IsNullOrWhiteSpace(authenticatedUserId))
            authenticatedUser = await AuthenticateUser(viewModel.Login, realm, cancellationToken);
        else
            authenticatedUser = await FetchAuthenticatedUser(realm, authenticatedUserId, cancellationToken);

        return authenticatedUser;
    }

    protected override async Task<CredentialsValidationResult> Validate(string realm, string authenticatedUserId, AuthenticatePasswordViewModel viewModel, CancellationToken cancellationToken)
    {
        var authenticatedUser = await GetUser(authenticatedUserId, viewModel, realm, cancellationToken);
        if (authenticatedUser == null) return CredentialsValidationResult.Error(ValidationStatus.UNKNOWN_USER);
        return await Validate(realm, authenticatedUser, viewModel, cancellationToken);
    }

    protected override Task<CredentialsValidationResult> Validate(string realm, User authenticatedUser, AuthenticatePasswordViewModel viewModel, CancellationToken cancellationToken)
    {
        if (authenticatedUser.IsBlocked())
        {
            return Task.FromResult(CredentialsValidationResult.Error(AuthFormErrorMessages.UserBlocked, AuthFormErrorMessages.UserBlocked));
        }

        var authService = _authServices.SingleOrDefault(s => s.Name == authenticatedUser.Source);
        bool isTemporaryCredential = false;
        if (authService != null)
        {
            if (!authService.Authenticate(authenticatedUser, authenticatedUser.IdentityProvisioning, viewModel.Password))
            {
                return Task.FromResult(CredentialsValidationResult.InvalidCredentials(authenticatedUser));
            }
        }
        else
        {
            var credential = authenticatedUser.Credentials.FirstOrDefault(c => c.CredentialType == Constants.AreaPwd && c.IsActive);
            if (credential == null)
            {
                return Task.FromResult(CredentialsValidationResult.Error(AuthFormErrorMessages.NoCredential, AuthFormErrorMessages.NoCredential));
            }

            if (!PasswordHelper.VerifyHash(credential, viewModel.Password))
            {
                return Task.FromResult(CredentialsValidationResult.InvalidCredentials(authenticatedUser));
            }

            isTemporaryCredential = credential.IsTemporary;
        }

        return Task.FromResult(CredentialsValidationResult.Ok(authenticatedUser, isTemporaryCredential));
    }
}
