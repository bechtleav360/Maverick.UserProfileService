using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="WasAssignedToGroup" /> regarding assignments.
/// </summary>
internal class WasAssignedToGroupHandler : WasAssignedToHandlerBase<Group, WasAssignedToGroup>
{
    /// <inheritdoc />
    public WasAssignedToGroupHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<WasAssignedToGroupHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }
}
