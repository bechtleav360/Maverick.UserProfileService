using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     Describe a class to define objects by id and type.
    /// </summary>
    public interface IObjectIdent
    {
        /// <summary>
        ///     Unique identifier of object.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        ///     Type of object.
        /// </summary>
        ObjectType Type { get; set; }
    }
}
