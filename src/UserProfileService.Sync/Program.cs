using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using NLog;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Health;
using UserProfileService.Common.Health.Consumers;
using UserProfileService.Common.Health.Implementations;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Hosting;
using UserProfileService.Messaging.DependencyInjection;
using UserProfileService.Redis;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Converter;
using UserProfileService.Sync.Factories;
using UserProfileService.Sync.Handlers;
using UserProfileService.Sync.Services;
using UserProfileService.Sync.States;
using UserProfileService.Sync.Utilities;
using UserProfileService.Sync.Validation;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace UserProfileService.Sync;

/// <summary>
///     Represents the main program class.
/// </summary>
public class Program
{
    private const string DefaultLoggerName = "UserProfileService.Saga.Sync";
    
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
            IHostBuilder host = CreateHostBuilder(args);
            await host.Build().RunAsync();
        }
        catch (Exception ex)
        {
            LogManager.GetLogger(DefaultLoggerName).Fatal(ex);
        }
        // Shutdown the log manager
        finally
        {
            LogManager.Shutdown();
        }
    }
}
