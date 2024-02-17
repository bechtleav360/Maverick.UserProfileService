using System.Collections.Generic;
using System;
using System.Linq;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Services.Comparer;

namespace UserProfileService.Sync.Extensions
{
    /// <summary>
    ///     Contains some extensions method for <see cref="SyncConfiguration"/>.
    /// </summary>
    public static class SyncConfigurationExtensions
    {
        
        /// <summary>
        ///     Get all configured systems sorted by priority (if the priority values are not set, the systems will sorted alphabetically in descending order).
        /// </summary>
        /// <param name="configuration">The configuration of the UPS-Sync <see cref="SyncConfiguration"/></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">will be thrown when <see cref="SyncConfiguration"/> is null.</exception>
        /// <exception cref="ArgumentException">will be thrown when <see cref="SourceConfiguration"/> is null inside the <see cref="SyncConfiguration"/>.</exception>
        public static Dictionary<string, SourceSystemConfiguration> GetSystemSorted(
            this SyncConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration.SourceConfiguration == null)
            {
                throw new ArgumentException("The source configuration should not be null");
            }

            return configuration.SourceConfiguration.Systems.OrderByDescending(s => s, new SyncSystemComparer())
                                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
