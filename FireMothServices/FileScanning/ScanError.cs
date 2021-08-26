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
    public class ScanError : IEquatable<ScanError>
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
        public string? Path { get; }

        /// <summary>
        /// Gets the message describing this error.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the exception related to this error.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Implements the equality operator.
        /// </summary>
        /// <param name="left">An instance of <see cref="ScanError"/> to test for equality.</param>
        /// <param name="right">A second instance of <see cref="ScanError"/> to test for equality.
        /// </param>
        /// <returns><c>true</c> if the two <see cref="ScanError"/>s are equal; false otherwise.
        /// </returns>
        public static bool operator ==(ScanError? left, ScanError? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Implements the inequality operator.
        /// </summary>
        /// <param name="left">An instance of <see cref="ScanError"/> to test for inequality.
        /// </param>
        /// <param name="right">A second instance of <see cref="ScanError"/> to test for inequality.
        /// </param>
        /// <returns><c>true</c> if the two <see cref="ScanError"/>s are not equal; false otherwise.
        /// </returns>
        public static bool operator !=(ScanError? left, ScanError? right)
        {
            return !(left == right);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this.Equals(obj as ScanError);
        }

        /// <inheritdoc/>
        public bool Equals(ScanError? other)
        {
            return other != null
                && this.Path == other.Path
                && this.Message == other.Message;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Path, this.Message);
        }
    }
}
