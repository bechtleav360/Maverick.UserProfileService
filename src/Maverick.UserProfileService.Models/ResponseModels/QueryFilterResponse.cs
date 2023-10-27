using System.Collections.Generic;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     Contains properties and their values that can be used to filter.
    /// </summary>
    public class QueryFilterResponse
    {
        /// <summary>
        ///     The property which is used for filtering.
        /// </summary>
        public string FieldName { set; get; }

        /// <summary>
        ///     All values found for the desired field name.
        /// </summary>
        public IList<string> Values { set; get; } = new List<string>();
    }
}
