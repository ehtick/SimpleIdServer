﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Text.Json.Nodes;

namespace SimpleIdServer.Scim.ExternalEvents
{
    public class RepresentationRemovedEvent: IntegrationEvent
    {
        public RepresentationRemovedEvent()
        {

        }

        public RepresentationRemovedEvent(string id, string version, string resourceType, string realm, JsonObject representation, string token) : base(id, version, resourceType, realm, representation) 
        {
            Token = token;
        }

        public string Token { get; set; }
    }
}
