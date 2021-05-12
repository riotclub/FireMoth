// <copyright file="SHA256FileHasher.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides file hashing services using the SHA256 algorithm.
    /// </summary>
    public class SHA256FileHasher : IFileHasher, IDisposable
    {
        private readonly HashAlgorithm hashAlgorithm;

        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SHA256FileHasher"/> class.
        /// </summary>
        public SHA256FileHasher()
        {
            this.hashAlgorithm = SHA256.Create();
        }

        /// <inheritdoc/>
        public byte[] ComputeHashFromStream(Stream inputStream) =>
            this.hashAlgorithm.ComputeHash(inputStream);

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and, optionally, managed resources.
        /// </summary>
        /// <param name="disposing">If true, managed resources are freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.hashAlgorithm.Dispose();
            }

            this.disposed = true;
        }
    }
}
