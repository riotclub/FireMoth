// <copyright file="IAnalyzer.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis;

using System.IO;

/// <summary>Defines the public interface for a class that implements data analysis, hashing, and/or
/// fingerprinting for binary data.</summary>
public interface IAnalyzer<in T>
{
    /// <summary>Analyzes data from the provided stream.</summary>
    /// <param name="inputStream">The <see cref="Stream"/> containing the data to analyze.</param>
    /// <returns>An <see cref="AnalysisResult{IAnalyzer}"/> containing the result of the analysis.
    /// </returns>
    public AnalysisResult<T> AnalyzeFromStream(Stream inputStream);
}