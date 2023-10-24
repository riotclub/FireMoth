namespace FireMoth.ConsoleSandbox;

using System.CommandLine;
using System.CommandLine.Binding;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Define commands and command line arguments
        var fileOption = new Option<FileInfo?>(
            name: "--file",
            description: "An option whose argument is parsed as a FileInfo");

        var rootCommand = new RootCommand("Dependency Injection sample");
        rootCommand.Add(fileOption);

        
        rootCommand.SetHandler(
            async (fileOptionValue, logger) =>
            {
                await DoRootCommand(fileOptionValue!, logger);
            },
            fileOption, new MyCustomBinder());

        await rootCommand.InvokeAsync("--file scl.runtimeconfig.json");
    }

    public static async Task DoRootCommand(FileInfo aFile, ILogger logger)
    {
        Console.WriteLine($"File = {aFile?.FullName}");
        logger.LogCritical("Test message");
        await Task.Delay(1000);
    }

    public class MyCustomBinder : BinderBase<ILogger>
    {
        protected override ILogger GetBoundValue(
            BindingContext bindingContext) => GetLogger(bindingContext);

        ILogger GetLogger(BindingContext bindingContext)
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(
                builder => builder.AddConsole());
            ILogger logger = loggerFactory.CreateLogger("LoggerCategory");
            return logger;
        }
    }
}

