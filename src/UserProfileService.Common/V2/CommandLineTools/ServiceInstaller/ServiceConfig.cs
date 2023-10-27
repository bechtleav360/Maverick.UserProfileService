namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     A service configuration object for working on the commandline.
/// </summary>
public class ServiceConfig
{
    /// <summary>
    ///     (optional) The account name the program should run under.
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    ///     (optional) Startup arguments of the program .
    /// </summary>
    public string Arguments { get; set; }

    /// <summary>
    ///     The full binary path of the service to register.
    /// </summary>
    public string BinaryPath { get; set; }

    /// <summary>
    ///     (optional) Display name for the service.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     (optional) Password for the account or group object.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     The name of the service.
    /// </summary>
    public string ServiceName { get; set; }

    /// <summary>
    ///     (optional) Startup type which ca be one of the following (boot, system, auto, demand, disabled, delayed-auto).
    ///     defaults to "auto".
    /// </summary>
    public string Start { get; set; } = StartType.Auto;
}
