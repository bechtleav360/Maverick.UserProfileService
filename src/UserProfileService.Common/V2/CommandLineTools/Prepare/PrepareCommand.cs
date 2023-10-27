using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.CommandLineTools.Cleanup;

namespace UserProfileService.Common.V2.CommandLineTools.Prepare;

/// <summary>
///     The <see cref="CommandBase" /> implementation of the <c>cleanup</c> command.
/// </summary>
[Command(
    Name = "prepare",
    Description = "Prepare data state of 3rd party sources like database or event storage.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[HelpOption]
public class PrepareCommand : CommandBase
{
    private readonly List<IDatabasePreparationService> _PreparationServices;

    /// <summary>
    ///     The target type (like Arango, EventStore).
    /// </summary>
    [Option("-t|--target", "The name of the target storage.", CommandOptionType.MultipleValue)]
    public PreparationTargetType[] TargetStorage { get; set; }

    /// <summary>
    ///     Initializes a new instance of <see cref="CleanupCommand" />.
    /// </summary>
    /// <param name="output">The <see cref="IOutput" /> instance that takes logging messages.</param>
    /// <param name="cleanupServices">The sequence of <see cref="IStorageCleanupService" /> to be used.</param>
    public PrepareCommand(
        IOutput output,
        IEnumerable<IDatabasePreparationService> cleanupServices) : base(output)
    {
        _PreparationServices = cleanupServices?.ToList()
            ?? throw new ArgumentException("No preparation service was registered.");
    }

    /// <inheritdoc cref="CommandBase" />
    protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        if (TargetStorage == null || TargetStorage.Length == 0)
        {
            app.ShowHelp();

            return 0;
        }

        if (_PreparationServices.Count == 0)
        {
            Output.WriteErrorLine("No preparation services found. Maybe configuration is missing.");

            return -1;
        }

        try
        {
            List<IDatabasePreparationService> found =
                _PreparationServices.Where(
                        service => TargetStorage.Any(target => service.RelevantFor == target.ToString("G")))
                    .ToList();

            if (found.Count == 0)
            {
                Output.WriteErrorLine(
                    $"The specified target types '{string.Join("','", TargetStorage)}' are not supported by this application.");

                return -1;
            }

            foreach (IDatabasePreparationService preparationService in found)
            {
                Output.WriteLine($"Preparing data state in target '{preparationService.GetType().Name}'.");
                await preparationService.PrepareAsync();
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
