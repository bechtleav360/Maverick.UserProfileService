using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable 612

namespace UserProfileService.Utilities;

internal static class JsonUtilities
{
    internal static JsonConverter[] DefaultConverters { get; } =
    {
        JsonSubtypesConverterBuilder.Of<IAssignmentObject>(nameof(IAssignmentObject.Type))
            .RegisterSubtype<RoleBasic>(RoleType.Role)
            .RegisterSubtype<FunctionBasic>(RoleType.Function)
            .Build(),
        JsonSubtypesConverterBuilder.Of<IProfile>(nameof(IProfile.Kind))
            .RegisterSubtype<User>(ProfileKind.User)
            .RegisterSubtype<Group>(ProfileKind.Group)
            .Build()
    };

    internal static void AddDefaultConverters(this JsonSerializerSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(
                nameof(settings),
                $"Parameter settings of type {nameof(JsonSerializerSettings)} must not be null.");
        }

        foreach (JsonConverter converter in DefaultConverters)
        {
            settings.Converters.Add(converter);
        }

        settings.Converters.Add(new StringEnumConverter());
    }
}
