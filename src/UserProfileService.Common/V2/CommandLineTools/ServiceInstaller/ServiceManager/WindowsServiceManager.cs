using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

/// <summary>
///     An implementation for the <see cref="IServiceManager" /> to start/stop/restart the
///     service as a windows service.
/// </summary>
public class WindowsServiceManager : IServiceManager
{
    private readonly ServiceConfig _DefaultServiceConfig;
    private readonly ILogger _Logger;

    /// <summary>
    ///     The constructor for the class <see cref="WindowsServiceManager" />.
    /// </summary>
    /// <param name="logger">The logger for logging purposes. The logger will accept logging messages from this instance.</param>
    /// <param name="serviceConfig">A service configuration object for working on the commandline.</param>
    public WindowsServiceManager(IOutput logger, ServiceConfig serviceConfig)
    {
        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _DefaultServiceConfig = serviceConfig ?? throw new ArgumentNullException(nameof(serviceConfig));
    }

    private string GetServiceBinPath(string binaryPath, string arguments)
    {
        _Logger.LogTraceMessage("Building service bin path", LogHelpers.Arguments());
        string programPath;

        if (string.IsNullOrWhiteSpace(binaryPath))
        {
            binaryPath = GetAssemblyPath();

            _Logger.LogTraceMessage(
                "No binaryPath provided, using path from the calling assembly: {path}",
                LogHelpers.Arguments(binaryPath));
        }

        _Logger.LogTraceMessage("Determine if the calling assembly is a dll.", LogHelpers.Arguments());

        if (IsDll(binaryPath))
        {
            _Logger.LogTraceMessage("Check if an .exe wrapper exists", LogHelpers.Arguments());
            string exePath = GetExeFilePathFromDll(binaryPath);

            if (HasExe(exePath))
            {
                _Logger.LogTraceMessage("An .exe wrapper was found", LogHelpers.Arguments());
                programPath = exePath;
            }
            else
            {
                programPath = "dotnet";
                arguments = $"\\\"{binaryPath}\\\" {arguments}";

                _Logger.LogTraceMessage(
                    "Use startup via dotnet with arguments >> {Arguments} <<",
                    LogHelpers.Arguments(arguments));
            }
        }
        else
        {
            _Logger.LogTraceMessage(
                "Validate if the provided program is a fully qualified path.",
                LogHelpers.Arguments());

            if (Path.IsPathFullyQualified(binaryPath))
            {
                programPath = binaryPath;
            }
            else
            {
                string assemblyDirectoryPath = Path.GetDirectoryName(GetAssemblyPath());
                programPath = Path.Combine(assemblyDirectoryPath, binaryPath);

                _Logger.LogTraceMessage(
                    "Provided path was not fully qualified. Automatically expanded to {ProgramPath}",
                    LogHelpers.Arguments(programPath));
            }
        }

        return BuildServiceManagerBinPath(programPath, arguments);
    }

    private string BuildServiceManagerBinPath(string program, string arguments)
    {
        var binPath = $"\\\"{program}\\\"";

        if (string.IsNullOrWhiteSpace(arguments) == false)
        {
            binPath += $" {arguments}";
        }

        return binPath;
    }

    private string GetAssemblyPath()
    {
        return Assembly.GetEntryAssembly()?.Location;
    }

    private string GetExeFilePathFromDll(string dllAssemblyPath)
    {
        return dllAssemblyPath.Replace(".dll", ".exe");
    }

    private bool IsDll(string assemblyPath)
    {
        return assemblyPath.EndsWith("dll");
    }

    private bool HasExe(string exePath)
    {
        return File.Exists(exePath);
    }

    private int RunServiceControlManager(string arguments = "")
    {
        _Logger.LogInfoMessage("Executing windows service control manager:", LogHelpers.Arguments());

        _Logger.LogTraceMessage("SCM arguments: {Arguments}", LogHelpers.Arguments(arguments));

        // Start the child process.
        var process = new Process
        {
            StartInfo =
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "sc",
                Arguments = arguments,
                CreateNoWindow = true
            }
        };

        // Redirect the output stream of the child process.
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.EnableRaisingEvents = true;

        process.OutputDataReceived += ProcessOnOutputDataReceived;
        process.ErrorDataReceived += ProcessOnErrorDataReceived;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();

        _Logger.LogInfoMessage(
            "Executed windows service control manager at {RunTime:o} and exit code {ExitCode}",
            LogHelpers.Arguments(process.ExitTime, process.ExitCode));

