using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.StateMachine.Utilities
{
    public static class ValidationExtension
    {
        /// <summary>
        ///     Checks if the list exists under the given key and removes empty entries from the list.
        ///     If remove == true, then the key is completely deleted from the dictionary.
        /// </summary>
        /// <typeparam name="TObject">IEnumerable type of the value which is stored under the key in the dictionary.</typeparam>
        /// <typeparam name="TType">Type of entries in the list.</typeparam>
        /// <param name="dict">Dictionary in which the list is stored.</param>
        /// <param name="key">Key of the dictionary under which the list is stored.</param>
        /// <param name="remove">Specifies whether empty lists are removed from the dictionary. </param>
        public static void RemoveEnumerableNullValues<TObject, TType>(
            IDictionary<string, object> dict,
            string key,
            bool remove = false) where TObject : IEnumerable<TType>
        {
            if (dict.TryGetValue(key, out object @object) && @object.TryConvertObject(out TObject objectList))
            {
                IEnumerable<TType> filteredList = objectList.Where(o => o != null);

                // If the list is empty and empty list should be deleted.
                if (!filteredList.Any() && remove)
                {
                    dict.Remove(key);
                }
                else
                {
                    dict[key] = filteredList;
                }
            }
        }
    }
}
