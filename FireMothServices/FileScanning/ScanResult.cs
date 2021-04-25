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
        /// Gets or sets a value indicating whether the file scan was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets a list of the files that were successfully scanned during the file scan operation.
        /// </summary>
        public List<string> FilesScanned { get; } = new List<string>();

        /// <summary>
        /// Gets a list of the files that were skipped during the file scan operation.
        /// </summary>
        public List<string> FilesSkipped { get; } = new List<string>();
    }
}
