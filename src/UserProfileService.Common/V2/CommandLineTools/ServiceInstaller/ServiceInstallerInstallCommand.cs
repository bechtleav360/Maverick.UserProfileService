using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     Object for install the application as a service.
/// </summary>
[Command(
    Name = "install",
    Description = "Installs the application as a service.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption]
public class ServiceInstallerInstallCommand : CommandBase
{
    private readonly IServiceManager _ServiceManager;

    [Option("-ac|--account", Description = "The account name who runs the service.")]
    public string Account { get; set; }

    [Option("-a|--arguments", Description = "The arguments of the service.")]
    public string Arguments { get; set; }

    [Option("-b|--binary-path", Description = "The binary of the service.")]
    public string BinaryPath { get; set; }

    [Option("-dn|--display-name", Description = "The display name of the service.")]
    public string DisplayName { get; set; }

    [Option("-p|--password", Description = "The password for the account or object.")]
    public string Password { get; set; }

    [Option("-sn|--service-name", Description = "The name of the service.")]
    public string ServiceName { get; set; }

    [Option("-s|--start", Description = "The startup type (boot|system|auto|demand|disabled|delayed-auto).")]
    public string Start { get; set; }

    /// <summary>
    ///     The constructor for the class <see cref="ServiceInstallerInstallCommand" />.
    /// </summary>
    /// <param name="output">Component that can output data to the configured console.</param>
    /// <param name="serviceManager">
    ///     Simple service management interface to install/uninstall/start/stop a service manages by
    ///     some system.
    /// </param>
    public ServiceInstallerInstallCommand(
        IOutput output,
        IServiceManager serviceManager) : base(output)
    {
        _ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        var config = new ServiceConfig
        {
            Arguments = Arguments,
            Account = Account,
            Password = Password,
            BinaryPath = BinaryPath,
            DisplayName = DisplayName,
            ServiceName = ServiceName,
            Start = StartType.Contains(Start) ? Start : StartType.Auto
        };

        try
        {
            _ServiceManager.InstallService(config);

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Output.WriteErrorLine(ex.Message);

            return Task.FromResult(-1);
        }
    }
}
