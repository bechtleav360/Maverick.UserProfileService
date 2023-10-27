using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.CommandLineTools.Cleanup;

/// <summary>
///     The <see cref="CommandBase" /> implementation of the <c>cleanup</c> command.
/// </summary>
[Command(
    Name = "cleanup",
    Description = "Cleanup 3rd party sources like database or event storage.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption]
public class CleanupCommand : CommandBase
{
    private readonly List<IStorageCleanupService> _CleanupServices;

    [Option(
        "-s|--scope",
        "The scope of the cleanup operation (either 'all' or 'main' (classic, legacy) data or 'extended' (first-/second-level-projection) data).",
        CommandOptionType.SingleValue,
        ValueName = "all|main|extended")]
    public CleanupTargetScope Scope { get; set; }

    /// <summary>
    ///     The target type (like Arango, EventStore).
    /// </summary>
    [Option("-t|--target", "The name of the target storage.", CommandOptionType.MultipleValue)]
    public CleanupTargetType[] TargetStorage { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="CleanupCommand" />.
    /// </summary>
    /// <param name="output">The <see cref="IOutput" /> instance that takes logging messages.</param>
    /// <param name="cleanupServices">The sequence of <see cref="IStorageCleanupService" /> to be used.</param>
    public CleanupCommand(
        IOutput output,
        IEnumerable<IStorageCleanupService> cleanupServices) : base(output)
    {
        _CleanupServices = cleanupServices?.ToList()
            ?? throw new ArgumentException("No cleanup service was registered.");
    }

    /// <inheritdoc cref="CommandBase" />
    protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        if (TargetStorage == null || TargetStorage.Length == 0)
        {
            app.ShowHelp();

            return 0;
        }

        if (_CleanupServices.Count == 0)
        {
            Output.WriteErrorLine("No clean up services found. Maybe configuration is missing.");

            return -1;
        }

        try
        {
            List<IStorageCleanupService> found =
                _CleanupServices.Where(
                        service => TargetStorage.Any(target => service.RelevantFor == target.ToString("G")))
                    .ToList();

            if (found.Count == 0)
            {
                Output.WriteErrorLine(
                    $"The specified target types '{string.Join("','", TargetStorage)}' are not supported by this application.");

                return -1;
            }

            foreach (IStorageCleanupService cleanupService in found)
            {
                Output.WriteLine($"Doing cleanup for target '{cleanupService.GetType().Name}'.");

                switch (Scope)
                {
                    case CleanupTargetScope.All:
                        await cleanupService.CleanupAll();

                        break;
                    case CleanupTargetScope.Main:
                        await cleanupService.CleanupMainProjectionDataAsync();

                        break;
                    case CleanupTargetScope.Extended:
                        await cleanupService.CleanupExtendedProjectionDataAsync();

                        break;
                }
            }

            return 0;
        }
        catch (Exception e)
        {
            Output.WriteErrorLine(e.Message);

            return -1;
        }
    }
}
