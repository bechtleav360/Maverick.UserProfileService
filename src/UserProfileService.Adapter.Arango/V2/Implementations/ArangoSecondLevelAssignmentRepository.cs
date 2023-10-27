using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

internal class ArangoSecondLevelAssignmentRepository : ArangoRepositoryBase, ISecondLevelAssignmentRepository
{
    /// <summary>
    ///     Holds the information which model should be stored in which database.
    /// </summary>
    private readonly ModelBuilderOptions _modelsInfo;

    /// <summary>
    ///     All operations of <see cref="IProjectionStateRepository" /> will be forwarded to this instance.
    /// </summary>
    private readonly ArangoProjectionStateRepository _projectionStateRepository;

    //TODO change
    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <inheritdoc />
    public ArangoSecondLevelAssignmentRepository(
        string arangodbClientName,
        string collectionPrefix,
        string queryPrefix,
        ILogger<ArangoSecondLevelAssignmentRepository> logger,
        IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        _modelsInfo = DefaultModelConstellation.NewAssignmentsProjectionRepository(collectionPrefix, queryPrefix)
            .ModelsInfo;

        ArangoDbClientName = arangodbClientName;

        _projectionStateRepository = new ArangoProjectionStateRepository(
            logger,
            serviceProvider,
            arangodbClientName,
            _modelsInfo.GetCollectionName<ProjectionState>());
    }

    private async Task<SecondLevelProjectionAssignmentsUser> GetAssignmentUserInternalAsync(
        string userId,
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        GetDocumentResponse<SecondLevelProjectionAssignmentsUser> user = await GetArangoDbClient()
            .GetDocumentAsync<SecondLevelProjectionAssignmentsUser>(
                GetArangoId<SecondLevelProjectionAssignmentsUser>(userId),
                transaction?.TransactionId);

        await CheckAResponseAsync(
            user,
            context: transaction?.CallingService,
            cancellationToken: cancellationToken);

        return Logger.ExitMethod(user.Result);
    }

    /// <summary>
    ///     Validates the given <see cref="IDatabaseTransaction" /> and throws an exception if the exception is not valid.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <returns>The <see cref="ArangoTransaction" /> which was passed.</returns>
    /// <exception cref="ArgumentException">Will be yielded if the transaction is not suitable for this repository.</exception>
    private ArangoTransaction ValidateTransaction(IDatabaseTransaction transaction)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            return Logger.ExitMethod<ArangoTransaction>(null);
        }

        if (transaction is not ArangoTransaction arangoTransaction)
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations.",
                nameof(transaction));
        }

        if (!arangoTransaction.IsActive)
        {
            throw new ArgumentException(
                "The given transaction is not active anymore and has either been committed or aborted.",
                nameof(transaction));
        }

        if (string.IsNullOrWhiteSpace(arangoTransaction.TransactionId))
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations. The transactionId must not be null or only contain whitespaces.",
                nameof(transaction));
        }

        if (!_modelsInfo.GetDocumentCollections()
                .Concat(_modelsInfo.GetEdgeCollections())
                .Concat(_modelsInfo.GetQueryDocumentCollections())
                .All(arangoTransaction.Collections.Contains))
        {
            throw new ArgumentException(
                "The passed transaction is not suited for ArangoDB operations. All first level projection collections have to be included within the transaction.",
                nameof(transaction));
        }

        return Logger.ExitMethod(arangoTransaction);
    }

    protected async Task CommitTransactionInternalAsync(
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IArangoDbClient client = GetArangoDbClient();
        TransactionOperationResponse response = await client.CommitTransactionAsync(transaction.TransactionId);
        transaction.MarkAsInactive();

        cancellationToken.ThrowIfCancellationRequested();

        await CheckAResponseAsync(
            response,
            true,
            context: transaction.CallingService,
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task AbortTransactionInternalAsync(
        ArangoTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        IArangoDbClient client = GetArangoDbClient();
        TransactionOperationResponse response = await client.AbortTransactionAsync(transaction.TransactionId);
        transaction.MarkAsInactive();

        cancellationToken.ThrowIfCancellationRequested();

        await CheckAResponseAsync(
            response,
            true,
            context: transaction.CallingService,
            cancellationToken: cancellationToken);

        Logger.ExitMethod();
    }

    protected string GetArangoId<TEntity>(string entityId)
    {
        return $"{_modelsInfo.GetQueryCollectionName<TEntity>()}/{entityId}";
    }

    /// <inheritdoc />
    public Task<SecondLevelProjectionAssignmentsUser> GetAssignmentUserAsync(
        string userId,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        Logger.EnterMethod();

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task<SecondLevelProjectionAssignmentsUser> userTask = arangoTransaction.ExecuteWithLock(
            () => GetAssignmentUserInternalAsync(userId, arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(userTask);
    }

    /// <inheritdoc />
    public Task SaveAssignmentUserAsync(
        SecondLevelProjectionAssignmentsUser user,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task saveTask = arangoTransaction.ExecuteWithLock(
            () => SendRequestAsync(
                c => c
                    .CreateDocumentAsync(
                        _modelsInfo.GetQueryCollectionName<SecondLevelProjectionAssignmentsUser>(),
                        user.InjectDocumentKey(u => u.ProfileId, c.UsedJsonSerializerSettings),
                        new CreateDocumentOptions
                        {
                            Overwrite = true
                        },
                        arangoTransaction?.TransactionId),
                throwExceptionIfNotFound: true,
                cancellationToken: cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(saveTask);
    }

    /// <inheritdoc />
    public Task RemoveAssignmentUserAsync(
        string userId,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Task deleteTask = arangoTransaction.ExecuteWithLock(
            () => SendRequestAsync(
                c => c
                    .DeleteDocumentAsync(
                        _modelsInfo.GetQueryCollectionName<SecondLevelProjectionAssignmentsUser>(),
                        userId,
                        transactionId: arangoTransaction?.TransactionId),
                throwExceptionIfNotFound: true,
                cancellationToken: cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(deleteTask);
    }

    /// <inheritdoc />
    public async Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();
        IArangoDbClient client = GetArangoDbClient();

        IList<string> collections = _modelsInfo.GetDocumentCollections()
            .Concat(_modelsInfo.GetEdgeCollections())
            .Concat(_modelsInfo.GetQueryDocumentCollections())
            .ToImmutableList();

        TransactionOperationResponse transactionResponse =
            await client.BeginTransactionAsync(collections, Array.Empty<string>());

        await CheckAResponseAsync(
            transactionResponse,
            true,
            context: CallingServiceContext.CreateNewOf<ArangoSecondLevelAssignmentRepository>(),
            cancellationToken: cancellationToken);

        IDatabaseTransaction transaction = new ArangoTransaction
        {
            Collections = collections,
            TransactionId = transactionResponse.GetTransactionId()
        };

        return Logger.ExitMethod(transaction);
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => CommitTransactionInternalAsync(arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        cancellationToken.ThrowIfCancellationRequested();

        Task task = arangoTransaction.ExecuteWithLock(
            () => AbortTransactionInternalAsync(arangoTransaction, cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default)
    {
        Logger.EnterMethod();

        Task<Dictionary<string, ulong>> task =
            _projectionStateRepository.GetLatestProjectedEventIdsAsync(stoppingToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public Task SaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = null,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        Task task =
            _projectionStateRepository.SaveProjectionStateAsync(projectionState, transaction, cancellationToken);

        return Logger.ExitMethod(task);
    }

    /// <inheritdoc />
    public async Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        return Logger.ExitMethod(
            await _projectionStateRepository.GetPositionOfLatestProjectedEventAsync(cancellationToken));
    }
}
