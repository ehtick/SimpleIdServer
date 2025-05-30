﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using SimpleIdServer.IdServer.Domains;
using System.Collections.Generic;

namespace SimpleIdServer.IdServer.Config;

public static class DefaultKeys
{
    public static List<SerializedFileKey> All
    {
        get
        {
            var result = new List<SerializedFileKey>
            {
                KeyGenerator.GenerateRSASigningCredentials(DefaultRealms.Master, "rsa-1"),
                KeyGenerator.GenerateX509SigningCredentials(DefaultRealms.Master, "certificate")
            };
            return result;
        }
    }
}
