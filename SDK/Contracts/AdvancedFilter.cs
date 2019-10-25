// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class AdvancedFilter
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AdvancedFilterOperatorType OperatorType { get; set; }

        public string Key { get; set; }
    }

    public class BoolEqualsAdvancedFilter : AdvancedFilter
    {
        public BoolEqualsAdvancedFilter(string key, bool value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public BoolEqualsAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.BoolEquals;
        }

        public bool Value { get; set; }
    }

    public class NumberLessThanAdvancedFilter : AdvancedFilter
    {
        public NumberLessThanAdvancedFilter(string key, decimal value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public NumberLessThanAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberLessThan;
        }

        public decimal Value { get; set; }
    }

    public class NumberGreaterThanAdvancedFilter : AdvancedFilter
    {
        public NumberGreaterThanAdvancedFilter(string key, decimal value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public NumberGreaterThanAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberGreaterThan;
        }

        public decimal Value { get; set; }
    }

    public class NumberLessThanOrEqualsAdvancedFilter : AdvancedFilter
    {
        public NumberLessThanOrEqualsAdvancedFilter(string key, decimal value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public NumberLessThanOrEqualsAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberLessThanOrEquals;
        }

        public decimal Value { get; set; }
    }

    public class NumberGreaterThanOrEqualsAdvancedFilter : AdvancedFilter
    {
        public NumberGreaterThanOrEqualsAdvancedFilter(string key, decimal value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public NumberGreaterThanOrEqualsAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberGreaterThanOrEquals;
        }

        public decimal Value { get; set; }
    }

    public class NumberInAdvancedFilter : AdvancedFilter
    {
        public NumberInAdvancedFilter(string key, params decimal[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public NumberInAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberIn;
        }

        public decimal[] Values { get; set; }
    }

    public class NumberNotInAdvancedFilter : AdvancedFilter
    {
        public NumberNotInAdvancedFilter(string key, params decimal[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public NumberNotInAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.NumberNotIn;
        }

        public decimal[] Values { get; set; }
    }

    public class StringInAdvancedFilter : AdvancedFilter
    {
        public StringInAdvancedFilter(string key, params string[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public StringInAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.StringIn;
        }

        public string[] Values { get; set; }
    }

    public class StringNotInAdvancedFilter : AdvancedFilter
    {
        public StringNotInAdvancedFilter(string key, params string[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public StringNotInAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.StringNotIn;
        }

        public string[] Values { get; set; }
    }

    public class StringBeginsWithAdvancedFilter : AdvancedFilter
    {
        public StringBeginsWithAdvancedFilter(string key, params string[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public StringBeginsWithAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.StringBeginsWith;
        }

        public string[] Values { get; set; }
    }

    public class StringEndsWithAdvancedFilter : AdvancedFilter
    {
        public StringEndsWithAdvancedFilter(string key, params string[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public StringEndsWithAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.StringEndsWith;
        }

        public string[] Values { get; set; }
    }

    public class StringContainsAdvancedFilter : AdvancedFilter
    {
        public StringContainsAdvancedFilter(string key, params string[] values)
            : this()
        {
            this.Key = key;
            this.Values = values;
        }

        public StringContainsAdvancedFilter()
        {
            this.OperatorType = AdvancedFilterOperatorType.StringContains;
        }

        public string[] Values { get; set; }
    }
}
