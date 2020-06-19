// <copyright file="CsvDataAccessProvider.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAccess
{
    using System;
    using System.Globalization;
    using System.IO;
    using CsvHelper;
    using FireMothServices.DataAccess;
    using Microsoft.Extensions.FileProviders;

    /// <summary>
    /// Implementation of a data access provider that persists data to a stream in CSV format.
    /// </summary>
    public class CsvDataAccessProvider : IDataAccessProvider
    {
        private TextWriter csvWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDataAccessProvider"/> class.
        /// </summary>
        /// <param name="outputWriter">The <see cref="TextWriter"/> object to which data is written.
        /// </param>
        public CsvDataAccessProvider(TextWriter outputWriter)
        {
            this.csvWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
        }

        /// <inheritdoc/>
        public void AddFileRecord(IFileInfo fileInfo, string base64Hash)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (fileInfo.PhysicalPath == null)
            {
                throw new ArgumentException(
                    "Provided file is not directly accessible.", nameof(fileInfo));
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

            FileFingerprint fingerprint = new FileFingerprint();
            fingerprint.FilePath = Path.GetDirectoryName(fileInfo.PhysicalPath);
            fingerprint.FileName = fileInfo.Name;
            fingerprint.Base64Hash = base64Hash;

            using (var csvHelper = new CsvWriter(this.csvWriter, CultureInfo.InvariantCulture, true))
            {
                csvHelper.WriteRecord(fingerprint);
                csvHelper.Flush();
            }

            this.csvWriter.WriteLine();
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
    }
}
