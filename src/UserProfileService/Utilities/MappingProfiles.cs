using AutoMapper;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Maverick.UserProfileService.Models.ResponseModels;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Saga.Events.Messages;

#pragma warning disable 612

namespace UserProfileService.Utilities;

/// <summary>
///     Mapping Configuration for RestAPI Service
/// </summary>
public class MappingProfiles : Profile
{
    private const string Source = Constants.Source.Api;

    public MappingProfiles()
    {
        CreateMap<IProfile, Member>();

        CreateMap(typeof(IList<>), typeof(ListResponse), MemberList.None)
            .ForMember(
                nameof(ListResponse.Count),
                opt => opt
                    .MapFrom(a => (a as List<object>).Count));

        CreateMap<QueryObjectTags, QueryObject>();

        CreateMap<CreateUserRequest, UserCreatedMessage>()
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));

        CreateMap<CreateGroupRequest, GroupCreatedMessage>()
            .ForMember(
                m => m.Members,
                m => m.MapFrom(
                    t =>
                        t.Members.Select(
                                member =>
                                    new ConditionObjectIdent(member.Id, ObjectType.Profile, member.Conditions))
                            .ToList()))
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));

        CreateMap<CreateRoleRequest, RoleCreatedMessage>()
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));

        CreateMap<CreateTagRequest, TagCreatedMessage>()
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));

        CreateMap<CreateOrganizationRequest, OrganizationCreatedMessage>()
            .ForMember(
                m => m.Members,
                m => m.MapFrom(
                    t =>
                        t.Members.Select(
                                member =>
                                    new ConditionObjectIdent(member.Id, ObjectType.Profile, member.Conditions))
                            .ToList()))
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));

        CreateMap<CreateFunctionRequest, FunctionCreatedMessage>()
            .ForMember(m => m.Source, m => m.MapFrom(t => Source));
    }
}
