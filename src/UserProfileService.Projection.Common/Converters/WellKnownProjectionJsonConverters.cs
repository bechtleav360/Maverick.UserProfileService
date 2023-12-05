using System.Collections.Generic;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Projection.Common.Converters;

/// <summary>
///     Contains some well-known converters used by entity (de)-serialization
/// </summary>
public static class WellKnownProjectionJsonConverters
{
    /// <summary>
    ///     The default profile converters for the interface <see cref="IAssignmentObject" />
    ///     and the objects that are derived from <see cref="FunctionView" />, <see cref="RoleView" />
    ///     and <see cref="Organization" />.
    /// </summary>
    public static JsonConverter DefaultFunctionConverter =>
        JsonSubtypesConverterBuilder
            .Of<IAssignmentObject>(nameof(IAssignmentObject.Type))
            .RegisterSubtype<FunctionView>(RoleType.Function)
            .RegisterSubtype<RoleView>(RoleType.Role)
            .Build();

    /// <summary>
    ///     The default profile converters for the interface <see cref="IProfile" />
    ///     and the objects that are derived from <see cref="User" />, <see cref="Group" />
    ///     and <see cref="Organization" />.
    /// </summary>
    public static JsonConverter DefaultProfileConverter =>
        JsonSubtypesConverterBuilder.Of<IProfile>(nameof(IProfile.Kind))
            .RegisterSubtype<UserBasic>(ProfileKind.User.ToString())
            .RegisterSubtype<GroupBasic>(ProfileKind.Group.ToString())
            .RegisterSubtype<OrganizationBasic>(ProfileKind.Organization.ToString())
            .Build();

    /// <summary>
    ///     Get All well-known converters for entities.
    /// </summary>
    /// <returns> A list of <see cref="JsonConverter" /></returns>
    public static List<JsonConverter> GetAllEntitiesConverters()
    {
        return new List<JsonConverter>
        {
            DefaultFunctionConverter,
            DefaultProfileConverter
        };
    }
}
