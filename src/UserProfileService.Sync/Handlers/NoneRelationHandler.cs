using System.Threading.Tasks;
using MassTransit;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.States;

namespace UserProfileService.Sync.Handlers;

/// <summary>
///     The none relation is doing nothing. An other relation handler can be
///     registered if the sync system has relations between the entities.
/// </summary>
public class NoneRelationHandler : IRelationHandler<NoneSyncModel>
{
    /// <inheritdoc />
    public Task HandleRelationsAsync(
        SagaConsumeContext<ProcessState, ISyncMessage> context,
        bool addRelation,
        bool deleteRelation)
    {
        return Task.CompletedTask;
    }
}
