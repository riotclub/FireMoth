// <copyright file="ScanResult.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning;

using System;
using System.Collections.Generic;
using Repository;

/// <summary>
/// Specifies the result of a file scan operation.
/// </summary>
public class ScanResult
{
    /// <summary>
    /// Gets a list of <see cref="FileFingerprint"/>s for files that have been successfully scanned.
    /// </summary>
    public List<FileFingerprint> ScannedFiles { get; } = [];

    /// <summary>
    /// Gets a key-value list of files that were skipped and the reason for the skip.
    /// </summary>
    public Dictionary<string, string> SkippedFiles { get; } = new();

    /// <summary>
    /// Gets a list of errors that occurred.
    /// </summary>
    public List<ScanError> Errors { get; } = [];

    /// <summary>
    /// Combines two <see cref="ScanResult"/> objects by combining their
    /// <see cref="ScannedFiles"/>, <see cref="SkippedFiles"/>, and <see cref="Errors"/>
    /// collections.
    /// </summary>
    /// <param name="a">The first of two <see cref="ScanResult"/> operands to sum.</param>
    /// <param name="b">The second of two <see cref="ScanResult"/> operands to sum.</param>
    /// <returns>A new <see cref="ScanResult"/> containing the sum of the two operands.
    /// </returns>
    public static ScanResult operator +(ScanResult? a, ScanResult? b)
    {
        if (a is null && b is null)
        {
            throw new ArgumentNullException(
                string.Join(",", nameof(a), nameof(b)),
                "Cannot combine two null ScanResult objects.");
        }
        
        var result = new ScanResult();
        if (a is not null)
        {
            result.ScannedFiles.AddRange(a.ScannedFiles);
            result.Errors.AddRange(a.Errors);
            foreach (var pair in a.SkippedFiles)
                result.SkippedFiles.TryAdd(pair.Key, pair.Value);
        }

        if (b is not null)
        {
            result.ScannedFiles.AddRange(b.ScannedFiles);
            result.Errors.AddRange(b.Errors);
            foreach (var pair in b.SkippedFiles)
                result.SkippedFiles.TryAdd(pair.Key, pair.Value);
        }

        return result;
    }
}