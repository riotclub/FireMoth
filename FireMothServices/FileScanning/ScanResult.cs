// <copyright file="ScanResult.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the result of a file scan operation.
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Gets a list of the files that were successfully scanned during the file scan operation.
        /// </summary>
        public List<string> ScannedFiles { get; } = new List<string>();

        /// <summary>
        /// Gets a key-value list of the files that were skipped during the file scan operation and
        /// the reason the files were skipped.
        /// </summary>
        public Dictionary<string, string> SkippedFiles { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets a key-value list of errors that occurred during the file scan operation and any
        /// exceptions associated with the error
        /// </summary>
        public Dictionary<string, Exception> Errors { get; } = new Dictionary<string, Exception>();

        /// <summary>
        /// Combines two <see cref="ScanResult"/> objects by combining their
        /// <see cref="ScannedFiles"/> and <see cref="SkippedFiles"/> collections.
        /// </summary>
        /// <param name="a">The first of two <see cref="ScanResult"/> operands to sum.</param>
        /// <param name="b">The second of two <see cref="ScanResult"/> operands to sum.</param>
        /// <returns>A new <see cref="ScanResult"/> containing the sum of the two operands.</returns>
        public static ScanResult operator +(ScanResult a, ScanResult b)
        {
            var result = new ScanResult();
            result.ScannedFiles.AddRange(a.ScannedFiles);
            result.ScannedFiles.AddRange(b.ScannedFiles);

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
