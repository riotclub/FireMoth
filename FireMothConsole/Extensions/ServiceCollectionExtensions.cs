﻿// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Extensions;

using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
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
        // Build service provider here so we can resolve command line parse result
        var provider = services.BuildServiceProvider();
        var parseResult = provider.GetRequiredService<ParseResult>();
        var commandLineOptions = parseResult.CommandResult.Children;
        
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

        //services.Configure<DuplicateFileHandlingOptions>(config.GetSe)
        var duplicateHandlingMethod = config.GetValue<DuplicateFileHandlingMethod>("DuplicatesAction");
        if (duplicateHandlingMethod
            is DuplicateFileHandlingMethod.Delete or DuplicateFileHandlingMethod.Move)
        {
            services.AddTransient<ITaskHandler, DuplicateFileHandler>();
            services.AddTransient<ITaskHandler, NullHandler>();
        }
        
        services.AddTransient<IFileFingerprintRepository, FileFingerprintRepository>();
        services.AddTransient<IFileFingerprintWriter, CsvFileFingerprintWriter>();

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