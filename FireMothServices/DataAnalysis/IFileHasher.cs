// <copyright file="IFileHasher.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis
{
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
}
