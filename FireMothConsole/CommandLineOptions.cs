// <copyright file="CommandLineOptions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    /// <summary>
    /// Defines options available when invoking the application via command line.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the ScanDirectory option, indicating the input directory to scan.
        /// </summary>
        public string ScanDirectory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories of <see cref="ScanDirectory"/>
        /// will be recursively scanned in addition to its file contents.
        /// </summary>
        public bool RecursiveScan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output should include only files that have
        /// duplicate hash values.
        /// </summary>
        public bool DuplicatesOnly { get; set; }
    }
}
