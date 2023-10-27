using System.Collections.Generic;

namespace UserProfileService.IntegrationTests.Extensions
{
    internal static class DictionaryExtension
    {
        public static T Get<T>(this IDictionary<string, object> dict, string key)
        {
            return (T)dict[key];
        }
    }
}
