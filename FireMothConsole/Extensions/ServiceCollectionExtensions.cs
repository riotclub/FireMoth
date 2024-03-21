﻿// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Extensions;

using System;
using System.IO;
using CsvHelper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.DataAccess.Sqlite;
using RiotClub.FireMoth.Services.DataAnalysis;
using RiotClub.FireMoth.Services.Orchestration;
using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Output.Csv;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;

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

        services.Configure<DuplicateFileHandlingOptions>(config.GetSection("CommandLine"));
        var serviceProvider = services.BuildServiceProvider();
        var duplicateOptions = serviceProvider
            .GetRequiredService<IOptions<DuplicateFileHandlingOptions>>();
        if (duplicateOptions.Value.DuplicateFileHandlingMethod
            is DuplicateFileHandlingMethod.Delete or DuplicateFileHandlingMethod.Move)
        {
            services.AddTransient<ITaskHandler, DuplicateFileHandler>();
        }
        
        services.AddTransient<IFileFingerprintRepository, FileFingerprintRepository>();
        services.AddTransient<IFileFingerprintWriter, CsvFileFingerprintWriter>();
        services.AddTransient<IFactory, Factory>();     // CSVHelper factory

        var directoryScanOptions = 
            serviceProvider.GetRequiredService<IOptions<DirectoryScanOptions>>();
        // Null-forgiven; we check for null values in command-line argument validator in Program
        services.AddTransient(_ => new StreamWriter(directoryScanOptions.Value.Directory!));

        return services;
    }

    private static string GetSqliteConnectionString(IConfiguration sqliteConfigSection)
    {
        var useAppData = sqliteConfigSection.GetValue("UseAppDataDirectory", false);
        var dbDirectory = sqliteConfigSection.GetValue<string>("DbDirectory");
        var dbFileName = sqliteConfigSection.GetValue<string>("DbFileName");
        
        string dbFullPath;
        if (useAppData)
        {
            const Environment.SpecialFolder appDataFolder = 
                Environment.SpecialFolder.LocalApplicationData;
            dbFullPath = Environment.GetFolderPath(appDataFolder);
        }
        else
        {
            dbFullPath = Environment.CurrentDirectory;
        }

        dbFullPath = dbFullPath + Path.DirectorySeparatorChar + dbDirectory;
        if (!Directory.Exists(dbFullPath))
            Directory.CreateDirectory(dbFullPath);
        
        var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Join(dbFullPath, dbFileName)
        };
        
        return sqliteConnectionStringBuilder.ConnectionString;
    }
}