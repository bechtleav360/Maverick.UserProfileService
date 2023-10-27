using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Projection.Common.Converters;

public class PropertiesChangedEventJsonConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanWrite => false;

    private static Type GetEntityType(ObjectType objectType)
    {
        return objectType switch
        {
            ObjectType.Unknown
                => throw new InvalidOperationException("Cannot get an entity type of an unknown object type."),
            ObjectType.Profile => throw new NotSupportedException(
                "Object type 'profile' is not mapped to an entity type. It should be something like 'User'."),
            ObjectType.Role => typeof(RoleBasic),
            ObjectType.Function => typeof(FunctionBasic),
            ObjectType.Group => typeof(GroupBasic),
            ObjectType.User => typeof(UserBasic),
            ObjectType.Organization => typeof(OrganizationBasic),
            ObjectType.Tag => typeof(Tag),
            _ => throw new ArgumentOutOfRangeException(
                nameof(objectType),
                objectType,
                "Unsupported object type!")
        };
    }

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
        JObject jObj = JObject.Load(reader);
        JToken type = jObj.Property(nameof(PropertiesChanged.ObjectType))?.Value;

        if (type == null)
        {
            throw new JsonException("The object type parameter could not be deserialized.");
        }

        var oType = Enum.Parse<ObjectType>(type.ToString(), true);

        if (!(jObj.Property(nameof(PropertiesChanged.Properties))?.Value is JObject propertyBag))
        {
            return jObj.ToObject<PropertiesChanged>(
                JsonSerializer.Create(serializer.GetSettings().RemoveConverter(this)));
        }

        Dictionary<string, object> innerDictionary = GetEntityType(oType)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(
                p =>
                {
                    bool found = propertyBag.TryGetValue(
                        p.Name,
                        StringComparison.OrdinalIgnoreCase,
                        out JToken value);

                    return new
                    {
                        PropertyName = p.Name,
                        Value = value?.ToObject(
                            p.PropertyType,
                            serializer),
                        Found = found
                    };
                })
            .Where(item => item.Found)
            .ToDictionary(item => item.PropertyName, item => item.Value);

        jObj.Remove(nameof(PropertiesChanged.Properties));

        var domainEvent =
            jObj.ToObject<PropertiesChanged>(JsonSerializer.Create(serializer.GetSettings().RemoveConverter(this)));

        if (domainEvent == null)
        {
            throw new JsonException(
                $"Cannot deserialize object to {nameof(PropertiesChanged)}. The inner converter returned null.");
        }

        domainEvent.Properties = innerDictionary;

        return domainEvent;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PropertiesChanged);
    }
}
