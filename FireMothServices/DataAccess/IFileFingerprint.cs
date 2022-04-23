// <copyright file="IFileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.IO.Abstractions;

    /// <summary>
    /// Defines the public interface for a class that implements properties that define a file and
    /// its hash value.
    /// </summary>
    public interface IFileFingerprint
    {
        /// <summary>
        /// Gets the <see cref="IFileInfo"/> for the file represented by this fingerprint.
        /// </summary>
        public IFileInfo FileInfo { get; }

        /// <summary>
        /// Gets a base 64 string representation of the file's hash.
        /// </summary>
        public string Base64Hash { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> at which this file fingerprint was created.
        /// </summary>
        public DateTime CreatedDateTime { get; }
    }
}
