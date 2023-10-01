// // <copyright file="IDirectoryScanOrchestrator.cs" company="Riot Club">
// // Copyright (c) Riot Club. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.
// // </copyright>

namespace RiotClub.FireMoth.Services.Orchestration;

using System.Threading.Tasks;
using DataAccess;
using FileScanning;

/// <summary>
/// Defines the interface for a class that implements a directory scanner that reads the files in
/// the specified directory and writes the file's unique fingerprint data to an
/// <see cref="IDataAccessLayer{TValue}"/>.
/// </summary>
public interface IDirectoryScanOrchestrator
{
    /// <summary>
    /// Scans the files in the provided directory.
    /// </summary>
    /// <param name="scanDirectory">A <see cref="string"/> containing the path to the directory to
    /// scan.</param>
    /// <param name="recursive">If <c>true</c>, recursively scans subdirectories in the provided
    /// directory.</param>
    /// <returns>A <see cref="ScanResult"/> containing the result of the directory scan.</returns>
    Task<ScanResult> ScanDirectoryAsync(string scanDirectory, bool recursive);
}