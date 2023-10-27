using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using TagType = Maverick.UserProfileService.Models.EnumModels.TagType;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding
{
    internal static class SeedObjectHelpers
    {
        private static SeedObject CreateGroupObject(
            string groupId,
            GroupAttribute groupInfo)
        {
            GroupView newGroup = MockDataGenerator.GenerateGroupViewInstances().Single();
            newGroup.Id = groupId;

            if (!string.IsNullOrWhiteSpace(groupInfo.Name))
            {
                newGroup.Name = newGroup.DisplayName = groupInfo.Name;
            }

            return new SeedObject(groupId, newGroup);
        }

        private static SeedObject CreateUserObject(
            string userId,
            UserAttribute userInfo)
        {
            UserView newUser = MockDataGenerator.GenerateUserViewInstances().Single();
            newUser.Id = userId;

            if (!string.IsNullOrWhiteSpace(userInfo.Name))
            {
                newUser.Name = newUser.DisplayName = userInfo.Name;
            }

            return new SeedObject(userId, newUser);
        }

        private static SeedObject CreateTagObject(
            string tagId,
            TagAttribute tagInfo)
        {
            CalculatedTag tag = MockDataGenerator.GenerateCalculatedTags().Single();
            tag.Id = tagId;

            if (!string.IsNullOrWhiteSpace(tagInfo.Name))
            {
                tag.Name = tagInfo.Name;
            }

            tag.Type = tagInfo.TagType ?? TagType.Custom;

            return new SeedObject(tagId, tag);
        }

        private static SeedObject CreateOrganizationObject(
            string orgId,
            OrganizationAttribute organizationInfo)
        {
            Organization organization = MockDataGenerator.GenerateOrganizationInstances().Single();
            organization.Id = orgId;

            if (!string.IsNullOrWhiteSpace(organizationInfo.Name))
            {
                organization.Name = organization.Name;
            }

            return new SeedObject(orgId, organization);
        }

        private static SeedObject CreateRoleObject(
            string roleId,
            RoleAttribute roleInfo)
        {
            RoleView role = MockDataGenerator.GenerateRoleViewInstances().Single();
            role.Id = roleId;

            if (!string.IsNullOrWhiteSpace(roleInfo.Name))
            {
                role.Name = roleInfo.Name;
            }

            return new SeedObject(roleId, role);
        }

        private static SeedObject CreateFunctionObject(
            string funcId,
            FunctionAttribute funcInfo)
        {
            FunctionView function = MockDataGenerator.GenerateFunctionViewInstance(0, 0);
            function.Id = funcId;

            if (!string.IsNullOrWhiteSpace(funcInfo.Name))
            {
                function.Name = funcInfo.Name;
            }

            return new SeedObject(funcId, function);
        }

        private static SeedObject CreateExtendedProfileNodeObject(
            string objectId,
            ProfileVertexNodeAttribute profileVertexNodeInfo
        )
        {
            var profileNode = new ExtendedProfileVertexData
                              {
                                  RelatedProfileId = profileVertexNodeInfo.RelatedEntityId,
                                  ObjectId = objectId,
                                  Key = $"{profileVertexNodeInfo.RelatedEntityId}-{objectId}"
                              };

            return new SeedObject($"{profileVertexNodeInfo.RelatedEntityId}-{objectId}", profileNode);
        }
        
        private static SeedObject CreateExtendedProfileRootNode(
            string relatedEntityId
        )
        {
            var profileNode = new ExtendedProfileVertexData
                              {
                                  RelatedProfileId = relatedEntityId,
                                  ObjectId = relatedEntityId,
                                  Key = $"{relatedEntityId}-{relatedEntityId}"
                              };

            return new SeedObject($"{relatedEntityId}-{relatedEntityId}", profileNode);
        }

        private static SeedObject CreateExtendedProfileEdgeObject(
            int numberOfRangeConditions,
            ProfileVertexEdgeAttribute edgeInfo)
        {
            var profileNode = new ExtendedProfileEdgeData
                              {
                                  RelatedProfileId = edgeInfo.RelatedId,
                                  Conditions = MockDataGenerator.GenerateRangeConditionsAggregated(numberOfRangeConditions, 0),
                                  FromId = $"{edgeInfo.RelatedId}-{edgeInfo.RelatedId}",
                                  ToId = $"{edgeInfo.RelatedId}-{edgeInfo.ObjectId}"
                              };

            return new SeedObject($"{edgeInfo.RelatedId}-{edgeInfo.ObjectId}", profileNode);
        }
        
        private static TValue GetValue<TValue>(this MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo pInfo)
            {
                return (TValue)pInfo.GetValue(null);
            }

            if (memberInfo is FieldInfo fInfo)
            {
                return (TValue)fInfo.GetRawConstantValue();
            }

            throw new NotSupportedException();
        }

        private static bool TryGetCustomAttribute<TAttribute>(
            this MemberInfo member,
            out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = member.GetCustomAttribute<TAttribute>();

            return attribute != null;
        }

        private static SeedObject AddTagsAndAssignments(
            this SeedObject obj,
            ProfileKind profileKind,
            IEnumerable<HasTagAttribute> tagged,
            IEnumerable<AssignedToAttribute> assigned)
        {
            obj.AssignedTags = tagged.ToTagList();
            obj.Assignments = assigned.ToAssignments(obj.Id, profileKind);

            return obj;
        }

        public static SeedObject ToSeedObject(this MemberInfo memberInfo)
        {
            IEnumerable<AssignedToAttribute> assignments = memberInfo.GetCustomAttributes<AssignedToAttribute>();
            IEnumerable<HasTagAttribute> tagged = memberInfo.GetCustomAttributes<HasTagAttribute>();

            if (memberInfo.TryGetCustomAttribute(out UserAttribute userInfo))
            {
                return CreateUserObject(
                        memberInfo.GetValue<string>(),
                        userInfo)
                    .AddTagsAndAssignments(ProfileKind.User, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out GroupAttribute groupInfo))
            {
                return CreateGroupObject(
                        memberInfo.GetValue<string>(),
                        groupInfo)
                    .AddTagsAndAssignments(ProfileKind.Group, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out TagAttribute tagInfo))
            {
                return CreateTagObject(
                        memberInfo.GetValue<string>(),
                        tagInfo)
                    .AddTagsAndAssignments(ProfileKind.Unknown, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out OrganizationAttribute orgInfo))
            {
                return CreateOrganizationObject(
                        memberInfo.GetValue<string>(),
                        orgInfo)
                    .AddTagsAndAssignments(ProfileKind.Unknown, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out FunctionAttribute funcInfo))
            {
                return CreateFunctionObject(
                        memberInfo.GetValue<string>(),
                        funcInfo)
                    .AddTagsAndAssignments(ProfileKind.Unknown, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out RoleAttribute roleInfo))
            {
                return CreateRoleObject(
                        memberInfo.GetValue<string>(),
                        roleInfo)
                    .AddTagsAndAssignments(ProfileKind.Unknown, tagged, assignments);
            }

            if (memberInfo.TryGetCustomAttribute(out ProfileVertexNodeAttribute profileNode))
            {
                return CreateExtendedProfileNodeObject(memberInfo.GetValue<string>(), profileNode);
            }

            if (memberInfo.TryGetCustomAttribute(out ProfileVertexEdgeAttribute profileEdge))
            {
                return CreateExtendedProfileEdgeObject(memberInfo.GetValue<int>(), profileEdge);
            }

            if (memberInfo.TryGetCustomAttribute(out ProfileVertexRootNodeAttribute _))
            {
                return CreateExtendedProfileRootNode(memberInfo.GetValue<string>());
            }
            
            throw new NotSupportedException();
        }

        public static IList<SeedObject> ToSeedObjects(this Type rootType)
        {
            return rootType.GetNestedTypes(BindingFlags.Static | BindingFlags.Public)
                .Append(rootType)
                .SelectMany(
                    t => t
                        .GetMembers(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.CustomAttributes.Any())
                .Select(ToSeedObject)
                .Where(o => o != null)
                .ToList();
        }
    }
}
