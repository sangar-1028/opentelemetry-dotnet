﻿// <copyright file="EntryMetadata.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace OpenTelemetry.Context
{
    /// <summary>
    /// Metadata associated with the Distributed Context entry.
    /// </summary>
    public readonly struct EntryMetadata
    {
        /// <summary>
        /// TTL indicating in-process only propagation of an entry.
        /// </summary>
        public const int NoPropagation = 0;

        /// <summary>
        /// TTL indicating unlimited propagation of an entry.
        /// </summary>
        public const int UnlimitedPropagation = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntryMetadata"/> struct.
        /// </summary>
        /// <param name="entryTTL">TTL for the distributed context entry.</param>
        public EntryMetadata(int entryTTL)
        {
            this.EntryTTL = EntryMetadata.NoPropagation;
        }

        /// <summary>
        /// Gets the EntryTTL is either NO_PROPAGATION (0) or UNLIMITED_PROPAGATION (-1).
        /// </summary>
        public int EntryTTL { get; }
    }
}
