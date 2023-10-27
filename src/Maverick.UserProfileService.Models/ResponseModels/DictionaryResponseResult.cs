using System.Collections.Generic;

namespace Maverick.UserProfileService.Models.ResponseModels
{
    /// <summary>
    ///     The response object if the result set is a list of key-value-pairs.<br />
    ///     The list response object inside the body stores the total amount of these key-value-pairs.
    /// </summary>
    /// <typeparam name="TVal">The type of each value inside the result set.</typeparam>
    /// <typeparam name="TTKey">The type of each key.</typeparam>
    public class DictionaryResponseResult<TTKey, TVal>
    {
        /// <summary>
        ///     Contains the pagination metadata of the current result set.
        /// </summary>
        public ListResponse Response { set; get; }

        /// <summary>
        ///     The current result set as <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        public IDictionary<TTKey, TVal> Result { set; get; }
    }
}
