// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class CloudEvent : CloudEvent<object>
    {
    }

    public class CloudEvent<T> : IEquatable<CloudEvent<T>>
    {
        public string Id { get; set; }

        public string Source { get; set; }

        public string SpecVersion { get; set; }

        public string Type { get; set; }

        public string DataContentType { get; set; }

        public string DataSchema { get; set; }

        public string Subject { get; set; }

        public DateTime? Time { get; set; }

        public T Data { get; set; }

        [SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "cloudevents spec demands it")]
        public T Data_Base64 { get; set; }

        public bool Equals(CloudEvent<T> other)
        {
            return StringEquals(this.Id, other.Id) &&
                StringEquals(this.Source, other.Source) &&
                StringEquals(this.SpecVersion, other.SpecVersion) &&
                StringEquals(this.Type, other.Type) &&
                StringEquals(this.DataContentType, other.DataContentType) &&
                StringEquals(this.DataSchema, other.DataSchema) &&
                StringEquals(this.Subject, other.Subject) &&
                this.Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            else if (obj is CloudEvent<T> ce)
            {
                return this.Equals(ce);
            }

            return false;
        }

#pragma warning disable CA1307 // Specify StringComparison
        public override int GetHashCode() => this.Id?.GetHashCode() ?? base.GetHashCode();
#pragma warning restore CA1307 // Specify StringComparison

        private static bool StringEquals(string str1, string str2)
        {
            return (str1 is null && str2 is null) ||
                (!(str1 is null) && !(str2 is null) && str1.Equals(str2, StringComparison.Ordinal));
        }
    }
}
