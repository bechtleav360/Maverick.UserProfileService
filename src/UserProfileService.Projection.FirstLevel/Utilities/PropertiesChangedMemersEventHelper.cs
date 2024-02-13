using System;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.FirstLevel.Abstractions;

namespace UserProfileService.Projection.FirstLevel.Utilities;

/// <summary>
///     This helper is used to help to set the right related context
///     for two entities. One entity is the changed entity and one entity
///     is the affected entity that maybe has to be changed. The affected entity and the
///     changed entity has a relation <see cref="PropertiesChangedRelation" />. With the
///     relation and the two entities an <see cref="EventTuple" /> can be created that  describes
///     which members have to be changed.
/// </summary>
public static class PropertiesChangedMembersEventHelper
{
    /// <summary>
    ///     Handles two entities where the reference entity is a group and compute
    ///     an  <see cref="EventTuple" />  for the relation between the two entities.
    /// </summary>
    /// <param name="groupIdReference"> The id of the group reference entity.</param>
    /// <param name="relatedEntity">The related entity as an <see cref="ObjectIdent" />.</param>
    /// <param name="relationToChangedObject">
    ///     The relation between the <paramref name="groupIdReference" /> and the
    ///     <paramref name="relatedEntity" />.
    /// </param>
    /// <param name="originalEvent">The original event that is need to created a <see cref="EventTuple" />.</param>
    /// <param name="creator">The creator the create an <see cref="EventTuple" />.</param>
    /// <returns>
    ///     An <paramref name="originalEvent" /> that contains the right event to changed members that are affected by the
    ///     changed group.
    /// </returns>
    public static EventTuple HandleGroupAsReference(
        string groupIdReference,
        ObjectIdent relatedEntity,
        PropertiesChangedRelation relationToChangedObject,
        ProfilePropertiesChangedEvent originalEvent,
        IFirstLevelEventTupleCreator creator)
    {
        if (relatedEntity == null)
        {
            throw new ArgumentNullException(nameof(relatedEntity));
        }

        if (originalEvent == null)
        {
            throw new ArgumentNullException(nameof(originalEvent));
        }

        if (creator == null)
        {
            throw new ArgumentNullException(nameof(creator));
        }

        if (string.IsNullOrWhiteSpace(groupIdReference))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(groupIdReference));
        }

        var groupPropertiesChanged = new PropertiesChanged
        {
            Id = groupIdReference,
            Properties = originalEvent.Payload.Properties,
            ObjectType = ObjectType.Group
        };

        switch (relatedEntity.Type)
        {
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Group:
                groupPropertiesChanged.RelatedContext = relationToChangedObject switch
                {
                    PropertiesChangedRelation.MemberOf =>
                        PropertiesChangedContext.Members,
                    PropertiesChangedRelation.Member =>
                        PropertiesChangedContext.MemberOf,
                    PropertiesChangedRelation.IndirectMember =>
                        PropertiesChangedContext.IndirectMember,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(relationToChangedObject),
                        relationToChangedObject,
                        $"Argument did not match any values of {nameof(PropertiesChangedRelation)}")
                };

                break;
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.User:
                groupPropertiesChanged.RelatedContext = PropertiesChangedContext.MemberOf;

                break;
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Function:
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Role:
                groupPropertiesChanged.RelatedContext = PropertiesChangedContext.LinkedProfiles;

                break;
        }

        return creator.CreateEvent(relatedEntity, groupPropertiesChanged, originalEvent);
    }

    /// <summary>
    ///     Handles two entities where the reference entity is a user and compute
    ///     an  <see cref="EventTuple" />  for the relation between the two entities.
    /// </summary>
    /// <param name="userReferenceId"> The id of the user reference entity.</param>
    /// <param name="relatedEntity">The related entity as an <see cref="ObjectIdent" />.</param>
    /// <param name="originalEvent">The original event that is need to created a <see cref="EventTuple" />.</param>
    /// <param name="creator">The creator the create an <see cref="EventTuple" />.</param>
    /// <returns>
    ///     An <paramref name="originalEvent" /> that contains the right event to changed members that are affected by the
    ///     changed user.
    /// </returns>
    public static EventTuple HandleUserAsReference(
        string userReferenceId,
        ObjectIdent relatedEntity,
        ProfilePropertiesChangedEvent originalEvent,
        IFirstLevelEventTupleCreator creator)
    {
        if (originalEvent == null)
        {
            throw new ArgumentNullException(nameof(originalEvent));
        }

        if (creator == null)
        {
            throw new ArgumentNullException(nameof(creator));
        }

        if (string.IsNullOrWhiteSpace(userReferenceId))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(userReferenceId));
        }

        var userPropertiesChanged = new PropertiesChanged
        {
            Id = userReferenceId,
            Properties = originalEvent.Payload.Properties,
            ObjectType = ObjectType.User
        };

        switch (relatedEntity.Type)
        {
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Group:
                userPropertiesChanged.RelatedContext = PropertiesChangedContext.Members;

                break;
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Function:
            case Maverick.UserProfileService.Models.EnumModels.ObjectType.Role:
                userPropertiesChanged.RelatedContext = PropertiesChangedContext.LinkedProfiles;

                break;
        }

        return creator.CreateEvent(relatedEntity, userPropertiesChanged, originalEvent);
    }

    /// <summary>
    ///     Handles two entities where the reference entity is a organization and compute
    ///     an  <see cref="EventTuple" />  for the relation between the two entities.
    /// </summary>
    /// <param name="organizationReferenceId"> The id of the group reference entity.</param>
    /// <param name="relatedEntity">The related entity as an <see cref="ObjectIdent" />.</param>
    /// <param name="relationToChangedObject">
    ///     The relation between the <paramref name="organizationReferenceId" /> and the
    ///     <paramref name="relatedEntity" />.
    /// </param>
    /// <param name="originalEvent">The original event that is need to created a <see cref="EventTuple" />.</param>
    /// <param name="creator">The creator the create an <see cref="EventTuple" />.</param>
    /// <returns>
    ///     An <paramref name="originalEvent" /> that contains the right event to changed members that are affected by the
    ///     changed organization.
    /// </returns>
    public static EventTuple HandleOrganizationAsReference(
        string organizationReferenceId,
        ObjectIdent relatedEntity,
        PropertiesChangedRelation relationToChangedObject,
        ProfilePropertiesChangedEvent originalEvent,
        IFirstLevelEventTupleCreator creator)
    {
        var organizationPropertiesChanged = new PropertiesChanged
        {
            Id = organizationReferenceId,
            Properties = originalEvent.Payload.Properties,
            ObjectType = ObjectType.Organization
        };

        organizationPropertiesChanged.RelatedContext = relationToChangedObject switch
        {
            PropertiesChangedRelation.MemberOf =>
                PropertiesChangedContext.Members,
            PropertiesChangedRelation.Member =>
                PropertiesChangedContext.MemberOf,
            _ => organizationPropertiesChanged.RelatedContext
        };

        return creator.CreateEvent(relatedEntity, organizationPropertiesChanged, originalEvent);
    }
}
