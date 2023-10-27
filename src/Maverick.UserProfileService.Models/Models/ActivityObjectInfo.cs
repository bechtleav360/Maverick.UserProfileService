using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Defines the object overview in <see cref="ActivityLogEntry" /> objects.
    /// </summary>
    public class ActivityObjectInfo
    {
        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     The unique identifier of the object.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The name of the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Type of object.
        /// </summary>
        public ObjectType Type { get; set; }

        /// <summary>
        ///     Create an instance of <see cref="ActivityObjectInfo" />.
        /// </summary>
        /// <param name="id">The unique identifier of the object.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="displayName">The name that is used for displaying.</param>
        /// <param name="type">Type of object.</param>
        public ActivityObjectInfo(string id, string name, string displayName, ObjectType type)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            Type = type;
        }
    }
}
