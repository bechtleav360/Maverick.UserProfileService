using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models
{
    internal class TreeNode
    {
        public string DisplayName { get; }
        public string Name { get; }
        public List<TreeNodeCondition> Parents { get; } = new List<TreeNodeCondition>();
        public string RegardingObjectId { get; }
        public ObjectType Type { get; }

        public TreeNode(Member member)
        {
            RegardingObjectId = member.Id;

            Type = member.Kind == ProfileKind.User
                ? ObjectType.User
                : member.Kind == ProfileKind.Group
                    ? ObjectType.Group
                    : member.Kind == ProfileKind.Organization
                        ? ObjectType.Organization
                        : ObjectType.Unknown;

            Name = member.Name;
            DisplayName = member.DisplayName;
        }

        public TreeNode(IProfileEntityModel profile)
        {
            RegardingObjectId = profile.Id;

            Type = profile.Kind == ProfileKind.User
                ? ObjectType.User
                : profile.Kind == ProfileKind.Group
                    ? ObjectType.Group
                    : profile.Kind == ProfileKind.Organization
                        ? ObjectType.Organization
                        : ObjectType.Unknown;

            Name = profile.Name;
            DisplayName = profile.DisplayName;
        }

        public TreeNode(ILinkedObject functionOrRole)
        {
            RegardingObjectId = functionOrRole.Id;

            Type = functionOrRole is LinkedFunctionObject
                ? ObjectType.Function
                : functionOrRole is LinkedRoleObject
                    ? ObjectType.Role
                    : throw new NotSupportedException();

            Name = functionOrRole.Name;
        }

        public override string ToString()
        {
            return $"{Name} ({Type:G})";
        }
    }
}
