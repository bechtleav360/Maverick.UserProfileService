using System.Collections.Generic;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     The default json setting, when no json setting are set.
/// </summary>
public class DefaultJsonSettingsProvider : IJsonSerializerSettingsProvider
{
    private JsonConverter GetIProfileConverter()
    {
        JsonConverter jsonConverter = JsonSubtypesConverterBuilder
            .Of<IProfile>(nameof(IProfile.Kind))
            .RegisterSubtype<UserBasic>(ProfileKind.User)
            .RegisterSubtype<GroupBasic>(ProfileKind.Group)
            .RegisterSubtype<OrganizationBasic>(ProfileKind.Organization)
            .Build();

        return jsonConverter;
    }

    /// <inheritdoc />
    public JsonSerializerSettings GetNewtonsoftSettings()
    {
        return new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter(),
                GetIProfileConverter()
            },
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Formatting = Formatting.None,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new DefaultNamingStrategy()
            }
        };
    }
}
