using Marten;
using Marten.Services;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Adapter.Marten.EntityModels;
using UserProfileService.Adapter.Marten.Exceptions;
using UserProfileService.Adapter.Marten.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Marten.Implementations;

/// <summary>
///     Represents the default repository of volatile data projections that used Marten as backend
///     implementation.
/// </summary>
public class MartenVolatileDataProjectionRepository : ISecondLevelVolatileDataRepository, IDisposable
{
    private readonly IDocumentSession _documentSession;
    private readonly IVolatileDataStore _documentStore;
    private readonly ILogger<MartenVolatileDataProjectionRepository> _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="MartenVolatileDataProjectionRepository" />.
    /// </summary>
    /// <param name="documentStore">The Marten document session to be used to query and modify data.</param>
    /// <param name="logger">The logging instance that will take care of logging messages from this instance.</param>
    public MartenVolatileDataProjectionRepository(
        IVolatileDataStore documentStore,
        ILogger<MartenVolatileDataProjectionRepository> logger)
    {
        _documentSession = documentStore.LightweightSession();
        _documentStore = documentStore;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (transaction is not MartenAdapterDatabaseTransaction specificTransaction)
        {
            throw new NotSupportedException(
                $"The provided object of {nameof(IDatabaseTransaction)} (of type {transaction.GetType().Name}) is not allowed in this repository.");
        }

        await specificTransaction.CurrentTransaction.RollbackAsync(cancellationToken);

        _logger.LogDebugMessage("Transaction: Changes aborted.", LogHelpers.Arguments());

        await specificTransaction.DisposeAsync();

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDocumentSession documentSession = _documentStore.LightweightSession();

        if (documentSession.Connection == null)
        {
            throw new ConnectionNotAvailableException("Could not start transaction, because connection object is null");
        }

        return new MartenAdapterDatabaseTransaction(
            await documentSession.Connection.BeginTransactionAsync(cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default)
    {
        _logger.EnterMethod();

        IReadOnlyList<ProjectionStateLightDbModel> response = await _documentSession
            .Query<ProjectionStateLightDbModel>()
            .ToListAsync(stoppingToken);

        _logger.LogInfoMessage(
            "Found {numEntries} entries in Marten projection state table",
            LogHelpers.Arguments(response.Count));

        return _logger.ExitMethod(
            response.ToDictionary(
                entry => entry.StreamName,
                entry => entry.EventNumberVersion));
    }

    /// <inheritdoc />
    public async Task SaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IDocumentSession session;

        if (transaction is MartenAdapterDatabaseTransaction specificTransaction)
        {
            session = _documentStore.LightweightSession(
                SessionOptions.ForTransaction(specificTransaction.CurrentTransaction));
        }
        else
        {
            session = _documentSession;
        }

        var converted = new ProjectionStateLightDbModel
        {
            StreamName = projectionState.StreamName,
            EventName = projectionState.EventName,
            EventNumberSequence = (ulong)projectionState.EventNumberSequence,
            EventNumberVersion = (ulong)projectionState.EventNumberVersion,
            ProcessedOn = projectionState.ProcessedOn,
            ErrorMessage = projectionState.ErrorMessage
        };

        session.Store(converted);

        await session.SaveChangesAsync(cancellationToken);

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This method is not supported by this repository implementation.");
    }

    /// <inheritdoc />
    public async Task SaveUserIdAsync(
        string userId,
        IDatabaseTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        IDocumentSession session;

        if (transaction is MartenAdapterDatabaseTransaction specificTransaction)
        {
            session = _documentStore.LightweightSession(
                SessionOptions.ForTransaction(specificTransaction.CurrentTransaction));
        }
        else
        {
            session = _documentSession;
        }

        var newUser = new UserDbModel
        {
            Id = userId
        };

        session.Store(newUser);

        cancellationToken.ThrowIfCancellationRequested();

        await session.SaveChangesAsync(cancellationToken);

        _logger.LogInfoMessage("User {userId} saved.", userId.AsArgumentList());

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task TryDeleteUserAsync(
        string userId,
        IDatabaseTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        _logger.LogDebugMessage("User {userId} shall be deleted.", userId.AsArgumentList());

        IDocumentSession session;

        if (transaction is MartenAdapterDatabaseTransaction specificTransaction)
        {
            session = _documentStore.LightweightSession(
                SessionOptions.ForTransaction(specificTransaction.CurrentTransaction));
        }
        else
        {
            session = _documentSession;
        }

        bool userExists = await session
            .Query<UserDbModel>()
            .AnyAsync(
                u => u.Id == userId,
                cancellationToken);

        if (!userExists)
        {
            _logger.LogDebugMessage(
                "User with id {userId} not found. Skipping method.",
                userId.AsArgumentList());

            _logger.ExitMethod();

            return;
        }

        session.Delete<UserDbModel>(userId);

        cancellationToken.ThrowIfCancellationRequested();

        await session.SaveChangesAsync(cancellationToken);

        _logger.LogInfoMessage("User {userId} deleted.", userId.AsArgumentList());

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(
        IDatabaseTransaction? transaction,
        CancellationToken cancellationToken = default)
    {
        _logger.EnterMethod();

        if (transaction is not MartenAdapterDatabaseTransaction specificTransaction)
        {
            throw new NotSupportedException(
                $"The provided object of {nameof(IDatabaseTransaction)} (of type {transaction?.GetType().Name}) is not allowed in this repository.");
        }

        await specificTransaction.CurrentTransaction.CommitAsync(cancellationToken);

        _logger.LogDebugMessage("Transaction: Changes committed.", LogHelpers.Arguments());

        await specificTransaction.DisposeAsync();

        _logger.ExitMethod();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _documentSession.Connection?.Close();
        _documentSession.Dispose();
    }
}
