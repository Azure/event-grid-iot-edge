// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class WebHookEventSubscriptionDestinationProperties
    {
        /// <summary>
        /// The URL that represents the endpoint of the destination of an event subscription.
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        /// The base URL that represents the endpoint of the destination of an event subscription.
        /// </summary>
        public string EndpointBaseUrl { get; }

        /// <summary>
        /// Controls the max events to batch to this subscription.
        /// </summary>
        public int? MaxEventsPerBatch { get; set; }

        /// <summary>
        /// Controls the preferred batch size in Kilobytes to be used to deliver to this subscription.
        /// </summary>
        public int? PreferredBatchSizeInKilobytes { get; set; }
    }
}
