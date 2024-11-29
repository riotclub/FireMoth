// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the GNU GPLv3 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console.Extensions;

using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
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
using RiotClub.FireMoth.Services.Tasks.Output;
using RiotClub.FireMoth.Services.Tasks.Output.Csv;
using RiotClub.FireMoth.Services.Repository;
using RiotClub.FireMoth.Services.Tasks;

/// <summary>Extensions to support service configuration.</summary>
public static class ServiceCollectionExtensions
{
    private const string DefaultFilePrefix = "FireMoth_";
    private const string DefaultFileExtension = "csv";
    private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";
    
    private static readonly FileSystem FileSystem = new FileSystem();
    
    /// <summary>Adds services required to perform directory scanning via the FireMoth API.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.
    /// </param>
    /// <param name="config">An <see cref="IConfiguration"/> containing program runtime
    /// configuration.</param>
    /// <returns>The configured <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddFireMothServices(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IFileSystem, FileSystem>();
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

        services.Configure<DuplicateFileHandlingOptions>(config.GetSection("CommandLine"));
        var serviceProvider = services.BuildServiceProvider();
        var duplicateOptions = serviceProvider
            .GetRequiredService<IOptions<DuplicateFileHandlingOptions>>().Value;
        switch (duplicateOptions.DuplicateFileHandlingMethod)
        {
            case DuplicateFileHandlingMethod.Delete:
                services.AddTransient<ITaskHandler, DuplicateFileDeleteHandler>();
                break;
            case DuplicateFileHandlingMethod.Move:
                services.AddTransient<ITaskHandler, DuplicateFileMoveHandler>();
                break;
            case DuplicateFileHandlingMethod.NoAction:
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "Unrecognized DuplicateFileHandlingMethod option " +
                        $"'{duplicateOptions.DuplicateFileHandlingMethod}'.");
        }
        
        services.AddTransient<ITaskHandler, CsvFileFingerprintWriter>();
        services.AddTransient<IFactory, Factory>();     // CSVHelper factory
        var outputOptions = serviceProvider.GetRequiredService<IOptions<ScanOutputOptions>>().Value;
        services.AddScoped(_ => new StreamWriter(GetOutputFileName(outputOptions.OutputFile)));

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
        if (!FileSystem.Directory.Exists(dbFullPath))
            FileSystem.Directory.CreateDirectory(dbFullPath);
        
        var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = FileSystem.Path.Join(dbFullPath, dbFileName)
        };
        
        return sqliteConnectionStringBuilder.ConnectionString;
    }

    private static string GetOutputFileName(string? outputFile)
    {
        // If no outputFile was provided, use default path and filename.
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            var outputFilePath = string.IsNullOrWhiteSpace(outputFile) 
                ? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) 
                : FileSystem.Path.GetDirectoryName(outputFile);
            return outputFilePath + Path.DirectorySeparatorChar + DefaultFilePrefix
                   + Program.ProgramStartDateTime.ToString(
                       DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)                   
                   + '.' + DefaultFileExtension;
        }

        return FileSystem.Path.GetFullPath(outputFile);
    }
}