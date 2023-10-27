using System.Collections.Generic;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Defines the binary expression that is be used to combine single filter items.
    /// </summary>
    public class Filter
    {
        /// <summary>
        ///     Defines the binary expression that is be used to combine single filter items.
        /// </summary>
        public BinaryOperator CombinedBy { set; get; } = BinaryOperator.Or;

        /// <summary>
        ///     A List of definitions.
        /// </summary>
        // can be null, but if set, should be valid!
        [DefinitionCollectionValid]
        public List<Definitions> Definition { get; set; }
    }
}
