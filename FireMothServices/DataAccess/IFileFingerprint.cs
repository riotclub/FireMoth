// <copyright file="IFileFingerprint.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
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
        IFileInfo FileInfo { get; }

        /// <summary>
        /// Gets a base 64 string representation of the file's hash.
        /// </summary>
        string Base64Hash { get; }
    }
}
