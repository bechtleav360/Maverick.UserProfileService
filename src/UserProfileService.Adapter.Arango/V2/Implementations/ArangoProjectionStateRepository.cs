using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Query;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An implementation of <see cref="IProjectionStateRepository" /> utilizing ArangoDB to store data.
/// </summary>
internal class ArangoProjectionStateRepository : ArangoRepositoryBase, IProjectionStateRepository
{
    //TODO change
    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <summary>
    ///     Specifies the name of the logs collection.
    /// </summary>
    public string LogCollection { get; }

    /// <summary>
    ///     Offers the opportunity to add custom settings to the used JsonConverter.
    /// </summary>
    public JsonSerializerSettings SerializerSettings { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ArangoProjectionStateRepository" />
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The root service provider.</param>
    /// <param name="clientName">The name of the ArangoDb client to use.</param>
    /// <param name="logsCollection">The name of the collection in which to store the information.</param>
    /// <param name="settings">Optional <see cref="JsonSerializerSettings" /> to add for serialization.</param>
    /// <exception cref="ArgumentException">Will be yielded if <paramref name="settings" /> is not compatible.</exception>
    public ArangoProjectionStateRepository(
        ILogger logger,
        IServiceProvider serviceProvider,
        string clientName,
        string logsCollection,
        JsonSerializerSettings settings = null) : base(logger, serviceProvider)
    {
        ArangoDbClientName = clientName;
        LogCollection = logsCollection;

        SerializerSettings = settings
            ?? new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter() },
                ContractResolver = new DefaultContractResolver()
            };

        if (SerializerSettings.ContractResolver is not DefaultContractResolver)
        {
            throw new ArgumentException(
                "The specified json serializer settings do not match the requirements for the projection state store (ContractResolver=DefaultContractResolver)",
                nameof(settings));
        }

        if (!SerializerSettings.Converters.Any(c => c is StringEnumConverter))
        {
            Logger.LogWarnMessage(
                "The serializer settings provided to ProjectionStateRepositoryV1 for client {clientName} does not seem to have the StringEnumConverter enabled",
                Arguments(clientName));
        }
    }

    /// <summary>
    ///     Validates the given transaction for suitability. If no transaction was passed in, <c>null</c> will be returned.
    /// </summary>
    /// <param name="transaction">The <see cref="IDatabaseTransaction" /> to validate.</param>
    /// <returns>The given transaction as <see cref="ArangoTransaction" /> or null.</returns>
    /// <exception cref="ArgumentException">
    ///     WIll be yielded if the passed transaction is not suitable for further ArangoDb
    ///     operations.
    /// </exception>
    protected virtual ArangoTransaction ValidateTransaction(IDatabaseTransaction transaction)
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

        if (!arangoTransaction.Collections.Contains(LogCollection))
        {
            throw new ArgumentException(
                $"The passed transaction is not suited for ArangoDB operations. The collection {LogCollection} has to be included within the transaction.",
                nameof(transaction));
        }

        return Logger.ExitMethod(arangoTransaction);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, ulong>> GetLatestProjectedEventIdsAsync(
        CancellationToken stoppingToken = default)
    {
        Logger.EnterMethod();

        MultiApiResponse<Dictionary<string, ulong>> response = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<Dictionary<string, ulong>>(
                new CreateCursorBody
                {
                    Query = $@"
                            LET states = (
                                FOR log IN @@logsCollection
                                COLLECT stream = log.{
                                    nameof(ProjectionState.StreamName)
                                } AGGREGATE eventNumber = MAX(log.{
                                    nameof(ProjectionState.EventNumberVersion)
                                })
                                RETURN {{
                                    stream, eventNumber
                                }}
                            )

                            RETURN ZIP(states[*].stream, states[*].eventNumber)",
                    BindVars = new Dictionary<string, object>
                    {
                        { "@logsCollection", LogCollection }
                    }
                },
                cancellationToken: stoppingToken),
            cancellationToken: stoppingToken);

        Dictionary<string, ulong> latestEvents =
            response.QueryResult.SingleOrDefault() ?? new Dictionary<string, ulong>();

        return Logger.ExitMethod(latestEvents);
    }

    /// <inheritdoc />
    public async Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken stoppingToken = default)
    {
        Logger.EnterMethod();

        MultiApiResponse<GlobalPosition> response = await SendRequestAsync(
            c => c.ExecuteQueryWithCursorOptionsAsync<GlobalPosition>(
                new CreateCursorBody
                {
                    Query = $@"
                                   FOR log IN @@logsCollection
                                   SORT log.{
                                       nameof(ProjectionState.EventNumberVersion)
                                   } DESC, log.{
                                       nameof(ProjectionState.EventNumberSequence)
                                   } DESC
                                   LIMIT 1
                                   RETURN {{
                                      {
                                          nameof(GlobalPosition.Version)
                                      }: NOT_NULL(log.{
                                          nameof(ProjectionState.EventNumberVersion)
                                      },0),
                                      {
                                          nameof(GlobalPosition.SequencePosition)
                                      }: NOT_NULL(log.{
                                          nameof(ProjectionState.EventNumberSequence)
                                      },0)
                                   }}",
                    BindVars = new Dictionary<string, object>
                    {
                        { "@logsCollection", LogCollection }
                    }
                },
                cancellationToken: stoppingToken),
            cancellationToken: stoppingToken);

        GlobalPosition latestEvents = response.QueryResult.SingleOrDefault();

        return Logger.ExitMethod(latestEvents);
    }

    /// <inheritdoc />
    public Task SaveProjectionStateAsync(
        ProjectionState projectionState,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (projectionState == null)
        {
            throw new ArgumentNullException(nameof(projectionState));
        }

        ArangoTransaction arangoTransaction = ValidateTransaction(transaction);

        Logger.LogInfoMessage(
            "Writing {projectionState} to event log {logCollection} with transactionId: {transactionId} ",
            Arguments(projectionState, LogCollection, arangoTransaction?.TransactionId.ToLogString()));

        Task<CreateDocumentResponse> task = arangoTransaction.ExecuteWithLock(
            () => SendRequestAsync(
                c => c.CreateDocumentAsync(
                    LogCollection,
                    JObject.FromObject(projectionState, JsonSerializer.Create(SerializerSettings)),
                    new CreateDocumentOptions
                    {
                        OverWriteMode = AOverwriteMode.Conflict
                    },
                    arangoTransaction?.TransactionId),
                true,
                true,
                CallingServiceContext.CreateNewOf<ArangoProjectionStateRepository>(),
                cancellationToken),
            cancellationToken);

        return Logger.ExitMethod(task);
    }
}
