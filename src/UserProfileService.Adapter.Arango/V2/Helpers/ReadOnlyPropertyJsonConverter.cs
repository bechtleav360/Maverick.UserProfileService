using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Projection.Abstractions.Annotations;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Is used to convert instances of first- and second-level models that have readonly marked properties.
/// </summary>
internal class ReadOnlyPropertyJsonConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanRead => false;

    /// <inheritdoc />
    public override bool CanWrite => true;

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();

            return;
        }

        // this can be an issue, if the serializer is used more than one time
        // but in the cases, we are using it, is will not be the case.
        // without such a call, this method will be in an endless loop.
        serializer.Converters.Remove(this);

        Type objectType = value.GetType();

        JObject jObj = JObject.FromObject(value, serializer);

        foreach (JProperty jProperty in jObj.Properties().ToList())
        {
            PropertyInfo reflectionProperty = objectType.GetProperty(
                jProperty.Name,
                BindingFlags.Instance | BindingFlags.Public);

            // ignore all invalid properties (null check only for the worst case)
            if (reflectionProperty == null
                || reflectionProperty.GetCustomAttribute<ReadonlyAttribute>() != null)
            {
                jObj.Remove(jProperty.Name);
            }
        }

        jObj.WriteTo(writer);
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        throw new NotSupportedException("This method is not supported by this converter.");
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsClass && !objectType.IsGenericType && !objectType.IsArray && !objectType.IsCollectible;
    }
}
