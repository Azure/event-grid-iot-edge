// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGridEdge.SDK;
using Microsoft.Azure.EventGridEdge.Samples.Auth;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public static class Program
    {
        public static async Task Main()
        {
            var resetEvent = new ManualResetEventSlim();

            // signals to long running components when to power down (either due to a Ctrl+C, or Ctrl-Break, or SIGTERM, or SIGKILL)
            CancellationTokenSource lifetimeCts = SetupGracefulShutdown(resetEvent);

            GridConfiguration gridConfig = GetGridConfiguration();
            EventGridEdgeClient egClient = GetEventGridClientAsync(gridConfig).GetAwaiter().GetResult();
            SubscriberHost host = SetupSubscriberHostAsync(lifetimeCts).GetAwaiter().GetResult();

            // certificate issued by IoT Edge takes a while to be current so will wait for a bit
            Thread.Sleep(120 * 1000);

            // wait for topic to exist
            await WaitUntilEventGridModuleIsUpAndTopicExistsAsync(egClient, gridConfig.Topic.Name).ConfigureAwait(false);

            // register subscription
            await RegisterSubscriptionAsync(egClient, gridConfig).ConfigureAwait(false);

            // wait until shutdown
            await host.WaitForShutdownAsync().ConfigureAwait(false);

            // signal to gracefully shutdown
            resetEvent.Set();
        }

        private static GridConfiguration GetGridConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("HostSettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            IConfigurationSection hostConfigurationSection = configuration.GetSection("configuration");
            if (!hostConfigurationSection.GetValue("enableEventGrid", true))
            {
                throw new Exception("Need to set configuration:enableEventGrid=true to come up!");
            }

            IConfigurationSection eventGridSection = hostConfigurationSection.GetSection("eventGrid");
            GridConfiguration gridConfig = eventGridSection.Get<GridConfiguration>();
            ValidateConfiguration(gridConfig);
            return gridConfig;
        }

        private static async Task<SubscriberHost> SetupSubscriberHostAsync(CancellationTokenSource lifetimeCts)
        {
            IoTSecurity iotSecurity = new IoTSecurity();

            // get server certificate to configure with
            (X509Certificate2 serverCertificate, IEnumerable<X509Certificate2> certificateChain) =
                await iotSecurity.GetServerCertificateAsync().ConfigureAwait(false);
            iotSecurity.ImportCertificate(new List<X509Certificate2>() { serverCertificate });
            iotSecurity.ImportCertificate(certificateChain);

            Console.WriteLine($"Server Certificate issue is valid from {serverCertificate.NotBefore}, {serverCertificate.NotAfter}");

            // start subscriber webhost
            SubscriberHost host = new SubscriberHost(serverCertificate, lifetimeCts);
            await host.StartAsync().ConfigureAwait(false);
            return host;
        }

        private static async Task RegisterSubscriptionAsync(EventGridEdgeClient egClient, GridConfiguration gridConfig)
        {
            string topicName = gridConfig.Topic.Name;

            // create subscription
            EventSubscription eventSubscription = CreateEventSubscription(gridConfig);
            using (CancellationTokenSource cts = new CancellationTokenSource(30 * 1000))
            {
                await egClient.Subscriptions.PutSubscriptionAsync(topicName: topicName, subscriptionName: eventSubscription.Name, eventSubscription: eventSubscription, cts.Token).ConfigureAwait(false);
                Console.WriteLine($"Successfully created subscription {JsonConvert.SerializeObject(eventSubscription)} for topic {topicName}");
            }
        }

        private static async Task WaitUntilEventGridModuleIsUpAndTopicExistsAsync(EventGridEdgeClient egClient, string topicName)
        {
            while (true)
            {
                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(30 * 1000))
                    {
                        var topic = await egClient.Topics.GetTopicAsync(topicName: topicName, cts.Token).ConfigureAwait(false);
                        Console.WriteLine($"Successfully retrieved topic with name {topicName} so event grid must be up...");
                        break;
                    }
                }
                catch (EventGridApiException e)
                {
                    LogAndBackoff(topicName, e);
                }
                catch (HttpRequestException e)
                {
                    LogAndBackoff(topicName, e);
                }
            }
        }

        private static async Task<EventGridEdgeClient> GetEventGridClientAsync(GridConfiguration gridConfig)
        {
            IoTSecurity iotSecurity = new IoTSecurity();

            // get the client certificate to use when communicating with eventgrid
            (X509Certificate2 clientCertificate, IEnumerable<X509Certificate2> chain) = await iotSecurity.GetClientCertificateAsync().ConfigureAwait(false);
            Console.WriteLine($"Client Certificate issue is valid from {clientCertificate.NotBefore}, {clientCertificate.NotAfter}");
            string[] urlTokens = gridConfig.Url.Split(":");
            if (urlTokens.Length != 3)
            {
                throw new Exception($"URL should be of the form '<protocol>://<moduleName>:<portNo>' ");
            }

            string baseUrl = urlTokens[0] + ":" + urlTokens[1];
            int port = int.Parse(urlTokens[2], CultureInfo.InvariantCulture);

            return new EventGridEdgeClient(baseUrl, port, new CustomHttpClientFactory(chain.First(), clientCertificate));
        }

        private static void LogAndBackoff(string topicName, Exception e)
        {
            Console.WriteLine($"Failed to retrieve topic with name {topicName}. Reason: {e.ToString()}");
            Console.WriteLine("Retrying in 30 seconds...");
            Thread.Sleep(30 * 1000);
        }

        private static CancellationTokenSource SetupGracefulShutdown(ManualResetEventSlim resetEvent)
        {
            var cts = new CancellationTokenSource();

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Shutdown();

            Console.CancelKeyPress += (sender, args) =>
            {
                // Cancel this event so that the process doesn't get killed immediately, and we wait for graceful shutdown.
                args.Cancel = true;

                Shutdown();
            };

            return cts;

            void Shutdown()
            {
                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        cts.Cancel(throwOnFirstException: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cancelling gracefulShutdownCts failed, Swallowing the exception. Ex:\n{ex}");
                    }
                }

                resetEvent.Wait();
            }
        }

        private static EventSubscription CreateEventSubscription(GridConfiguration gridConfig)
        {
            string subscriptionName = gridConfig.Subscription.Name;
            string subscriptionEventSchema = gridConfig.Subscription.EventSchema;
            string subscriptionUrl = gridConfig.Subscription.Url;

            return new EventSubscription()
            {
                Name = subscriptionName,
                Properties = new EventSubscriptionProperties()
                {
                    EventDeliverySchema = (EventDeliverySchema)Enum.Parse(typeof(EventDeliverySchema), subscriptionEventSchema, true),
                    Destination = new WebHookEventSubscriptionDestination()
                    {
                        EndpointType = "WebHook",
                        Properties = new WebHookEventSubscriptionDestinationProperties()
                        {
                            EndpointUrl = subscriptionUrl,
                        },
                    },

                    Topic = gridConfig.Topic.Name,
                },
            };
        }

        private static void ValidateConfiguration(GridConfiguration gridConfig)
        {
            if (gridConfig == null)
            {
                throw new Exception("GridConfiguration is null. Please configure the section configuration:eventGrid");
            }

            if (string.IsNullOrEmpty(gridConfig.Url))
            {
                throw new Exception("Please configure the section configuration:eventGrid:url");
            }

            if (gridConfig.Topic == null ||
                string.IsNullOrEmpty(gridConfig.Topic.Name))
            {
                throw new Exception("Please configure configuration:eventGrid:topic:name");
            }

            if (gridConfig.Subscription == null)
            {
                throw new Exception("Please configure configuration:eventGrid:subscription");
            }

            if (string.IsNullOrEmpty(gridConfig.Subscription.Name) ||
                string.IsNullOrEmpty(gridConfig.Subscription.EventSchema) ||
                string.IsNullOrEmpty(gridConfig.Subscription.Url))
            {
                throw new Exception("Please configure configuration:eventGrid:subscription:name, configuration:eventGrid:subscription:url, configuration:eventGrid:subscription:eventSchema");
            }
        }
    }
}
