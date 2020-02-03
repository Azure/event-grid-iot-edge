// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public static class HostBuilder
    {
        public static IWebHostBuilder GetHostBuilder(X509Certificate2 serverCertificate, IConfiguration configuration)
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(configuration)
                .UseKestrel((KestrelServerOptions options) =>
                {
                    IConfigurationSection kestrelConfig = configuration.GetSection("kestrel");
                    options.Configure(kestrelConfig);
                    options.AddServerHeader = kestrelConfig.GetValue("addServerHeader", false);
                    options.Limits.MaxRequestBodySize = kestrelConfig.GetValue("maxRequestBodySize", 1024 * 1034); // allow 10 extra KB over the 1 MB payload
                    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(kestrelConfig.GetValue("keepAliveTimeoutInSeconds", 120)); // default of 120 seconds
                    options.ConfigureHttpsDefaults((HttpsConnectionAdapterOptions o) =>
                    {
                        o.ServerCertificate = serverCertificate;
                        o.ClientCertificateMode = ClientCertificateMode.AllowCertificate;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            o.AllowAnyClientCertificate();
                            o.CheckCertificateRevocation = false;

                            // this is needed because IoTEdge generates a self signed certificate that is not rooted in a root certificate that is trusted by the trust provider.
                            // Kestrel rejects the request automatically because of this. We return true here so that client validation can happen when routing requests.
                            o.ClientCertificateValidation = (cert, chain, errors) =>
                            {
                                Console.WriteLine("ClientCertValidation invoked");
                                return true;
                            };
                        }
                    });

                    options.AllowSynchronousIO = true;
                })
                .ConfigureLogging((ILoggingBuilder logging) =>
                {
                    IConfigurationSection loggingConfig = configuration.GetSection("logging");
                    logging
                    .AddConfiguration(loggingConfig)
                    .AddConsole();
                })
                .UseStartup<HostStartup>();

            return hostBuilder;
        }
    }
}
