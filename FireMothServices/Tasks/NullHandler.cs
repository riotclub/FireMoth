// <copyright file="NullHandler.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Services.Tasks;

using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Configuration;

public class NullHandler : ITaskHandler
{
    private ILogger<NullHandler> _logger;
    
    public NullHandler(ILogger<NullHandler> logger)
    {
        Guard.IsNotNull(logger);
        _logger = logger;
    }
    
    public Task<bool> RunTaskAsync()
    {
        _logger.LogDebug("Running task for NullHandler...");
        return Task.FromResult(true);
    }
}