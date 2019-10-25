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
            switch (operatorType)
            {
                case AdvancedFilterOperatorType.NumberIn:
                    return typeof(NumberInAdvancedFilter);

                case AdvancedFilterOperatorType.NumberNotIn:
                    return typeof(NumberNotInAdvancedFilter);

                case AdvancedFilterOperatorType.NumberLessThan:
                    return typeof(NumberLessThanAdvancedFilter);

                case AdvancedFilterOperatorType.NumberLessThanOrEquals:
                    return typeof(NumberLessThanOrEqualsAdvancedFilter);

                case AdvancedFilterOperatorType.NumberGreaterThan:
                    return typeof(NumberGreaterThanAdvancedFilter);

                case AdvancedFilterOperatorType.NumberGreaterThanOrEquals:
                    return typeof(NumberGreaterThanOrEqualsAdvancedFilter);

                case AdvancedFilterOperatorType.BoolEquals:
                    return typeof(BoolEqualsAdvancedFilter);

                case AdvancedFilterOperatorType.StringIn:
                    return typeof(StringInAdvancedFilter);

                case AdvancedFilterOperatorType.StringNotIn:
                    return typeof(StringNotInAdvancedFilter);

                case AdvancedFilterOperatorType.StringBeginsWith:
                    return typeof(StringBeginsWithAdvancedFilter);

                case AdvancedFilterOperatorType.StringEndsWith:
                    return typeof(StringEndsWithAdvancedFilter);

                case AdvancedFilterOperatorType.StringContains:
                    return typeof(StringContainsAdvancedFilter);

                default:
                    throw new ArgumentException($"Advanced Filter operatorType {operatorType} not supported");
            }
        }

        public static AdvancedFilterOperatorType AdvancedFilterToOperatorType(Type type)
        {
            if (type == typeof(NumberInAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberIn;
            }
            else if (type == typeof(NumberNotInAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberNotIn;
            }
            else if (type == typeof(NumberLessThanAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberLessThan;
            }
            else if (type == typeof(NumberLessThanOrEqualsAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberLessThanOrEquals;
            }
            else if (type == typeof(NumberGreaterThanAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberGreaterThan;
            }
            else if (type == typeof(NumberGreaterThanOrEqualsAdvancedFilter))
            {
                return AdvancedFilterOperatorType.NumberGreaterThanOrEquals;
            }
            else if (type == typeof(BoolEqualsAdvancedFilter))
            {
                return AdvancedFilterOperatorType.BoolEquals;
            }
            else if (type == typeof(StringInAdvancedFilter))
            {
                return AdvancedFilterOperatorType.StringIn;
            }
            else if (type == typeof(StringNotInAdvancedFilter))
            {
                return AdvancedFilterOperatorType.StringNotIn;
            }
            else if (type == typeof(StringBeginsWithAdvancedFilter))
            {
                return AdvancedFilterOperatorType.StringBeginsWith;
            }
            else if (type == typeof(StringEndsWithAdvancedFilter))
            {
                return AdvancedFilterOperatorType.StringEndsWith;
            }
            else if (type == typeof(StringContainsAdvancedFilter))
            {
                return AdvancedFilterOperatorType.StringContains;
            }

            throw new InvalidOperationException($"Unknown type {type.Name}. Cannot map it to an {nameof(AdvancedFilterOperatorType)}.");
        }
    }
}
