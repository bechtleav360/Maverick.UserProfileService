using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Events.Payloads.V2;
using FunctionCreatedPayload = UserProfileService.Events.Payloads.V3.FunctionCreatedPayload;

namespace UserProfileService.Saga.Worker.Profiles;

/// <summary>
///     Class for Automapper profiles
/// </summary>
// ReSharper disable once UnusedType.Global => The assembly is needed to register the mappings.
public class MappingProfiles : Profile
{
    /// <summary>
    ///     Create an instance of <see cref="MappingProfiles" /> to initialize Automapper profiles
    /// </summary>
    public MappingProfiles()
    {
        CreateMap<RoleCreatedPayload, RoleBasic>().ReverseMap();
        CreateMap<RoleCreatedPayload, CreateRoleRequest>().ReverseMap();
        CreateMap<UserCreatedPayload, CreateUserRequest>().ReverseMap();
        CreateMap<RoleCreatedPayload, CreateRoleRequest>().ReverseMap();
        CreateMap<FunctionCreatedPayload, CreateFunctionRequest>().ReverseMap();
        CreateMap<GroupCreatedPayload, CreateGroupRequest>().ReverseMap();
        CreateMap<OrganizationCreatedPayload, CreateOrganizationRequest>().ReverseMap();
        CreateMap<TagCreatedPayload, CreateTagRequest>().ReverseMap();
    }
}
