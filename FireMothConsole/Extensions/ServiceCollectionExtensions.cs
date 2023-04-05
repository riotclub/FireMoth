// <copyright file="ServiceCollectionExtensions.cs" company="Riot Club">
// Copyright (c) Riot Club. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console;

using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RiotClub.FireMoth.Services.DataAccess;
using RiotClub.FireMoth.Services.DataAccess.Csv;
using RiotClub.FireMoth.Services.DataAnalysis;
using RiotClub.FireMoth.Services.Orchestration;
using RiotClub.FireMoth.Services.Output;
using RiotClub.FireMoth.Services.Repository;
using Services.Output.Csv;

public static class ServiceCollectionExtensions
{
    private const string DefaultFilePrefix = "FireMothData_";
    private const string DefaultFileExtension = "csv";
    private const string DefaultFileDateTimeFormat = "yyyyMMdd-HHmmss";

    public static IServiceCollection AddFireMothServices(
        this IServiceCollection services, IConfiguration config)
    {
        //var outputOption = config.GetSection("CommandLineOptions").DuplicatesOnly
        //    ? OutputDuplicateFileFingerprintsOption.Duplicates
        //    : OutputDuplicateFileFingerprintsOption.All;
        services.AddTransient<IDirectoryScanOrchestrator, DirectoryScanOrchestrator>();
        services.AddTransient<IFileScanOrchestrator, FileScanOrchestrator>();
        services.AddTransient<IFileHasher, SHA256FileHasher>();
        services.AddTransient<IDataAccessLayer<IFileFingerprint>, MemoryDataAccessLayer>();
        services.AddTransient<IFileFingerprintRepository, FileFingerprintRepository>();
        services.AddTransient(provider =>
            new StreamWriter(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + Path.DirectorySeparatorChar + DefaultFilePrefix
                + DateTime.Now.ToString(DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                + '.' + DefaultFileExtension));
        services.AddTransient<IFileFingerprintWriter, CsvFileFingerprintWriter>();
            
        return services;
    }
}