// <copyright file="IScanOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System.IO.Abstractions;

    /// <summary>
    /// Defines the public interface for a class that specifies options used during a file scan.
    /// </summary>
    public interface IScanOptions
    {
        /// <summary>
        /// Gets the directory to be scanned.
        /// </summary>
        public IDirectoryInfo ScanDirectory { get; }

        /// <summary>
        /// Gets a value indicating whether subdirectories of <see cref="ScanDirectory"/> will be
        /// recursively scanned.
        /// </summary>
        public bool RecursiveScan { get; }
    }
}
