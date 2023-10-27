using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace UserProfileService.IntegrationTests.Utilities
{
    internal static class ObjectExtensions
    {
        public static StringContent ToJsonContent(this object obj)
        {
            // Serialize our concrete class into a JSON String
            string stringPayload = JsonConvert.SerializeObject(obj);

            // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

            return httpContent;
        }
    }
}
