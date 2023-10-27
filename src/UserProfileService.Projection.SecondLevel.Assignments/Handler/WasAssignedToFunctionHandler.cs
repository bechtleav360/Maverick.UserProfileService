using System;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="WasAssignedToRole" /> regarding assignments.
/// </summary>
internal class WasAssignedToFunctionHandler : WasAssignedToHandlerBase<Function, WasAssignedToFunction>
{
    /// <inheritdoc />
    public WasAssignedToFunctionHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<WasAssignedToFunctionHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }

    /// <inheritdoc />
    protected override void AddContainer(SecondLevelProjectionAssignmentsUser user, IContainer container)
    {
        Logger.EnterMethod();

        if (!(container is Function function))
        {
            throw new NotSupportedException("The container must be a function");
        }

        base.AddContainer(user, container);

        if (user.Containers.All(c => c.Id != function.RoleId))
        {
            user.Containers.Add(Mapper.Map<ISecondLevelAssignmentContainer>(function.Role));
        }

        if (user.Containers.All(c => c.Id != function.OrganizationId))
        {
            user.Containers.Add(Mapper.Map<ISecondLevelAssignmentContainer>(function.Organization));
        }

        Logger.ExitMethod();
    }
}
