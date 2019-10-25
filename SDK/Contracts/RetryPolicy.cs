// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class RetryPolicy
    {
        /// <summary>
        /// Maximum number of delivery retry attempts for events.
        /// </summary>
        public int? MaxDeliveryAttempts { get; set; }

        /// <summary>
        /// Time To Live (in minutes) for events.
        /// </summary>
        public int? EventExpiryInMinutes { get; set; }
    }
}
