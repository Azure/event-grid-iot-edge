// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------
namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class Topic
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public TopicProperties Properties { get; set; }
    }
}
