// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public sealed class EventGridEdgeClient
    {
        private static readonly MediaTypeHeaderValue JsonContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private readonly JsonSerializer jsonSerializer;
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;
        private readonly UTF8Encoding encoding;

        public EventGridEdgeClient(string baseAddress, int port)
            : this(baseAddress, port, null)
        {
        }

        public EventGridEdgeClient(string baseAddress, int port, IHttpClientFactory httpClientFactory)
        {
            this.BaseUri = new Uri($"{baseAddress}:{port}");

            this.encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            this.HttpClient = httpClientFactory?.CreateClient() ?? new HttpClient();
            this.HttpClient.BaseAddress = this.BaseUri;

            this.jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Converters = new JsonConverter[]
                {
                    new StringEnumConverter(),
                },
            });

            this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

            this.Topics = new TopicsAPI(this);
            this.Subscriptions = new SubscriptionsAPI(this);
            this.Events = new EventsAPI(this);
        }

        public Uri BaseUri { get; }

        public TopicsAPI Topics { get; }

        public SubscriptionsAPI Subscriptions { get; }

        public EventsAPI Events { get; }

        public HttpClient HttpClient { get; }

        public StreamContent CreateJsonContent<T>(T item, [CallerMemberName] string callerMemberName = "", MediaTypeHeaderValue contentType = null)
        {
            var stream = new RecyclableMemoryStream(this.recyclableMemoryStreamManager, callerMemberName);
            using (var sw = new StreamWriter(stream, this.encoding, 1024, leaveOpen: true))
            using (var jw = new JsonTextWriter(sw))
            {
                this.jsonSerializer.Serialize(jw, item);
                sw.Flush();
            }

            long finalPosition = stream.Position;
            stream.Position = 0;

            // the stream will get disposed when streamContent is disposed off.
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = contentType == null ? JsonContentType : contentType;
            streamContent.Headers.ContentLength = finalPosition;
            return streamContent;
        }

        public async Task<T> DeserializeAsync<T>(HttpResponseMessage response)
        {
            using (Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return this.Deserialize<T>(stream);
            }
        }

        internal T Deserialize<T>(Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            using (var jr = new JsonTextReader(sr))
            {
                return this.jsonSerializer.Deserialize<T>(jr);
            }
        }
    }
}
