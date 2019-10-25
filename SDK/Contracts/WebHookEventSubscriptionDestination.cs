// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class WebHookEventSubscriptionDestination : EventSubscriptionDestination
    {
        public WebHookEventSubscriptionDestination()
        {
            this.EndpointType = EndpointTypes.WebHook;
        }

        /// <summary>
        /// WebHook Properties of the event subscription destination.
        /// </summary>
        public WebHookEventSubscriptionDestinationProperties Properties { get; set; }
    }
}
