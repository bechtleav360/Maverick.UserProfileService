using System.Globalization;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Common.V2.Abstractions;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Provides JSON serializer settings for entity views.
/// </summary>
public class EntityViewJsonSerializerSettingsProvider : IJsonSerializerSettingsProvider
{
    private static JsonConverter[] ViewConverters =>
        new[]
        {
            JsonSubtypesConverterBuilder
                .Of<IProfile>(nameof(IProfile.Kind))
                .RegisterSubtype<UserView>(ProfileKind.User)
                .RegisterSubtype<GroupView>(ProfileKind.Group)
                .RegisterSubtype<OrganizationView>(ProfileKind.Organization)
                .Build(),
            JsonSubtypesConverterBuilder
                .Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                .RegisterSubtype<GroupView>(ProfileKind.Group)
                .RegisterSubtype<OrganizationView>(ProfileKind.Organization)
                .Build(),
            JsonSubtypesConverterBuilder
                .Of<IAssignmentObject>(nameof(IAssignmentObject.Type))
                .RegisterSubtype<FunctionView>(RoleType.Function)
                .RegisterSubtype<RoleView>(RoleType.Role)
                .Build(),
            JsonSubtypesConverterBuilder
                .Of<ILinkedObject>(nameof(ILinkedObject.Type))
                .RegisterSubtype<LinkedRoleObject>(RoleType.Role.ToString())
                .RegisterSubtype<LinkedFunctionObject>(RoleType.Function.ToString())
                .Build()
        };

    /// <summary>
    ///     Returns the <see cref="JsonSerializerSettings"/> for Newtonsoft.Json,
    ///     configured for use with entity views.
    /// </summary>
    /// <returns>The configured <see cref="JsonSerializerSettings"/>.</returns>
    public JsonSerializerSettings GetNewtonsoftSettings()
    {
        return new JsonSerializerSettings
        {
            Converters = ViewConverters,
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new DefaultContractResolver(),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = CultureInfo.InvariantCulture
        };
    }
}
