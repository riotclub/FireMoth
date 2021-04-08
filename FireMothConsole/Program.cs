// <copyright file="Program.cs" company="Dark Hours Development">
// Copyright (c) Dark Hours Development. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace RiotClub.FireMoth.Console
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Application entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Class and application entry point. Validates command-line arguments, performs startup
        /// configuration, and invokes the directory scanning process.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>An <c>int</c> return code indicating invocation result.</returns>
        /// <seealso cref="CommandLineOptions"/>
        public static async Task Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                await host.StartAsync();

                try
                {
                    var initializer = host.Services.GetRequiredService<Initializer>();
                    initializer.Start();
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine("ERROR: " + exception.Message);
                }

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                lifetime.StopApplication();
                await host.WaitForShutdownAsync();
            }
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">Arguments to supply to the host builder.</param>
        /// <returns>The configured <see cref="IHostBuilder"/>.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Information))
                .UseConsoleLifetime()
                .ConfigureHostConfiguration(configuration =>
                {
                    // Perform any configuration needed when building the host here.
                    // ConfigureDefaultBuilder sets content root to GetCurrentDirectory(); loads
                    // host config from DOTNET_* env vars and cmd line args; loads app config from
                    // appsettings.json, appsettings.{env}.json, env vars, and cmd-line args;
                    // and adds logging providers for Console, Debug, EventSource, and EventLog.
                })
                .ConfigureAppConfiguration((hostContext, configuration) =>
                {
                    // Perform any app configuration here (after the host is built).
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services and add them to the IoC container here.
                    services.Configure<CommandLineOptions>(hostContext.Configuration);
                    services.AddTransient<Initializer>();
                    services.AddSingleton (Console.Out);
                });
    }
}