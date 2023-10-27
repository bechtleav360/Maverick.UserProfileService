using System.Text.Json;
using System.Text.Json.Nodes;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Marten.EntityModels;

namespace UserProfileService.Adapter.Marten.Helpers;

/// <summary>
///     Mapping for the volatile store.
/// </summary>
public class UserSettingMapper : Profile
{
    /// <summary>
    ///     Creates an object of type <see cref="UserSettingMapper" />.
    /// </summary>
    public UserSettingMapper()
    {
        // The JsonObject mapping has to be explicitly defined, otherwise AutoMapper
        // don't know what to do.
        CreateMap<JsonObject, JsonObject>()
            .ConvertUsing(
                src => JsonNode.Parse(src.ToJsonString(new JsonSerializerOptions()), null, default)!
                    .AsObject());

        CreateMap<UserSettingSectionDbModel, UserSettingSection>().ReverseMap();
        CreateMap<UserSettingObjectDbModel, UserSettingObject>().ReverseMap();
    }
}
