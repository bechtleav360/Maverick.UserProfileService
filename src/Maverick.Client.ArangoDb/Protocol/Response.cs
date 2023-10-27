using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Maverick.Client.ArangoDb.Public;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Protocol;

internal class Response
{
    internal DebugInfo DebugInfo { get; set; }

    internal JsonSerializerSettings DefaultSerializerSettings { get; set; }

    internal Exception Exception { get; set; }

    internal bool IsSuccessStatusCode { get; set; }

    internal HttpRequestMessage RequestMessage { get; set; }

    internal string ResponseBodyAsString { get; set; }

    internal HttpResponseHeaders ResponseHeaders { get; set; }
    internal HttpStatusCode StatusCode { get; set; }

    internal T ParseBody<T>(ILogger logger = null)
    {
        if (string.IsNullOrEmpty(ResponseBodyAsString))
        {
            logger?.LogError("The response body is null or empty and can't be parsed");

            return default;
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)ResponseBodyAsString;
        }

        // just to be on the safe side -> a json converter will be added, that should not change the original instance of the default json serializer settings class
        JsonSerializerSettings newSettings = DefaultSerializerSettings != null
            ? new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultSerializerSettings.DefaultValueHandling,
                CheckAdditionalContent = DefaultSerializerSettings.CheckAdditionalContent,
                ConstructorHandling = DefaultSerializerSettings.ConstructorHandling,
                Context = DefaultSerializerSettings.Context,
                ContractResolver = DefaultSerializerSettings.ContractResolver,
                Culture = DefaultSerializerSettings.Culture,
                DateFormatHandling = DefaultSerializerSettings.DateFormatHandling,
                DateFormatString = DefaultSerializerSettings.DateFormatString,
                DateParseHandling = DefaultSerializerSettings.DateParseHandling,
                DateTimeZoneHandling = DefaultSerializerSettings.DateTimeZoneHandling,
                EqualityComparer = DefaultSerializerSettings.EqualityComparer,
                Error = DefaultSerializerSettings.Error,
                FloatFormatHandling = DefaultSerializerSettings.FloatFormatHandling,
                FloatParseHandling = DefaultSerializerSettings.FloatParseHandling,
                Formatting = DefaultSerializerSettings.Formatting,
                MaxDepth = DefaultSerializerSettings.MaxDepth,
                MetadataPropertyHandling = DefaultSerializerSettings.MetadataPropertyHandling,
                MissingMemberHandling = DefaultSerializerSettings.MissingMemberHandling,
                NullValueHandling = DefaultSerializerSettings.NullValueHandling,
                ObjectCreationHandling = DefaultSerializerSettings.ObjectCreationHandling,
                PreserveReferencesHandling = DefaultSerializerSettings.PreserveReferencesHandling,
                ReferenceLoopHandling = DefaultSerializerSettings.ReferenceLoopHandling,
                ReferenceResolverProvider = DefaultSerializerSettings.ReferenceResolverProvider,
                SerializationBinder = DefaultSerializerSettings.SerializationBinder,
                StringEscapeHandling = DefaultSerializerSettings.StringEscapeHandling,
                TraceWriter = DefaultSerializerSettings.TraceWriter,
                TypeNameAssemblyFormatHandling = DefaultSerializerSettings.TypeNameAssemblyFormatHandling,
                TypeNameHandling = DefaultSerializerSettings.TypeNameHandling,
                Converters = DefaultSerializerSettings.Converters.ToList() // the list instance should be a new one
            }
            : null;

        newSettings?.Converters.Add(new CursorResponseToStringJsonConverter());

        try
        {
            return newSettings == null
                ? JsonConvert.DeserializeObject<T>(ResponseBodyAsString, new CursorResponseToStringJsonConverter())
                : JsonConvert.DeserializeObject<T>(ResponseBodyAsString, newSettings);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error by deserializing JSON to {type}", typeof(T));
        }

        return default;
    }

    private class CursorResponseToStringJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        public override bool CanWrite { get; } = false;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JToken obj = JToken.Load(
                reader,
                new JsonLoadSettings
                {
                    CommentHandling = CommentHandling.Ignore,
                    LineInfoHandling = LineInfoHandling.Load,
                    DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace
                });

            serializer.Converters.Remove(this);
            serializer.Converters.Add(new ToStringJsonConverter());

            var conv = obj.ToObject(objectType, serializer);

            return conv;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType != null && typeof(ICursorInnerResponse<string>).IsAssignableFrom(objectType);
        }
    }

    private class ToStringJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        public override bool CanWrite { get; } = false;

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            JToken result = JToken.ReadFrom(reader);

            if (result.Type == JTokenType.String)
            {
                return result.Value<string>();
            }

            return result.ToString(Formatting.None);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType != null && typeof(string) == objectType;
        }
    }
}
