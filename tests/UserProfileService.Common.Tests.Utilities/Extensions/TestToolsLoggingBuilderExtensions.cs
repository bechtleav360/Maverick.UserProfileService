using System;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Tests.Utilities.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class TestToolsLoggingBuilderExtensions
    {
        /// <summary>
        ///     Adds a log provider to the log builder pipeline that will do a simple log message check.<br />
        ///     All log messages should be generated without exception and must not be empty or just whitespace strings.
        /// </summary>
        public static ILoggingBuilder AddSimpleLogMessageCheckLogger(
            this ILoggingBuilder builder,
            bool throwOnError = false)
        {
            builder.Services.AddSingleton<ILoggerProvider>(_ => new SimpleLogMessageCheckLoggerProvider(throwOnError));

            return builder;
        }

        /// <summary>
        ///     Adds a log provider to the log builder pipeline that will do a simple log message check.<br />
        ///     All log messages should be generated without exception and must not be empty or just whitespace strings.<br />
        ///     The exception handling will be called if an exception is passed to the logger and the log level is at least
        ///     warning.
        /// </summary>
        public static ILoggingBuilder AddSimpleLogMessageCheckLogger(
            this ILoggingBuilder builder,
            Action<Exception, LogLevel> exceptionHandling,
            bool throwOnError = false)
        {
            builder.Services.AddSingleton<ILoggerProvider>(
                _ => new SimpleLogMessageCheckLoggerProvider(exceptionHandling, throwOnError));

            return builder;
        }
    }
}
