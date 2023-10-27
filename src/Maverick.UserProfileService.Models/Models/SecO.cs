using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    /// </summary>
    public class SecO : SecOBasic
    {
        /// <summary>
        ///     Defines the children of an object.
        /// </summary>
        public IList<SecOBasic> Children { set; get; } = new List<SecOBasic>();

        /// <summary>
        ///     Defines the parent of an object.
        /// </summary>
        public SecOBasic Parent { set; get; }
    }
}
