// <copyright file="StringExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Extensions
{
    using System;
    using System.Linq;

    /// <summary>
    /// Contains extension methods for <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the provided string is empty or contains only whitespace
        /// characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns><c>true</c> if the provided string is empty or contains only whitespace.
        /// </returns>
        public static bool IsEmptyOrWhiteSpace(this string value) =>
            value.All(char.IsWhiteSpace);

        /// <summary>
        /// Returns <c>true</c> if the provided string is a valid base 64 string.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns><c>true</c> if the provided string is a valid base 64 string.</returns>
        public static bool IsBase64String(this string value)
        {
            var buffer = new Span<byte>(new byte[value.Length]);
            return Convert.TryFromBase64String(value, buffer, out _);
        }
    }
}
