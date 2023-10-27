using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using UserProfileService.Common.Logging.Contract;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UserProfileService.Common.Logging.Extensions;

/// <summary>
///     Extension Method to register Maverick logging using NLog
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    ///     Configures the <see cref="ILoggingBuilder" /> to setup Maverick logging
    ///     and sets up the NLog <see cref="ConfigSettingLayoutRenderer" />
    ///     to allow usage of the NLog ${configsetting} layout renderer
    /// </summary>
    /// <param name="loggingBuilder">The <see cref="ILoggingBuilder" /></param>
    /// <param name="configuration">The <see cref="IConfiguration" /> instance containing the log configuration</param>
    /// <param name="sectionName">
    ///     The section name in the <see cref="Microsoft.Extensions.Configuration" /> containing the logging configuration.
    ///     (default: Logging)
    /// </param>
    /// <returns>Initializes the maverick logging pipeline</returns>
    public static ILoggingBuilder UseSpecificLogging(
        this ILoggingBuilder loggingBuilder,
        IConfiguration configuration,
        string sectionName = "Logging")
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConfiguration(configuration.GetSection(sectionName));
        loggingBuilder.SetMinimumLevel(LogLevel.Information);

        var loggingOptions = new LoggingOptions();
        configuration.Bind(sectionName, loggingOptions);

        ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;

        LogManager.Configuration ??= NlogConstants.GetLoggingConfiguration(loggingOptions);

        loggingBuilder.AddNLog(
            new NLogProviderOptions
            {
                RemoveLoggerFactoryFilter = false
            });

        return loggingBuilder;
    }
}
