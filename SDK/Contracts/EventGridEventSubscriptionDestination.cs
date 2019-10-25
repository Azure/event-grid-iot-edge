// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventGridEventSubscriptionDestination : EventSubscriptionDestination
    {
        public EventGridEventSubscriptionDestination()
        {
            this.EndpointType = EndpointTypes.EventGrid;
        }

        /// <summary>
        /// WebHook Properties of the event subscription destination.
        /// </summary>
        public EventGridEventSubscriptionDestinationProperties Properties { get; set; }
    }
}
