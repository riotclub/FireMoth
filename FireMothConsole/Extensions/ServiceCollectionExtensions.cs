// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.DataAccess.InMemory;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.DataAnalysis;
using RiotClub.FireMoth.Services.Orchestration;
using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Output.Csv;
using RiotClub.FireMoth.Services.Repository;

/// <summary>
/// Extensions to support service configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required to perform directory scanning via the FireMoth API.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.</param>
    /// <param name="config">An <see cref="IConfiguration"/> containing program runtime configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddFireMothServices(
        this IServiceCollection services, IConfiguration config)
    {
        //var outputOption = config.GetSection("CommandLineOptions").DuplicatesOnly
        //    ? OutputDuplicateFileFingerprintsOption.Duplicates
        //    : OutputDuplicateFileFingerprintsOption.All;
        services.AddTransient<IDirectoryScanOrchestrator, DirectoryScanOrchestrator>();
        services.AddTransient<IFileScanOrchestrator, FileScanOrchestrator>();
        services.AddTransient<IFileHasher, SHA256FileHasher>();
        
#region UseInMemoryDataAccessLayer
        // services.AddTransient<IDataAccessLayer<IFileFingerprint>, MemoryDataAccessLayer>();
#endregion

#region UseSqliteDataAccessLayer
        services.AddDbContext<FireMothContext>();
        services.AddTransient<IDataAccessLayer<IFileFingerprint>, SqliteDataAccessLayer>();
#endregion

        services.AddTransient<IFileFingerprintRepository, FileFingerprintRepository>();
        services.AddTransient<IFileFingerprintWriter, CsvFileFingerprintWriter>();
            
        return services;
    }
}