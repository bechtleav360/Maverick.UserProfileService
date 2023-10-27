using System;
using System.Collections.Generic;
using System.Linq;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Defines a JSON converter for Newtonsoft to deserialize JSON strings of expected type of IList&gt;
///     <see cref="IProfile" />&lt;.
/// </summary>
public class GroupMembersJsonConverter : JsonConverter
{
    /// <inheritdoc cref="JsonConverter" />
    public override bool CanRead => true;

    /// <inheritdoc cref="JsonConverter" />
    public override bool CanWrite => false;

    private static JsonSerializerSettings ExtractFrom(JsonSerializer serializer)
    {
        return new JsonSerializerSettings
        {
            CheckAdditionalContent = serializer.CheckAdditionalContent,
            ConstructorHandling = serializer.ConstructorHandling,
            ContractResolver = serializer.ContractResolver,
            DateFormatHandling = serializer.DateFormatHandling,
            Culture = serializer.Culture,
            DateFormatString = serializer.DateFormatString,
            DateParseHandling = serializer.DateParseHandling,
            DateTimeZoneHandling = serializer.DateTimeZoneHandling,
            DefaultValueHandling = serializer.DefaultValueHandling,
            EqualityComparer = serializer.EqualityComparer,
            FloatFormatHandling = serializer.FloatFormatHandling,
            Formatting = serializer.Formatting,
            FloatParseHandling = serializer.FloatParseHandling,
            MaxDepth = serializer.MaxDepth,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            MissingMemberHandling = serializer.MissingMemberHandling,
            NullValueHandling = serializer.NullValueHandling,
            ObjectCreationHandling = serializer.ObjectCreationHandling,
            PreserveReferencesHandling = serializer.PreserveReferencesHandling,
            ReferenceLoopHandling = serializer.ReferenceLoopHandling,
            SerializationBinder = serializer.SerializationBinder,
            StringEscapeHandling = serializer.StringEscapeHandling,
            TypeNameAssemblyFormatHandling =
                serializer.TypeNameAssemblyFormatHandling,
            TypeNameHandling = serializer.TypeNameHandling,
            TraceWriter = serializer.TraceWriter,
            Converters = serializer.Converters
                .Where(c => c is not GroupMembersJsonConverter)
                .Concat(
                    new[]
                    {
                        JsonSubtypesConverterBuilder
                            .Of<IProfile>(nameof(IProfile.Kind))
                            .RegisterSubtype<UserBasic>(ProfileKind.User)
                            .RegisterSubtype<GroupBasic>(ProfileKind.Group)
                            .RegisterSubtype<OrganizationBasic>(ProfileKind.Organization)
                            .Build()
                    })
                .ToArray()
        };
    }

    private static JsonSerializer Copy(JsonSerializer old)
    {
        return JsonSerializer.CreateDefault(ExtractFrom(old));
    }

    /// <inheritdoc cref="JsonConverter" />
    public override void WriteJson(
        JsonWriter writer,
        object value,
        JsonSerializer serializer)
    {
        throw new NotSupportedException("This converter supports only reading json strings.");
    }

    /// <inheritdoc cref="JsonConverter" />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        JsonSerializer temporarySerializer = Copy(serializer);

        JToken jToken = JToken.Load(
            reader,
            new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore,
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
                LineInfoHandling = LineInfoHandling.Ignore
            });

        var listObject = jToken.ToObject<IList<IProfile>>(temporarySerializer);

        return listObject;
    }

    /// <inheritdoc cref="JsonConverter" />
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IList<IProfile>);
    }
}
