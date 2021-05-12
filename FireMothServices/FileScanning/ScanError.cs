// <copyright file="ScanError.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;

    /// <summary>
    /// Represents an error that occured during a file scan.
    /// </summary>
    public class ScanError
    {
        /// <summary>
        /// Gets or sets the path to the file or directory related to this error.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Gets or sets any exception related to this error.
        /// </summary>
        public Exception? Exception { get; set; }
    }
}
