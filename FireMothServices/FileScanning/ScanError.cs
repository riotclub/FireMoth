// <copyright file="ScanError.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.FileScanning
{
    using System;

    /// <summary>
    /// Represents an error that occured during a file scan.
    /// </summary>
    public class ScanError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScanError"/> class.
        /// </summary>
        /// <param name="path">The path to the file or directory that this error pertains to.
        /// </param>
        /// <param name="message">A message describing this error.</param>
        /// <param name="exception">Any exception associated with this error.</param>
        public ScanError(string? path, string message, Exception? exception)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            this.Path = path;
            this.Message = message;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the path to the file or directory related to this error.
        /// </summary>
        public string? Path { get; private set; }

        /// <summary>
        /// Gets the message describing this error.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the exception related to this error.
        /// </summary>
        public Exception? Exception { get; private set; }
    }
}
