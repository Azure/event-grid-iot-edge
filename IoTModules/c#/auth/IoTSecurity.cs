// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Microsoft.Azure.EventGridEdge.Samples.Auth
{
    public class IoTSecurity
    {
        public void ImportCertificate(IEnumerable<X509Certificate2> certificates)
        {
            if (certificates != null)
            {
                StoreName storeName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? StoreName.CertificateAuthority : StoreName.Root;
                StoreLocation storeLocation = StoreLocation.CurrentUser;
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);
                    foreach (X509Certificate2 cert in certificates)
                    {
                        store.Add(cert);
                    }
                }
            }
        }

        public async Task<(X509Certificate2, IEnumerable<X509Certificate2>)> GetClientCertificateAsync()
        {
            Uri workloadUri = this.GetWorkloadUri();
            string moduleId = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.ModuleId);
            Uri workloadRequestUri = this.GetIdentityCertificateRequestUri(workloadUri);

            int certificateValidityInDays = IoTEdgeConstants.DefaultIdentityCertificateValidityInDays;
            DateTime expirationTime = DateTime.UtcNow.AddDays(certificateValidityInDays);
            var identityCertificateRequest = new IdentityCertificateRequest() { Expiration = expirationTime };

            var errorMessage = "Failed to retrieve ClientCertificate from IoTEdge Security Daemon.";

            try
            {
                using (HttpClient httpClient = this.GetHttpClient(workloadUri))
                {
                    string requestString = JsonConvert.SerializeObject(identityCertificateRequest);
                    var content = new StringContent(requestString);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, workloadRequestUri))
                    {
                        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpRequest.Content = content;

                        using (HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None))
                        {
                            if (httpResponse.StatusCode == HttpStatusCode.Created)
                            {
                                string responseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                                CertificateResponse cr = JsonConvert.DeserializeObject<CertificateResponse>(responseData);

                                IEnumerable<string> rawCerts = this.ParseResponse(cr.Certificate);
                                if (rawCerts.FirstOrDefault() == null)
                                {
                                    throw new Exception("Did not receive an identity certificate from IoTEdge daemon!");
                                }

                                return this.CreateX509Certificates(rawCerts, cr.PrivateKey.Bytes, moduleId);
                            }

                            errorMessage = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(errorMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve client certificate from IoTEdge Security Daemon. Reason: {e.Message}");
            }
        }

        public async Task<(X509Certificate2 serverCertificate, IEnumerable<X509Certificate2> certificateChain)> GetServerCertificateAsync()
        {
            Uri workloadUri = this.GetWorkloadUri();
            string moduleId = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.ModuleId);
            Uri workloadRequestUri = this.GetServerCertificateRequestUri(workloadUri);

            ServerCertificateRequest scRequest = this.GetServerCertificateRequest(IoTEdgeConstants.DefaultServerCertificateValidityInDays);
            try
            {
                using (HttpClient httpClient = this.GetHttpClient(workloadUri))
                {
                    string scrString = JsonConvert.SerializeObject(scRequest);
                    var content = new StringContent(scrString);
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, workloadRequestUri))
                    {
                        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpRequest.Content = content;

                        using (HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                        {
                            if (httpResponse.StatusCode == HttpStatusCode.Created)
                            {
                                string responseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                                CertificateResponse cr = JsonConvert.DeserializeObject<CertificateResponse>(responseData);
                                IEnumerable<string> rawCerts = this.ParseResponse(cr.Certificate);
                                if (rawCerts.FirstOrDefault() == null)
                                {
                                    throw new Exception($"Failed to retrieve serverCertificate from IoTEdge Security daemon. Reason: Security daemon return empty response.");
                                }

                                (X509Certificate2 serverCertificate, IEnumerable<X509Certificate2> certificateChain) = this.CreateX509Certificates(rawCerts, cr.PrivateKey.Bytes, moduleId);
                                return (serverCertificate, certificateChain);
                            }

                            string errorData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(errorData);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve server certificate from IoTEdge Security Daemon. Reason: {e.Message}");
            }
        }

        private Uri GetWorkloadUri() => new Uri(Environment.GetEnvironmentVariable(IoTEdgeConstants.WorkloadUri));

        private string GetIoTEdgeEnvironmentVariable(string envVarName) => Environment.GetEnvironmentVariable(envVarName);

        private Uri GetIdentityCertificateRequestUri(Uri workloadUri)
        {
            string workloadApiVersion = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.WorkloadApiVersion);
            string moduleId = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.ModuleId);

            string urlEncodedModuleId = WebUtility.UrlEncode(moduleId);
            string urlEncodedWorkloadApiVersion = WebUtility.UrlEncode(workloadApiVersion);

            string workloadBaseUrl = this.GetBaseUrl(workloadUri).TrimEnd('/');
            var workloadRequestUriBuilder = new StringBuilder(workloadBaseUrl);
            workloadRequestUriBuilder.Append($"/modules/{urlEncodedModuleId}/certificate/identity?api-version={urlEncodedWorkloadApiVersion}");
            return new Uri(workloadRequestUriBuilder.ToString());
        }

        private Uri GetServerCertificateRequestUri(Uri workloadUri)
        {
            string workloadApiVersion = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.WorkloadApiVersion);
            string moduleId = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.ModuleId);
            string moduleGenerationId = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.ModuleGenerationId);

            string urlEncodedModuleId = WebUtility.UrlEncode(moduleId);
            string urlEncodedModuleGenerationId = WebUtility.UrlEncode(moduleGenerationId);
            string urlEncodedWorkloadApiVersion = WebUtility.UrlEncode(workloadApiVersion);

            string workloadBaseUrl = this.GetBaseUrl(workloadUri).TrimEnd('/');
            var workloadRequestUriBuilder = new StringBuilder(workloadBaseUrl);
            workloadRequestUriBuilder.Append($"/modules/{urlEncodedModuleId}/genid/{urlEncodedModuleGenerationId}/certificate/server?api-version={urlEncodedWorkloadApiVersion}");
            return new Uri(workloadRequestUriBuilder.ToString());
        }

        private Uri GetTrustBundleRequestUri(Uri workloadUri)
        {
            string workloadApiVersion = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.WorkloadApiVersion);
            string urlEncodedWorkloadApiVersion = WebUtility.UrlEncode(workloadApiVersion);

            string workloadBaseUrl = this.GetBaseUrl(workloadUri).TrimEnd('/');
            var workloadRequestUriBuilder = new StringBuilder(workloadBaseUrl);
            workloadRequestUriBuilder.Append($"/trust-bundle?api-version={urlEncodedWorkloadApiVersion}");
            return new Uri(workloadRequestUriBuilder.ToString());
        }

        private ServerCertificateRequest GetServerCertificateRequest(int validityInDays = 90)
        {
            string edgeDeviceHostName = this.GetIoTEdgeEnvironmentVariable(IoTEdgeConstants.EdgeGatewayHostName);
            DateTime expirationTime = DateTime.UtcNow.AddDays(validityInDays);

            return new ServerCertificateRequest()
            {
                CommonName = edgeDeviceHostName,
                Expiration = expirationTime,
            };
        }

        private string GetBaseUrl(Uri workloadUri)
        {
            if (workloadUri.Scheme.Equals(IoTEdgeConstants.UnixScheme, StringComparison.OrdinalIgnoreCase))
            {
                return $"http://{workloadUri.Segments.Last()}";
            }

            return workloadUri.OriginalString;
        }

        private HttpClient GetHttpClient(Uri workloadUri)
        {
            if (workloadUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || workloadUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return new HttpClient();
            }
            else if (workloadUri.Scheme.Equals(IoTEdgeConstants.UnixScheme, StringComparison.OrdinalIgnoreCase))
            {
                return new HttpClient(new HttpUdsMessageHandler(workloadUri));
            }

            throw new Exception($"Unknow workloadUri schema specified. {workloadUri}");
        }

        private IList<string> ParseResponse(string certificateChain)
        {
            if (string.IsNullOrEmpty(certificateChain))
            {
                throw new InvalidOperationException("Trusted certificates can not be null or empty.");
            }

            // Extract each certificate's string. The final string from the split will either be empty
            // or a non-certificate entry, so it is dropped.
            string delimiter = "-----END CERTIFICATE-----";
            string[] rawCerts = certificateChain.Split(new[] { delimiter }, StringSplitOptions.None);
            return rawCerts.Take(count: rawCerts.Count() - 1).Select(c => $"{c}{delimiter}").ToList();
        }

        private (X509Certificate2 serverCertificate, IEnumerable<X509Certificate2> certificateChain) CreateX509Certificates(IEnumerable<string> rawCerts, string privateKey, string moduleId)
        {
            string primaryCert = rawCerts.First();
            RsaPrivateCrtKeyParameters keyParams = null;

            IEnumerable<X509Certificate2> x509CertsChain = this.ConvertToX509(rawCerts.Skip(1));

            IList<X509CertificateEntry> chainCertEntries = new List<X509CertificateEntry>();
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            // note: the seperator between the certificate and private key is added for safety to delinate the cert and key boundary
            var sr = new StringReader(primaryCert + "\r\n" + privateKey);
            var pemReader = new PemReader(sr);
            object certObject = pemReader.ReadObject();
            while (certObject != null)
            {
                if (certObject is Org.BouncyCastle.X509.X509Certificate x509Cert)
                {
                    chainCertEntries.Add(new X509CertificateEntry(x509Cert));
                }

                // when processing certificates generated via openssl certObject type is of AsymmetricCipherKeyPair
                if (certObject is AsymmetricCipherKeyPair)
                {
                    certObject = ((AsymmetricCipherKeyPair)certObject).Private;
                }

                if (certObject is RsaPrivateCrtKeyParameters)
                {
                    keyParams = (RsaPrivateCrtKeyParameters)certObject;
                }

                certObject = pemReader.ReadObject();
            }

            if (keyParams == null)
            {
                throw new InvalidOperationException("Private key is required");
            }

            store.SetKeyEntry(moduleId, new AsymmetricKeyEntry(keyParams), chainCertEntries.ToArray());
            using (var p12File = new MemoryStream())
            {
                store.Save(p12File, Array.Empty<char>(), new SecureRandom());
                var x509PrimaryCert = new X509Certificate2(p12File.ToArray());
                return (x509PrimaryCert, x509CertsChain);
            }
        }

        public async Task<IEnumerable<X509Certificate2>> GetTrustBundleAsync()
        {
            Uri workloadUri = this.GetWorkloadUri();
            using (HttpClient httpClient = this.GetHttpClient(workloadUri))
            {
                Uri workloadRequestUri = this.GetTrustBundleRequestUri(workloadUri);
                
                using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, workloadRequestUri))
                {
                    httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None))
                    {
                        if (httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            string responseData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                            TrustBundleResponse trustBundleResponse = JsonConvert.DeserializeObject<TrustBundleResponse>(responseData);
                            IEnumerable<string> rawCerts = this.ParseResponse(trustBundleResponse.Certificate);
                            if (rawCerts.FirstOrDefault() == null)
                            {
                                throw new Exception($"Failed to retrieve trustbundle from security daemon.");
                            }

                            return this.ConvertToX509(rawCerts);
                        }

                        string errorData = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new Exception($"Failed to retrieve trustbundle from security daemon. Reason: {errorData}");
                    }
                }
            }
        }

        public async Task ValidateClientCertificateAsync(X509Certificate2 clientCertificate)
        {
            // Please add validation more validations as appropriate

            if (this.IsCACertificate(clientCertificate))
            {
                throw new Exception("Cannot use CA certificate for client authentication!");
            }

            IEnumerable<X509Certificate2> trustedCertificates = await this.GetTrustBundleAsync();

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

        private X509Certificate2[] ConvertToX509(IEnumerable<string> rawCerts)
        {
            return rawCerts
                .Select(c => Encoding.UTF8.GetBytes(c))
                .Select(c => new X509Certificate2(c))
                .ToArray();
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
