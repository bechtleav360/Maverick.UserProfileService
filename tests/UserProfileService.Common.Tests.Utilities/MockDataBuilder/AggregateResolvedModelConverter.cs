using System;
using System.Collections.Generic;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    public static class AggregateResolvedModelConverter
    {
        private static IMapper ModelMapper =>
            new Mapper(
                new MapperConfiguration(
                    opt =>
                    {
                        opt.CreateMap<UserBasic, UserCreated>()
                            .ReverseMap();

                        opt.CreateMap<ExternalIdentifier, AggregateModels.ExternalIdentifier>().ReverseMap();
                        opt.CreateMap<GroupBasic, GroupCreated>().ReverseMap();
                        opt.CreateMap<FunctionBasic, FunctionCreated>().ReverseMap();
                        opt.CreateMap<OrganizationBasic, OrganizationCreated>().ReverseMap();
                        opt.CreateMap<RoleBasic, RoleCreated>().ReverseMap();
                        opt.CreateMap<OrganizationBasic, ResolvedModels.Organization>();
                        opt.CreateMap<RoleBasic, ResolvedModels.Role>();
                        opt.CreateMap<Tag, TagCreated>().ReverseMap();

                        opt.CreateMap<AggregateModels.Tag, AggregateModels.TagAssignment>()
                            .ForPath(s => s.TagDetails.Id, s => s.MapFrom(t => t.Id))
                            .ForPath(s => s.TagDetails.Name, s => s.MapFrom(t => t.Name))
                            .ForPath(s => s.TagDetails.Type, s => s.MapFrom(t => t.Type))
                            .ReverseMap();

                        opt.CreateMap<ObjectType, Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType>()
                            .ReverseMap();

                        opt.CreateMap<SecondLevelProjectionUser, UserCreated>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionGroup, GroupCreated>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionOrganization, OrganizationCreated>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionRole, RoleCreated>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionFunction, FunctionCreated>().ReverseMap();
                        opt.CreateMap<AggregateModels.Tag, TagCreated>().ReverseMap();

                        opt.CreateMap<SecondLevelProjectionOrganization, ResolvedModels.Organization>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionRole, ResolvedModels.Role>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionGroup, ResolvedModels.Member>().ReverseMap();
                        opt.CreateMap<SecondLevelProjectionUser, ResolvedModels.Member>().ReverseMap();
                        opt.CreateMap<ResolvedModels.Function, SecondLevelProjectionFunction>().ReverseMap();
                        opt.CreateMap<ResolvedModels.Group, SecondLevelProjectionGroup>().ReverseMap();
                        opt.CreateMap<AggregateModels.RangeCondition, RangeCondition>().ReverseMap();
                    }));

        public static UserCreated CreateProfileCreated(UserBasic user)
        {
            return ModelMapper.Map<UserCreated>(user);
        }

        public static UserCreated CreateProfileCreated(SecondLevelProjectionUser user)
        {
            return ModelMapper.Map<UserCreated>(user);
        }

        public static GroupCreated CreateProfileCreated(GroupBasic group)
        {
            return ModelMapper.Map<GroupCreated>(group);
        }

        public static GroupCreated CreateProfileCreated(ResolvedModels.Group group)
        {
            return ModelMapper.Map<GroupCreated>(group);
        }

        public static GroupCreated CreateProfileCreated(SecondLevelProjectionGroup group)
        {
            return ModelMapper.Map<GroupCreated>(group);
        }

        public static FunctionCreated CreateFunctionCreated(ResolvedModels.Function function)
        {
            return ModelMapper.Map<FunctionCreated>(function);
        }

        public static FunctionCreated CreateFunctionCreated(SecondLevelProjectionFunction function)
        {
            return ModelMapper.Map<FunctionCreated>(function);
        }

        public static FunctionCreated CreateFunctionCreated(FunctionBasic function)
        {
            return ModelMapper.Map<FunctionCreated>(function);
        }

        public static RoleCreated CreateRoleCreated(ResolvedModels.Role role)
        {
            return ModelMapper.Map<RoleCreated>(role);
        }

        public static RoleCreated CreateRoleCreated(RoleBasic role)
        {
            return ModelMapper.Map<RoleCreated>(role);
        }

        public static RoleCreated CreateRoleCreated(SecondLevelProjectionRole role)
        {
            return ModelMapper.Map<RoleCreated>(role);
        }

        public static OrganizationCreated CreateOrganizationCreated(ResolvedModels.Organization organization)
        {
            return ModelMapper.Map<OrganizationCreated>(organization);
        }

        public static OrganizationCreated CreateOrganizationCreated(OrganizationBasic organization)
        {
            return ModelMapper.Map<OrganizationCreated>(organization);
        }

        public static OrganizationCreated CreateOrganizationCreated(SecondLevelProjectionOrganization organization)
        {
            return ModelMapper.Map<OrganizationCreated>(organization);
        }

        public static TagsAdded CreateTagsAdded(ObjectIdent obj, AggregateModels.Tag[] tags)
        {
            return new TagsAdded
            {
                Id = obj.Id,
                ObjectType =
                    ModelMapper.Map<Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType>(obj.Type),
                Tags = ModelMapper.Map<AggregateModels.TagAssignment[]>(tags)
            };
        }

        public static TagsRemoved CreateTagsRemoved(ObjectIdent obj, string[] tags)
        {
            return new TagsRemoved
            {
                Id = obj.Id,
                ObjectType =
                    ModelMapper.Map<Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType>(obj.Type),
                Tags = tags
            };
        }

        public static TagCreated CreateTagCreated(AggregateModels.Tag tag)
        {
            return ModelMapper.Map<TagCreated>(tag);
        }

        public static SecondLevelProjectionOrganization CreateOrganization(ResolvedModels.Organization organization)
        {
            return ModelMapper.Map<SecondLevelProjectionOrganization>(organization);
        }

        public static WasAssignedToGroup GenerateWasAssignedToGroup(
            SecondLevelProjectionGroup group,
            string profileId,
            AggregateModels.RangeCondition[] conditions)
        {
            return new WasAssignedToGroup
            {
                Target = ModelMapper.Map<ResolvedModels.Group>(group),
                Conditions = conditions,
                ProfileId = profileId
            };
        }

        public static WasAssignedToFunction GenerateWasAssignedToFunction(
            SecondLevelProjectionFunction function,
            string profileId,
            AggregateModels.RangeCondition[] conditions)
        {
            return new WasAssignedToFunction
            {
                Target = ModelMapper.Map<ResolvedModels.Function>(function),
                Conditions = conditions,
                ProfileId = profileId
            };
        }

        public static WasAssignedToOrganization GenerateWasAssignedToOrganization(
            SecondLevelProjectionOrganization group,
            string profileId,
            AggregateModels.RangeCondition[] conditions)
        {
            return new WasAssignedToOrganization
            {
                Target = ModelMapper.Map<ResolvedModels.Organization>(group),
                Conditions = conditions,
                ProfileId = profileId
            };
        }

        public static WasAssignedToRole GenerateWasAssignedToRole(
            SecondLevelProjectionRole role,
            string profileId,
            AggregateModels.RangeCondition[] conditions)
        {
            return new WasAssignedToRole
            {
                Target = ModelMapper.Map<ResolvedModels.Role>(role),
                Conditions = conditions,
                ProfileId = profileId
            };
        }

        public static MemberAdded CreateMemberAdded(
            ISecondLevelProjectionProfile member,
            ISecondLevelProjectionContainer parent)
        {
            return new MemberAdded
            {
                Member = ModelMapper.Map<ResolvedModels.Member>(member),
                ParentId = parent.Id,
                ParentType = parent.ContainerType,
                EventId = Guid.NewGuid().ToString()
            };
        }

        public static WasUnassignedFrom CreateWasUnassignedFrom(
            ISecondLevelProjectionProfile child,
            ISecondLevelProjectionContainer parent)
        {
            return new WasUnassignedFrom
            {
                ParentId = parent.Id,
                ChildId = child.Id,
                EventId = Guid.NewGuid().ToString(),
                ParentType = parent.ContainerType,
                Conditions = new List<AggregateModels.RangeCondition>
                {
                    new AggregateModels.RangeCondition
                    {
                        Start = DateTime.UtcNow,
                        End = DateTime.UtcNow.AddDays(2)
                    },

                    new AggregateModels.RangeCondition
                    {
                        Start = DateTime.UtcNow.AddDays(2),
                        End = DateTime.UtcNow.AddDays(12)
                    }
                }.ToArray()
            };
        }

        public static MemberRemoved CreateMemberRemoved(
            ISecondLevelProjectionProfile member,
            ISecondLevelProjectionContainer parent)
        {
            return new MemberRemoved
            {
                ParentType = parent.ContainerType,
                EventId = Guid.NewGuid().ToString(),
                MemberId = member.Id,
                MemberKind = member.Kind,
                ParentId = parent.Id,
                Conditions = new List<AggregateModels.RangeCondition>
                {
                    new AggregateModels.RangeCondition
                    {
                        Start = DateTime.UtcNow,
                        End = DateTime.UtcNow.AddDays(2)
                    },

                    new AggregateModels.RangeCondition
                    {
                        Start = DateTime.UtcNow.AddDays(2),
                        End = DateTime.UtcNow.AddDays(12)
                    }
                }
            };
        }

        public static MemberDeleted CreateMemberDeleted(
            ISecondLevelProjectionProfile member,
            ISecondLevelProjectionContainer parent)
        {
            return new MemberDeleted
            {
                EventId = Guid.NewGuid().ToString(),
                MemberId = member.Id,
                ContainerId = parent.Id
            };
        }
    }
}
