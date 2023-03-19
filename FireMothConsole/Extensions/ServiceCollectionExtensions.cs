namespace RiotClub.FireMoth.Console
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RiotClub.FireMoth.Services.DataAccess;
    using RiotClub.FireMoth.Services.DataAccess.Csv;
    using RiotClub.FireMoth.Services.DataAnalysis;
    using RiotClub.FireMoth.Services.FileScanning;
    using RiotClub.FireMoth.Services.Output;
    using System;
    using System.Globalization;
    using System.IO;

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
            services.AddSingleton<FileScanner>();
            services.AddTransient<IFileHasher, SHA256FileHasher>();
            services.AddTransient<IDataAccessLayer, CsvDataAccessLayer>();
            services.AddTransient(provider =>
                new StreamWriter(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    + Path.DirectorySeparatorChar + DefaultFilePrefix
                    + DateTime.Now.ToString(
                        DefaultFileDateTimeFormat, CultureInfo.InvariantCulture)
                    + '.' + DefaultFileExtension));
            services.AddTransient<IScanResultStreamWriter, CsvScanResultWriter>();
            
            return services;
        }
    }
}
