using System.Collections;
using System.Collections.Generic;
using UserProfileService.Redis.Configuration;
using UserProfileService.Redis.Validation;

namespace UserProfileService.Redis.UnitTests.Validation
{
    /// <summary>
    ///     Contains some test data to test the class <see cref="RedisConfigurationValidation" />
    /// </summary>
    public class RedisConfigurationTestData : IEnumerable<object[]>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 5000,
                    EndpointUrls = new List<string>
                    {
                        "172.16.10.50:6379"
                    },
                    ExpirationTime = 7200,
                    Password = null,
                    User = null
                },
                true
            };

            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 5000,
                    EndpointUrls = null,
                    ExpirationTime = 7200,
                    Password = null,
                    User = null
                },
                false
            };

            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 5000,
                    EndpointUrls = new List<string>(),
                    ExpirationTime = 7200,
                    Password = null,
                    User = null
                },
                false
            };

            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = -5,
                    ConnectTimeout = 5000,
                    EndpointUrls = new List<string>(),
                    ExpirationTime = 7200,
                    Password = null,
                    User = null
                },
                false
            };

            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = 5,
                    ConnectTimeout = -5000,
                    EndpointUrls = new List<string>(),
                    ExpirationTime = 7200,
                    Password = null,
                    User = null
                },
                false
            };

            yield return new object[]
            {
                new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    AllowAdmin = true,
                    ConnectRetry = 5,
                    ConnectTimeout = 5000,
                    EndpointUrls = new List<string>(),
                    ExpirationTime = -7200,
                    Password = null,
                    User = null
                },
                false
            };
        }
    }
}
