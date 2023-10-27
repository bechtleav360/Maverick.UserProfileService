using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions;

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
