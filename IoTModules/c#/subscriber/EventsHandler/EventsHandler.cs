// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.EventGridEdge.SDK;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class EventsHandler
    {
        private readonly JsonSerializer jsonSerializer = new JsonSerializer();

        public void HandleEvents(Stream requestStream)
        {
            using (var sr = new StreamReader(requestStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            using (var jtr = new JsonTextReader(sr))
            {
                List<EventGridEvent> outputEvents = this.jsonSerializer.Deserialize<List<EventGridEvent>>(jtr);
                foreach (EventGridEvent outputEvent in outputEvents)
                {
                    Console.WriteLine($"Received Event: {JsonConvert.SerializeObject(outputEvent)}");
                    Console.WriteLine();
                }
            }
        }
    }
}
