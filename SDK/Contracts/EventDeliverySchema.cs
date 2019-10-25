// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public enum EventDeliverySchema
    {
        /// <summary>
        /// Events are delivered to the destination using the Event Grid event schema.
        /// </summary>
        EventGridSchema = 0,

        /// <summary>
        /// Event Payloads are treated as byte arrays without any assumptions about their structure/format,
        /// and thus not validated / parsed / checked for errors.
        /// </summary>
        CustomEventSchema,

#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Events are delivered in the CloudEvent_1_0 schema
        /// </summary>
        CloudEventSchemaV1_0,
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
