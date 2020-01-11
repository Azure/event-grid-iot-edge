// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class TopicsAPI
    {
        public const string ApiVersionSuffix = "?api-version=2019-01-01-preview";
        private readonly EventGridEdgeClient client;

        internal TopicsAPI(EventGridEdgeClient client)
        {
            this.client = client;
        }

        public async Task<Topic> PutTopicAsync(string topicName, Topic topic, CancellationToken token)
        {
            using (StreamContent streamContent = this.client.CreateJsonContent(topic))
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"topics/{UrlEncoder.Default.Encode(topicName)}{ApiVersionSuffix}") { Content = streamContent })
            {
                using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
                {
                    await response.ThrowIfFailedAsync(request);
                    return await this.client.DeserializeAsync<Topic>(response);
                }
            }
        }

        public async Task<Topic> GetTopicAsync(string topicName, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"topics/{UrlEncoder.Default.Encode(topicName)}{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
                return await this.client.DeserializeAsync<Topic>(response);
            }
        }

        public async Task<IEnumerable<Topic>> GetTopicsAsync(CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"topics{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
                return await this.client.DeserializeAsync<IEnumerable<Topic>>(response);
            }
        }

        public async Task DeleteTopicAsync(string topicName, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"topics/{UrlEncoder.Default.Encode(topicName)}{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
            }
        }
    }
}
