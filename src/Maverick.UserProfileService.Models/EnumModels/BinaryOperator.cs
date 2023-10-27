using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.EnumModels
{
    /// <summary>
    ///     Defines the binary expression that is be used to combine single filter items.
    /// </summary>
    [FilterEncapsulate(false)]
    public enum BinaryOperator
    {
        /// <summary>
        /// Represents the binary 'or' operator.
        /// </summary>
        [FilterSerialize("Or")]
        Or,

        /// <summary>
        /// Represents the binary 'and' operator.
        /// </summary>
        [FilterSerialize("And")]
        And
    }
}
