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
        private readonly TextWriter outputWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileScanner"/> class.
        /// </summary>
        /// <param name="dataAccessProvider">A <see cref="IDataAccessProvider"/> that provides
        /// access to the application backing store.</param>
        /// <param name="hasher">A <see cref="HashAlgorithm"/> that is used to compute hash values
        /// for scanned files.</param>
        /// <param name="outputWriter">A <see cref="TextWriter"/> used for status output.</param>
        public FileScanner(
            IDataAccessProvider dataAccessProvider, HashAlgorithm hasher, TextWriter outputWriter)
        {
            this.dataAccessProvider =
                dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
            this.hasher =
                hasher ?? throw new ArgumentNullException(nameof(hasher));
            this.outputWriter =
                outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
        }

        /// <summary>
        /// Scans the provided directory, generates hash data for each file, and writes the file and
        /// hash data to a stream.
        /// </summary>
        /// <param name="directory">The directory to scan.</param>
        /// <returns>A <see cref="ScanResult"/> inticating the result of the scanning operation.
        /// </returns>
        public ScanResult ScanDirectory(IDirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!directory.Exists)
            {
                this.outputWriter.WriteLine("Error: \"{0}\" is not a valid directory.", directory);
                return ScanResult.ScanFailure;
            }

            this.outputWriter.WriteLine($"Scanning directory \"{directory}\"...");
            //IEnumerable<IFileInfo> files = directory.EnumerateFiles();
            this.ProcessFiles(directory.EnumerateFiles());

            /*
            using (var provider = new PhysicalFileProvider(directory.FullName))
            {
                IDirectoryContents directoryContents = provider.GetDirectoryContents(string.Empty);
                this.ProcessFiles(directoryContents);
            }
            */

            return ScanResult.ScanSuccess;
        }

        public ScanResult ScanDirectory(string directory)
        {
            var fileSystem = new FileSystem();
            var directoryInfo = fileSystem.DirectoryInfo.FromDirectoryName(directory);
            return this.ScanDirectory(directoryInfo);
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        protected internal virtual void ProcessFiles(IDirectoryContents files)
        {
            Contract.Requires(files != null);

            int scannedFiles = 0;
            int skippedFiles = 0;

            foreach (Microsoft.Extensions.FileProviders.IFileInfo file in files)
            {
                if (file.IsDirectory)
                {
                    continue;
                }

                try
                {
                    using (Stream fileStream = file.CreateReadStream())
                    {
                        this.outputWriter.Write(file.PhysicalPath);
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        //this.dataAccessProvider.AddFileRecord(file, hashString);
                        this.outputWriter.WriteLine($" [{hashString}]");
                        scannedFiles++;
                    }
                }
                catch (IOException exception)
                {
                    string msg = $"An error occurred while attempting to process "
                        + $"\"{file.PhysicalPath}\": \"{exception.Message}\"; skipping file.";
                    this.outputWriter.WriteLine(msg);
                    skippedFiles++;
                }
            }

            this.outputWriter.WriteLine("Completed scanning {0} files.", scannedFiles);
        }

        /// <summary>
        /// Hashes a set of files and records the filename and hash string.
        /// </summary>
        /// <param name="files">The set of files to hash and record.</param>
        protected internal virtual void ProcessFiles(IEnumerable<System.IO.Abstractions.IFileInfo> files)
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
                        this.outputWriter.Write(file.FullName);
                        var hashString = this.GetBase64HashFromStream(fileStream);
                        this.dataAccessProvider.AddFileRecord(file, hashString);
                        this.outputWriter.WriteLine($" [{hashString}]");
                        scannedFiles++;
                    }
                }
                catch (IOException exception)
                {
                    string msg = $"An error occurred while attempting to process "
                        + $"\"{file.FullName}\": \"{exception.Message}\"; skipping file.";
                    this.outputWriter.WriteLine(msg);
                    skippedFiles++;
                }
            }

            this.outputWriter.WriteLine("Completed scanning {0} files.", scannedFiles);
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
