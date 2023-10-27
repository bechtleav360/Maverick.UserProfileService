using System;
using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Arango.IntegrationTests.V2.Extensions
{
    internal static class ArangoSpecificExtensions
    {
        internal static JObject GetJsonObjectWithInjectedKey<TDocument>(
            this TDocument document,
            JsonSerializer jsonSerializer,
            Func<TDocument, string> keyProjection = null)
        {
            if (keyProjection == null)
            {
                return JObject.FromObject(document, jsonSerializer);
            }

            JObject o = MergeDocument(
                document,
                new Dictionary<string, string>
                {
                    { AConstants.KeySystemProperty, keyProjection.Invoke(document) }
                },
                AConstants.IdSystemProperty,
                jsonSerializer);

            return o;
        }

        internal static JObject MergeDocument(
            object firstDocument,
            object secondDocument,
            string ignoredPropertyName,
            JsonSerializer jsonSerializer)
        {
            JObject first = JObject.FromObject(firstDocument, jsonSerializer);
            JObject second = JObject.FromObject(secondDocument, jsonSerializer);

            first.Merge(
                second,
                new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union,
                    MergeNullValueHandling = MergeNullValueHandling.Ignore
                });

            first.Remove(ignoredPropertyName);

            return first;
        }
    }
}
