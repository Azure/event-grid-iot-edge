// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------
using System;

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public static class AdvancedFilterTypeConverter
    {
        public static Type OperatorTypeToAdvancedFilter(AdvancedFilterOperatorType operatorType)
        {
            return operatorType switch
            {
                AdvancedFilterOperatorType.NumberIn => typeof(NumberInAdvancedFilter),

                AdvancedFilterOperatorType.NumberNotIn => typeof(NumberNotInAdvancedFilter),

                AdvancedFilterOperatorType.NumberLessThan => typeof(NumberLessThanAdvancedFilter),

                AdvancedFilterOperatorType.NumberLessThanOrEquals => typeof(NumberLessThanOrEqualsAdvancedFilter),

                AdvancedFilterOperatorType.NumberGreaterThan => typeof(NumberGreaterThanAdvancedFilter),

                AdvancedFilterOperatorType.NumberGreaterThanOrEquals => typeof(NumberGreaterThanOrEqualsAdvancedFilter),

                AdvancedFilterOperatorType.BoolEquals => typeof(BoolEqualsAdvancedFilter),

                AdvancedFilterOperatorType.StringIn => typeof(StringInAdvancedFilter),

                AdvancedFilterOperatorType.StringNotIn => typeof(StringNotInAdvancedFilter),

                AdvancedFilterOperatorType.StringBeginsWith => typeof(StringBeginsWithAdvancedFilter),

                AdvancedFilterOperatorType.StringEndsWith => typeof(StringEndsWithAdvancedFilter),

                AdvancedFilterOperatorType.StringContains => typeof(StringContainsAdvancedFilter),

                _ => throw new ArgumentException($"Advanced Filter operatorType {operatorType} not supported"),
            };
        }
    }
}
