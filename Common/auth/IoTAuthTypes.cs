// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EventGridEdge.Samples.Common.Auth
{
    public enum PrivateKeyType
    {
        [System.Runtime.Serialization.EnumMember(Value = "ref")]
        Ref = 0,

        [System.Runtime.Serialization.EnumMember(Value = "key")]
        Key = 1,
    }

    public class IdentityCertificateRequest
    {
        [JsonProperty("expiration", Required = Newtonsoft.Json.Required.Always)]
        public DateTime Expiration { get; set; }
    }

    public class ServerCertificateRequest
    {
        [JsonProperty("commonName", Required = Newtonsoft.Json.Required.Always)]
        public string CommonName { get; set; }

        [JsonProperty("expiration", Required = Newtonsoft.Json.Required.Always)]
        public DateTime Expiration { get; set; }
    }

    public class CertificateResponse
    {
        [JsonProperty("privateKey", Required = Newtonsoft.Json.Required.Always)]
        public PrivateKey PrivateKey { get; set; }

        [JsonProperty("certificate", Required = Newtonsoft.Json.Required.Always)]
        public string Certificate { get; set; }

        [JsonProperty("expiration", Required = Newtonsoft.Json.Required.Always)]
        public DateTime Expiration { get; set; }
    }

    public class PrivateKey
    {
        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateKeyType Type { get; set; }

        [JsonProperty("ref", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Ref { get; set; }

        [JsonProperty("bytes", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
    }

    public class TrustBundleResponse
    {
        [Newtonsoft.Json.JsonProperty("certificate", Required = Newtonsoft.Json.Required.Always)]
        public string Certificate { get; set; }
    }
}
