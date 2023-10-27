using System.Text;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace UserProfileService.Common.Logging.Contract;

/// <summary>
///     NLog defaults
/// </summary>
public static class NlogConstants
{
    /// <summary>
    ///     Provides the default json layout to use
    /// </summary>
    /// <returns>
    ///     Return the default json log format
    /// </returns>
    public static Layout GetDefaultJsonLayout()
    {
        return new JsonLayout
        {
            EscapeForwardSlash = false,
            Attributes =
            {
                new JsonAttribute("timestamp", "${date:format=o}"),
                new JsonAttribute("level", "${level}"),
                new JsonAttribute("message", "${message}"),
                new JsonAttribute("logger", "${logger}"),
                new JsonAttribute("exception", "${exception:format=@}", false),
                new JsonAttribute("messageTemplate", "${message:raw=true}"),
                new JsonAttribute(
                    "properties",
                    new JsonLayout
                    {
                        IncludeEventProperties = true,
                        EscapeForwardSlash = false
                    },
                    false),
                new JsonAttribute(
                    "activity",
                    new JsonLayout
                    {
                        EscapeForwardSlash = false,
                        Attributes =
                        {
                            new JsonAttribute("id", "${activity:property=Id}"),
                            new JsonAttribute("spanId", "${activity:property=SpanId}"),
                            new JsonAttribute("parentId", "${activity:property=ParentId}"),
                            new JsonAttribute("traceId", "${activity:property=TraceId}"),
                            new JsonAttribute("operationName", "${activity:property=OperationName}")
                        }
                    },
                    false)
            }
        };
    }

    /// <summary>
    ///     Provides the default text layout to use
    /// </summary>
    /// <returns>
    ///     Return the default text log format
    ///     "${longdate} | ${pad:padding=5:inner=${level:uppercase=true}} | ${logger} | ${message} |
    ///     ${activity:property=TraceId}
    ///     | ${onexception:inner= |
    ///     ${exception:format=message,stacktrace:separator=\r\n:innerFormat=message,stacktrace:maxInnerExceptionLevel=3}"
    /// </returns>
    public static Layout GetDefaultTextLayout()
    {
        const string logString = "${longdate}"
            + " | ${pad:padding=5:inner=${level:uppercase=true}}"
            + " | ${logger}"
            + " | ${message}"
            + " | ${activity:property=TraceId}"
            + "${onexception:inner= | ${exception:format=message,stacktrace:separator=\r\n:innerFormat=message,stacktrace:maxInnerExceptionLevel=3}";

        return Layout.FromString(logString, true);
    }

    /// <summary>
    ///     Returns a console target named "console" wrapped inside an <see cref="AsyncTargetWrapper" />
    /// </summary>
    /// <param name="layout">The layout to apply</param>
    /// <returns>A file target</returns>
    public static Target GetConsoleTarget(Layout layout)
    {
        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = layout,
            Encoding = Encoding.UTF8
        };

        return new AsyncTargetWrapper("async-console", consoleTarget);
    }

    /// <summary>
    ///     Returns a file target named "file" wrapped inside an <see cref="AsyncTargetWrapper" />
    /// </summary>
    /// <param name="logPath">The log path where to write the log files</param>
    /// <param name="maxArchiveFiles">The number of archive files to keep</param>
    /// <param name="layout">The layout to apply</param>
    /// <returns>A file target</returns>
    public static Target GetFileTarget(string logPath, int maxArchiveFiles, Layout layout)
    {
        var fileTarget = new FileTarget("file")
        {
            Layout = layout,
            FileName = logPath + "/${date:format=yyyy-MM-dd_HH}.log",
            ArchiveDateFormat = "yyyy-MM-dd_HH",
            ArchiveEvery = FileArchivePeriod.Hour,
            ArchiveNumbering = ArchiveNumberingMode.Date,
            ArchiveFileName = logPath + "/{#}.log",
            MaxArchiveFiles = maxArchiveFiles,
            DeleteOldFileOnStartup = false,
            ArchiveOldFileOnStartup = true,
            Encoding = Encoding.UTF8
        };

        return new AsyncTargetWrapper("async-file", fileTarget);
    }

    /// <summary>
    ///     Returns a logging configuration for NLog
    /// </summary>
    /// <param name="loggingOptions">Logging configuration</param>
    /// <returns>Returns the logging configuration</returns>
    public static LoggingConfiguration GetLoggingConfiguration(LoggingOptions loggingOptions)
    {
        LogManager.Setup().SetupExtensions(s => s.RegisterAssembly("NLog.DiagnosticSource"));

        var config = new LoggingConfiguration();

        Layout logLayout = loggingOptions.LogFormat == LogFormat.Json
            ? GetDefaultJsonLayout()
            : GetDefaultTextLayout();

        Target consoleTarget = GetConsoleTarget(logLayout);
        config.AddRuleForAllLevels(consoleTarget);

        if (loggingOptions.EnableLogFile)
        {
            string logPath = loggingOptions.LogFilePath;

            if (logPath.EndsWith("/"))
            {
                logPath = logPath.Remove(logPath.Length - 1);
            }

            Target fileTarget = GetFileTarget(
                logPath,
                loggingOptions.LogFileMaxHistory,
                logLayout);

            config.AddRuleForAllLevels(fileTarget);
        }

        return config;
    }
}
