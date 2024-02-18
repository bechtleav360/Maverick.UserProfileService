using System.Threading.Tasks;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Handlers;

/// <summary>
///     The none relation is doing nothing. An other relation handler can be
///     registered if the sync system has relations between the entities.
/// </summary>
public class NoneRelationHandler : IRelationHandler<NoneSyncModel>
{
    /// <inheritdoc />
    public Task HandleRelationsAsync(Process process, bool addRelation, bool deleteRelation, ObjectType objectType)
    {
        return Task.CompletedTask;
    }
}
