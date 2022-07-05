// <copyright file="IFileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    /// <summary>
    /// Defines the public interface for a class that implements properties that define a file and
    /// its hash value.
    /// </summary>
    public interface IFileFingerprint
    {
        /// <summary>
        /// Gets the name of the file represented by this file fingerprint.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the name of the full directory path for the file represented by this file fingerprint.
        /// </summary>
        public string DirectoryName { get; }

        /// <summary>
        /// Gets the size, in bytes, for the file represented by this file fingerprint.
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// Gets a base 64 string representation of the hash for the file data represented by this file fingerprint.
        /// </summary>
        public string Base64Hash { get; }

        /// <summary>
        /// Gets the fully qualified path to the file represented by this file fingerprint.
        /// </summary>
        public string FullPath { get; }
    }
}
