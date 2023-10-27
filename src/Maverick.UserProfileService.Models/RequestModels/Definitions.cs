using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Contains single filter items that will be used in list queries.Each filter item compares
    ///     the value of the property called like the provided name with a
    ///     reference value.If the property is a primitive, the (case-insensitive) equals-operator will be used.
    ///     If the property is an array, the contains-operator (case-insensitive) will be used
    /// </summary>
    public class Definitions
    {
        /// <summary>
        ///     Defines the binary expression that is be used to combine single filter items.
        /// </summary>
        public BinaryOperator BinaryOperator { set; get; }

        /// <summary>
        ///     Defines the property name those value should match
        /// </summary>
        [Required]
        public string FieldName { set; get; }

        /// <summary>
        ///     The type of the operator that is used to build up rules to filter data.
        /// </summary>
        public FilterOperator Operator { set; get; }

        /// <summary>
        ///     Defines the value expressions that should match the property.Wildcard(*) can be used.
        ///     If multiple values are specified, they are combined using the
        ///     logical operator that is specified in the property binaryOperator.
        /// </summary>
        [Required]
        [AtLeastOneValidString(ErrorMessage = "Property {0}: At least one value should be set to be compared with!")]
        public string[] Values { set; get; }
    }
}
