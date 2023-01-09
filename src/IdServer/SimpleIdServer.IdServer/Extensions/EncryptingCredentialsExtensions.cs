﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.IdentityModel.Tokens
{
    public static class EncryptingCredentialsExtensions
    {
        public static JsonWebKey SerializePublicJWK(this EncryptingCredentials credentials)
        {
            var result = JsonWebKeyConverter.ConvertFromSecurityKey(credentials.Key);
            result.Use = "enc";
            return result;
        }
    }
}