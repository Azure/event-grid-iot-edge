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
    public class CaseInsensitiveDictionaryConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => typeof(Dictionary<string, string>).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var obj = JObject.Load(reader);
            Dictionary<string, string> defaultDictionary = obj.ToObject<Dictionary<string, string>>(serializer);
            return new Dictionary<string, string>(defaultDictionary, StringComparer.OrdinalIgnoreCase);
        }
    }
}
