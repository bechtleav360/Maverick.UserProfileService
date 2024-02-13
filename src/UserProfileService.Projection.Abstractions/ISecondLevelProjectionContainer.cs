using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     The interface marks all containers that are used for the second level projection repository.
/// </summary>
public interface ISecondLevelProjectionContainer
{
    /// <summary>
    ///     Defines the type of the container (like group, function, ...)
    /// </summary>
    ContainerType ContainerType { get; }

    /// <summary>
    ///     Defines the name of the container.
    /// </summary>
    string Id { get; set; }
}
