using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Models;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using AssignmentObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding
{
    public class ReflectionTestData : ITestData
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionRole>> Roles { get; }

        /// <inheritdoc />
        public IList<ExtendedProfile> Profiles { get; }

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionFunction>> Functions { get; }

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionTag>> Tags { get; }

        /// <inheritdoc />
        public IList<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>> TemporaryAssignments { get; set; } =
            new List<ExtendedEntity<FirstLevelProjectionTemporaryAssignment>>();

        public ReflectionTestData(Type testCase)
        {
            Name = testCase.Name;

            List<FieldInfo> fieldInfos = testCase.GetNestedTypes()
                .SelectMany(
                    nestedTypes =>
                        nestedTypes.GetFields(
                            BindingFlags.Public
                            | BindingFlags.Static
                            | BindingFlags.FlattenHierarchy))
                .Where(field => field.IsLiteral && !field.IsInitOnly)
                .ToList();

            Profiles = fieldInfos.Where(field => field.GetCustomAttribute<ProfileAttribute>() != null)
                .Select(GenerateProfileData)
                .ToList();

            Roles = fieldInfos.Where(field => field.GetCustomAttribute<RoleAttribute>() != null)
                .Select(GenerateRolesData)
                .ToList();

            Functions = fieldInfos.Where(field => field.GetCustomAttribute<FunctionAttribute>() != null)
                .Select(GenerateFunctionData)
                .ToList();

            Tags = fieldInfos.Where(field => field.GetCustomAttributes<TagAttribute>().Any())
                .Select(GenerateTagsData)
                .ToList();
        }

        private ExtendedProfile GenerateProfileData(FieldInfo field)
        {
            ProfileAttribute profileAttribute =
                field.GetCustomAttribute<ProfileAttribute>() ?? throw new NotSupportedException();

            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind kind = profileAttribute.Kind;

            string id = (string)field.GetRawConstantValue() ?? throw new NotSupportedException();

            IFirstLevelProjectionProfile profile = kind switch
            {
                Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Organization => MockDataGenerator
                    .GenerateOrganization(id),

                Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Group => MockDataGenerator
                    .GenerateGroup(id),
                Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.User => MockDataGenerator
                    .GenerateUser(id),
                _ => throw new NotSupportedException($"The profile kind {kind} is not supported.")
            };

            if (!string.IsNullOrWhiteSpace(profileAttribute.Name))
            {
                profile.Name = profileAttribute.Name;
            }

            IList<AssignedToAttribute> assignments = field.GetCustomAttributes<AssignedToAttribute>().ToList();
            IEnumerable<HasClientSettings> clientSettings = field.GetCustomAttributes<HasClientSettings>();

            TemporaryAssignments = TemporaryAssignments
                .Concat(
                    assignments
                        .Where(
                            a => a.Conditions?.Start != null
                                && a.Conditions.Start != DateTime.MinValue
                                && a.Conditions.End != null
                                && a.Conditions.End != DateTime.MaxValue)
                        .Select(
                            a =>
                                new ExtendedEntity<FirstLevelProjectionTemporaryAssignment>
                                {
                                    TagAssignments = new List<TagAssignment>(),
                                    Value = new FirstLevelProjectionTemporaryAssignment
                                    {
                                        Id = Guid.NewGuid().ToString("D"),
                                        Start = a.Conditions.Start,
                                        End = a.Conditions.End,
                                        LastModified = DateTime.UtcNow,
                                        ProfileId = profile.Id,
                                        TargetId = a.TargetId,
                                        TargetType = ConvertToObjectType(a.TargetType),
                                        State = TemporaryAssignmentState.NotProcessed,
                                        ProfileType = ConvertToObjectType(profile.Kind)
                                    }
                                }))
                .ToList();

            return new ExtendedProfile
            {
                Value = profile,
                TagAssignments = GenerateTagsForProfilesData(field),
                Assignments = assignments.Select(
                        attr => new Assignment
                        {
                            ProfileId = profile.Id,
                            TargetId = attr.TargetId,
                            TargetType = ConvertContainerType(attr.TargetType),
                            Conditions = new[]
                            {
                                new RangeCondition
                                {
                                    Start = attr.Conditions.Start,
                                    End = attr.Conditions.End
                                }
                            }
                        })
                    .ToList(),
                ClientSettings = clientSettings.ToDictionary(c => c.Key, c => c.Value)
            };
        }

        private ExtendedEntity<FirstLevelProjectionFunction> GenerateFunctionData(FieldInfo field)
        {
            FunctionAttribute attribute = field.GetCustomAttribute<FunctionAttribute>()
                ?? throw new NotSupportedException();

            FirstLevelProjectionOrganization orgUnit =
                (FirstLevelProjectionOrganization)Profiles
                    .FirstOrDefault(orgUnit => orgUnit.Value.Id == attribute.OrganizationId)
                    ?.Value
                ?? throw new ArgumentOutOfRangeException();

            FirstLevelProjectionRole role = Roles.FirstOrDefault(role => role.Value.Id == attribute.RoleId)?.Value
                ?? throw new ArgumentOutOfRangeException();

            string id = (string)field.GetRawConstantValue() ?? throw new ArgumentException();

            return new ExtendedEntity<FirstLevelProjectionFunction>
            {
                Value = MockDataGenerator.GenerateFunction(id, role, orgUnit),
                TagAssignments = GenerateTagsForProfilesData(field)
            };
        }

        private ExtendedEntity<FirstLevelProjectionRole> GenerateRolesData(FieldInfo field)
        {
            string id = (string)field.GetRawConstantValue() ?? throw new ArgumentNullException();

            return new ExtendedEntity<FirstLevelProjectionRole>
            {
                Value = MockDataGenerator.GenerateRole(id),
                TagAssignments = GenerateTagsForProfilesData(field)
            };
        }

        private AssignmentObjectType ConvertContainerType(ContainerType input)
        {
            return input switch
            {
                ContainerType.Group => AssignmentObjectType.Group,
                ContainerType.Function => AssignmentObjectType
                    .Function,
                ContainerType.Organization => AssignmentObjectType
                    .Organization,
                ContainerType.Role => AssignmentObjectType.Role,
                _ => AssignmentObjectType.Unknown
            };
        }

        private static AssignmentObjectType ConvertToObjectType(
            ProfileKind input)
        {
            return input switch
            {
                ProfileKind.Unknown => AssignmentObjectType
                    .Unknown,
                ProfileKind.User => AssignmentObjectType.User,
                ProfileKind.Group => AssignmentObjectType
                    .Group,
                ProfileKind.Organization =>
                    AssignmentObjectType.Organization,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
        }

        private static AssignmentObjectType ConvertToObjectType(ContainerType input)
        {
            return input switch
            {
                ContainerType.NotSpecified => AssignmentObjectType.Unknown,
                ContainerType.Group => AssignmentObjectType.Group,
                ContainerType.Organization => AssignmentObjectType.Organization,
                ContainerType.Function => AssignmentObjectType.Function,
                ContainerType.Role => AssignmentObjectType.Role,
                _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
            };
        }

        private ExtendedEntity<FirstLevelProjectionTag> GenerateTagsData(FieldInfo field)
        {
            TagAttribute attribute = field.GetCustomAttribute<TagAttribute>() ?? throw new ArgumentException();

            string id = (string)field.GetRawConstantValue() ?? throw new ArgumentException();

            return new ExtendedEntity<FirstLevelProjectionTag>
            {
                Value = new FirstLevelProjectionTag
                {
                    Id = id,
                    Name = attribute.Name
                }
            };
        }

        private List<TagAssignment> GenerateTagsForProfilesData(FieldInfo field)
        {
            IEnumerable<HasTagAttribute> attribute =
                field.GetCustomAttributes<HasTagAttribute>();

            return attribute.Select(
                    tag => new TagAssignment
                    {
                        TagDetails = new Tag
                        {
                            Id = tag.TagId
                        },
                        IsInheritable = tag.IsInheritable
                    })
                .ToList();
        }
    }
}
