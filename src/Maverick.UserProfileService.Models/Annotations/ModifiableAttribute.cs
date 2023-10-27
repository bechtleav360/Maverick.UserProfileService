using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines an attribute that specifies whether a property may be changed.
    /// </summary>
    public class ModifiableAttribute : Attribute
    {
        /// <summary>
        ///     Specifies whether the property may be changed.
        /// </summary>
        public bool AllowEdit { get; }

        /// <summary>
        ///     Create an instance of <see cref="ModifiableAttribute" />
        /// </summary>
        /// <param name="allowEdit">Specifies whether the property may be changed</param>
        public ModifiableAttribute(bool allowEdit = true)
        {
            AllowEdit = allowEdit;
        }
    }
}
