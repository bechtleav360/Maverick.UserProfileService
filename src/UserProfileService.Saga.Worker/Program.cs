using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Hosting;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UserProfileService.Saga.Worker;

/// <summary>
///     Represents the main program class.
/// </summary>
public class Program
{
    private static ILogger _logger;
    private const string DefaultLoggerName = "UserProfileService.Saga.Worker";

    /// <summary>
    ///     This activity should only created once on a central place and
    ///     is used for logging reason.
    /// </summary>
    private static ActivitySource Source { get; } = new ActivitySource(
        "Maverick.UserProfileService.Saga.Worker",
        GetAssemblyVersion());

    private static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }

    private static IHostBuilder CreateWebHost(string[] args)
    {
        return UseProfileServiceHostBuilder.CreateDefaultBuilder<SagaWorkerStartUp>(args);
    }

    /// <summary>
    ///     The entry point method for the service.
    /// </summary>
    /// <param name="args">The arguments parameter from the console.</param>
    public static async Task Main(string[] args)
    {
        _logger = SetIntermediateLogger();

        try
        {
            IHostBuilder host = CreateWebHost(args);
            await host.Build().RunAsync();
        }
        catch (Exception ex)
        {
            if (_logger == null)
            {
                LogManager.GetLogger(DefaultLoggerName).Fatal(ex);
            }
            else
            {
                _logger.LogErrorMessage(
                    ex,
                    "Stopped program because of an exception!",
                    LogHelpers.Arguments());
            }
        }
        // Shutdown the log manager
        finally
        {
            LogManager.Shutdown();
        }
    }

    private static ILogger SetIntermediateLogger()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.ClearProviders()
                .SetMinimumLevel(LogLevel.Trace)
                .AddDebug()
                .AddConsole());

        ILogger logger = loggerFactory.CreateLogger("MainIntermediate");

        return logger;
    }
}
