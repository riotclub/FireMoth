// <copyright file="ScanOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

/// <summary>
/// Specifies options that are used when performing a directory scan.
/// </summary>
public class DirectoryScanOptions
{
    /// <summary>
    /// Gets or sets the directory to be scanned.
    /// </summary>
    public string? Directory { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether subdirectories of <see cref="Directory"/> will be
    /// recursively scanned.
    /// </summary>
    public bool Recursive { get; init; }
}