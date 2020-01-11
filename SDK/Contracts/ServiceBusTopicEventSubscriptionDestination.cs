// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK.Contracts
{
    public class ServiceBusTopicEventSubscriptionDestination : EventSubscriptionDestination
    {
        public ServiceBusTopicEventSubscriptionDestination()
        {
            this.EndpointType = EndpointTypes.ServiceBusTopic;
        }

        /// <summary>
        /// EventHub Properties of the event subscription destination.
        /// </summary>
        public ServiceBusTopicEventSubscriptionDestinationProperties Properties { get; set; }
    }
}
