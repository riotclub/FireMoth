// <copyright file="IFileHasher.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis;

using System.IO;

/// <summary>
/// Defines the public interface for a class that implements file hashing operations.
/// </summary>
public interface IFileHasher
{
    /// <summary>
    /// Computes the hash of the provided stream's data.
    /// </summary>
    /// <param name="inputStream">The <see cref="Stream"/> containing the data to hash.</param>
    /// <returns>An array of <c>byte</c> containing the value of the computed hash.</returns>
    public byte[] ComputeHashFromStream(Stream inputStream);
}