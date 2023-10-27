using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using UserProfileService.IntegrationTests.Configuration;
using UserProfileService.IntegrationTests.Constants;

namespace UserProfileService.IntegrationTests.Abstractions
{
    public class ControllerTestBase
    {
        protected readonly HttpClient Client;

        public ControllerTestBase()
        {
            Client = GetClient();
        }

        protected TestConfiguration LoadConfig()
        {
            string path = Path.Join(Directory.GetCurrentDirectory(), WellKnownFiles.TestSettingsFile);

            return JsonConvert.DeserializeObject<TestConfiguration>(File.ReadAllText(path));
        }

        public HttpClient GetClient()
        {
            TestConfiguration config = LoadConfig();

            return new HttpClient
            {
                BaseAddress = config.BaseUrl
            };
        }
    }
}