        return process.ExitCode;
    }

    private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
        {
            _Logger.LogInfoMessage("{e.Data}", LogHelpers.Arguments(e.Data));
        }
    }

    private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e != null && string.IsNullOrWhiteSpace(e.Data) == false)
        {
            _Logger.LogError(e.Data);
        }
    }

    private ServiceConfig MergeConfig(ServiceConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.BinaryPath))
        {
            config.BinaryPath = _DefaultServiceConfig.BinaryPath;
        }

        if (string.IsNullOrWhiteSpace(config.Account))
        {
            config.Account = _DefaultServiceConfig.Account;
        }

        if (string.IsNullOrWhiteSpace(config.Arguments))
        {
            config.Arguments = _DefaultServiceConfig.Arguments;
        }

        if (string.IsNullOrWhiteSpace(config.Password))
        {
            config.Password = _DefaultServiceConfig.Password;
        }

        if (string.IsNullOrWhiteSpace(config.Start))
        {
            config.Start = _DefaultServiceConfig.Start;
        }

        if (string.IsNullOrWhiteSpace(config.DisplayName))
        {
            config.DisplayName = _DefaultServiceConfig.DisplayName;
        }

        if (string.IsNullOrWhiteSpace(config.ServiceName))
        {
            config.ServiceName = _DefaultServiceConfig.ServiceName;
        }

        return config;
    }

    /// <inheritdoc />
    public void InstallService(ServiceConfig configuration)
    {
        ServiceConfig mergedConfig = MergeConfig(configuration);

        if (string.IsNullOrWhiteSpace(mergedConfig.ServiceName))
        {
            mergedConfig.ServiceName = Path.GetFileName(GetAssemblyPath());
        }

        string binPath = GetServiceBinPath(mergedConfig.BinaryPath, mergedConfig.Arguments);
        var scArguments = $"create \"{mergedConfig.ServiceName}\" binPath= \"{binPath}\"";

        if (string.IsNullOrWhiteSpace(mergedConfig.DisplayName) == false)
        {
            scArguments += $" DisplayName= \"{mergedConfig.DisplayName}\"";
        }

        if (string.IsNullOrWhiteSpace(mergedConfig.Account) == false)
        {
            scArguments += $" obj= \"{mergedConfig.Account}\"";
        }

        if (string.IsNullOrWhiteSpace(mergedConfig.Password) == false)
        {
            scArguments += $" password= \"{mergedConfig.Password}\"";
        }

        string start = StartType.Contains(configuration.Start)
            ? configuration.Start
            : StartType.Auto;

        scArguments += $" start= {start}";

        _Logger.LogInfoMessage("Installing service with the following parameters:", LogHelpers.Arguments());
        _Logger.LogInfoMessage("Service Name: {ServiceName}", LogHelpers.Arguments(mergedConfig.ServiceName));
        _Logger.LogInfoMessage("Display Name: {DisplayName}", LogHelpers.Arguments(mergedConfig.DisplayName));
        _Logger.LogInfoMessage("Application: {ApplicationPath}", LogHelpers.Arguments(binPath));
        _Logger.LogInfoMessage("Arguments: {Arguments}", LogHelpers.Arguments(mergedConfig.Arguments));
        _Logger.LogInfoMessage("Account: {Account}", LogHelpers.Arguments(mergedConfig.Account));

        if (string.IsNullOrWhiteSpace(mergedConfig.Account) == false)
        {
            _Logger.LogWarnMessage(
                "Make sure to set the permission \"LogonAsAService\" for the user/group",
                LogHelpers.Arguments());
        }

        _Logger.LogInfoMessage(
            "Password: {Password}",
            LogHelpers.Arguments(mergedConfig.Password?.Length > 0 ? "****" : ""));

        int exitCode = RunServiceControlManager(scArguments);

        if (exitCode != 0)
        {
            _Logger.LogErrorMessage(
                null,
                "Install service failed with exit code: {ExitCode}",
                LogHelpers.Arguments(exitCode));
        }
        else
        {
            _Logger.LogInfoMessage("Service install completed", LogHelpers.Arguments());
        }
    }

    /// <inheritdoc />
    public void UninstallService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = _DefaultServiceConfig.ServiceName;
        }

        int exitCode = RunServiceControlManager($"delete \"{serviceName}\"");

        if (exitCode != 0)
        {
            _Logger.LogErrorMessage(
                null,
                "Uninstall service failed with exit code: {ExitCode}",
                LogHelpers.Arguments(exitCode));
        }
        else
        {
            _Logger.LogInfoMessage("Service uninstall completed", LogHelpers.Arguments());
        }
    }

    /// <inheritdoc />
    public void StartService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = _DefaultServiceConfig.ServiceName;
        }

        int exitCode = RunServiceControlManager($"start \"{serviceName}\"");

        if (exitCode != 0)
        {
            _Logger.LogErrorMessage(
                null,
                "Start service failed with exit code: {ExitCode}",
                LogHelpers.Arguments(exitCode));
        }
        else
        {
            _Logger.LogInfoMessage("Service start completed", LogHelpers.Arguments());
        }
    }

    /// <inheritdoc />
    public void StopService(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = _DefaultServiceConfig.ServiceName;
        }

        int exitCode = RunServiceControlManager($"stop \"{serviceName}\"");

        if (exitCode != 0)
        {
            _Logger.LogErrorMessage(
                null,
                "Stop service failed with exit code: {ExitCode}",
                LogHelpers.Arguments(exitCode));
        }
        else
        {
            _Logger.LogInfoMessage("Service stop completed", LogHelpers.Arguments());
        }
    }

    /// <inheritdoc />
    public void RestartService(string serviceName)
    {
        StopService(serviceName);
        StartService(serviceName);
    }
}
