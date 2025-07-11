﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using FormBuilder.Models;
using SimpleIdServer.IdServer.Layout;
using SimpleIdServer.IdServer.Layout.AuthFormLayout;
using SimpleIdServer.IdServer.Resources;

namespace SimpleIdServer.IdServer.Email;

public static class StandardEmailAuthForms
{
    public static string emailAuthForm = "457701ae-cb96-42d7-ae15-ba83e8eb2193";
    public static string emailSendConfirmationCode = "d472f712-26f6-4e6d-9752-cc9c623c2c69";


    public static FormRecord EmailForm = OtpAuthFormLayoutBuilder.New("dd0dc677-40fd-48fa-9246-2adca1a06db0", "emailAuth", Constants.AMR, LayoutTranslations.Email)
        .ConfigureSendConfirmationCode(emailSendConfirmationCode)
        .ConfigureAuthentication(emailAuthForm)
        .AddErrorMessage(AuthFormErrorMessages.MaximumNumberActiveSessions, Global.MaximumNumberActiveSessions)
        .AddErrorMessage(AuthFormErrorMessages.MissingLogin, Global.MissingLogin)
        .AddErrorMessage(AuthFormErrorMessages.InvalidCaptcha, Global.CaptchaIsNotValid)
        .AddErrorMessage(AuthFormErrorMessages.MissingReturnUrl, Global.MissingReturnUrl)
        .AddErrorMessage(AuthFormErrorMessages.UserDoesntExist, Global.UserDoesntExist)
        .AddErrorMessage(AuthFormErrorMessages.InvalidCredential, Global.InvalidCredential)
        .AddErrorMessage(AuthFormErrorMessages.UserBlocked, Global.UserAccountIsBlocked)
        .AddErrorMessage(AuthFormErrorMessages.NoActiveOtp, Global.NoActiveOtp)
        .AddErrorMessage(AuthFormErrorMessages.AuthenticationMethodIsNotWellConfigured, Global.AuthenticationMethodIsNotWellConfigured)
        .AddErrorMessage(AuthFormErrorMessages.MissingConfirmationCode, Global.MissingConfirmationCode)
        .AddErrorMessage(AuthFormErrorMessages.InvalidEmail, Resources.Global.InvalidEmail)
        .AddSuccessMessage(AuthFormSuccessMessages.ConfirmationcodeSent, Global.ConfirmationcodeSent)
        .Build();
}
