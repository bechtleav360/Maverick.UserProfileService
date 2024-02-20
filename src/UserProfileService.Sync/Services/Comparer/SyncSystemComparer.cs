using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Configurations;

namespace UserProfileService.Sync.Services.Comparer
{
    /// <summary>
    /// <inheritdoc cref="IComparer{T}"/> 
    /// </summary>
    /// <remarks>
    ///     This method is used to compare source system configuration using the priority value.
    ///     If the priority values are not set, the systems will sorted alphabetically.
    /// </remarks>
    public class SyncSystemComparer : IComparer<KeyValuePair<string, SourceSystemConfiguration>>
    {
        /// <inheritdoc />
        public int Compare(
            KeyValuePair<string, SourceSystemConfiguration> x,
            KeyValuePair<string, SourceSystemConfiguration> y)
        {
            if (x.Value.Priority == y.Value.Priority)
            {
                return string.CompareOrdinal(x.Key, y.Key);
            }

            return x.Value.Priority > y.Value.Priority ? 1 : -1;
        }
    }
}
