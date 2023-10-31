﻿using System;
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

namespace UserProfileService.Sync;

/// <summary>
///     Represents the main program class.
/// </summary>
public class Program
{
    private const string DefaultLoggerName = "UserProfileService.Saga.Sync";
    private static ILogger _logger;
    
    /// <summary>
    ///     This activity should only created once on a central place and
    ///     is used for logging reason.
    /// </summary>
    public static ActivitySource Source { set; get; } = new ActivitySource(
        "Maverick.UserProfileService.Sync",
        GetAssemblyVersion());

    internal static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    }

    internal static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder host = UseProfileServiceHostBuilder.CreateDefaultBuilder<SyncStartup>(args);

        return host;
    }

    /// <summary>
    ///     The entry point method for the service.
    /// </summary>
    /// <param name="args">The arguments parameter from the console.</param>
    public static async Task Main(string[] args)
    {
        try
        {
            _logger = SetIntermediateLogger();
            IHostBuilder host = CreateHostBuilder(args);
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