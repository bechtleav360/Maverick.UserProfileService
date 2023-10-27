using System;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Converter converts an string from base64 to <see cref="ReadOnlyMemory{T}" /> objects
///     and vice versa.
/// </summary>
public class EventLogTupleReadOnlyMemoryJsonConverter : JsonConverter
{
    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();

            return;
        }

        var dataAsByteReadOnlyMemory = (ReadOnlyMemory<byte>)value;
        byte[] byteAsArray = dataAsByteReadOnlyMemory.Span.ToArray();
        writer.WriteValue(byteAsArray);
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            return new ReadOnlyMemory<byte>(Array.Empty<byte>());
        }

        try
        {
            var objectToken = (string)reader.Value;

            return new ReadOnlyMemory<byte>(Convert.FromBase64String(objectToken));
        }
        catch (Exception)
        {
            return new ReadOnlyMemory<byte>(Array.Empty<byte>());
        }
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ReadOnlyMemory<byte>);
    }
}
