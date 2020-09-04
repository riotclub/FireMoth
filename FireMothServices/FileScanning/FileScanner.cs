// <copyright file="FileScanner.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Security.Cryptography;
    using Microsoft.Extensions.FileProviders;
    using RiotClub.FireMoth.Services.DataAccess;

    /// <summary>
    /// Directory scanner implementation that reads the files in a directory and writes the file
    /// and hash to an <see cref="IDataAccessProvider"/>.
    /// </summary>
    public class FileScanner : IFileScanner
    {
        private readonly IDataAccessProvider dataAccessProvider;
        private readonly HashAlgorithm hasher;
        private readonly TextWriter logWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanner"/> class.
        /// </summary>
        /// <param name="dataAccessProvider">A <see cref="IDataAccessProvider"/> that provides
        /// access to the application backing store.</param>
        /// <param name="hasher">A <see cref="HashAlgorithm"/> that is used to compute hash values
        /// for scanned files.</param>
        /// <param name="logWriter">A <see cref="TextWriter"/> to which logging output will be
        /// written.</param>
        public FileScanner(
            IDataAccessProvider dataAccessProvider, HashAlgorithm hasher, TextWriter logWriter)
        {
            this.dataAccessProvider =
                dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
            this.hasher =
                hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.logWriter =
                logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        }

        /// <inheritdoc/>
        public ScanResult ScanDirectory(IDirectoryInfo directory, bool recursive)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!directory.Exists)
            {
                this.logWriter.WriteLine("Error: \"{0}\" is not a valid directory.", directory);
                return ScanResult.ScanFailure;
            }

            this.logWriter.WriteLine($"Scanning directory \"{directory}\"...");

            if (recursive && directory.EnumerateDirectories().Any())
            {
                foreach (IDirectoryInfo subDirectory in directory.EnumerateDirectories())
                {
                    this.ScanDirectory(subDirectory, true);
                }
            }

            this.ProcessFiles(directory.EnumerateFiles());

            return ScanResult.ScanSuccess;
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        protected internal virtual void ProcessFiles(
            IEnumerable<System.IO.Abstractions.IFileInfo> files)
        {
            Contract.Requires(files != null);

            int scannedFiles = 0;
            int skippedFiles = 0;

            foreach (System.IO.Abstractions.IFileInfo file in files)
            {
                try
                {
                    using (Stream fileStream = file.OpenRead())
                    {
                        this.logWriter.Write(file.FullName);
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        this.dataAccessProvider.AddFileRecord(file, hashString);
                        this.logWriter.WriteLine($" [{hashString}]");
                        scannedFiles++;
                    }
                }
                catch (IOException exception)
                {
                    var msg = $"An error occurred while attempting to process "
                        + $"\"{file.FullName}\": \"{exception.Message}\"; skipping file.";
                    this.logWriter.WriteLine(msg);
                    skippedFiles++;
                }
            }

            this.logWriter.WriteLine("Completed scanning {0} files.", scannedFiles);
        }

        /// <summary>
        /// Calculates a hash of the provided stream's data and returns a base 64 encoded string of
        /// the hash.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the data to hash.</param>
        /// <returns>A base 64 encoded <see cref="string"/> of the hash.</returns>
        private string GetBase64HashFromStream(Stream stream)
        {
            byte[] hashBytes = this.hasher.ComputeHash(stream);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
