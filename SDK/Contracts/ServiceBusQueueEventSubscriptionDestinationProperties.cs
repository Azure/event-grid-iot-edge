// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class ServiceBusQueueEventSubscriptionDestinationProperties
    {
        /// <summary>
        /// Represents the connectionstring to servicebus queue instance.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
