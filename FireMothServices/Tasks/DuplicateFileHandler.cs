// <copyright file="DuplicateFileHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// 
/// </summary>
public class DuplicateFileHandler : ITaskHandler
{
    private IFileFingerprintRepository _fileFingerprintRepository;
    private ILogger<DuplicateFileHandler> _logger;
    
    public DuplicateFileHandler(
        IFileFingerprintRepository fileFingerprintRepository,
        DuplicateFileHandlingMethod duplicateFileHandlingMethod,
        ILogger<DuplicateFileHandler> logger)
    {
        Guard.IsNotNull(fileFingerprintRepository);
        Guard.IsNotNull(logger);
        _fileFingerprintRepository = fileFingerprintRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task<bool> RunTaskAsync()
    {
        _logger.LogDebug("Running task for DuplicateFileHandler...");
        return Task.FromResult(true);
    }
}