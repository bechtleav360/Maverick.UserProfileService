using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Arango.Tests.V2.Helpers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UserProfileService.Arango.Tests.V2.Mocks
{
    internal class MockedArangoDb
    {
        private const string DefaultEmptyResult =
            "{\"hasMore\":false,\"error\":false,\"result\":[],\"code\":201,\"count\":0}";

        private readonly
            Dictionary<string, Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>>> _mapping
                = new Dictionary<string, Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                >(StringComparer.OrdinalIgnoreCase);

        private readonly ITestOutputHelper _output;

        private readonly Action<string> _queryCheck;

        private readonly Func<string, bool> _queryValidation;

        private readonly Action<XunitException> _storeExceptionFunction;

        private int _queryCount;

        private string _resultItems;

        private Func<string, IEnumerable<object>> _resultItemsGenerator;

        internal bool UseAlwaysSetResponse { get; set; }

        internal List<ActionsHistoryItem> History { get; } = new List<ActionsHistoryItem>();

        private MockedArangoDb()
        {
            When("/_api/collection", HttpMethod.Get, GetAllCollectionsAsync);
            When("/_api/collection", HttpMethod.Post, CreateCollectionAsync);
            When("/_api/cursor", HttpMethod.Post, CreateCursorAsync);
            When("/_api/transaction/begin", HttpMethod.Post, CreateTransactionAsync);
            When("/_api/transaction", HttpMethod.Put, CommitTransactionAsync);
            When("/_api/transaction", HttpMethod.Get, GetTransactionStatusAsync);

            _resultItems = "[]";
            _queryCount = 0;
        }

        public MockedArangoDb(
            ITestOutputHelper output,
            Action<string> queryCheck,
            Action<XunitException> storeExceptionFunction = null) : this()
        {
            _output = output;
            _queryCheck = queryCheck;
            _storeExceptionFunction = storeExceptionFunction;
        }

        public MockedArangoDb(
            ITestOutputHelper output,
            Func<string, bool> queryValidation) : this()
        {
            _output = output;
            _queryValidation = queryValidation;
        }

        internal async Task<HttpResponseMessage> HandleMessage(HttpRequestMessage message)
        {
            int cutMe = message.RequestUri.AbsolutePath.IndexOf("/_api/", StringComparison.OrdinalIgnoreCase);
            int cutMeTill = message.RequestUri.AbsolutePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            string uriStringPath = message.RequestUri.AbsolutePath.Substring(cutMe);
            string alternativePath = message.RequestUri.AbsolutePath.Substring(cutMe, cutMeTill - cutMe);

            if (!_mapping.ContainsKey(uriStringPath) && !_mapping.ContainsKey(alternativePath))
            {
                _output?.WriteLine(
                    $"EXCEPTION: Path '{uriStringPath}' not mapped (in testing class 'MockedArangoDb').");

                throw new NotSupportedException(
                    $"Path '{uriStringPath}' not mapped (in testing class 'MockedArangoDb').");
            }

            Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>> checkup =
                _mapping.TryGetValue(
                    uriStringPath,
                    out Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>> dictionary)
                    ? dictionary
                    : _mapping.TryGetValue(
                        alternativePath,
                        out Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>>
                            alternativeDictionary)
                        ? alternativeDictionary
                        : null;

            if (checkup?.ContainsKey(message.Method) != true)
            {
                _output?.WriteLine(
                    $"EXCEPTION: Method '{message.Method}' not mapped for Path: '{uriStringPath}' (in testing class 'MockedArangoDb').");

                throw new NotSupportedException(
                    $"Method '{message.Method}' not mapped for Path: '{uriStringPath}' (in testing class 'MockedArangoDb').");
            }

            return await checkup[message.Method].Invoke(message);
        }

        internal static string GetDefaultQueryResponseJson<TElem>(IEnumerable<TElem> resultSet)
        {
            List<TElem> referenceResults = resultSet as List<TElem> ?? resultSet.ToList();

            return GetDefaultQueryResponseJson(
                JsonConvert.SerializeObject(referenceResults, Formatting.None),
                referenceResults.Count);
        }

        private Task<HttpResponseMessage> GetAllCollectionsAsync(HttpRequestMessage message)
        {
            return
                Task.FromResult(
                    "{\"error\":false,\"code\":200,\"result\":[{\"id\":\"43\",\"name\":\"Users\",\"status\":3,\"type\":2,\"isSystem\":false,\"globallyUniqueId\":\"Users\"},{\"id\":\"40\",\"name\":\"groups\",\"status\":3,\"type\":2,\"isSystem\":false,\"globallyUniqueId\":\"groups\"}]}"
                        .GetRawStringMessage(HttpStatusCode.OK, "application/json"));
        }

        private async Task<HttpResponseMessage> CreateCollectionAsync(HttpRequestMessage message)
        {
            try
            {
                string json = await message.Content.ReadAsStringAsync();
                JObject jObj = JObject.Parse(json);
                var dbName = jObj.Property("name")?.Value.ToString();

                if (string.IsNullOrEmpty(dbName))
                {
                    throw new FormatException("Body has wrong format or does not contain mandatory property 'name'!");
                }

                var isSystem = jObj.Property("isSystem")?.Value.ToObject<bool>();

                if (isSystem != null && isSystem.Value)
                {
                    throw new UnauthorizedAccessException("Not allowed to create system collections!");
                }

                var type = jObj.Property("type")?.Value.ToObject<int>();

                var collType = (ACollectionType)(type ?? 2);

                History.Add(new ActionsHistoryItem("created", dbName, $"collection:{collType:G}"));

                return
                    $"{{\"error\":false,\"code\":200,\"writeConcern\":1,\"waitForSync\":false,\"type\":{collType:D},\"tempObjectId\":\"0\",\"id\":\"70183\",\"cacheEnabled\":false,\"isSmartChild\":false,\"objectId\":\"70182\",\"globallyUniqueId\":\"h5461D3D4A886/70183\",\"schema\":null,\"keyOptions\":{{\"allowUserKeys\":true,\"type\":\"traditional\",\"lastValue\":0}},\"isSystem\":false,\"isDisjoint\":false,\"name\":\"{dbName}\",\"statusString\":\"loaded\",\"status\":3}}"
                        .GetRawStringMessage(HttpStatusCode.Created);
            }
            catch (Exception exception)
            {
                _output.WriteLine(exception.ToString());

                throw;
            }
        }

        private async Task<HttpResponseMessage> GetTransactionStatusAsync(HttpRequestMessage message)
        {
            await Task.Yield();

            try
            {
                string path = message.RequestUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                string id = path.Substring(path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));

                return $"{{\"code\":200,\"error\":false,\"result\":{{\"id\":\"{id}\",\"status\":\"running\"}}}}"
                    .GetRawStringMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        private async Task<HttpResponseMessage> CommitTransactionAsync(HttpRequestMessage message)
        {
            await Task.Yield();

            try
            {
                string path = message.RequestUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
                string id = path.Substring(path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));

                return $"{{\"code\":200,\"error\":false,\"result\":{{\"id\":\"{id}\",\"status\":\"committed\"}}}}"
                    .GetRawStringMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        private async Task<HttpResponseMessage> CreateTransactionAsync(HttpRequestMessage message)
        {
            try
            {
                string json = await message.Content.ReadAsStringAsync();
                JObject jObj = JObject.Parse(json);
                var dbName = jObj.Property("collections")?.Value.ToString();

                if (string.IsNullOrEmpty(dbName))
                {
                    throw new FormatException("Body has wrong format or does not contain mandatory property 'name'!");
                }

                return "{\"code\":201,\"error\":false,\"result\":{\"id\":\"72365\",\"status\":\"running\"}}"
                    .GetRawStringMessage(HttpStatusCode.Created);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                throw;
            }
        }

        private async Task<HttpResponseMessage> CreateCursorAsync(HttpRequestMessage message)
        {
            string query = null;

            try
            {
                string json = await message.Content.ReadAsStringAsync();
                JObject jObj = JObject.Parse(json);
                query = jObj.Property("query")?.Value.ToString();

                if (string.IsNullOrEmpty(query))
                {
                    throw new FormatException("Body has wrong format or does not contain mandatory property 'query'!");
                }

                _queryCheck?.Invoke(query);

                if (_queryValidation != null && !_queryValidation.Invoke(query))
                {
                    throw new FormatException($"Query '{query}' not valid!");
                }
            }
            catch (XunitException xe)
            {
                if (_storeExceptionFunction != null)
                {
                    _storeExceptionFunction.Invoke(xe);

                    if (!UseAlwaysSetResponse)
                    {
                        return DefaultEmptyResult
                            .GetRawStringMessage(HttpStatusCode.Created, "application/json");
                    }
                }
                else
                {
                    _output.WriteLine(xe.ToString());

                    throw;
                }
            }
            catch (Exception exception)
            {
                _output.WriteLine(exception.ToString());

                throw;
            }

            string resultItemsJson = _resultItems;
            int queryCount = _queryCount;

            if (_resultItemsGenerator != null)
            {
                List<object> resultItems = _resultItemsGenerator.Invoke(query).ToList();
                queryCount = resultItems.Count;
                resultItemsJson = JsonConvert.SerializeObject(resultItems, Formatting.None);
            }

            string elements = GetDefaultQueryResponseJson(resultItemsJson, queryCount);

            return elements.GetRawStringMessage(HttpStatusCode.Created);
        }

        private static string GetDefaultQueryResponseJson(string resultItemsJson, int queryCount)
        {
            return $"{{\"result\":{resultItemsJson},\"hasMore\":false,\"id\":\"70574\",\"count\":{queryCount},"
                + "\"extra\":{\"stats\":{\"writesExecuted\":0,\"writesIgnored\":0,\"scannedFull\":5,"
                + "\"scannedIndex\":0,\"filtered\":0,\"httpRequests\":0,\"executionTime\":0.0004760260053444654,"
                + "\"peakMemoryUsage\":19719},\"warnings\":[]},\"cached\":false,\"error\":false,\"code\":201}";
        }

        private void When(
            string reqUri,
            HttpMethod method,
            Func<HttpRequestMessage, Task<HttpResponseMessage>> thenMethod)
        {
            if (!_mapping.ContainsKey(reqUri))
            {
                _mapping.Add(reqUri, new Dictionary<HttpMethod, Func<HttpRequestMessage, Task<HttpResponseMessage>>>());
            }

            if (_mapping[reqUri].ContainsKey(method))
            {
                _output.WriteLine($"EXCEPTION: <{reqUri}, {method}> already mapped!");

                throw new Exception($"<{reqUri}, {method}> already mapped!");
            }

            _mapping[reqUri].Add(method, thenMethod);
        }

        // query string to IEnumerable<object>
        public void SetupAqlResult(Func<string, IEnumerable<object>> resultItemsGenerator)
        {
            _resultItemsGenerator = resultItemsGenerator;
        }

        public void SetupAqlResult(Func<IEnumerable<object>> resultItemsGenerator)
        {
            _resultItemsGenerator = _ => resultItemsGenerator();
        }

        public void SetupAqlResult<TElem>(IEnumerable<TElem> elements)
        {
            TElem[] converted = elements.ToArray();
            _resultItems = JsonConvert.SerializeObject(converted, Formatting.None);
            _queryCount = converted.Length;
        }
    }
}
