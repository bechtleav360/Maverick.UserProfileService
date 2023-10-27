using System.Collections.Generic;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Newtonsoft.Json;

namespace UserProfileService.Common.V2.Utilities;

/// <summary>
///     Class to build a list of json converters for different services.
/// </summary>
public class DefaultJsonConverterBuilder
{
    private readonly IList<JsonConverter> _Converters;

    public DefaultJsonConverterBuilder()
    {
        _Converters = new List<JsonConverter>();
    }

    /// <summary>
    ///     Add the default <see cref="JsonConverter" /> for <see cref="IProfile" />.
    /// </summary>
    /// <returns>
    ///     <see cref="DefaultJsonConverterBuilder" />
    /// </returns>
    public DefaultJsonConverterBuilder WithIProfileJsonConverter()
    {
        JsonConverter jsonConverter = JsonSubtypesConverterBuilder
            .Of<IProfile>(nameof(IProfile.Kind))
            .RegisterSubtype<UserBasic>(ProfileKind.User)
            .RegisterSubtype<GroupBasic>(ProfileKind.Group)
            .RegisterSubtype<OrganizationBasic>(ProfileKind.Organization)
            .Build();

        _Converters.Add(jsonConverter);

        return this;
    }

    /// <summary>
    ///     Return all converters.
    /// </summary>
    /// <returns>A collection fo <see cref="JsonConverter" />.</returns>
    public IList<JsonConverter> Build()
    {
        return _Converters;
    }
}
