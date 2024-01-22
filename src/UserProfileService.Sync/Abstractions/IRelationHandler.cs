using System.Threading.Tasks;
using MassTransit;
using UserProfileService.Sync.Abstraction.Models;
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
    /// <param name="context">The context for the next step.</param>
    /// <param name="addRelation">If the relation should be added.</param>
    /// <param name="deleteRelation">If the relation should be deleted.</param>
    /// <returns></returns>
    Task HandleRelationsAsync(
        SagaConsumeContext<ProcessState, ISyncMessage> context,
        bool addRelation,
        bool deleteRelation);
}