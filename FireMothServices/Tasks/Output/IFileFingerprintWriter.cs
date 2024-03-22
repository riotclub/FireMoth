// <copyright file="IFileFingerprintWriter.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks.Output;

using System.Collections.Generic;
using System.Threading.Tasks;
using Repository;
using RiotClub.FireMoth.Services.Tasks;

/// <summary>
/// Defines the public interface for a class that implements 
/// </summary>
public interface IFileFingerprintWriter
{
    /// <summary>
    /// Writes the provided collection of file fingerprints.
    /// </summary>
    /// <param name="fileFingerprints">An <see cref="IEnumerable{IFileFingerprint}"/> collection to
    /// write.</param>
    public Task WriteFileFingerprintsAsync(IEnumerable<IFileFingerprint> fileFingerprints);
}