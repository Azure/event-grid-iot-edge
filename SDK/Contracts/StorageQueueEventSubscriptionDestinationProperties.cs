// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class StorageQueueEventSubscriptionDestinationProperties
    {
        /// <summary>
        /// The connection string that points to the event subscription destination.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The storage queue of the event subscription destination.
        /// </summary>
        public string QueueName { get; set; }
    }
}
