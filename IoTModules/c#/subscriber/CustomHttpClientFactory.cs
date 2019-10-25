// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class CustomHttpClientFactory : IHttpClientFactory
    {
        private readonly X509Certificate2 rootCA;
        private readonly X509Certificate2 clientCert;

        public CustomHttpClientFactory(X509Certificate2 rootCA, X509Certificate2 clientCert)
        {
            this.rootCA = rootCA;
            this.clientCert = clientCert;
        }

        public HttpClient CreateClient(string name)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => ValidateCertificate(this.rootCA, cert, chain, errors),
            };

            if (this.clientCert != null)
            {
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                httpClientHandler.ClientCertificates.Add(this.clientCert);
            }

            return new HttpClient(httpClientHandler);
        }

        private bool ValidateCertificate(X509Certificate2 trustedCertificateRoot, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            SslPolicyErrors terminatingErrors = sslErrors & ~SslPolicyErrors.RemoteCertificateChainErrors;
            if (terminatingErrors != SslPolicyErrors.None)
            {
                Console.WriteLine($"Server certificate validation failed due to {terminatingErrors}");
                return false;
            }

            chain.ChainPolicy.ExtraStore.Add(trustedCertificateRoot);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            if (!chain.Build(new X509Certificate2(certificate)))
            {
                var errorMessageBuilder = new StringBuilder();
                foreach (X509ChainStatus cs in chain.ChainStatus)
                {
                    errorMessageBuilder.AppendFormat(CultureInfo.InvariantCulture, $"ChainStatus: {cs.Status}, ChainStatusInfo: {cs.StatusInformation}");
                    errorMessageBuilder.AppendLine();
                }

                Console.WriteLine($"Server certificate failed chain validation due to {errorMessageBuilder}");
                return false;
            }

            return true;
        }
    }
}
