using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     The type of the operator that is used to build up rules to filter data.
    /// </summary>
    [FilterEncapsulate(false)]
    public enum FilterOperator
    {
        /// <summary>
        ///     Represents the equals operator.
        /// </summary>
        [FilterSerialize("==")]
        Equals,

        /// <summary>
        ///     Represents the contains operator.
        /// </summary>
        [FilterSerialize(":")]
        Contains,

        /// <summary>
        ///     Represents the not equals operator.
        /// </summary>
        [FilterSerialize("!=")]
        NotEquals,

        /// <summary>
        ///     Represents the lower than operator.
        /// </summary>
        [FilterSerialize("<")]
        LowerThan,

        /// <summary>
        ///     Represents the greater than operator.
        /// </summary>
        [FilterSerialize(">")]
        GreaterThan,

        /// <summary>
        ///     Represents the lower than equals operator.
        /// </summary>
        [FilterSerialize("<=")]
        LowerThanEquals,

        /// <summary>
        ///     Represents the greater than equals operator.
        /// </summary>
        [FilterSerialize(">=")]
        GreaterThanEquals,

        /// <summary>
        ///     Represents the equals operator, that ignores the case.
        /// </summary>
        [FilterSerialize("~=")]
        EqualsCaseInsensitive
    }
}
