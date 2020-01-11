// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK.Contracts
{
    public class StorageQueueEventSubscriptionDestination : EventSubscriptionDestination
    {
        public StorageQueueEventSubscriptionDestination()
        {
            this.EndpointType = EndpointTypes.StorageQueue;
        }

        /// <summary>
        /// StorageQueue Properties of the event subscription destination.
        /// </summary>
        public StorageQueueEventSubscriptionDestinationProperties Properties { get; set; }
    }
}
