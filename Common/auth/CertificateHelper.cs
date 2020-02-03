using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Azure.EventGridEdge.Samples.Common.Auth
{
    public static class CertificateHelper
    {
        public static void ImportCertificate(params X509Certificate2[] certificates)
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

        public static bool IsCACertificate(X509Certificate2 certificate)
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
