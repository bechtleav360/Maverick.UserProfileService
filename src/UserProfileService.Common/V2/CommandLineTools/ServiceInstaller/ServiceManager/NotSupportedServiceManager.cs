using System;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

/// <summary>
///     Is used for other operation platform system then windows.
/// </summary>
public class NotSupportedServiceManager : IServiceManager
{
    /// <inheritdoc />
    public void InstallService(ServiceConfig configuration)
    {
        throw new NotSupportedException("InstallService is not available for this service manager implementation");
    }

    /// <inheritdoc />
    public void UninstallService(string serviceName)
    {
        throw new NotSupportedException("UninstallService is not available for this service manager implementation");
    }

    /// <inheritdoc />
    public void StartService(string serviceName)
    {
        throw new NotSupportedException("StartService is not available for this service manager implementation");
    }

    /// <inheritdoc />
    public void StopService(string serviceName)
    {
        throw new NotSupportedException("StopService is not available for this service manager implementation");
    }

    /// <inheritdoc />
    public void RestartService(string serviceName)
    {
        throw new NotSupportedException("RestartService is not available for this service manager implementation");
    }
}
