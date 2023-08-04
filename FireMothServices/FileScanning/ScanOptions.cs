// <copyright file="ScanOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning;

using System;
using System.IO.Abstractions;

/// <summary>
/// Specifies options that are used when performing a file scan.
/// </summary>
public class ScanOptions : IScanOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScanOptions"/> class for the provided
    /// directory.
    /// </summary>
    /// <param name="scanDirectory">The directory to scan.</param>
    /// <param name="recursiveScan">A flag indicating whether the scan will recursively scan
    /// subdirectories of <paramref name="scanDirectory"/>.</param>
    public ScanOptions(IDirectoryInfo scanDirectory, bool recursiveScan = false)
    {
        ScanDirectory = scanDirectory ?? throw new ArgumentNullException(nameof(scanDirectory));
        RecursiveScan = recursiveScan;
    }

    /// <inheritdoc/>
    public IDirectoryInfo ScanDirectory { get; }

    /// <inheritdoc/>
    public bool RecursiveScan { get; }
}