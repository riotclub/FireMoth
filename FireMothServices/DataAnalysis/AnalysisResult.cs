// <copyright file="AnalysisResult.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.DataAnalysis;

using System;

public class AnalysisResult<T>
{
    public AnalysisResult(byte[] analysisData)
    {
        AnalysisData = analysisData;
    }
    
    public byte[] AnalysisData { get; }

    public Type AnalyzerType => typeof(T);
}