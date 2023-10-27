using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     Defines all properties of a object containing profiles.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        ///     Defines the type of the current container object.
        /// </summary>
        ContainerType ContainerType { get; }

        /// <summary>
        ///     Used to identify the resource.
        /// </summary>
        string Id { get; }
    }
}
