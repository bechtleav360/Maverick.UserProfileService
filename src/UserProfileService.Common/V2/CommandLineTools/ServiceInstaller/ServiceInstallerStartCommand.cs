using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using UserProfileService.Common.V2.CommandLineTools.ServiceInstaller.ServiceManager;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     Object for starting the application via the commandline as a windows service.
///     Before staring the service it has to be registered as windows service.
/// </summary>
[Command(
    Name = "start",
    Description = "Starts the application service.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption]
public class ServiceInstallerStartCommand : CommandBase
{
    private readonly IServiceManager _ServiceManager;

    [Option("-sn|--service-name", Description = "The name of the service.")]
    public string ServiceName { get; set; }

    /// <summary>
    ///     The constructor for the class <inheritdoc cref="ServiceInstallerStartCommand" />.
    /// </summary>
    /// <param name="output"> Component that can output data to the configured console.</param>
    /// <param name="serviceManager">
    ///     Simple service management interface to install/uninstall/start/stop a service manages by
    ///     some system.
    /// </param>
    public ServiceInstallerStartCommand(
        IOutput output,
        IServiceManager serviceManager) : base(output)
    {
        _ServiceManager = serviceManager ?? throw new ArgumentNullException(nameof(serviceManager));
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        try
        {
            _ServiceManager.StartService(ServiceName);

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            Output.WriteErrorLine(ex.Message);

            return Task.FromResult(-1);
        }
    }
}
