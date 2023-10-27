using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json;
using UserProfileService.IntegrationTests.Configuration;
using UserProfileService.IntegrationTests.Constants;

namespace UserProfileService.IntegrationTests.Fixtures
{
    public class ControllerFixture : IDisposable
    {
        public IDictionary<string, object> TestData = new Dictionary<string, object>();

        public HttpClient Client { get; }

        public IMapper Mapper { get; }

        public ControllerFixture()
        {
            Client = GetClient();
            Mapper = GetMapper();
        }

        private HttpClient GetClient()
        {
            TestConfiguration config = LoadConfig();

            return new HttpClient
            {
                BaseAddress = config.BaseUrl
            };
        }

        private IMapper GetMapper()
        {
            var config = new MapperConfiguration(t => { t.CreateMap<CreateUserRequest, User>(); });

            return config.CreateMapper();
        }

        protected TestConfiguration LoadConfig()
        {
            string path = Path.Join(Directory.GetCurrentDirectory(), WellKnownFiles.TestSettingsFile);

            return JsonConvert.DeserializeObject<TestConfiguration>(File.ReadAllText(path));
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}
