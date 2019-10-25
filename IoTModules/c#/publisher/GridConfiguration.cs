// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Azure.EventGridEdge.Samples.Publisher
{
    public class GridConfiguration
    {
        public string Url { get; set; }

        public int PublishIntervalInSeconds { get; set; }

        public int InitialDelayInSeconds { get; set; }

        public ClientAuthOptions ClientAuth { get; set; }

        public TopicOptions Topic { get; set; }
    }

    public class ClientAuthOptions
    {
        public string Source { get; set; }

        public string Token1 { get; set; }
    }

    public class TopicOptions
    {
        public string Name { get; set; }

        public string Schema { get; set; }
    }
}
