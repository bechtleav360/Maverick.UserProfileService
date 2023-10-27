using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Services;

internal class FirstLevelProjectionCronJobService : ICronJobService
{
    private readonly ITemporaryAssignmentsExecutor _EventExecutor;
    private readonly ILogger _Logger;

    public FirstLevelProjectionCronJobService(
        ILogger<FirstLevelProjectionCronJobService> logger,
        ITemporaryAssignmentsExecutor eventExecutor)
    {
        _Logger = logger;
        _EventExecutor = eventExecutor;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _Logger.EnterMethod();

        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        _Logger.LogDebugMessage(
            "Running work step 'TemporaryAssignments'.",
            LogHelpers.Arguments());

        await _EventExecutor.CheckTemporaryAssignmentsAsync(stoppingToken);

        _Logger.ExitMethod();
    }
}
