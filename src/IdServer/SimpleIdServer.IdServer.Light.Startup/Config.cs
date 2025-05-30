﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Negotiate;
using SimpleIdServer.IdServer.Builders;
using SimpleIdServer.IdServer.Domains;
using SimpleIdServer.IdServer.Light.Startup.Converters;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdServer.IdServer.Light.Startup;

public class Config
{
    private static AuthenticationSchemeProviderDefinition Google = AuthenticationSchemeProviderDefinitionBuilder.Create("google", "Google", typeof(GoogleHandler), typeof(GoogleOptionsLite)).Build();
    private static AuthenticationSchemeProviderDefinition Negotiate = AuthenticationSchemeProviderDefinitionBuilder.Create("negotiate", "Negotiate", typeof(NegotiateHandler), typeof(NegotiateOptionsLite)).Build();

    public static List<User> Users => new List<User>
    {
        UserBuilder.Create("administrator", "password").SetFirstname("Administrator").SetEmail("adm@mail.com").SetPhoneNumber("0485").GenerateRandomTOTPKey().Build()
    };

    public static List<AuthenticationSchemeProvider> AuthenticationSchemes => new List<AuthenticationSchemeProvider>
    {
        AuthenticationSchemeProviderBuilder.Create(Google, "Google", "Google", "Google").Build(),
        AuthenticationSchemeProviderBuilder.Create(Negotiate, "Negotiate", "Negotiate", "Negotiate").SetSubject(ClaimTypes.Name).Build()
    };

    public static List<AuthenticationSchemeProviderDefinition> AuthenticationSchemeDefinitions => new List<AuthenticationSchemeProviderDefinition>
    {
        Google,
        Negotiate
    };

    public static List<Realm> Realms => new List<Realm>
    {
        RealmBuilder.CreateMaster().Build()
    };

    public static List<Language> Languages => new List<Language>
    {
        LanguageBuilder.Build(Language.Default).AddDescription("English", "en").AddDescription("Anglais", "fr").Build(),
        LanguageBuilder.Build("fr").AddDescription("French", "en").AddDescription("Français", "fr").Build()
    };
}