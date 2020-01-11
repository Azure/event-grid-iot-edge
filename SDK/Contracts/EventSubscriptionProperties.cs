// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventSubscriptionProperties
    {
        /// <summary>
        /// Name of the topic of the event subscription.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Information about the destination where events have to be delivered for the event subscription.
        /// </summary>
        [JsonConverter(typeof(EventSubscriptionDestinationConverter))]
        public EventSubscriptionDestination Destination { get; set; }

        /// <summary>
        /// Information about the filter for the event subscription.
        /// </summary>
        public EventSubscriptionFilter Filter { get; set; }

        /// <summary>
        /// The event delivery schema for the event subscription.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EventDeliverySchema? EventDeliverySchema { get; set; }

        // The following two properties aren't wired up yet, so commenting it out.

        ///// <summary>
        ///// Expiration time of the event subscription.
        ///// </summary>
        // public DateTime? ExpirationTimeUtc { get; set; }

        ///// <summary>
        ///// The retry policy for events. This can be used to configure maximum number of delivery attempts
        ///// and time to live for events.
        ///// </summary>
        public RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Controls the behavior of how events are persisted for this subscription. By default there is no persistence.
        /// </summary>
        public PersistencePolicy PersistencePolicy { get; set; }
    }
}
