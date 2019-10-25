// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class TopicProperties
    {
        /// <summary>
        /// Endpoint for the topic.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// This determines the format that Event Grid should expect for incoming events published to the topic.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public InputSchema? InputSchema { get; set; }
    }
}
