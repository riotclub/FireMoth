// <copyright file="Program.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static class Program
    {
        private static IServiceProvider serviceProvider;

        /// <summary>
        /// Class and application entry point. Validates command-line arguments, performs startup
        /// configuration, and invokes the directory scanning process.
        /// </summary>
        /// <param name="args">Command-line arguments. A single argument, --directory, is currently
        /// supported and required. This value must be a well-formed and existing directory path.
        /// </param>
        /// <returns>An <c>int</c> return code indicating invocation result.</returns>
        public static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            await Host.CreateDefaultBuilder(args)
                //.ConfigureLogging((context) => context.AddEventLog());
                .UseConsoleLifetime()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Initializer>();
                })
                .RunConsoleAsync();

            IServiceScope scope = serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<Initializer>().Initialize();

            /*
            var initializer = new Initializer(args, Console.Out);
            bool initResult = initializer.Initialize();

            if (initResult)
            {
                ExitState exitState = initializer.Start();
                Console.WriteLine("Process completed with exit state: {0}.", exitState);
            }
            */

            await host.RunAsync();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">Arguments to supply to the host builder.</param>
        /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Warning))
                .ConfigureHostConfiguration(builder =>
                {
                    // Perform any configuration needed when building the host here.
                    // ConfigureDefaultBuilder sets content root to GetCurrentDirectory(); loads
                    // host config from DOTNET_* env vars and cmd line args; loads app config from
                    // appsettings.json, appsettings.{env}.json, env vars, and cmd-line args;
                    // and adds logging providers for Console, Debug, EventSource, and EventLog.
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    // Perform any app configuration here (after the host is built).
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services and add them to the IoC container here.
                    // services.AddHostedService<Initializer>().AddScoped<IMessageWriter, MessageWriter>());
                });
    }
}