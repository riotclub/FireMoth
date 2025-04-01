// <copyright file="StringExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Extensions;

using System;
using System.Collections.Generic;

/// <summary>A set of extension methods for string objects used to facilitate command line argument
/// parsing. Implementation by Daniel Earwicker via Stack Overflow.
/// </summary>
/// <seealso href="https://stackoverflow.com/a/298990">Stack Overflow thread: &quot;Split string
/// containing command-line parameters into string[] in C#&quot;</seealso>
public static class StringExtensions
{
    /// <summary>Splits a string based on the provided controller function.</summary>
    /// <param name="str">The <see cref="string"/> to split.</param>
    /// <param name="controller">A method used the examine each character in the string and decide
    /// when to yield a token.</param>
    /// <returns>An <see cref="IEnumerable{String}"/> containing the individual tokens from the
    /// provided string.</returns>
    public static IEnumerable<string> Split(this string str, Func<char, bool> controller)
    {
        int nextPiece = 0;

        for (int c = 0; c < str.Length; c++)
        {
            if (controller(str[c]))
            {
                yield return str.Substring(nextPiece, c - nextPiece);
                nextPiece = c + 1;
            }
        }

        yield return str.Substring(nextPiece);
    }
    
    /// <summary>Given a string, returns the same string with any single matching pair of characters
    /// from the start and end of the string removed.</summary>
    /// <param name="input">The <see cref="string"/> from which matching start and end characters
    /// will be removed.</param>
    /// <param name="quote">The character to remove from the input string.</param>
    /// <returns>The provided string with single matching start and end characters specified by
    /// <paramref name="quote"/> removed.</returns>
    public static string TrimMatchingQuotes(this string input, char quote)
    {
        if ((input.Length >= 2) && 
            (input[0] == quote) && (input[input.Length - 1] == quote))
            return input.Substring(1, input.Length - 2);

        return input;
    }
}