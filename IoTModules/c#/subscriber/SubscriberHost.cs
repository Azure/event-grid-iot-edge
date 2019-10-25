// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class SubscriberHost
    {
        private readonly CancellationTokenSource lifetimeCts;
        private readonly IWebHost subscriberHost;

        public SubscriberHost(X509Certificate2 serverCertificate, CancellationTokenSource lifetimeCts)
        {
            this.lifetimeCts = lifetimeCts;

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("HostSettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            this.subscriberHost = HostBuilder
                .GetHostBuilder(serverCertificate, configuration.GetSection("api"))
                .Build();
        }

        public async Task StartAsync()
        {
            await this.subscriberHost.StartAsync().ConfigureAwait(false);
            PrintAsciiArt();
        }

        public async Task WaitForShutdownAsync()
        {
            var shutdownTasks = new List<Task>
            {
                Task.Run(() => this.subscriberHost.WaitForShutdownAsync(this.lifetimeCts.Token)),
            };

            while (shutdownTasks.Count > 0)
            {
                Task completedTask = await Task.WhenAny(shutdownTasks).ConfigureAwait(false);
                if (completedTask.IsFaulted)
                {
                    Console.WriteLine($"WaitForShutdown faulted with exception:{completedTask.Exception}");

                    if (!this.lifetimeCts.IsCancellationRequested)
                    {
                        try
                        {
                            this.lifetimeCts.Cancel(throwOnFirstException: false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cancelling lifetimeCts failed. Swallowing the exception. Ex:\n{ex}");
                        }
                    }
                }

                shutdownTasks.Remove(completedTask);
            }
        }

        private static void PrintAsciiArt()
        {
            string asciiArt = @"
*************************************************************************************************************************************************
|       ___                           ______                 __     ______     _     __   _____       __                   _ __                 |
|      /   |____  __  __________     / ____/   _____  ____  / /_   / ____/____(_)___/ /  / ___/__  __/ /_  _______________(_) /_  ___  _____    |
|     / /| /_  / / / / / ___/ _ \   / __/ | | / / _ \/ __ \/ __/  / / __/ ___/ / __  /   \__ \/ / / / __ \/ ___/ ___/ ___/ / __ \/ _ \/ ___/    |
|    / ___ |/ /_/ /_/ / /  /  __/  / /___ | |/ /  __/ / / / /_   / /_/ / /  / / /_/ /   ___/ / /_/ / /_/ (__  ) /__/ /  / / /_/ /  __/ /        |
|   /_/  |_/___/\__,_/_/   \___/  /_____/ |___/\___/_/ /_/\__/   \____/_/  /_/\__,_/   /____/\__,_/_.___/____/\___/_/  /_/_.___/\___/_/         | 
|                                                                                                                                               |
*************************************************************************************************************************************************";
            Console.WriteLine(asciiArt);
        }
    }
}
