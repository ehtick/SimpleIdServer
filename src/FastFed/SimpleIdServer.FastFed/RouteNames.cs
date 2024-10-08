﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace SimpleIdServer.FastFed;

public static class RouteNames
{
    public const string FastFed = "/fastfed";
    public const string ProviderMetadata = $"{FastFed}/provider-metadata";
    public const string Register = $"{FastFed}/register";
    public const string Start = $"{FastFed}/start";
    public const string Jwks = "/jwks";
}