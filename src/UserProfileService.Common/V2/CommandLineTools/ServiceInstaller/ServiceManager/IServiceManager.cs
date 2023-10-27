namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

/// <summary>
///     Simple service management interface to install/uninstall/start/stop a service manages by some system.
/// </summary>
public interface IServiceManager
{
    /// <summary>
    ///     Installs a service by a given service configuration.
    /// </summary>
    /// <param name="configuration"> A service configuration object for working on the commandline.</param>
    void InstallService(ServiceConfig configuration);

    /// <summary>
    ///     Installs a service by a given service name.
    /// </summary>
    /// <param name="serviceName">The service name to uninstall.</param>
    void UninstallService(string serviceName);

    /// <summary>
    ///     Starts a service by a given service name.
    /// </summary>
    /// <param name="serviceName">The service name to start.</param>
    void StartService(string serviceName);

    /// <summary>
    ///     Stops a service by a given service name.
    /// </summary>
    /// <param name="serviceName">The service name to stop.</param>
    void StopService(string serviceName);

    /// <summary>
    ///     Restarts a service by a given service name.
    /// </summary>
    /// <param name="serviceName">The service name to restart.</param>
    void RestartService(string serviceName);
}
