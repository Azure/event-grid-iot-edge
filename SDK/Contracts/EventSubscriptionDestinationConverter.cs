// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventSubscriptionDestinationConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => typeof(EventSubscriptionDestination).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JObject.Load(reader);
                if (obj.TryGetValue("endpointType", StringComparison.OrdinalIgnoreCase, out JToken token) &&
                    token != null &&
                    token.Type == JTokenType.String)
                {
                    string endpointType = token.ToString();
                    Type deserializeClass = GetEventSubscriptionDestinationType(endpointType);
                    var destination = (EventSubscriptionDestination)obj.ToObject(deserializeClass, serializer);
                    return destination;
                }
                else
                {
                    throw new InvalidOperationException("Couldn't find an endpointType value on the subscription destination.");
                }
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        private static Type GetEventSubscriptionDestinationType(string endpointType)
        {
            if (endpointType.Equals(EndpointTypes.WebHook, StringComparison.OrdinalIgnoreCase))
            {
                return typeof(WebHookEventSubscriptionDestination);
            }
            else if (endpointType.Equals(EndpointTypes.EventGrid, StringComparison.OrdinalIgnoreCase))
            {
                return typeof(EventGridEventSubscriptionDestination);
            }
            else if (!string.IsNullOrWhiteSpace(endpointType))
            {
                return typeof(CustomEventSubscriptionDestination);
            }

            throw new InvalidOperationException($"Unknown endpoint type: {endpointType}");
        }
    }
}
