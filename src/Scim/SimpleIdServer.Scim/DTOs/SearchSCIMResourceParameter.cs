﻿// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Mvc;
using SimpleIdServer.Persistence.Filters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleIdServer.Scim.DTOs
{
    public class SearchSCIMResourceParameter
    {
        public SearchSCIMResourceParameter()
        {
            Attributes = new List<string>();
            StartIndex = 1;
            Count = 100;
            ExcludedAttributes = new List<string>();
            Attributes = new List<string>();
        }

        /// <summary>
        /// A multi-valued list of strings indicating the names of resource attributes to return in the response.
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.Attributes)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.Attributes)]
        public List<string> Attributes { get; set; }
        /// <summary>
        /// A multi-valued list of strings indicating the names of resource attributes to be removed from the default set of attributes to return.
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.ExcludedAttributes)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.ExcludedAttributes)]
        public List<string> ExcludedAttributes { get; set; }
        /// <summary>
        /// A string indicating the attribute whose value SHALL be used to order the returned responses.
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.SortBy)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.SortBy)]
        public string SortBy { get; set; }
        /// <summary>
        /// A string indicating the order in which the "sortBy" parameter is applied.
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.SortOrder)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.SortOrder)]
        public SearchSCIMRepresentationOrders? SortOrder { get; set; }
        /// <summary>
        /// An integer indicating the 1-based index of the first  query result.See Section 3.4.2.4.
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.StartIndex)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.StartIndex)]
        public int StartIndex { get; set; }
        /// <summary>
        /// An integer indicating the desired maximum number of query results per page
        /// </summary>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.Count)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.Count)]
        public int? Count { get; set; }
        /// <summary>
        /// The filter string used to request a subset of resources (RFC 7644) https://www.rfc-editor.org/rfc/rfc7644.html#section-3.4.2.2
        /// </summary>
        /// <example>userName eq john</example>
        [JsonPropertyName(SCIMConstants.StandardSCIMSearchAttributes.Filter)]
        [FromQuery(Name = SCIMConstants.StandardSCIMSearchAttributes.Filter)]
        public string Filter { get; set; }
    }
}
