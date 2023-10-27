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
internal class WasAssignedToOrganizationHandler : WasAssignedToHandlerBase<Organization, WasAssignedToOrganization>
{
    /// <inheritdoc />
    public WasAssignedToOrganizationHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<WasAssignedToOrganizationHandler> logger) : base(repository, mapper, streamNameResolver, logger)
    {
    }
}
