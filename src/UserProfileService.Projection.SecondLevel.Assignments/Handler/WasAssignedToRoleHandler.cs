using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="WasAssignedToRole" /> regarding assignments.
/// </summary>
internal class WasAssignedToRoleHandler : WasAssignedToHandlerBase<Role, WasAssignedToRole>
{
    /// <inheritdoc />
    public WasAssignedToRoleHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<WasAssignedToRoleHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }
}
