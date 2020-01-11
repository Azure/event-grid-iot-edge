// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventGridEdge.SDK;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.EventGridEdge.Samples.Common.Auth;

namespace Microsoft.Azure.EventGridEdge.Samples.Publisher
{
    public static class Program
    {
        private static readonly MediaTypeHeaderValue ApplicationJsonMTHV = new MediaTypeHeaderValue("application/json");

        public static async Task Main()
        {
            var resetEvent = new ManualResetEventSlim();

            // signals to long running components when to power down (either due to a Ctrl+C, or Ctrl-Break, or SIGTERM, or SIGKILL)
            CancellationTokenSource lifetimeCts = SetupGracefulShutdown(resetEvent);

            GridConfiguration gridConfig = GetGridConfiguration();
            EventGridEdgeClient egClient = GetEventGridClientAsync(gridConfig).GetAwaiter().GetResult();

            // certificate issued by IoT Edge takes a while to be current so will wait for a bit
            int delay = gridConfig.InitialDelayInSeconds * 1000;
            Thread.Sleep(delay);

            // wait for eventgrid module to come up
            await WaitUntilEventGridModuleIsUpAndTopicExistsAsync(gridConfig, egClient, lifetimeCts.Token).ConfigureAwait(false);

            // setup eventgrid topic and publish
            await PublishEventsAsync(gridConfig, egClient, lifetimeCts.Token).ConfigureAwait(false);

            resetEvent.Set();
        }

        private static GridConfiguration GetGridConfiguration()
        {
            IConfiguration configuration = new ConfigurationBuilder()
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

        private static void ValidateConfiguration(GridConfiguration gridConfig)
        {
            if (gridConfig == null)
            {
                throw new Exception("GridConfiguration is null. Please configure the section configuration:eventgrid");
            }

            if (string.IsNullOrEmpty(gridConfig.Url))
            {
                throw new Exception("Please configure the section configuration:eventgrid:url");
            }

            if (gridConfig.Topic == null ||
                string.IsNullOrEmpty(gridConfig.Topic.Name) ||
                string.IsNullOrEmpty(gridConfig.Topic.Schema))
            {
                throw new Exception("Please configure configuration:eventgrid:topic:name, configuration:eventgrid:topic:schema");
            }

            if (!Enum.TryParse<InputSchema>(gridConfig.Topic.Schema, ignoreCase: true, out InputSchema inputSchema))
            {
                throw new Exception("Unknown value specified in configuration:eventgrid:topic:schema");
            }

            if (gridConfig.ClientAuth == null)
            {
                throw new Exception("Please configure configuration:eventgrid:clientAuth");
            }

            if (string.IsNullOrEmpty(gridConfig.ClientAuth.Source))
            {
                throw new Exception("Please configure configuration:eventgrid:clientAuth:source");
            }

            if (gridConfig.ClientAuth.Source.Equals("IoTEdge", StringComparison.OrdinalIgnoreCase))
            {
                // nothing to configure more
            }
            else
            if (gridConfig.ClientAuth.Source.Equals("BearerToken", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrEmpty(gridConfig.ClientAuth.Token1))
            {
                throw new Exception("Please configure configuration:eventgrid:clientAuth:token1");
            }
            else
            {
                throw new Exception("Unknown value configured for configuration:eventgrid:clientAuth:token1");
            }
        }

        private static async Task PublishEventsAsync(
            GridConfiguration gridConfig,
            EventGridEdgeClient egClient,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"Will publish events every {gridConfig.PublishIntervalInSeconds} seconds");

            string topicName = gridConfig.Topic.Name;
            InputSchema inputSchema = GetTopicInputSchema(gridConfig);
            int publishIntervalInSeconds = gridConfig.PublishIntervalInSeconds;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(30 * 1000))
                    {
                        EventGridEvent evtPayload = (EventGridEvent)CreateEvent(topicName, inputSchema);
                        await egClient.Events.PublishJsonAsync(topicName: topicName, evtPayload.Id, payload: evtPayload, contentType: ApplicationJsonMTHV, cts.Token).ConfigureAwait(false);
                        Console.WriteLine($"Published event {JsonConvert.SerializeObject(evtPayload)} to eventgrid module ...");
                        Console.WriteLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to publish event to topic {topicName}. Reason: {e.ToString()}");
                }

                Thread.Sleep(publishIntervalInSeconds * 1000);
            }
        }

