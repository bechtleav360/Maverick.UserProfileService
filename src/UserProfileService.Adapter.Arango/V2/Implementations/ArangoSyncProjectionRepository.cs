using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Implementations;

/// <summary>
///     An Implementation of <see cref="ISecondLevelProjectionRepository" /> for the Arango DB.
/// </summary>
public class ArangoSyncProjectionRepository : ArangoRepositoryBase, IProjectionStateRepository
{
    /// <summary>
    ///     All operations of <see cref="IProjectionStateRepository" /> will be forwarded to this instance.
    /// </summary>
    private readonly ArangoProjectionStateRepository _projectionStateRepository;

    /// <inheritdoc />
    protected override string ArangoDbClientName { get; }

    /// <summary>
    ///     Create an instance of <see cref="ArangoSyncProjectionRepository" />.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The instance of <see cref="IServiceProvider" /> that is used to get required services.</param>
    /// <param name="collectionPrefix">Prefix to use for collections of database.</param>
    /// <param name="arangoDbClientName">Name of arango client to use.</param>
    public ArangoSyncProjectionRepository(
        ILogger<ArangoSyncProjectionRepository> logger,
        IServiceProvider serviceProvider,
        string collectionPrefix,
        string arangoDbClientName = null) : base(logger, serviceProvider)
    {
        ModelBuilderOptions modelsInfo =
            DefaultModelConstellation.CreateNewSecondLevelProjection(collectionPrefix).ModelsInfo;

        _projectionStateRepository = new ArangoProjectionStateRepository(
            logger,
            serviceProvider,
            arangoDbClientName,
            modelsInfo.GetCollectionName<ProjectionState>());

        if (arangoDbClientName != null)
        {
            ArangoDbClientName = arangoDbClientName;
        }
    }

    /// <inheritdoc />
    public async Task<GlobalPosition> GetPositionOfLatestProjectedEventAsync(
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        return Logger.ExitMethod(
            await _projectionStateRepository.GetPositionOfLatestProjectedEventAsync(cancellationToken));
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
}
