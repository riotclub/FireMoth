// <copyright file="CsvDataAccessProvider.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using CsvHelper;
    using FireMothServices.DataAccess;

    /// <summary>
    /// Implementation of a data access provider that persists data to a stream in CSV format.
    /// </summary>
    public class CsvDataAccessProvider : IDataAccessProvider, IDisposable
    {
        private CsvWriter csvWriter;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDataAccessProvider"/> class.
        /// </summary>
        /// <param name="outputWriter">The <see cref="TextWriter"/> object to which data is written.
        /// </param>
        /// <param name="leaveOpen">If <c>true</c>, the underlying <see cref="TextWriter"/> will not
        /// be closed when the <see cref="CsvDataAccessProvider"/> is disposed.</param>
        public CsvDataAccessProvider(TextWriter outputWriter, bool leaveOpen = false)
        {
            if (outputWriter == null)
            {
                throw new ArgumentNullException(nameof(outputWriter));
            }

            this.csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture, leaveOpen);
        }

        /// <inheritdoc/>
        public void AddFileRecord(IFileInfo fileInfo, string base64Hash)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (base64Hash == null)
            {
                throw new ArgumentNullException(nameof(base64Hash));
            }

            if (string.IsNullOrWhiteSpace(base64Hash))
            {
                throw new ArgumentException("Base 64 string cannot be empty.");
            }

            if (!IsBase64String(base64Hash))
            {
                throw new ArgumentException("Not a valid base 64 string.", nameof(base64Hash));
            }

            FileFingerprint fingerprint = BuildFileFingerprint(fileInfo, base64Hash);
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

        /// <summary>
        /// Determines if the provided string is a valid base 64 string.
        /// </summary>
        /// <param name="base64">A <see cref="string"/> to check for base 64 validity.</param>
        /// <returns><c>true</c> if the provided string is a valid base 64 string.</returns>
        private static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        /// <summary>
        /// Builds a <see cref="FileFingerprint"/> object from the provided <see cref="IFileInfo"/>
        /// and base 64 hash string.
        /// </summary>
        /// <param name="fileInfo">The <see cref="IFileInfo"/> implementation containing the file
        /// information to use for the returned <see cref="FileFingerprint"/>.</param>
        /// <param name="base64Hash">A <c>string</c> containing the base 64 hash string to use in
        /// the returned <see cref="FileFingerprint"/>.</param>
        /// <returns>A new <see cref="FileFingerprint"/> object containing the provided
        /// information.</returns>
        private static FileFingerprint BuildFileFingerprint(IFileInfo fileInfo, string base64Hash)
        {
            return new FileFingerprint
            {
                FilePath = Path.GetDirectoryName(fileInfo.FullName),
                FileName = fileInfo.Name,
                Base64Hash = base64Hash,
            };
        }
    }
}