        private static async Task<EventGridEdgeClient> GetEventGridClientAsync(GridConfiguration gridConfig)
        {
            string[] urlTokens = gridConfig.Url.Split(":");
            if (urlTokens.Length != 3)
            {
                throw new Exception($"URL should be of the form '<protocol>://<moduleName>:<portNo>' ");
            }

            string baseUrl = urlTokens[0] + ":" + urlTokens[1];
            int port = int.Parse(urlTokens[2], CultureInfo.InvariantCulture);

            if (gridConfig.ClientAuth.Source.Equals("IoTEdge", StringComparison.OrdinalIgnoreCase))
            {
                IoTSecurity iotSecurity = new IoTSecurity();
                (X509Certificate2 identityCertificate, IEnumerable<X509Certificate2> chain) = await iotSecurity.GetClientCertificateAsync();
                return new EventGridEdgeClient(baseUrl, port, new CustomHttpClientFactory(chain.First(), identityCertificate));
            }
            else if (gridConfig.ClientAuth.Source.Equals("BearerToken", StringComparison.OrdinalIgnoreCase))
            {
                EventGridEdgeClient egClient = new EventGridEdgeClient(baseUrl, port);

                HttpRequestHeaders defaultMgmtRequestHeaders = egClient.HttpClient.DefaultRequestHeaders;
                defaultMgmtRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{gridConfig.ClientAuth.Token1}");

                HttpRequestHeaders defaultRuntimeRequestHeaders = egClient.HttpClient.DefaultRequestHeaders;
                defaultRuntimeRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", $"{gridConfig.ClientAuth.Token1}");
            }

            throw new Exception("Cannot create eventgrid client!");
        }

        private static async Task WaitUntilEventGridModuleIsUpAndTopicExistsAsync(
            GridConfiguration gridConfig,
            EventGridEdgeClient egClient,
            CancellationToken cancellationToken)
        {
            InputSchema inputSchema = GetTopicInputSchema(gridConfig);
            Topic topic = new Topic
            {
                Name = gridConfig.Topic.Name,
                Properties = new TopicProperties()
                {
                    InputSchema = inputSchema,
                },
            };

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(30 * 1000))
                    {
                        var createdTopic = await egClient.Topics.PutTopicAsync(topicName: topic.Name, topic: topic, cts.Token).ConfigureAwait(false);
                        Console.WriteLine($"Successfully created topic with name {topic.Name} so event grid must be up...");
                        break;
                    }
                }
                catch (EventGridApiException e)
                {
                    LogAndBackoff(topic.Name, e);
                }
                catch (HttpRequestException e)
                {
                    LogAndBackoff(topic.Name, e);
                }
            }
        }

        private static object CreateEvent(string topicName, InputSchema inputSchema)
        {
            Random random = new Random();
            switch (inputSchema)
            {
                case InputSchema.EventGridSchema:
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
                            Ambient = new { Temperature = temperature, Humidity= humidity },
                        },
                    };

                default:
                    throw new NotImplementedException();
            }
        }

        private static void LogAndBackoff(string topicName, Exception e)
        {
            Console.WriteLine($"Failed to create topic with name {topicName}. Reason: {e.ToString()}");
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

        private static InputSchema GetTopicInputSchema(GridConfiguration gridConfig)
        {
            if (gridConfig.Topic == null || string.IsNullOrEmpty(gridConfig.Topic.Schema))
            {
                throw new Exception("Need to configure eventgrid's topic:schema");
            }

            return (InputSchema)Enum.Parse(typeof(InputSchema), gridConfig.Topic.Schema, ignoreCase: true);
        }
    }
}
