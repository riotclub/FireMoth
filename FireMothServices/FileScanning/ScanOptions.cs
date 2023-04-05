// <copyright file="ScanOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using RiotClub.FireMoth.Services.Orchestration;

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.IO.Abstractions;
    using RiotClub.FireMoth.Services.Output;

    /// <summary>
    /// Specifies options that are used when performing a file scan.
    /// </summary>
    /// <seealso cref="IScanOrchestrator"/>
    public class ScanOptions : IScanOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanOptions"/> class for the provided
        /// directory with a non-recursive scan and <see cref="OutputDuplicateFileFingerprintsOption.All"/> output.
        /// </summary>
        /// <param name="scanDirectory">The directory to scan.</param>
        public ScanOptions(IDirectoryInfo scanDirectory)
            : this(scanDirectory, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanOptions"/> class for the provided
        /// directory.
        /// </summary>
        /// <param name="scanDirectory">The directory to scan.</param>
        /// <param name="recursiveScan">A flag indicating whether the scan will recursively scan
        /// subdirectories of <paramref name="scanDirectory"/>.</param>
        public ScanOptions(
            IDirectoryInfo scanDirectory, bool recursiveScan)
        {
            this.ScanDirectory = scanDirectory
                ?? throw new ArgumentNullException(nameof(scanDirectory));
            this.RecursiveScan = recursiveScan;
        }

        /// <inheritdoc/>
        public IDirectoryInfo ScanDirectory { get; }

        /// <inheritdoc/>
        public bool RecursiveScan { get; }
    }
}
