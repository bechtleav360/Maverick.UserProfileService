using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Abstractions;

/// <summary>
///     The changed related events resolver is used to resolve
///     the second level events for the properties changed events.
///     This handler is shares also the same code base for the <see cref="RolePropertiesChangedEvent" />,
///     <see cref="ProfilePropertiesChangedEvent" /> and <see cref="FunctionPropertiesChangedEvent" />.
/// </summary>
public interface IPropertiesChangedRelatedEventsResolver
{
    /// <summary>
    ///     The methode creates an event that is used to detect the members that should be updated due to
    ///     changes of an entity.
    /// </summary>
    /// <param name="referenceEntity">
    ///     The reference entity has been changed and  is used to detected what kind of event has to
    ///     be created.
    /// </param>
    /// <param name="relatedEntity">The related entity is affected of the <paramref name="referenceEntity" />.</param>
    /// <param name="relationToChangedObject">
    ///     It used to show the relation between the <paramref name="referenceEntity" /> and
    ///     the <paramref name="referenceEntity" />.
    /// </param>
    /// <param name="originalEvent">The original event is needed to create an <see cref="EventTuple" />.</param>
    /// <returns>
    ///     An <see cref="PropertiesChanged" /> event to update the right members because of the change of the
    ///     <paramref name="referenceEntity" />.
    /// </returns>
    EventTuple CreateRelatedMemberEvent(
        ObjectIdent referenceEntity,
        ObjectIdent relatedEntity,
        PropertiesChangedRelation relationToChangedObject,
        ProfilePropertiesChangedEvent originalEvent);

    /// <summary>
    ///     The method is used to create second levels event because of the change of a function related.
    /// </summary>
    /// <param name="functionId">The function id that is used to identify a function.</param>
    /// <param name="functionPropertiesChanged">
    ///     The function properties changed event to is used to create an
    ///     <see cref="EventTuple" />.
    /// </param>
    /// <param name="context">
    ///     The kind of changes that has been done to the function. The function can be changes can be:
    ///     role-related, organization-related or self. Self states that other properties have been changed.
    /// </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A list of <see cref="EventTuple" />s related to the function that has been changed.</returns>
    Task<List<EventTuple>> CreateFunctionPropertiesChangedEventsAsync(
        string functionId,
        FunctionPropertiesChangedEvent functionPropertiesChanged,
        PropertiesChangedContext context,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken);
}
