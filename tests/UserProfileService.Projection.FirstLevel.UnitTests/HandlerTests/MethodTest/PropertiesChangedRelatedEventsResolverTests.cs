using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Implementation;
using UserProfileService.Projection.FirstLevel.UnitTests.Extensions;
using UserProfileService.Projection.FirstLevel.UnitTests.Utilities;
using Xunit;
using InitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.MethodTest
{
    public class PropertiesChangedRelatedEventsResolverTests
    {
        private const string FunctionId = "2D26D853-BBD1-4FCC-9FD1-F20274BE0476";
        private const string GroupDmId = "490B3675-BCAF-4FA4-BC89-5D7122C9E35F";
        private const string UserImId = "DC48FDCD-6662-459A-ABA9-9EB51937355D";

        public static readonly List<object[]> DataNullTests = new List<object[]>
                                                               {
                                                                   new object[]
                                                                   {
                                                                       null,
                                                                       new ObjectIdent(
                                                                           "FE639E5B-2C01-4525-8529-E374B6C10676",
                                                                           ObjectType.Group),
                                                                       PropertiesChangedRelation.Member,
                                                                       new ProfilePropertiesChangedEvent(
                                                                           DateTime.Now,
                                                                           new PropertiesUpdatedPayload())
                                                                   },
                                                                   new object[]
                                                                   {
                                                                       new ObjectIdent(
                                                                           "5BD5FF56-F0E2-4A2D-9045-E42F2FF3D0A5",
                                                                           ObjectType.Group),
                                                                       null,
                                                                       PropertiesChangedRelation.Member,
                                                                       new ProfilePropertiesChangedEvent(
                                                                           DateTime.Now,
                                                                           new PropertiesUpdatedPayload())
                                                                   },
                                                                   new object[]
                                                                   {
                                                                       new ObjectIdent(
                                                                           "5BD5FF56-F0E2-4A2D-9045-E42F2FF3D0A5",
                                                                           ObjectType.Group),
                                                                       new ObjectIdent(
                                                                           "FE639E5B-2C01-4525-8529-E374B6C10676",
                                                                           ObjectType.Group),
                                                                       PropertiesChangedRelation.Member,
                                                                       null
                                                                   }
                                                               };

        public static readonly List<object[]> DataIndirectMember = new List<object[]>
                                                                    {
                                                                        new object[]
                                                                        {
                                                                            new ObjectIdent(
                                                                                "5BD5FF56-F0E2-4A2D-9045-E42F2FF3D0A5-Reference",
                                                                                ObjectType.Group),
                                                                            new ObjectIdent(
                                                                                "FE639E5B-2C01-4525-8529-E374B6C10676-Related",
                                                                                ObjectType.Group),
                                                                            PropertiesChangedRelation.IndirectMember,
                                                                            new ProfilePropertiesChangedEvent(
                                                                                DateTime.Now,
                                                                                new PropertiesUpdatedPayload())
                                                                        }
                                                                    };

        public static readonly List<object[]> DataCreateFunctionEvents = new List<object[]>
                                                                          {
                                                                              new object[]
                                                                              {
                                                                                  "842FD79D-256C-4885-8659-CEF1184A3ED6",
                                                                                  null,
                                                                                  PropertiesChangedContext.Role,
                                                                                  new ArangoTransaction(),
                                                                                  new CancellationToken()
                                                                              },
                                                                              new object[]
                                                                              {
                                                                                  "842FD79D-256C-4885-8659-CEF1184A3ED6",
                                                                                  new FunctionPropertiesChangedEvent(),
                                                                                  PropertiesChangedContext.Role,
                                                                                  null,
                                                                                  new CancellationToken()
                                                                              }
                                                                          };

        private static readonly EventMetaData _eventMetaData = new EventMetaData
                                                               {
                                                                   Initiator =
                                                                       new EventInitiator
                                                                       {
                                                                           Id = "B0170B7A-02F5-4BF5-84F7-4D34589D3AA4",
                                                                           Type = InitiatorType.System
                                                                       },
                                                                   Timestamp = DateTime.Now,
                                                                   CorrelationId =
                                                                       "575105F3-E211-410E-BB3D-EAF2A2A19C8B",
                                                                   ProcessId = "273FF462-492B-4946-BC3F-66CBB036C120",
                                                                   VersionInformation = 1,
                                                                   HasToBeInverted = false
                                                               };

        internal readonly IMapper Mapper;
        private readonly IStreamNameResolver _streamNameResolver;
        internal readonly IFirstLevelEventTupleCreator TupleCreator;

        private readonly FirstLevelProjectionFunction _function;
        private readonly FirstLevelProjectionGroup _groupDm;

        internal readonly IPropertiesChangedRelatedEventsResolver PropertiesResolver;
        private readonly FirstLevelProjectionUser _userIm;

        public PropertiesChangedRelatedEventsResolverTests()
        {
            var repositoryMock = new Mock<IFirstLevelProjectionRepository>();

            _function = MockDataGenerator.GenerateFirstLevelProjectionFunctionInstancesWithId(FunctionId);

            _groupDm = MockDataGenerator.GenerateFirstLevelProjectionGroupWithId(GroupDmId);

            _userIm = MockDataGenerator.GenerateFirstLevelProjectionUserWithId(UserImId);

            repositoryMock.Setup(
                              repo => repo.GetAllChildrenAsync(
                                  It.IsAny<ObjectIdent>(),
                                  It.IsAny<IDatabaseTransaction>(),
                                  It.IsAny<CancellationToken>()))
                          .ReturnsAsync(
                              (
                                  ObjectIdent identifierPayload,
                                  IDatabaseTransaction _,
                                  CancellationToken _) =>

                              {
                                  if (identifierPayload.Id == FunctionId)
                                  {
                                      return new List<FirstLevelRelationProfile>
                                             {
                                                 new FirstLevelRelationProfile(
                                                     _groupDm,
                                                     FirstLevelMemberRelation.DirectMember),
                                                 new FirstLevelRelationProfile(
                                                     _userIm,
                                                     FirstLevelMemberRelation.IndirectMember)
                                             };
                                  }

                                  return new List<FirstLevelRelationProfile>();
                              });

            repositoryMock.Setup(
                              repo => repo.GetFunctionAsync(
                                  It.IsAny<string>(),
                                  It.IsAny<IDatabaseTransaction>(),
                                  It.IsAny<CancellationToken>()))
                          .ReturnsAsync(() => _function);

            IServiceProvider serviceProvider = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(repositoryMock.Object); });

            PropertiesResolver = new PropertiesChangedRelatedEventsResolver(
                serviceProvider.GetRequiredService<ILogger<ProfilePropertiesChangedEvent>>(),
                serviceProvider.GetRequiredService<IMapper>(),
                serviceProvider.GetRequiredService<IFirstLevelEventTupleCreator>(),
                serviceProvider.GetRequiredService<IFirstLevelProjectionRepository>());

            Mapper = serviceProvider.GetRequiredService<IMapper>();

            TupleCreator = serviceProvider.GetRequiredService<IFirstLevelEventTupleCreator>();
            _streamNameResolver = serviceProvider.GetRequiredService<IStreamNameResolver>();
        }

        [Theory]
        [MemberData(nameof(DataNullTests))]
        public void CreateMember_Null_Argument_Exception(
            ObjectIdent referenceItem,
            ObjectIdent relatedItem,
            PropertiesChangedRelation context,
            ProfilePropertiesChangedEvent originalEvent)
        {
            Assert.Throws<ArgumentNullException>(
                () => PropertiesResolver.CreateRelatedMemberEvent(
                    referenceItem,
                    relatedItem,
                    context,
                    originalEvent));
        }

        [Theory]
        [MemberData(nameof(DataIndirectMember))]
        public void CreateMember_Indirect_Member(
            ObjectIdent referenceItem,
            ObjectIdent relatedItem,
            PropertiesChangedRelation context,
            ProfilePropertiesChangedEvent originalEvent)
        {
            EventTuple result = PropertiesResolver.CreateRelatedMemberEvent(
                referenceItem,
                relatedItem,
                context,
                originalEvent);

            result.Should().NotBeNull();
            result.TargetStream.Should().Contain("Related");
            ((PropertiesChanged)result.Event).RelatedContext.Should().Be(PropertiesChangedContext.IndirectMember);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("\t\t")]
        public async Task CreateFunctionPropertiesChanged_ArgumentException(string functionId)
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                    functionId,
                    new FunctionPropertiesChangedEvent(),
                    PropertiesChangedContext.Role,
                    new ArangoTransaction(),
                    new CancellationToken()));
        }

        [Theory]
        [MemberData(nameof(DataCreateFunctionEvents))]
        public async Task CreateFunctionPropertiesChanged_ArgumentNullException(
            string functionId,
            FunctionPropertiesChangedEvent changedEvent,
            PropertiesChangedContext context,
            IDatabaseTransaction transaction,
            CancellationToken token)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => PropertiesResolver
                    .CreateFunctionPropertiesChangedEventsAsync(
                        functionId,
                        changedEvent,
                        context,
                        transaction,
                        token));
        }

        [Fact]
        public async Task CreateFunctionPropertiesChanged_OrganizationChanged()
        {
            var nameValue = "New_Organization_Name";
            var displayNameValue = "New_Organization_DisplayName";

            var functionPropertiesChanged = new FunctionPropertiesChangedEvent
                                            {
                                                Payload = new PropertiesUpdatedPayload
                                                          {
                                                              Properties = new Dictionary<string, object>
                                                                           {
                                                                               {
                                                                                   nameof(
                                                                                       FirstLevelProjectionOrganization
                                                                                           .Name),
                                                                                   nameValue
                                                                               },
                                                                               {
                                                                                   nameof(
                                                                                       FirstLevelProjectionOrganization
                                                                                           .DisplayName),
                                                                                   displayNameValue
                                                                               }
                                                                           }
                                                          }
                                            };

            await PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                "59CEC8EE-4CF4-41BC-80EC-0E331C511E7E",
                functionPropertiesChanged,
                PropertiesChangedContext.Organization,
                new ArangoTransaction(),
                new CancellationToken());

            _function.Organization.Name.Should().BeEquivalentTo(nameValue);
            _function.Organization.DisplayName.Should().BeEquivalentTo(displayNameValue);
        }

        [Fact]
        public async Task CreateFunctionPropertiesChanged_RoleChanged()
        {
            var nameValue = "New_Role_Changed";
            var permissionValue = new[] { "read", "write", "freeze" };
            var deniedPermissionValue = new[] { "freeze", "write" };

            var functionPropertiesChanged = new FunctionPropertiesChangedEvent
                                            {
                                                Payload = new PropertiesUpdatedPayload
                                                          {
                                                              Id = FunctionId,
                                                              Properties = new Dictionary<string, object>
                                                                           {
                                                                               {
                                                                                   nameof(FirstLevelProjectionRole
                                                                                       .Name),
                                                                                   nameValue
                                                                               },
                                                                               {
                                                                                   nameof(FirstLevelProjectionRole
                                                                                       .Permissions),
                                                                                   permissionValue
                                                                               },
                                                                               {
                                                                                   nameof(FirstLevelProjectionRole
                                                                                       .DeniedPermissions),
                                                                                   deniedPermissionValue
                                                                               }
                                                                           }
                                                          }
                                            };

            await PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                "59CEC8EE-4CF4-41BC-80EC-0E331C511E7E",
                functionPropertiesChanged,
                PropertiesChangedContext.Role,
                new ArangoTransaction(),
                new CancellationToken());

            _function.Role.Name.Should().BeEquivalentTo(nameValue);
            _function.Role.Permissions.Should().BeEquivalentTo(permissionValue);
            _function.Role.DeniedPermissions.Should().BeEquivalentTo(deniedPermissionValue);
        }

        [Fact]
        public async Task CreateFunctionPropertiesChanged_FunctionChanged()
        {
            const string sourceValue = "function_SourceProperty";

            var functionPropertiesChanged = new FunctionPropertiesChangedEvent
                                            {
                                                Payload = new PropertiesUpdatedPayload
                                                          {
                                                              Properties = new Dictionary<string, object>
                                                                           {
                                                                               {
                                                                                   nameof(FirstLevelProjectionFunction
                                                                                       .Source),
                                                                                   sourceValue
                                                                               }
                                                                           }
                                                          }
                                            };

            await PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                "59CEC8EE-4CF4-41BC-80EC-0E331C511E7E",
                functionPropertiesChanged,
                PropertiesChangedContext.Self,
                new ArangoTransaction(),
                new CancellationToken());

            _function.Source.Should().BeEquivalentTo(sourceValue);
        }

        [Fact]
        public void CreateFunctionPropertiesChanged_NotSupportedException()
        {
            var sourceValue = "function_SourceProperty";

            var functionPropertiesChanged = new FunctionPropertiesChangedEvent
                                            {
                                                Payload = new PropertiesUpdatedPayload
                                                          {
                                                              Properties = new Dictionary<string, object>
                                                                           {
                                                                               {
                                                                                   nameof(FirstLevelProjectionFunction
                                                                                       .Source),
                                                                                   sourceValue
                                                                               }
                                                                           }
                                                          }
                                            };

            Assert.ThrowsAsync<NotSupportedException>(
                () => PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                    "59CEC8EE-4CF4-41BC-80EC-0E331C511E7E",
                    functionPropertiesChanged,
                    PropertiesChangedContext.IndirectMember,
                    new ArangoTransaction(),
                    new CancellationToken()));
        }

        // Is State:
        //      function
        //          |
        //       groupDM
        //          |
        //       UserIM
        //
        //      IM = indirect member
        //      DM = direct member
        [Fact]
        public async Task CreateFunctionPropertiesChanged_CompareEventTuple()
        {
            var sourceValue = "function_SourceProperty";
            DateTime dataNow = DateTime.Now.ToUniversalTime();

            var functionPropertiesChanged = new FunctionPropertiesChangedEvent
                                            {
                                                Payload = new PropertiesUpdatedPayload
                                                          {
                                                              Properties = new Dictionary<string, object>
                                                                           {
                                                                               {
                                                                                   nameof(FirstLevelProjectionFunction
                                                                                       .Source),
                                                                                   sourceValue
                                                                               }
                                                                           },
                                                              Id = FunctionId
                                                          },
                                                Initiator = new EventSourcing.Abstractions.Models.EventInitiator
                                                            {
                                                                Id = "B0170B7A-02F5-4BF5-84F7-4D34589D3AA4",
                                                                Type = EventSourcing.Abstractions.Models.InitiatorType
                                                                             .System
                                                            },
                                                Timestamp = dataNow,
                                                CorrelationId = "575105F3-E211-410E-BB3D-EAF2A2A19C8B",
                                                VersionInformation = 2,
                                                RequestSagaId = "273FF462-492B-4946-BC3F-66CBB036C120",
                                                MetaData = _eventMetaData.CloneEventDate()
                                            };

            List<EventTuple> result = await PropertiesResolver.CreateFunctionPropertiesChangedEventsAsync(
                "59CEC8EE-4CF4-41BC-80EC-0E331C511E7E",
                functionPropertiesChanged,
                PropertiesChangedContext.Self,
                new ArangoTransaction(),
                new CancellationToken());

            var eventTupleFunctionChanged =
                Mapper.Map<PropertiesChanged>(functionPropertiesChanged);

            var eventTupleGroupDm = new FunctionChanged
                                    {
                                        Function = Mapper.Map<Function>(_function),
                                        Context = PropertiesChangedContext.SecurityAssignments,
                                        MetaData = new EventMetaData()
                                    };

            var eventTupleUserId = new FunctionChanged
                                   {
                                       Function = Mapper.Map<Function>(_function),
                                       Context = PropertiesChangedContext.IndirectMember,
                                       MetaData = new EventMetaData()
                                   };

            eventTupleFunctionChanged.MetaData = _eventMetaData.CloneEventDate();
            eventTupleGroupDm.MetaData = _eventMetaData.CloneEventDate();
            eventTupleUserId.MetaData = _eventMetaData.CloneEventDate();

            EventTuple eventTuplePropertiesChanged = TupleCreator.CreateEvent(
                _function.ToObjectIdent(),
                eventTupleFunctionChanged.SetRelatedContext(PropertiesChangedContext.Self)
                                         .SetRelatedEntityId(
                                             _streamNameResolver.GetStreamName(_function.ToObjectIdent())),
                functionPropertiesChanged);

            EventTuple eventTuplePropertiesGroupChanged = TupleCreator.CreateEvent(
                _groupDm.ToObjectIdent(),
                eventTupleGroupDm
                    .SetRelatedEntityId(_streamNameResolver.GetStreamName(_groupDm.ToObjectIdent())),
                functionPropertiesChanged);

            EventTuple eventTuplePropertiesUserChanged = TupleCreator.CreateEvent(
                _userIm.ToObjectIdent(),
                eventTupleUserId
                    .SetRelatedEntityId(_streamNameResolver.GetStreamName(_userIm.ToObjectIdent())),
                functionPropertiesChanged);

            var eventTuplesExpected = new List<EventTuple>
                                      {
                                          eventTuplePropertiesChanged, eventTuplePropertiesGroupChanged,
                                          eventTuplePropertiesUserChanged
                                      };

            eventTuplesExpected.Should()
                               .BeEquivalentTo(
                                   result,
                                   opt => opt.Excluding(o => o.Event.MetaData.Timestamp)
                                             .Excluding(o => o.Event.MetaData.Batch)
                                             .Excluding(o => o.Event.EventId)
                                             .RespectingRuntimeTypes());
        }
    }
}
