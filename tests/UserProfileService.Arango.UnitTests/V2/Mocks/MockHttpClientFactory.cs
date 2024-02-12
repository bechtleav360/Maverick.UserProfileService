using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace UserProfileService.Arango.UnitTests.V2.Mocks
{
    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly Func<HttpClient> _httpClientCreation;
        private readonly string _namePattern;
        private readonly ITestOutputHelper _output;

        public MockHttpClientFactory(
            string namePattern,
            Func<HttpClient> httpClientCreation,
            ITestOutputHelper output = null)
        {
            _httpClientCreation = httpClientCreation;
            _output = output;
            _namePattern = namePattern ?? ".*";
        }

        /// <inheritdoc />
        public HttpClient CreateClient(string name)
        {
            if (name == null)
            {
                _output?.WriteLine($"EXCEPTION: ArgumentNullException: Parameter: {nameof(name)}");

                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                _output?.WriteLine(
                    $"EXCEPTION: ArgumentException: Parameter: {nameof(name)}: Value cannot be null or whitespace.");

                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            if (!Regex.IsMatch(name, _namePattern))
            {
                _output?.WriteLine(
                    $"EXCEPTION: ArgumentException: Parameter: {nameof(name)}: Client name is in a wrong format. Current value: '{name}'; expected pattern: {_namePattern}");

                throw new ArgumentException(
                    $"Client name is in a wrong format. Current value: '{name}'; expected pattern: {_namePattern}",
                    nameof(name));
            }

            return _httpClientCreation.Invoke();
        }
    }
}
