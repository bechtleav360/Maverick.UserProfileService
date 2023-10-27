using System.Net.Http;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Models;
using Xunit;

namespace UserProfileService.IntegrationTests.Extensions
{
    internal static class HttpClientExtension
    {
        internal static async Task<User> GetUser(this HttpClient client, string userId)
        {
            HttpResponseMessage response = await client.GetAsync($"users/{userId}");

            response.EnsureSuccessStatusCode();

            (bool success, User user) = await response.TryParseContent<User>();

            Assert.True(success);

            return user;
        }
    }
}
