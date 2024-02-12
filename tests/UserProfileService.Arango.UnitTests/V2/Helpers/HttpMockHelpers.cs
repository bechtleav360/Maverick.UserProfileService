using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace UserProfileService.Arango.UnitTests.V2.Helpers
{
    internal static class HttpMockHelpers
    {
        internal static Mock<HttpMessageHandler> GetHttpMessageHandlerMock(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> messagesToBeReturned)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                // prepare the expected response of the mocked http call
                .Returns((HttpRequestMessage req, CancellationToken cancel) => messagesToBeReturned(req))
                .Verifiable();

            //handlerMock.Protected().Setup("Dispose", true, It.IsAny<bool>());

            return handlerMock;
        }

        internal static Mock<HttpMessageHandler> GetHttpMessageHandlerMock(
            Func<HttpRequestMessage, HttpResponseMessage> messagesToBeReturned)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);

            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                // prepare the expected response of the mocked http call
                .ReturnsAsync((HttpRequestMessage req, CancellationToken cancel) => messagesToBeReturned(req))
                .Verifiable();

            return handlerMock;
        }

        internal static void Verify(
            this Mock<HttpMessageHandler> mockedHandler,
            string baseUri,
            string relativeUri,
            HttpMethod method,
            ITestOutputHelper output)
        {
            Verify(mockedHandler, new Uri(new Uri(baseUri), relativeUri), method, output);
        }

        internal static void Verify(
            this Mock<HttpMessageHandler> mockedHandler,
            Uri expectedUri,
            HttpMethod method,
            ITestOutputHelper output)
        {
            try
            {
                mockedHandler
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Exactly(1),
                        ItExpr.Is<HttpRequestMessage>(
                            req
                                => (method == null || req.Method == method)
                                && (expectedUri == null || CompareRequestUris(req.RequestUri, expectedUri))),
                        ItExpr.IsAny<CancellationToken>());
            }
            catch (MockException)
            {
                if (mockedHandler.Invocations?.Any() == true)
                {
                    output?.WriteLine($"Used http message: {mockedHandler.Invocations[0]}");
                }

                throw;
            }
        }

        internal static void VerifyBody<TBody>(
            Mock<HttpMessageHandler> mockedHandler,
            Func<TBody, bool> isValidFunction,
            ITestOutputHelper output)
        {
            try
            {
                mockedHandler
                    .Protected()
                    .Verify(
                        "SendAsync",
                        Times.Exactly(1),
                        ItExpr.Is<HttpRequestMessage>(
                            req
                                => isValidFunction(GetObjectFromJsonContent<TBody>(req.Content).Result)),
                        ItExpr.IsAny<CancellationToken>());
            }
            catch (MockException)
            {
                if (mockedHandler.Invocations?.Any() == true)
                {
                    output?.WriteLine($"Used http message: {mockedHandler.Invocations[0]}");
                }

                throw;
            }
        }

        internal static HttpResponseMessage GetRawStringMessage(
            this string messageText,
            HttpStatusCode statusCode,
            string mediaType = "text/plain")
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(messageText, Encoding.UTF8, mediaType)
            };
        }

        internal static HttpResponseMessage GetJsonContent(
            this object obj,
            HttpStatusCode statusCode,
            JsonSerializerSettings serializerSettings = null)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.None, serializerSettings);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        private static bool CompareRequestUris(Uri first, Uri second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            if (first == second)
            {
                return true;
            }

            if (first.Scheme != second.Scheme || first.Authority != second.Authority)
            {
                return false;
            }

            NameValueCollection firstQueryP = HttpUtility.ParseQueryString(first.Query);
            NameValueCollection secondQueryP = HttpUtility.ParseQueryString(second.Query);

            if (firstQueryP.Count != secondQueryP.Count)
            {
                return false;
            }

            foreach (string key in firstQueryP.AllKeys)
            {
                if (firstQueryP[key] != secondQueryP[key]
                    && !(bool.TryParse(firstQueryP[key], out bool firstBool)
                        && bool.TryParse(secondQueryP[key], out bool secBool)
                        && firstBool == secBool))
                {
                    return false;
                }
            }

            foreach (string key in secondQueryP.AllKeys)
            {
                if (firstQueryP[key] != secondQueryP[key]
                    && !(bool.TryParse(firstQueryP[key], out bool firstBool)
                        && bool.TryParse(secondQueryP[key], out bool secBool)
                        && firstBool == secBool))
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task<TContent> GetObjectFromJsonContent<TContent>(HttpContent content)
        {
            string json = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<TContent>(json);
        }
    }
}
