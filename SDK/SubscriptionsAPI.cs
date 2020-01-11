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
    public class SubscriptionsAPI
    {
        private const string ApiVersionSuffix = "?api-version=2019-01-01-preview";
        private readonly EventGridEdgeClient client;

        internal SubscriptionsAPI(EventGridEdgeClient client)
        {
            this.client = client;
        }

        public async Task<EventSubscription> PutSubscriptionAsync(string topicName, string subscriptionName, EventSubscription eventSubscription, CancellationToken token)
        {
            using (StreamContent streamContent = this.client.CreateJsonContent(eventSubscription))
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"topics/{UrlEncoder.Default.Encode(topicName)}/eventSubscriptions/{UrlEncoder.Default.Encode(subscriptionName)}{ApiVersionSuffix}") { Content = streamContent })
            {
                using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
                {
                    await response.ThrowIfFailedAsync(request);
                    return await this.client.DeserializeAsync<EventSubscription>(response);
                }
            }
        }

        public async Task<EventSubscription> GetEventSubscriptionAsync(string topicName, string subscriptionName, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"topics/{UrlEncoder.Default.Encode(topicName)}/eventSubscriptions/{UrlEncoder.Default.Encode(subscriptionName)}{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
                return await this.client.DeserializeAsync<EventSubscription>(response);
            }
        }

        public async Task<IEnumerable<EventSubscription>> GetEventSubscriptionsAsync(string topicName, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"topics/{UrlEncoder.Default.Encode(topicName)}/eventSubscriptions/{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
                return await this.client.DeserializeAsync<IEnumerable<EventSubscription>>(response);
            }
        }

        public async Task DeleteEventSubscriptionAsync(string topicName, string subscriptionName, CancellationToken token)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"topics/{UrlEncoder.Default.Encode(topicName)}/eventSubscriptions/{UrlEncoder.Default.Encode(subscriptionName)}{ApiVersionSuffix}"))
            using (HttpResponseMessage response = await this.client.HttpClient.SendAsync(request, token).ConfigureAwait(false))
            {
                await response.ThrowIfFailedAsync(request);
            }
        }
    }
}
