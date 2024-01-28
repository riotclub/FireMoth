// <copyright file="CsvFileFingerprintWriter.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Output.Csv;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Writes <see cref="IFileFingerprint"/> data in CSV format.
/// </summary>
public class CsvFileFingerprintWriter : IFileFingerprintWriter, IDisposable
{
    private readonly CsvWriter _csvWriter;
    private readonly ILogger<CsvFileFingerprintWriter> _logger;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvFileFingerprintWriter"/> class. 
    /// </summary>
    /// <param name="outputWriter">The <see cref="TextWriter"/> object to which data is written.
    /// </param>
    /// <param name="logger">The logger.</param>
    /// <param name="leaveOpen">If <c>true</c>, the underlying <see cref="TextWriter"/> will not be
    /// closed when the <see cref="CsvFileFingerprintWriter"/> is disposed.</param>
    public CsvFileFingerprintWriter(
        StreamWriter outputWriter,
        IFactory csvWriterFactory,
        ILogger<CsvFileFingerprintWriter> logger,
        bool leaveOpen = false)
    {
        csvWriterFactory.CreateWriter(outputWriter, CultureInfo.InvariantCulture);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (outputWriter == null)
        {
            throw new ArgumentNullException(nameof(outputWriter));
        }

        _csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture, leaveOpen);
        _csvWriter.Context.RegisterClassMap<FileFingerprintMap>();
        _csvWriter.WriteHeader<IFileFingerprint>();
        _csvWriter.NextRecord();
    }

    /// <inheritdoc/>
    public async Task WriteFileFingerprintsAsync(IEnumerable<IFileFingerprint> fileFingerprints)
    {
        if (_disposed)
        {
            _logger.LogCritical("Tried to call WriteFileFingerprintsAsync on disposed object.");
            throw new ObjectDisposedException(GetType().FullName);
        }

        var fileFingerprintList = fileFingerprints.ToList();
        _logger.LogDebug(
            "Writing {FileFingerprintCount} fingerprints to stream.", fileFingerprintList.Count);
        await _csvWriter.WriteRecordsAsync(fileFingerprintList);
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
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _csvWriter.Dispose();

        _disposed = true;
    }
}