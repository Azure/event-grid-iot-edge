// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGridEdge.Samples.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class HostStartup : IStartup
    {
        private readonly EventsHandler eventsHandler = new EventsHandler();
        private readonly IoTSecurity iotSecurity = new IoTSecurity();

        public void Configure(IApplicationBuilder app)
        {
            app.Use(this.RouteRequestAsync);
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }

        private async Task RouteRequestAsync(HttpContext context, Func<Task> next)
        {
            string method = context.Request.Method;
            if (method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                HttpRequest request = context.Request;
                X509Certificate2 clientCert = await request.HttpContext.Connection.GetClientCertificateAsync().ConfigureAwait(false);
                if (clientCert == null)
                {
                    throw new Exception("Client certificate not provided!");
                }

                await iotSecurity.ValidateClientCertificateAsync(clientCert);

                // TODO: Verify it is eventgrid instance indeed!
                using (var cts = new CancellationTokenSource(1000 * 30))
                {
                    this.eventsHandler.HandleEvents(request.Body);
                }
            }
            else
            {
                await next().ConfigureAwait(false);
            }
        }
    }
}
