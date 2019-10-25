// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventSubscription
    {
        public string Id { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// Name of the resource.
        /// </summary>
        public string Name { get; set; }

        public EventSubscriptionProperties Properties { get; set; }
    }
}
