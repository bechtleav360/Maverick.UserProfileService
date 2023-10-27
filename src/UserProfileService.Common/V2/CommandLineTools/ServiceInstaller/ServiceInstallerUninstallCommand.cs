using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     Object to uninstall the application as a service.
/// </summary>
[Command(
    Name = "uninstall",
    Description = "Uninstalls the application as a service.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption]
public class ServiceInstallerUninstallCommand : CommandBase
{
    private readonly IServiceManager _ServiceManager;

    [Option("-sn|--service-name", Description = "The name of the service.")]
    public string ServiceName { get; set; }

    /// <summary>
    ///     The constructor for the class <see cref="ServiceInstallerUninstallCommand" />.
    /// </summary>
    /// <param name="output">Component that can output data to the configured console.</param>
    /// <param name="serviceManager">
    ///     Simple service management interface to install/uninstall/start/stop a service manages by
    ///     some system.
    /// </param>
    public ServiceInstallerUninstallCommand(
        IOutput output,
        IServiceManager serviceManager) : base(output)
    {
        _ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        try
        {
            _ServiceManager.UninstallService(ServiceName);

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Output.WriteErrorLine(ex.Message);

            return Task.FromResult(-1);
        }
    }
}
