using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     The interface marks all containers that are used for the  first level projection repository.
/// </summary>
public interface IFirstLevelProjectionContainer : IFirstLevelProjectionSimplifier
{
    /// <summary>
    ///     The type of the container.
    /// </summary>
    ContainerType ContainerType { get; }

    string Id { get; set; }
}
