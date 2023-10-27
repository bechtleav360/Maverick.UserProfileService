using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     A <see cref="ISecondLevelAssignmentContainer" /> storing roles, groups, organization.
/// </summary>
public class SecondLevelAssignmentContainer : ISecondLevelAssignmentContainer
{
    /// <inheritdoc />
    public ContainerType ContainerType { get; set; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }
}
