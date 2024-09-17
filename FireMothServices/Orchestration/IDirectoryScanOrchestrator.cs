// // <copyright file="IDirectoryScanOrchestrator.cs" company="Riot Club">
// // Copyright (c) Riot Club. All rights reserved.
// // Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// // </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

using System.Threading.Tasks;
using DataAccess;
using FileScanning;

/// <summary>
/// Defines the interface for a class that implements a directory scanner that reads the files in
/// the configured directory and writes the file's unique fingerprint data to an
/// <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public interface IDirectoryScanOrchestrator
{
    /// <summary>
    /// Scans the files in the configured.
    /// </summary>
    /// <returns>A <see cref="ScanResult"/> containing the result of the directory scan.</returns>
    Task<ScanResult> ScanDirectoryAsync();
}