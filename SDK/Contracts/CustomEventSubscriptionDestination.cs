// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class CustomEventSubscriptionDestination : EventSubscriptionDestination
    {
        public CustomEventSubscriptionDestination(string endpointType)
        {
            this.EndpointType = endpointType;
        }

        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter))]
        public Dictionary<string, string> Properties { get; set; }
    }
}
