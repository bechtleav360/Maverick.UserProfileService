using System.Collections.Generic;

namespace Maverick.UserProfileService.FilterUtility.Models
{
    /// <summary>
    ///     Contains the serialization values for an enum
    /// </summary>
    public class EnumFilterDefinition
    {
        /// <summary>
        ///     If values should be encapsulated
        /// </summary>
        public bool EncapsulateValues { get; set; }

        /// <summary>
        ///     Valid values for the enum when serializing
        /// </summary>
        public IEnumerable<string> ValidValues { get; set; }

        /// <summary>
        ///     Initializes a <see cref="EnumFilterDefinition" />
        /// </summary>
        public EnumFilterDefinition()
        {
        }

        /// <summary>
        ///     Initializes a <see cref="EnumFilterDefinition" />
        /// </summary>
        /// <param name="encapsulateValues">If enum values are encapsulated</param>
        /// <param name="validValues">Valid values for the enum</param>
        public EnumFilterDefinition(bool encapsulateValues, IEnumerable<string> validValues)
        {
            EncapsulateValues = encapsulateValues;
            ValidValues = validValues;
        }
    }
}
