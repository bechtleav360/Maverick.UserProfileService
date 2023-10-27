using System.Collections.Generic;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.Models.BasicModels
{
    /// <summary>
    ///     Defines the base model of an object instance.
    /// </summary>
    public class SecOBasic
    {
        /// <summary>
        ///     Determines whether the inheritance should be interrupted or not.
        /// </summary>
        public bool BreakInheritance { get; set; }

        /// <summary>
        ///     A link where to find the custom properties for an object.
        /// </summary>
        public string CustomPropertyUrl { set; get; }

        /// <summary>
        ///     The identifier of the group member.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     The name of the group member.
        /// </summary>
        [Searchable]
        [Modifiable]
        public string Name { set; get; }

        /// <summary>
        ///     Is used to classify the object.
        /// </summary>
        [Modifiable]
        public IList<Tag> Tags { set; get; } = new List<Tag>();
    }
}
