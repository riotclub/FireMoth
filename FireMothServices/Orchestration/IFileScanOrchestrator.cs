// <copyright file="IFileScanOrchestrator.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Defines the public interface for a class that implements a file scan orchestrator. A file scan orchestrator is
/// responsible for analyzing a collection of files and persisting a signature that uniquely identifies each file's
/// contents.
/// </summary>
public interface IFileScanOrchestrator
{
    /// <summary>
    /// Scans a set of files, analyzing and storing signatures that uniquely identify them.
    /// </summary>
    /// <param name="files">An <see cref="IEnumerable{String}"/> containing the full paths of the files to scan.</param>
    /// <returns>A <see cref="ScanResult"/> value indicating the result of the scanning operation.</returns>
    Task<ScanResult> ScanFilesAsync(IEnumerable<string> files);
}