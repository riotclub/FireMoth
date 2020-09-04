// <copyright file="IFileScanner.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System.IO.Abstractions;

    /// <summary>
    /// Defines the public interface for a class that implements a directory scanner. A directory scanner is responsible
    /// for reading, analyzing, and persisting a hash or other signature that uniquely identifies a file and its
    /// contents.
    /// </summary>
    public interface IFileScanner
    {
        /// <summary>
        /// Scans the provided directory, analyzing and storing hashes or signatures that uniquely identify the files
        /// contained within.
        /// </summary>
        /// <param name="directory">The path of the directory to scan.</param>
        /// <param name="recursive">Determines whether subdirectories of the provided directory are
        /// recursively scanned.</param>
        /// <returns>A <see cref="ScanResult"/> value indicating the result of the directory scanning operation.
        /// </returns>
        public ScanResult ScanDirectory(IDirectoryInfo directory, bool recursive);
    }
}
