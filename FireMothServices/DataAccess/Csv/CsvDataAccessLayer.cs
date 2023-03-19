// <copyright file="CsvDataAccessLayer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using CsvHelper;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RiotClub.FireMoth.Services.DataAccess.Csv
{
    /// <summary>
    /// Implementation of a data access layer that persists data to a stream in CSV format.
    /// </summary>
    public class CsvDataAccessLayer : IDataAccessLayer<IFileFingerprint>, IDisposable
    {
        private readonly CsvWriter _csvWriter;
        private readonly IList<IFileFingerprint> _fileFingerprints;
        private readonly ILogger<CsvDataAccessLayer> _logger;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDataAccessLayer"/> class.
        /// </summary>
        /// <param name="outputWriter">The <see cref="TextWriter"/> object to which data is written.</param>
        /// <param name="logger">An <see cref="ILogger"/> used to log information.</param>
        /// <param name="leaveOpen">If <c>true</c>, the underlying <see cref="TextWriter"/> will not be closed when the
        /// <see cref="CsvDataAccessLayer"/> is disposed.</param>
        public CsvDataAccessLayer(
            StreamWriter outputWriter,
            ILogger<CsvDataAccessLayer> logger,
            bool leaveOpen = false)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (outputWriter == null) throw new ArgumentNullException(nameof(outputWriter));
            
            _fileFingerprints = new List<IFileFingerprint>();
            _csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture, leaveOpen);
            _csvWriter.Context.RegisterClassMap<FileFingerprintMap>();
            _csvWriter.WriteHeader<IFileFingerprint>();
            _csvWriter.NextRecord();
        }

        /// <summary>
        /// Retrieves file fingerprints from the data access layer.
        /// </summary>
        /// <param name="filter">A lambda expression that specifies a filter condition.</param>
        /// <param name="orderBy">A lambda expression that specifies an ordering.</param>
        /// <returns>IEnumerable collection of file fingerprints.</returns>
        public IEnumerable<IFileFingerprint> Get(
            Func<IFileFingerprint, bool>? filter = null,
            Func<IFileFingerprint, string>? orderBy = null)
        {
            ThrowIfDisposed();
            
            var filteredResult = _fileFingerprints.AsEnumerable();

            if (filter is not null) filteredResult = _fileFingerprints.Where(filter);
            if (orderBy is not null) filteredResult = _fileFingerprints.OrderBy(orderBy);

            return filteredResult.ToList();
        }

        /// <summary>
        /// Adds the provided <see cref="IFileFingerprint"/> to the data access layer.
        /// </summary>
        /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when provided <see cref="IFileFingerprint"/> reference is
        /// null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when object is in a disposed state.</exception>
        public void Add(IFileFingerprint fileFingerprint)
        {
            ThrowIfDisposed();

            if (fileFingerprint == null) throw new ArgumentNullException(nameof(fileFingerprint));

            _fileFingerprints.Add(fileFingerprint);

            var fullPath = Path.Combine(fileFingerprint.DirectoryName, fileFingerprint.FileName);
            _logger.LogDebug(
                "Writing fingerprint for file {FileName} with hash {HashString}.",
                fullPath,
                fileFingerprint.Base64Hash);
            _csvWriter.WriteRecord(fileFingerprint);
            _csvWriter.NextRecord();
        }

        /// <summary>
        /// Updates the provided <see cref="IFileFingerprint"/> in the data access layer.
        /// </summary>
        /// <param name="fileFingerprint">A <see cref="IFileFingerprint"/> to update.</param>
        /// <returns><c>true</c> if a file matching the provided <see cref="IFileFingerprint"/>'s full path was updated,
        /// <c>false</c> if no file with a matching path could be found.</returns>
        public bool Update(IFileFingerprint fileFingerprint)
        {
            ThrowIfDisposed();

            var match = _fileFingerprints.FirstOrDefault(fingerprint =>
                fingerprint.FullPath == fileFingerprint.FullPath);
            
            if (match is null) return false;

            _fileFingerprints.Remove(match);
            _fileFingerprints.Add(fileFingerprint);
            
            return true;
        }

        /// <summary>
        /// Deletes the provided <see cref="IFileFingerprint"/> from the data access layer.
        /// </summary>
        /// <param name="fileFingerprint"></param>
        /// <returns></returns>
        public bool Delete(IFileFingerprint fileFingerprint)
        {
            ThrowIfDisposed();

            return _fileFingerprints.Remove(fileFingerprint);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and, optionally, managed resources.
        /// </summary>
        /// <param name="disposing">If true, managed resources are freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _logger.LogDebug("Flushing CsvWriter buffer and disposing.");
                _csvWriter.Flush();
                _csvWriter.Dispose();
            }

            _disposed = true;
        }

        private void ThrowIfDisposed([CallerMemberName] string methodName = "")
        {
            if (!_disposed) return;

            _logger.LogCritical($"Tried to call {methodName} on disposed object.");
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
