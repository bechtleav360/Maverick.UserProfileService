using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Services;

/// <summary>
///     Executes action during startup of a service/worker.
/// </summary>
public class InitializationService : BackgroundService
{
    private readonly List<IDbInitializer> _DbInitializer;
    private readonly ILogger<InitializationService> _Logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="InitializationService" /> with a specified logger and an instance of
    ///     <see cref="IDbInitializer" />.
    /// </summary>
    /// <param name="logger">The logger that will be used for writing log messages of this instance.</param>
    /// <param name="dbInitializer">The initializes of database user by this service.</param>
    public InitializationService(
        ILogger<InitializationService> logger,
        IEnumerable<IDbInitializer> dbInitializer)
    {
        _Logger = logger;
        _DbInitializer = dbInitializer?.ToList() ?? new List<IDbInitializer>();
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _Logger.EnterMethod();

        if (_DbInitializer.Count == 0)
        {
            _Logger.LogInfoMessage("No database initializer registered.", LogHelpers.Arguments());
            _Logger.ExitMethod();

            return;
        }

        _Logger.LogDebugMessage(
            "Starting initialization of {database/s}.",
            LogHelpers.Arguments(_DbInitializer.Count == 1 ? "the database" : "databases"));

        foreach (IDbInitializer db in _DbInitializer)
        {
            _Logger.LogDebugMessage("Using initializer {dbName}.", LogHelpers.Arguments(db.GetType().Name));
            await db.EnsureDatabaseAsync(cancellationToken: stoppingToken);
        }

        _Logger.LogInfoMessage("Database initialized.", LogHelpers.Arguments());
        _Logger.ExitMethod();
    }
}
