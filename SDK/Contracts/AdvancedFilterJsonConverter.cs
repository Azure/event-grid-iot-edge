// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class AdvancedFilterJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => typeof(AdvancedFilter[]).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                return Array.Empty<AdvancedFilter>();
            }

            var array = JArray.Load(reader);
            if (array.Count == 0)
            {
                return Array.Empty<AdvancedFilter>();
            }

            var result = new List<AdvancedFilter>();
            foreach (JToken token in array)
            {
                if (token.Type != JTokenType.Object)
                {
                    continue;
                }

                var obj = (JObject)token;
                var operatorType = (AdvancedFilterOperatorType)Enum.Parse(typeof(AdvancedFilterOperatorType), obj.GetValue("operatorType", StringComparison.OrdinalIgnoreCase).ToString(), true);
                Type deserializeClass = AdvancedFilterTypeConverter.OperatorTypeToAdvancedFilter(operatorType);
                var filter = (AdvancedFilter)obj.ToObject(deserializeClass, serializer);
                result.Add(filter);
            }

            return result.ToArray();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
