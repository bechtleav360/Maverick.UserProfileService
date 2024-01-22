using System.Threading.Tasks;
using MassTransit;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Models.State;
using UserProfileService.Sync.States;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     The relation interface that
/// </summary>
/// <typeparam name="TSync">The Type of entities where the relation should be resolved.</typeparam>
public interface IRelationHandler<TSync> : IRelationHandler where TSync : ISyncModel
{
}

/// <summary>
///     The relation handler that can resolve relation within entities.
/// </summary>
public interface IRelationHandler
{
    /// <summary>
    ///     Resolve resolve relation within entities and creates or deleted existing
    ///     relations.
    /// </summary>
    /// <param name="process">The saga process the relation step is running it.</param>
    /// <param name="addRelation">If the relation should be added.</param>
    /// <param name="deleteRelation">If the relation should be deleted.</param>
    /// <param name="objectType">The object type for those relations should be added.</param>
    /// <returns></returns>
    Task HandleRelationsAsync(Process process, bool addRelation, bool deleteRelation, ObjectType objectType);
}