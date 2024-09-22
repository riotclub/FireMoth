// <copyright file="CsvFileFingerprintWriter.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks.Output.Csv;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks.Output;
using Serilog;

/// <summary>
/// Writes <see cref="IFileFingerprint"/> data in CSV format.
/// </summary>
public class CsvFileFingerprintWriter : ITaskHandler, IDisposable
{
    private readonly IWriter _csvWriter;
    private readonly StreamWriter _streamWriter;
    private readonly ILogger<CsvFileFingerprintWriter> _logger;
    private readonly IFileFingerprintRepository _fileFingerprintRepository;
    private readonly ScanOutputOptions _scanOutputOptions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvFileFingerprintWriter"/> class. 
    /// </summary>
    /// <param name="streamWriter">The stream to which data is written. </param>
    /// <param name="fileFingerprintRepository">Used to retrieve <see cref="FileFingerprint"/>s to
    /// be written to the output.</param>
    /// <param name="csvWriterFactory">Used to create a <see cref="CsvWriter"/> instance for writing
    /// CSV output.</param>
    /// <param name="scanOutputOptions">Used to retrieve duplicate scan output options via
    /// <see cref="ScanOutputOptions.OutputDuplicateInfoOnly"/>.</param>
    /// <param name="logger">Used to output logging data.</param>
    public CsvFileFingerprintWriter(
        StreamWriter streamWriter,
        IFileFingerprintRepository fileFingerprintRepository,
        IFactory csvWriterFactory,
        IOptions<ScanOutputOptions> scanOutputOptions,
        ILogger<CsvFileFingerprintWriter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileFingerprintRepository = fileFingerprintRepository
                                     ?? throw new ArgumentNullException(
                                         nameof(fileFingerprintRepository));
        _scanOutputOptions = scanOutputOptions.Value 
                             ?? throw new ArgumentNullException(nameof(scanOutputOptions));
        _streamWriter = streamWriter ?? throw new ArgumentNullException(nameof(streamWriter));

        _csvWriter = csvWriterFactory.CreateWriter(streamWriter, CultureInfo.InvariantCulture);
        _csvWriter.Context.RegisterClassMap<FileFingerprintMap>();
        _csvWriter.WriteHeader<FileFingerprint>();
        _csvWriter.NextRecord();
    }

    /// <inheritdoc/>
    public async Task RunTaskAsync()    
    {
        if (_disposed)
        {
            _logger.LogCritical("Tried to call WriteFileFingerprintsAsync on disposed object.");
            throw new ObjectDisposedException(GetType().FullName);
        }

        if (_streamWriter.BaseStream is FileStream fileStream)
        {
            Log.Information(
                "Writing output to '{OutputFileName}' ({DuplicatesOnlyFlag}).",
                fileStream.Name,
                _scanOutputOptions.OutputDuplicateInfoOnly ? "duplicates only" : "all records");       
        }
        
        var fingerprintsToOutput = _scanOutputOptions.OutputDuplicateInfoOnly
            ? (await _fileFingerprintRepository.GetRecordsWithDuplicateHashesAsync()).ToList()
            : (await _fileFingerprintRepository.GetAsync()).ToList();

        _logger.LogDebug(
            "Writing {FileFingerprintCount} fingerprints to stream.", fingerprintsToOutput.Count);
        await _csvWriter.WriteRecordsAsync(fingerprintsToOutput);
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