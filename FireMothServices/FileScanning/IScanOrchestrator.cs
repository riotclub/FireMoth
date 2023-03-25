// <copyright file="IScanOrchestrator.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    /// <summary>
    /// Defines the public interface for a class that implements a file scan orchestrator. A scan orchestrator is
    /// responsible for reading, analyzing, and persisting a hash or other signature that uniquely identifies a file
    /// and its contents.
    /// </summary>
    public interface IScanOrchestrator
    {
        /// <summary>
        /// Scans a set of files, analyzing and storing signatures that uniquely identify them.
        /// </summary>
        /// <returns>A <see cref="ScanResult"/> value indicating the result of the scanning operation.</returns>
        public ScanResult ScanDirectory();
    }
}
