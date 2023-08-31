// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiotClub.FireMoth.Services.DataAccess;
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
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.
    /// </param>
    /// <param name="config">An <see cref="IConfiguration"/> containing program runtime
    /// configuration.</param>
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
        // services.AddScoped<IDataAccessLayer<FileFingerprint>, MemoryDataAccessLayer>();
#endregion

#region UseSqliteDataAccessLayer
        var connectionString = GetSqliteConnectionString(config.GetRequiredSection("Sqlite"));
        services.AddDbContext<FireMothContext>(options =>
        {
            options.UseSqlite(connectionString);
        });
        
        services.AddTransient<IDataAccessLayer<FileFingerprint>, SqliteDataAccessLayer>();
#endregion

        services.AddTransient<IFileFingerprintRepository, FileFingerprintRepository>();
        services.AddTransient<IFileFingerprintWriter, CsvFileFingerprintWriter>();

        return services;
    }

    private static string GetSqliteConnectionString(IConfiguration sqliteConfigSection)
    {
        // Get required values from configuration
        var dbDirectory = sqliteConfigSection.GetRequiredSection("DbDirectory").Value;
        var dbFileName = sqliteConfigSection.GetRequiredSection("DbFileName").Value;
        
        // If necessary, do %APPDATA% replacement in directory retrieved from config
        if (dbDirectory!.Contains("%APPDATA%"))
        {
            const Environment.SpecialFolder appDataFolder = 
                Environment.SpecialFolder.LocalApplicationData;
            dbDirectory =
                dbDirectory.Replace("%APPDATA%", Environment.GetFolderPath(appDataFolder));
        }
        
        // Verify directory exists, create if necessary
        if (!Directory.Exists(dbDirectory))
            Directory.CreateDirectory(dbDirectory);
        
        // Build and return Sqlite connection string
        var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Join(dbDirectory, dbFileName)
        };

        return sqliteConnectionStringBuilder.ConnectionString;
    }
}