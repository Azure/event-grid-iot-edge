// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventHubEventSubscriptionDestination : EventSubscriptionDestination
    {
        public EventHubEventSubscriptionDestination()
        {
            this.EndpointType = EndpointTypes.EventHub;
        }

        /// <summary>
        /// EventHub Properties of the event subscription destination.
        /// </summary>
        public EventHubEventSubscriptionDestinationProperties Properties { get; set; }
    }
}
