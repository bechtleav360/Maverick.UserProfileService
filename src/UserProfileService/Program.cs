using NLog;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Hosting;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UserProfileService;

/// <summary>
///     Represents the main program class.
/// </summary>
public class Program
{
    private static ILogger _logger;
    private const string DefaultLoggerName = "UserProfileService";

    internal static IWebHostBuilder CreateHostBuilder(string[] args)
    {
        // Why set a global timeout?
        // Regular expressions could be used by an attacker to launch a denial-of-service attack for a website
        // by consuming excessive resources. Setting a timeout allows the operation to stop at a configured timeout,
        // rather than running until completion, using resources the entire time.
        // Suggested by: SonarQube
        // https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.matchtimeout?view=net-6.0#remarks
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));

        var host = UseProfileServiceHostBuilder.CreateDefaultBuilder<UserProfileStartUp>(args);

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
            IWebHostBuilder host = CreateHostBuilder(args);
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
                _logger.LogErrorMessage(ex, "Stopped program because of an exception!", LogHelpers.Arguments());
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
            builder => builder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddDebug().AddConsole());

        ILogger logger = loggerFactory.CreateLogger("MainIntermediate");

        return logger;
    }
}