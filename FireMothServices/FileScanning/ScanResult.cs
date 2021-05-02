// <copyright file="ScanResult.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the result of a file scan operation.
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// Gets a list of the files that were successfully scanned during the file scan operation.
        /// </summary>
        public List<string> FilesScanned { get; } = new List<string>();

        /// <summary>
        /// Gets a key-value list of the files that were skipped during the file scan operation and
        /// the reason the files were skipped.
        /// </summary>
        public Dictionary<string, string> FilesSkipped { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Combines two <see cref="ScanResult"/> objects by combining their
        /// <see cref="FilesScanned"/> and <see cref="FilesSkipped"/> collections.
        /// </summary>
        /// <param name="a">The first of two <see cref="ScanResult"/> operands to sum.</param>
        /// <param name="b">The second of two <see cref="ScanResult"/> operands to sum.</param>
        /// <returns>A new <see cref="ScanResult"/> containing the sum of the two operands.</returns>
        public static ScanResult operator +(ScanResult a, ScanResult b)
        {
            var result = new ScanResult();
            result.FilesScanned.AddRange(a.FilesScanned);
            result.FilesScanned.AddRange(b.FilesScanned);

            foreach (KeyValuePair<string, string> pair in a.FilesSkipped)
            {
                result.FilesSkipped.TryAdd(pair.Key, pair.Value);
            }

            foreach (KeyValuePair<string, string> pair in b.FilesSkipped)
            {
                result.FilesSkipped.TryAdd(pair.Key, pair.Value);
            }

            return result;
        }
    }
}
