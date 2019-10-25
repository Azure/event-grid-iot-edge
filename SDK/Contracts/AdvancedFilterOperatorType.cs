// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public enum AdvancedFilterOperatorType
    {
        NumberIn,
        NumberNotIn,
        NumberLessThan,
        NumberGreaterThan,
        NumberLessThanOrEquals,
        NumberGreaterThanOrEquals,
        BoolEquals,
        StringIn,
        StringNotIn,
        StringBeginsWith,
        StringEndsWith,
        StringContains,
    }
}
