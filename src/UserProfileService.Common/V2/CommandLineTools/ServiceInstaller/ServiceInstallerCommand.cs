using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace UserProfileService.Common.V2.CommandLineTools.ServiceInstaller;

/// <summary>
///     Object for installation commands.
/// </summary>
[Command(
    Name = "svc",
    Description = "Windows service installer.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[Subcommand(
    typeof(ServiceInstallerInstallCommand),
    typeof(ServiceInstallerUninstallCommand),
    typeof(ServiceInstallerStartCommand),
    typeof(ServiceInstallerStopCommand),
    typeof(ServiceInstallerRestartCommand))]
[HelpOption]
public class ServiceInstallerCommand : CommandBase
{
    /// <summary>
    ///     The constructor for the installation commands.
    /// </summary>
    /// <param name="output">Component that can output data to the configured console.</param>
    public ServiceInstallerCommand(IOutput output) : base(output)
    {
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        app.ShowHelp();

        return Task.FromResult(0);
    }
}
