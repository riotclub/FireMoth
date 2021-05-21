// <copyright file="CsvDataAccessProvider.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of a data access provider that persists data to a stream in CSV format.
    /// </summary>
    public class CsvDataAccessProvider : IDataAccessProvider, IDisposable
    {
        private readonly CsvWriter csvWriter;
        private readonly ILogger<CsvDataAccessProvider> logger;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDataAccessProvider"/> class.
        /// </summary>
        /// <param name="outputWriter">The <see cref="TextWriter"/> object to which data is written.
        /// </param>
        /// <param name="logger">The logger.</param>
        /// <param name="leaveOpen">If <c>true</c>, the underlying <see cref="TextWriter"/> will not
        /// be closed when the <see cref="CsvDataAccessProvider"/> is disposed.</param>
        public CsvDataAccessProvider(
            StreamWriter outputWriter,
            ILogger<CsvDataAccessProvider> logger,
            bool leaveOpen = false)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (outputWriter == null)
            {
                throw new ArgumentNullException(nameof(outputWriter));
            }

            this.csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture, leaveOpen);
            this.csvWriter.WriteHeader<FileFingerprint>();
            this.csvWriter.NextRecord();
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when provided
        /// <see cref="IFileFingerprint"/> reference is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when object is in a disposed state.
        /// </exception>
        public void AddFileRecord(IFileFingerprint fingerprint)
        {
            if (this.disposed)
            {
                this.logger.LogCritical("Tried to call AddFileRecord on disposed object.");
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (fingerprint == null)
            {
                throw new ArgumentNullException(nameof(fingerprint));
            }

            var fullPath =
                fingerprint.DirectoryName + Path.DirectorySeparatorChar + fingerprint.Name;
            this.logger.LogDebug(
                "Writing fingerprint for file {FileName} with hash {HashString}.",
                fullPath,
                fingerprint.Base64Hash);
            this.csvWriter.WriteRecord(fingerprint);
            this.csvWriter.NextRecord();
        }

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
                this.csvWriter.Dispose();
            }

            this.disposed = true;
        }
    }
}
