using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Converter ignores the property of type <see cref="IUserProfileServiceEvent" />.
/// </summary>
public class EventLogIgnoreEventJsonConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotSupportedException(
            $"The write operation in the {nameof(EventLogIgnoreEventJsonConverter)} is not supported.");
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        return default;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IUserProfileServiceEvent);
    }
}
