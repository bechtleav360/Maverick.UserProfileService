using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     A <see cref="ISecondLevelAssignmentContainer" /> storing functions.
/// </summary>
public class SecondLevelAssignmentFunction : ISecondLevelAssignmentContainer
{
    /// <inheritdoc />
    public ContainerType ContainerType => ContainerType.Function;

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <summary>
    ///     Contains the id of the correlated organization.
    /// </summary>
    public string OrganizationId { get; set; }

    /// <summary>
    ///     Contains the id of the correlated role.
    /// </summary>
    public string RoleId { get; set; }
}
