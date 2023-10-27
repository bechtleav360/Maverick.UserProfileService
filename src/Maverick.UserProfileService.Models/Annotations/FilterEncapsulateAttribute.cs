using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines whether enum values should be encapsulated or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class FilterEncapsulateAttribute : Attribute
    {
        /// <summary>
        ///     A boolean value indicating whether the values should be encapsulated or not. Default: <c>true</c>
        /// </summary>
        public bool EncapsulateValues { get; set; } = true;

        /// <summary>
        ///     Initializes a new instance of <see cref="FilterEncapsulateAttribute" /> without defining any arguments.
        /// </summary>
        public FilterEncapsulateAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="FilterEncapsulateAttribute" /> with setting the
        ///     <see cref="EncapsulateValues" /> boolean value.
        /// </summary>
        /// <param name="doEncapsulateValues">A boolean value indicating whether the values should be encapsulated or not.</param>
        public FilterEncapsulateAttribute(bool doEncapsulateValues)
        {
            EncapsulateValues = doEncapsulateValues;
        }
    }
}
