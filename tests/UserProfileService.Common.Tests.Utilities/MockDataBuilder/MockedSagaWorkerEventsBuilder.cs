using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bogus;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using TagAssignments = Maverick.UserProfileService.Models.RequestModels.TagAssignment;
using V3FunctionCreatedEvent = UserProfileService.Events.Implementation.V3.FunctionCreatedEvent;
using V3FunctionCreatedPayload = UserProfileService.Events.Payloads.V3.FunctionCreatedPayload;
using V3UserCreatedEvent = UserProfileService.Events.Implementation.V3.UserCreatedEvent;
using V3UserCreatedPayload = UserProfileService.Events.Payloads.V3.UserCreatedPayload;

namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     Used to build events for testing
    /// </summary>
    public class MockedSagaWorkerEventsBuilder
    {
        public const string DefaultCorrelationId = UserProfileServiceEventExtensions.DefaultCorrelationId;
        public const string DefaultEventId = "event-id-1";
        public const string RequestSagaId = UserProfileServiceEventExtensions.DefaultProcessId;

        private static IMapper ModelMapper =>
            new Mapper(
                new MapperConfiguration(
                    opt =>
                    {
                        opt.CreateMap<FirstLevelProjectionUser, UserCreatedPayload>().ReverseMap();
                        opt.CreateMap<FirstLevelProjectionRole, RoleCreatedPayload>().ReverseMap();
                        opt.CreateMap<FirstLevelProjectionTag, TagCreatedPayload>().ReverseMap();
                        opt.CreateMap<FirstLevelProjectionOrganization, OrganizationCreatedPayload>().ReverseMap();
                        opt.CreateMap<FirstLevelProjectionUser, V3UserCreatedPayload>().ReverseMap();

                        opt.CreateMap<FirstLevelProjectionFunction, FunctionDeletedEvent>()
                            .ForMember(
                                x => x.Payload,
                                m => m.MapFrom(
                                    y => new IdentifierPayload
                                    {
                                        Id = y.Id,
                                        IsSynchronized = y.SynchronizedAt.HasValue
                                    }));

                        opt.CreateMap<FirstLevelProjectionRole, RoleDeletedEvent>()
                            .ForMember(
                                x => x.Payload,
                                m => m.MapFrom(
                                    y => new IdentifierPayload
                                    {
                                        Id = y.Id,
                                        IsSynchronized = y.SynchronizedAt.HasValue
                                    }));

                        opt.CreateMap<IFirstLevelProjectionProfile, ProfileDeletedEvent>()
                            .ForMember(
                                x => x.Payload,
                                m => m.MapFrom(
                                    y => new ProfileIdentifierPayload
                                    {
                                        Id = y.Id,
                                        IsSynchronized = y.SynchronizedAt.HasValue,
                                        ExternalIds = y.ExternalIds,
                                        ProfileKind = y.Kind
                                    }));

                        opt.CreateMap<OrganizationCreatedEvent, FirstLevelProjectionOrganization>()
                            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
                            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
                            .IncludeMembers(x => x.Payload);

                        opt.CreateMap<FirstLevelProjectionFunction, V3FunctionCreatedPayload>().ReverseMap();

                        opt.CreateMap<FirstLevelProjectionFunction, FunctionCreatedPayload>()
                            .ForMember(t => t.TagFilters, t => t.MapFrom(m => new[] { m.Organization.Id }));

                        opt.CreateMap<FirstLevelProjectionGroup, GroupCreatedPayload>().ReverseMap();

                        opt.CreateMap<V3FunctionCreatedEvent, FirstLevelProjectionFunction>()
                            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
                            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
                            .IncludeMembers(x => x.Payload);

                        opt.CreateMap<FunctionDeletedEvent, FirstLevelProjectionFunction>()
                            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

                        opt.CreateMap<ProfileDeletedEvent, IFirstLevelProjectionProfile>()
                            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

                        opt.CreateMap<RoleDeletedEvent, FirstLevelProjectionRole>()
                            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

                        opt.CreateMap<FirstLevelProjectionFunction, IdentifierPayload>().ReverseMap();

                        opt.CreateMap<FunctionCreatedPayload, FirstLevelProjectionFunction>()
                            .ForMember(
                                t => t.Organization,
                                t => t.MapFrom(
                                    m => new FirstLevelProjectionOrganization
                                    {
                                        Id = m.TagFilters.First()
                                    }));
                    }));

        public static DateTime DefaultTimestamp => DateTime.Parse("2022-02-26 16:34:56.890Z");

        public static EventInitiator EventInitiator =>
            UserProfileServiceEventExtensions.DefaultEventInitiator.ConvertToEventStoreModel();

        private static List<RangeCondition> GenerateRangeConditions(
            int number,
            float nullWeight = 0.75F)
        {
            return new Faker<RangeCondition>()
                .RuleFor(
                    c => c.Start,
                    faker => faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(3))
                        .OrNull(faker, nullWeight))
                .RuleFor(
                    c => c.End,
                    (faker, current) =>
                        current?.Start != null
                            ? faker.Date.Between(current.Start.Value, DateTime.UtcNow.AddMonths(6)) as DateTime?
                            : null)
                .Generate(number);
        }

        private static ObjectType ObjectTypeAssignments(ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.Group => new[] { ObjectType.Group, ObjectType.User }.PickRandom(),
                ObjectType.Organization => ObjectType.Organization,
                ObjectType.Function => new[] { ObjectType.Group, ObjectType.User }.PickRandom(),
                _ => throw new ArgumentOutOfRangeException(nameof(objectType), "Unknown type to match.")
            };
        }

        /// <summary>
        ///     Maps an <see cref="OrganizationBasic" /> to an <see cref="OrganizationCreatedEvent" /> event and returns it.
        /// </summary>
        /// <param name="user"><see cref="UserBasic" /> to map</param>
        /// <param name="tagAssignments"><see cref="TagAssignments" /> to add to <see cref="UserCreatedEvent" /></param>
        /// <returns>Mapped <see cref="UserCreatedEvent" /></returns>
        public static UserCreatedEvent CreateUserCreatedEvent(UserBasic user, TagAssignments[] tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<UserCreatedPayload>(user);
            createdPayload.Tags = tagAssignments ?? Array.Empty<TagAssignments>();
            var userCreatedEvent = new UserCreatedEvent(DateTime.UtcNow, createdPayload);

            return userCreatedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionOrganization" /> to an <see cref="OrganizationCreatedEvent" /> event and
        ///     returns it.
        /// </summary>
        /// <param name="organization"><see cref="FirstLevelProjectionOrganization" /> to map</param>
        /// <param name="tagAssignments"><see cref="TagAssignments" /> to add to <see cref="OrganizationCreatedEvent" /></param>
        /// <param name="conditionalMember">The optional members that should be assigned to the new created organization.</param>
        /// <returns>Mapped <see cref="OrganizationCreatedEvent" /></returns>
        public static OrganizationCreatedEvent CreateOrganizationCreatedEvent(
            FirstLevelProjectionOrganization organization,
            TagAssignments[] tagAssignments = null,
            List<ConditionObjectIdent> conditionalMember = null)
        {
            var createdPayload = ModelMapper.Map<OrganizationCreatedPayload>(organization);
            createdPayload.Tags = tagAssignments ?? Array.Empty<TagAssignments>();

            var organizationCreatedEvent = new OrganizationCreatedEvent(
                organization.CreatedAt,
                createdPayload);

            organizationCreatedEvent.Payload.Members = conditionalMember == null
                ? Array.Empty<ConditionObjectIdent>()
                : conditionalMember.ToArray();

            return organizationCreatedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionFunction" /> to an <see cref="V3FunctionCreatedEvent" /> event and returns
        ///     it.
        /// </summary>
        /// <param name="function"><see cref="FirstLevelProjectionFunction" /> to map</param>
        /// <param name="tagAssignments"><see cref="TagAssignments" /> to add to <see cref="V3FunctionCreatedEvent" /></param>
        /// <returns>Mapped <see cref="V3FunctionCreatedEvent" /></returns>
        public static V3FunctionCreatedEvent CreateV3FunctionCreatedEvent(
            FirstLevelProjectionFunction function,
            TagAssignments[] tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<V3FunctionCreatedPayload>(function);
            createdPayload.Tags = tagAssignments ?? Array.Empty<TagAssignments>();
            var functionCreatedEvent = new V3FunctionCreatedEvent(DateTime.UtcNow, createdPayload);

            return functionCreatedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionFunction" /> to an <see cref="V3FunctionCreatedEvent" /> event and returns
        ///     it.
        /// </summary>
        /// <param name="function"><see cref="FirstLevelProjectionFunction" /> to map</param>
        /// <param name="tagAssignments"><see cref="TagAssignments" /> to add to <see cref="V3FunctionCreatedEvent" /></param>
        /// <returns>Mapped <see cref="V3FunctionCreatedEvent" /></returns>
        public static FunctionCreatedEvent CreateV2FunctionCreatedEvent(
            FirstLevelProjectionFunction function,
            TagAssignments[] tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<FunctionCreatedPayload>(function);
            createdPayload.Tags = tagAssignments ?? Array.Empty<TagAssignments>();
            var functionCreatedEvent = new FunctionCreatedEvent(DateTime.UtcNow, createdPayload);

            return functionCreatedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionFunction" /> to an <see cref="FunctionDeletedEvent" /> event and returns it.
        /// </summary>
        /// <param name="function"><see cref="FirstLevelProjectionFunction" /> to map</param>
        /// <returns>Mapped <see cref="FunctionDeletedEvent" /></returns>
        public static FunctionDeletedEvent CreateFunctionDeletedEvent(FirstLevelProjectionFunction function)
        {
            var functionDeletedEvent = ModelMapper.Map<FunctionDeletedEvent>(function);

            return functionDeletedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionFunction" /> to an <see cref="ProfileDeletedEvent" /> event and returns it.
        /// </summary>
        /// <param name="profile"><see cref="FirstLevelProjectionFunction" /> to map</param>
        /// <returns>Mapped <see cref="ProfileDeletedEvent" /></returns>
        public static ProfileDeletedEvent CreateProfileDeletedEvent(IFirstLevelProjectionProfile profile)
        {
            var profileDeletedEvent = ModelMapper.Map<ProfileDeletedEvent>(profile);

            return profileDeletedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="FirstLevelProjectionFunction" /> to an <see cref="RoleDeletedEvent" /> event and returns it.
        /// </summary>
        /// <param name="role"><see cref="FirstLevelProjectionFunction" /> to map</param>
        /// <returns>Mapped <see cref="RoleDeletedEvent" /></returns>
        public static RoleDeletedEvent CreateRoleDeletedEvent(FirstLevelProjectionRole role)
        {
            var roleDeletedEvent = ModelMapper.Map<RoleDeletedEvent>(role);

            return roleDeletedEvent;
        }

        /// <summary>
        ///     Maps an <see cref="IFirstLevelProjectionProfile" /> to an <see cref="ProfilePropertiesChangedEvent" /> event and
        ///     returns it.
        /// </summary>
        /// <param name="profile"><see cref="IFirstLevelProjectionProfile" /> to map</param>
        /// <param name="changedProperties"></param>
        /// <returns>Mapped <see cref="FunctionPropertiesChangedEvent" /></returns>
        public static ProfilePropertiesChangedEvent CreatePropertiesChangedEvent(
            IFirstLevelProjectionProfile profile,
            IDictionary<string, object> changedProperties)
        {
            var profileChangedEvent = new PropertiesUpdatedPayload
            {
                Id = profile.Id,
                Properties = changedProperties
            };

            return new ProfilePropertiesChangedEvent
            {
                Payload = profileChangedEvent,
                ProfileKind = profile.Kind
            };
        }

        public static UserCreatedEvent CreateUserCreatedEvent(
            FirstLevelProjectionUser user,
            List<TagAssignments> tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<UserCreatedPayload>(user);
            createdPayload.Tags = tagAssignments == null ? Array.Empty<TagAssignments>() : tagAssignments.ToArray();
            var userCreatedEvent = new UserCreatedEvent(user.CreatedAt, createdPayload);

            return userCreatedEvent;
        }

        public static V3UserCreatedEvent CreateUserCreatedEventV3(
            FirstLevelProjectionUser user,
            List<TagAssignments> tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<V3UserCreatedPayload>(user);
            createdPayload.Tags = tagAssignments == null ? Array.Empty<TagAssignments>() : tagAssignments.ToArray();
            var userCreatedEvent = new V3UserCreatedEvent(user.CreatedAt, createdPayload);

            return userCreatedEvent;
        }

        public static TagDeletedEvent CreateTagDeletedEvent(FirstLevelProjectionTag tag)
        {
            return new TagDeletedEvent
            {
                Payload = new IdentifierPayload
                {
                    Id = tag.Id
                }
            };
        }

        public static RoleCreatedEvent CreateRoleCreatedEvent(
            FirstLevelProjectionRole role,
            List<TagAssignments> tagAssignments = null)
        {
            var createdPayload = ModelMapper.Map<RoleCreatedPayload>(role);
            createdPayload.Tags = tagAssignments == null ? Array.Empty<TagAssignments>() : tagAssignments.ToArray();
            var roleCreatedEvent = new RoleCreatedEvent(role.CreatedAt, createdPayload);

            return roleCreatedEvent;
        }

        public static TagCreatedEvent CreateTagCreatedEvent(
            FirstLevelProjectionTag tag)
        {
            var createdTagPayload = ModelMapper.Map<TagCreatedPayload>(tag);
            var tagCreatedEvent = new TagCreatedEvent(DateTime.UtcNow, createdTagPayload);

            return tagCreatedEvent;
        }

        public static GroupCreatedEvent CreateGroupCreatedEvent(
            FirstLevelProjectionGroup group,
            List<TagAssignments> tagAssignments = null,
            List<ConditionObjectIdent> conditionalMember = null)
        {
            var createdTagPayload = ModelMapper.Map<GroupCreatedPayload>(group);
            var groupCreatedEvent = new GroupCreatedEvent(group.CreatedAt, createdTagPayload);

            groupCreatedEvent.Payload.Members = conditionalMember == null
                ? Array.Empty<ConditionObjectIdent>()
                : conditionalMember.ToArray();

            groupCreatedEvent.Payload.Tags =
                tagAssignments == null ? Array.Empty<TagAssignments>() : tagAssignments.ToArray();

            return groupCreatedEvent;
        }

        public static ProfileClientSettingsSetEvent CreateProfileClientSettingsSetEvent(
            IFirstLevelProjectionProfile group,
            Dictionary<string, string> clientSettings)
        {
            string key = clientSettings.Keys.First();
            string value = clientSettings.Values.First();

            var clientSettingsSetEvent = new ProfileClientSettingsSetEvent
            {
                Payload = new ClientSettingsSetPayload
                {
                    Key = key,
                    Settings = JObject.Parse(value),
                    IsSynchronized = false,
                    Resource = new ProfileIdent(group.Id, group.Kind)
                }
            };

            return clientSettingsSetEvent;
        }

        public static ProfileClientSettingsSetBatchEvent CreateProfileClientSettingsSetBatchEvent(
            List<ProfileIdent> profileIdent,
            KeyValuePair<string, string> clientSettings,
            DateTime startDateTime)
        {
            return new ProfileClientSettingsSetBatchEvent
            {
                Payload = new ClientSettingsSetBatchPayload
                {
                    Key = clientSettings.Key,
                    Settings = JObject.Parse(clientSettings.Value),
                    IsSynchronized = false,
                    Resources = profileIdent.ToArray()
                },
                Timestamp = startDateTime
            };
        }

        public static ProfileClientSettingsDeletedEvent CreateProfileClientSettingsDeletedEvent(
            IFirstLevelProjectionProfile profile,
            string key)
        {
            ProfileClientSettingsDeletedEvent clientSettingsDeleteEvent = new ProfileClientSettingsDeletedEvent
                {
                    Payload = new ClientSettingsDeletedPayload
                    {
                        Key = key,
                        IsSynchronized = false,
                        Resource = new ProfileIdent(
                            profile.Id,
                            profile.Kind)
                    },
                    EventId = DefaultEventId,
                    VersionInformation = 1,
                    RequestSagaId = RequestSagaId,
                    Timestamp = DefaultTimestamp
                }
                .AddDefaultMetadata(profile.Id, DefaultTimestamp);

            clientSettingsDeleteEvent.CorrelationId = clientSettingsDeleteEvent.MetaData.CorrelationId;

            clientSettingsDeleteEvent.Initiator =
                clientSettingsDeleteEvent.MetaData.Initiator.ConvertToEventStoreModel();

            return clientSettingsDeleteEvent;
        }

        public static ProfileTagsAddedEvent GenerateProfileTagsAddedEvent(
            ProfileIdent profile,
            ICollection<TagAssignments> tags)
        {
            var payload = new TagsSetPayload
            {
                Id = profile.Id,
                IsSynchronized = false,
                Tags = tags.ToArray()
            };

            var profileTagsAddedEvent = new ProfileTagsAddedEvent
            {
                Payload = payload,
                ProfileKind = profile.ProfileKind
            };

            return profileTagsAddedEvent;
        }

        public static ProfileTagsRemovedEvent GenerateProfileTagsRemovedEvent(
            ProfileIdent profile,
            ICollection<string> tags)
        {
            var payload = new TagsRemovedPayload
            {
                ResourceId = profile.Id,
                IsSynchronized = false,
                Tags = tags.ToArray()
            };

            var profileTagsRemovedEvent = new ProfileTagsRemovedEvent
            {
                Payload = payload,
                ProfileKind = profile.ProfileKind
            };

            return profileTagsRemovedEvent;
        }

        public static List<ConditionObjectIdent> GenerateConditionObjectIdents(int number, ObjectType parentType)
        {
            return new Faker<ConditionObjectIdent>()
                .RuleFor(
                    ci => ci.Id,
                    faker => faker.Random.Guid().ToString())
                .RuleFor(
                    ci => ci.Conditions,
                    faker => GenerateRangeConditions(faker.Random.Int(1, 20), faker.Random.Float()).ToArray())
                .RuleFor(
                    ci => ci.Type,
                    faker => ObjectTypeAssignments(ObjectType.Group))
                .Generate(number);
        }
    }
}
