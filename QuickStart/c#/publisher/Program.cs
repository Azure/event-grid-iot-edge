// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGridEdge.SDK;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.QuickStart.Publisher
{
    public static class Program
    {
        private static readonly string eventGridBaseAddress = "http://eventgridmodule";
        private static readonly int eventGridPort = 5888;
        private static readonly TimeSpan initialDelay = TimeSpan.FromMinutes(1);

        private static readonly string topicName = "quickstarttopic";
        private static readonly InputSchema topicSchema = InputSchema.CustomEventSchema;

        private static readonly string subscriptionName= "quickstartsub";
        private static readonly EventDeliverySchema deliverySchema = EventDeliverySchema.CustomEventSchema;
        private static readonly string subscriberUrl = "http://subscriber:80/api/subscriber";

        private static readonly MediaTypeHeaderValue ApplicationJsonMTHV = new MediaTypeHeaderValue("application/json");

        public static async Task Main()
        {
            Console.WriteLine($"\nWaiting a few minute(s) to create topic '{topicName}' ...\n");
            Thread.Sleep(initialDelay);

            Console.WriteLine($"EventGrid Module's URL: {eventGridBaseAddress}:{eventGridPort}");
            EventGridEdgeClient egClient = new EventGridEdgeClient(eventGridBaseAddress, eventGridPort);

            // create topic
            Topic topic = new Topic()
            {
                Name = topicName,
                Properties = new TopicProperties()
                {
                    InputSchema = topicSchema
                }
            };

            var createdTopic = await egClient.Topics.PutTopicAsync(topicName: topicName, topic: topic, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"Created topic with Name: {topic.Name}, Schema: {topic.Properties.InputSchema} ...");

            // the recommendation is to create subscribers from subscription modules or a "management" module. for the purposes of quickstart we are creating it here.
            EventSubscription eventSubscription = new EventSubscription()
            {
                Name = subscriptionName,
                Properties = new EventSubscriptionProperties
                {
                    Topic = topicName,
                    EventDeliverySchema = deliverySchema,
                    Destination = new WebHookEventSubscriptionDestination()
                    {
                        EndpointType = "Webhook",
                        Properties = new WebHookEventSubscriptionDestinationProperties()
                        {
                            EndpointUrl = subscriberUrl,
                         }
                    }
                }
            };

            var createdSubscription = await egClient.Subscriptions.PutSubscriptionAsync(topicName: topicName, subscriptionName: subscriptionName, eventSubscription: eventSubscription, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"Created subscription with Name: {createdSubscription.Name}, Schema: {topic.Properties.InputSchema}, EndpointUrl: {subscriberUrl} for topic: {topic.Name} ...");

            Console.WriteLine($"\nWaiting a few minute(s) before publishing events ...\n");
            Thread.Sleep(initialDelay);

            // keep publishing events
            while (true)
            {
                EventGridEvent evt = GetEvent();
                Console.WriteLine($"\nPublishing event: {JsonConvert.SerializeObject(evt)}");
                egClient.Events.PublishJsonAsync(topicName: topicName, (new List<EventGridEvent>() { evt }), ApplicationJsonMTHV, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        private static EventGridEvent GetEvent()
        {
            Random random = new Random();
            string subject = $"sensor:{random.Next(1, 100)}";
            double temperature = random.NextDouble();
            double pressure = random.NextDouble();
            double humidity = random.Next(1, 25);
            return new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                Topic = topicName,
                Subject = subject,
                EventType = "sensor.temperature",
                DataVersion = "1.0",
                EventTime = DateTime.UtcNow,
                Data = new
                {
                    Machine = new { Temperature = temperature, Pressure = pressure },
                    Ambient = new { Temperature = temperature, Humidity = humidity },
                },
            };
        }
    }
}
