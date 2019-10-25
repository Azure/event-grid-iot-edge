// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class EventsAPI
    {
        private const string ApiVersionSuffix = "?api-version=2019-01-01-preview";
        private readonly EventGridEdgeClient client;

        internal EventsAPI(EventGridEdgeClient client)
        {
            this.client = client;
        }

        public async Task PublishJsonAsync<T>(string topicName, string eventId, T payload, MediaTypeHeaderValue contentType, CancellationToken token)
        {
            using (StreamContent streamContent = this.client.CreateJsonContent(payload, nameof(this.PublishJsonAsync), contentType))
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"topics/{UrlEncoder.Default.Encode(topicName)}/events/{eventId}{ApiVersionSuffix}") { Content = streamContent })
            {
                using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token))
                {
                    await response.ThrowIfFailedAsync(request);
                }
            }
        }

        public async Task PublishJsonAsync<T>(string topicName, IEnumerable<T> payload, MediaTypeHeaderValue contentType, CancellationToken token)
        {
            using (StreamContent streamContent = this.client.CreateJsonContent(payload, nameof(this.PublishJsonAsync), contentType))
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"topics/{UrlEncoder.Default.Encode(topicName)}/events{ApiVersionSuffix}") { Content = streamContent })
            {
                using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token))
                {
                    await response.ThrowIfFailedAsync(request);
                }
            }
        }

        public async Task PublishRawAsync(string topicName, string eventId, byte[] payload, MediaTypeHeaderValue contentType, Dictionary<string, string> httpHeaders, CancellationToken token)
        {
            using (var byteArrayContent = new ByteArrayContent(payload))
            {
                byteArrayContent.Headers.ContentType = contentType;
                byteArrayContent.Headers.ContentLength = payload.Length;
                using (var request = new HttpRequestMessage(HttpMethod.Post, $"topics/{UrlEncoder.Default.Encode(topicName)}/events/{eventId}{ApiVersionSuffix}") { Content = byteArrayContent })
                {
                    if (httpHeaders != null)
                    {
                        foreach (KeyValuePair<string, string> httpHeader in httpHeaders)
                        {
                            request.Headers.Add(httpHeader.Key, httpHeader.Value);
                        }
                    }

                    using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token))
                    {
                        await response.ThrowIfFailedAsync(request);
                    }
                }
            }
        }

        public async Task PublishRawAsync(string topicName, byte[] payload, MediaTypeHeaderValue contentType, Dictionary<string, string> httpHeaders, CancellationToken token)
        {
            using (var byteArrayContent = new ByteArrayContent(payload))
            {
                byteArrayContent.Headers.ContentType = contentType;
                byteArrayContent.Headers.ContentLength = payload.Length;
                using (var request = new HttpRequestMessage(HttpMethod.Post, $"topics/{UrlEncoder.Default.Encode(topicName)}/events{ApiVersionSuffix}") { Content = byteArrayContent })
                {
                    if (httpHeaders != null)
                    {
                        foreach (KeyValuePair<string, string> httpHeader in httpHeaders)
                        {
                            request.Headers.Add(httpHeader.Key, httpHeader.Value);
                        }
                    }

                    using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token))
                    {
                        await response.ThrowIfFailedAsync(request);
                    }
                }
            }
        }
    }
}
