// <copyright file="ScanResult.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System.Collections.Generic;
    using RiotClub.FireMoth.Services.DataAccess;

    /// <summary>
    /// Specifies the result of a file scan operation.
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Gets a list of <see cref="FileFingerprint"/>s for files that have been successfully scanned.
        /// </summary>
        public List<FileFingerprint> ScannedFiles { get; } = new List<FileFingerprint>();

        /// <summary>
        /// Gets a key-value list of files that were skipped and the reason for the skip.
        /// </summary>
        public Dictionary<string, string> SkippedFiles { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets a list of errors that occurred.
        /// </summary>
        public List<ScanError> Errors { get; } = new List<ScanError>();

        /// <summary>
        /// Combines two <see cref="ScanResult"/> objects by combining their
        /// <see cref="ScannedFiles"/>, <see cref="SkippedFiles"/>, and <see cref="Errors"/>
        /// collections.
        /// </summary>
        /// <param name="a">The first of two <see cref="ScanResult"/> operands to sum.</param>
        /// <param name="b">The second of two <see cref="ScanResult"/> operands to sum.</param>
        /// <returns>A new <see cref="ScanResult"/> containing the sum of the two operands.
        /// </returns>
        public static ScanResult operator +(ScanResult a, ScanResult b)
        {
            var result = new ScanResult();
            result.ScannedFiles.AddRange(a.ScannedFiles);
            result.ScannedFiles.AddRange(b.ScannedFiles);
            result.Errors.AddRange(a.Errors);
            result.Errors.AddRange(b.Errors);

            foreach (KeyValuePair<string, string> pair in a.SkippedFiles)
            {
                result.SkippedFiles.TryAdd(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, string> pair in b.SkippedFiles)
            {
                result.SkippedFiles.TryAdd(pair.Key, pair.Value);
            }

            return result;
        }
    }
}
