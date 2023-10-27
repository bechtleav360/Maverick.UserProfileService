using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     Represents a container for the assignments calculation.
/// </summary>
public interface ISecondLevelAssignmentContainer
{
    /// <summary>
    ///     Specifies the container type.
    /// </summary>
    ContainerType ContainerType { get; }

    /// <summary>
    ///     Specifies the identifier of the container.
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     Contains the name of the container.
    /// </summary>
    string Name { get; set; }
}
