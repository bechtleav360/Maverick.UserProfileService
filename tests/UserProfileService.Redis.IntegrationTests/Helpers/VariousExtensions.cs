using System.Collections.Generic;

namespace UserProfileService.Redis.IntegrationTests.Helpers
{
    public static class VariousExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this T element)
        {
            return new[] { element };
        }
    }
}
