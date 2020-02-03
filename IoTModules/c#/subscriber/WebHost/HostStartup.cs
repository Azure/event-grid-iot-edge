// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGridEdge.IotEdge;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class HostStartup : IStartup
    {
        private readonly EventsHandler eventsHandler = new EventsHandler();
        private readonly SecurityDaemonClient iotSecurity = new SecurityDaemonClient();

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
                bool isHttps = request.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
                if (isHttps)
                {
                    X509Certificate2 clientCert = await context.Connection.GetClientCertificateAsync().ConfigureAwait(false);
                    if (clientCert == null)
                    {
                        throw new Exception("Client certificate not provided!");
                    }

                    Console.WriteLine("Validating client certificate...");
                    await this.ValidateClientCertificateAsync(clientCert);
                }

                // TODO: Verify it is eventgrid instance indeed!
                using var cts = new CancellationTokenSource(1000 * 30);
                this.eventsHandler.HandleEvents(request.Body);
            }
            else
            {
                await next().ConfigureAwait(false);
            }
        }

        private async Task ValidateClientCertificateAsync(X509Certificate2 clientCertificate)
        {
            // Please add validation more validations as appropriate

            if (this.IsCACertificate(clientCertificate))
            {
                throw new Exception("Cannot use CA certificate for client authentication!");
            }

            IEnumerable<X509Certificate2> trustedCertificates = await this.iotSecurity.GetTrustBundleAsync();

            using (X509Chain chain = new X509Chain())
            {
                foreach (X509Certificate2 trustedClientCert in trustedCertificates)
                {
                    chain.ChainPolicy.ExtraStore.Add(trustedClientCert);
                }

                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // IoTEdge generates a self-signed certificate by default, that is not rooted in a root certificate that is trusted by the trust provider hence this flag is needed
                    // so that build returns true if root terminates in a self-signed certificate
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                }

                if (!chain.Build(clientCertificate))
                {
                    var errorMessageBuilder = new StringBuilder();
                    foreach (X509ChainStatus cs in chain.ChainStatus)
                    {
                        errorMessageBuilder.AppendFormat(CultureInfo.InvariantCulture, $"ChainStatus: {cs.Status}, ChainStatusInfo: {cs.StatusInformation}");
                        errorMessageBuilder.AppendLine();
                    }

                    throw new Exception($"ClientCertificate is not valid! Reason: Failed chain validation. Details: {errorMessageBuilder}");
                }
            }
        }

        private bool IsCACertificate(X509Certificate2 certificate)
        {
            // https://tools.ietf.org/html/rfc3280#section-4.2.1.3
            // The keyCertSign bit is asserted when the subject public key is
            // used for verifying a signature on public key certificates.  If the
            // keyCertSign bit is asserted, then the cA bit in the basic
            // constraints extension (section 4.2.1.10) MUST also be asserted.

            // https://tools.ietf.org/html/rfc3280#section-4.2.1.10
            // The cA boolean indicates whether the certified public key belongs to
            // a CA.  If the cA boolean is not asserted, then the keyCertSign bit in
            // the key usage extension MUST NOT be asserted.
            X509ExtensionCollection extensionCollection = certificate.Extensions;
            foreach (X509Extension extension in extensionCollection)
            {
                if (extension is X509BasicConstraintsExtension basicConstraintExtension)
                {
                    if (basicConstraintExtension.CertificateAuthority)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
