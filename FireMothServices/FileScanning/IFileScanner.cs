﻿// <copyright file="IFileScanner.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    /// <summary>
    /// Defines the public interface for a class that implements a directory scanner. A directory
    /// scanner is responsible for reading, analyzing, and persisting a hash or other signature that
    /// uniquely identifies a file and its contents.
    /// </summary>
    public interface IFileScanner
    {
        /// <summary>
        /// Scans the provided directory, analyzing and storing signatures that uniquely identify
        /// the files contained within.
        /// </summary>
        /// <param name="scanOptions">An <see cref="IScanOptions"/> containing the options for the
        /// scan.</param>
        /// <returns>A <see cref="ScanResult"/> value indicating the result of the directory
        /// scanning operation.</returns>
        public ScanResult ScanDirectory(IScanOptions scanOptions);
    }
}
