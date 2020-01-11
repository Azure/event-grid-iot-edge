// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public static class EndpointTypes
    {
        public const string WebHook = nameof(WebHook);
        public const string EventGrid = nameof(EventGrid);
        public const string EventHub = nameof(EventHub);
        public const string ServiceBusQueue = nameof(ServiceBusQueue);
        public const string ServiceBusTopic = nameof(ServiceBusTopic);
        public const string StorageQueue = nameof(StorageQueue);
    }
}
