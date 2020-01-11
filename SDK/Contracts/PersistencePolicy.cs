// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EventGridEdge.SDK
{
    public class PersistencePolicy
    {
        /// <summary>
        /// Flag to control whether persistence is required for this subscription. By default it is false.
        /// </summary>
        public bool? IsPersisted { get; set; }
    }
}
