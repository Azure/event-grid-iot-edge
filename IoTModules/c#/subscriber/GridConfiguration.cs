// Copyright(c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Azure.EventGridEdge.Samples.Subscriber
{
    public class GridConfiguration
    {
        public string Url { get; set; }

        public int InitialDelayInSeconds { get; set; }

        public TopicOptions Topic { get; set; }

        public SubscriptionOptions Subscription { get; set; }
    }

    public class TopicOptions
    {
        public string Name { get; set; }
    }

    public class SubscriptionOptions
    {
        public string Name { get; set; }

        public string EventSchema { get; set; }

        public string Url { get; set; }
    }
}