// <copyright file="ScanOutputOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks.Output;

/// <summary>
/// Specifies options that are used when performing a directory scan.
/// </summary>
public class ScanOutputOptions
{
    /// <summary>
    /// Gets or sets the path of the file to output.
    /// </summary>
    public string? OutputFile { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether only file fingerprint data for exact data duplicates
    /// will be included in the output.
    /// </summary>
    public bool OutputDuplicateInfoOnly { get; init; }
}