using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Handler.V2;

namespace UserProfileService.Projection.FirstLevel.Utilities;

/// <summary>
///     The helper is need for the <see cref="RolePropertiesChangedFirstLevelEventHandler" />.
/// </summary>
public static class RolePropertiesChangedHelper
{
    /// <summary>
    ///     Create the events that are needed when a role has been changed.
    /// </summary>
    /// <param name="membersToChange">Members that are effected by the role change.</param>
    /// <param name="role">The role that has been changed.</param>
    /// <param name="eventObject">The event object that is needed to create an <see cref="EventTuple" />.</param>
    /// <param name="creator">The creator created an <see cref="EventTuple" />.</param>
    /// <returns>Returns a list of <see cref="EventTuple" /> that are effected by the role change.</returns>
    public static List<EventTuple> HandleMembersChangedThroughRole(
        List<ObjectIdentPath> membersToChange,
        Role role,
        IDomainEvent eventObject,
        IFirstLevelEventTupleCreator creator)
    {
        if (membersToChange == null)
        {
            throw new ArgumentNullException(nameof(membersToChange));
        }

        if (role == null)
        {
            throw new ArgumentNullException(nameof(role));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (creator == null)
        {
            throw new ArgumentNullException(nameof(creator));
        }

        var roleChanged = new RoleChanged
        {
            Role = role,
            Context = PropertiesChangedContext.SecurityAssignments
        };

        var roleChangedEventTuple = new List<EventTuple>();

        foreach (ObjectIdentPath objectIdent in membersToChange)
        {
            roleChangedEventTuple.AddRange(
                creator.CreateEvents(
                    objectIdent,
                    new List<IUserProfileServiceEvent>
                    {
                        roleChanged
                    },
                    eventObject));
        }

        return roleChangedEventTuple;
    }

    /// <summary>
    ///     Returns the effected functions an profile by a changed role.
    /// </summary>
    /// <param name="objects">The effected object by the role changed.</param>
    /// <param name="roleId">The role of the changed role, to that it can be filtered out of the result.</param>
    /// <param name="logger">The logger that is used to log message.</param>
    /// <returns>Returns the functions and profiles separately that are effected by the role change.</returns>
    public static (List<ObjectIdentPath> related, List<ObjectIdentPath> functions)
        SplitRelevantObjectsToSendEvents(ICollection<ObjectIdentPath> objects, string roleId, ILogger logger)
    {
        logger.EnterMethod();

        if (objects == null)
        {
            throw new ArgumentNullException(nameof(objects));
        }

        if (string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException(nameof(roleId));
        }

        List<ObjectIdentPath> functions = objects
            .Where(o => o.Type == ObjectType.Function)
            .ToList();

        logger.LogDebugMessage(
            "Found {count} functions to inform that role changed.",
            functions.Count.AsArgumentList());

        List<ObjectIdentPath> objectsIdentsToInform = objects
            .Where(
                o => o.Steps == 1
                    && (o.Type == ObjectType.Group
                        || o.Type == ObjectType.User))
            .ToList();

        logger.LogDebugMessage(
            "Found {count} objects to inform that role changed.",
            objectsIdentsToInform.Count.ToLogString().AsArgumentList());

        return logger.ExitMethod((objectsIdentsToInform.Where(o => o.Id != roleId).ToList(), functions));
    }
}
