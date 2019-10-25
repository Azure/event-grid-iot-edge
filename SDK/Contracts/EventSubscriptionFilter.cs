// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventSubscriptionFilter
    {
        /// <summary>
        /// An optional string to filter events for an event subscription based on a resource path prefix.
        /// The format of this depends on the publisher of the events.
        /// Wildcard characters are not supported in this path.
        /// </summary>
        // e.g. "blobservices/default/containers/blobContainer1/folder1/folder2"
        public string SubjectBeginsWith { get; set; }

        /// <summary>
        /// An optional string to filter events for an event subscription based on a resource path suffix.
        /// Wildcard characters are not supported in this path.
        /// </summary>
        // e.g. ".jpg"
        public string SubjectEndsWith { get; set; }

        /// <summary>
        /// A list of applicable event types that need to be part of the event subscription.
        /// If it is desired to subscribe to all event types, the string "all" needs to be specified as an element in this list.
        /// </summary>
        // e.g. "*" or "resourceCreated"
        public List<string> IncludedEventTypes { get; set; }

        /// <summary>
        /// Specifies if the SubjectBeginsWith and SubjectEndsWith properties of the filter
        /// should be compared in a case sensitive manner.
        /// </summary>
        public bool IsSubjectCaseSensitive { get; set; }

        [JsonConverter(typeof(AdvancedFilterJsonConverter))]
        public AdvancedFilter[] AdvancedFilters { get; set; }
    }
}
