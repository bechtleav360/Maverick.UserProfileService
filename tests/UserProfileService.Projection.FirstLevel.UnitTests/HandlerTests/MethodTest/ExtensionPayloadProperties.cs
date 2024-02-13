using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.FirstLevel.Extensions;
using Xunit;
using Api = UserProfileService.EventSourcing.Abstractions.Models;
using InitiatorType = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.MethodTest
{
    public class PropertiesUpdatedPayloadExtensionTest
    {
        public static IEnumerable<object[]> DataMember =>
            new List<object[]>
            {
                new object[]
                {
                    new PropertiesUpdatedPayload
                    {
                        Properties = new Dictionary<string, object>
                                     {
                                         { nameof(Member.Name), "NewName" },
                                         { nameof(Member.DisplayName), "DisplayName" },
                                         {
                                             nameof(Member.ExternalIds),
                                             new List<ExternalIdentifier>()
                                         }
                                     },
                        Id = "AF20B232-C397-4C2D-A39C-F686C229C645"
                    },
                    true
                },
                new object[]
                {
                    new PropertiesUpdatedPayload
                    {
                        Properties = new Dictionary<string, object>
                                     {
                                         { "False", "False" }
                                     },
                        Id = "550B84A3-ADA2-4FC8-BCBA-17C456C68E14"
                    },
                    false
                }
            };

        public static IEnumerable<object[]> DataArgumentException =>
            new List<object[]>
            {
                new object[]
                {
                    new PropertiesUpdatedPayload
                    {
                        Properties = null,
                        Id = "5956EF95-8755-47B8-9860-641147A60C9C"
                    }
                },
                new object[]
                {
                    new PropertiesUpdatedPayload
                    {
                        Id = "DBD9E9B2-DD52-4A38-B36F-E21684735027",
                        Properties = new Dictionary<string, object>()
                    }
                }
            };

        public static IEnumerable<object[]> DataFunctionArgumentNullException =>
            new List<object[]>
            {
                new object[]
                {
                    null,
                    new ProfilePropertiesChangedEvent(),
                },
                new object[]
                {
                    new PropertiesUpdatedPayload(),
                    null
                },
                new object[]
                {
                    new PropertiesUpdatedPayload(),
                    new ProfilePropertiesChangedEvent()
                }
            };

        public static IEnumerable<object[]> DataFunctionArgumentNull =>
            new List<object[]>
            {
                new object[]
                {
                    null,
                    new ProfilePropertiesChangedEvent()
                },
                new object[]
                {
                    new PropertiesUpdatedPayload(),
                    null
                }
            };

        public static IEnumerable<object[]> DataFunctionEvent =>
            new List<object[]>
            {
                new object[]
                {
                    new PropertiesUpdatedPayload
                    {
                        Id = "75F1B5D4-EE3E-4EC2-BA1D-7813C6A6FEB8",
                        Properties = new Dictionary<string, object>
                                     {
                                         { nameof(Member.Name), "NameGuide" },
                                         { nameof(Member.DisplayName), "DisplayGuide" }
                                     }
                    },

                    new ProfilePropertiesChangedEvent
                    {
                        MetaData = new EventMetaData
                                   {
                                       CorrelationId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF",
                                       ProcessId = "E21D474D-64A1-434E-80C9-B632298E80C2",
                                       HasToBeInverted = false,
                                       Initiator = new EventInitiator
                                                   {
                                                       Id = "52C60D7B-BCAD-43DF-A09B-6C9F9B17B04E",
                                                       Type = InitiatorType.ServiceAccount
                                                   },
                                       VersionInformation = 1
                                   },
                        Initiator = new Api.EventInitiator
                                    {
                                        Id = "52C60D7B-BCAD-43DF-A09B-6C9F9B17B04E",
                                        Type = Api.InitiatorType.ServiceAccount
                                    },
                        CorrelationId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF",
                        Payload = new PropertiesUpdatedPayload
                                  {
                                      Id = "F56BD0CE-7EEB-4EC5-9674-A7B6FDA52AD0",
                                      Properties = new Dictionary<string, object>
                                                   {
                                                       { "name", "Properties chagned" }
                                                   }
                                  },
                        Timestamp = DateTime.Now,
                        ProfileKind = ProfileKind.User,
                        VersionInformation = 1,
                        RequestSagaId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF"
                    },
                    
                    new FunctionPropertiesChangedEvent
                    {
                        Initiator = new Api.EventInitiator
                                    {
                                        Id = "52C60D7B-BCAD-43DF-A09B-6C9F9B17B04E",
                                        Type = Api.InitiatorType.ServiceAccount
                                    },
                        Payload = new PropertiesUpdatedPayload
                                  {
                                      Id = "75F1B5D4-EE3E-4EC2-BA1D-7813C6A6FEB8",

                                      Properties = new Dictionary<string, object>
                                                   {
                                                       { nameof(Member.Name), "NameGuide" },
                                                       { nameof(Member.DisplayName), "DisplayGuide" },
                                                       { nameof(UserBasic.UpdatedAt), DateTime.Now }
                                                   }
                                  },
                        Timestamp = DateTime.Now,
                        MetaData = new EventMetaData
                                   {
                                       CorrelationId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF",
                                       ProcessId = "E21D474D-64A1-434E-80C9-B632298E80C2",
                                       HasToBeInverted = false,
                                       Initiator = new EventInitiator
                                                   {
                                                       Id = "52C60D7B-BCAD-43DF-A09B-6C9F9B17B04E",
                                                       Type = InitiatorType.ServiceAccount
                                                   },
                                       VersionInformation = 1
                                   },
                        RequestSagaId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF",
                        CorrelationId = "E8BD9FAC-6E9D-4BD5-892A-1671407720FF",
                        VersionInformation = 2
                    }
                }
            };

        [Theory]
        [MemberData(nameof(DataMember))]
        public void PropertiesUpdatedPayloadExtensions_ResultTest(PropertiesUpdatedPayload payload, bool expectedResult)
        {
            bool result = payload.MembersHasToBeUpdated();
            result.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(DataArgumentException))]
        public void PropertiesUpdatedPayloadExtensions_ArgumentException(PropertiesUpdatedPayload payload)
        {
            Assert.Throws<ArgumentException>(() => payload.MembersHasToBeUpdated());
        }

        [Theory]
        [MemberData(nameof(DataFunctionArgumentNullException))]
        public void PropertiesCreatedPayloadExtensions_ArgumentException(
            PropertiesUpdatedPayload payload,
            Api.IDomainEvent domainEvent)
        {
            Assert.Throws<ArgumentNullException>(
                () => payload.CreateFunctionEventRelatedToPropertiesPayload(domainEvent));
        }

        [Theory]
        [MemberData(nameof(DataFunctionArgumentNull))]
        public void PropertiesCreatedPayloadExtensions_ArgumentExceptionNull(
            PropertiesUpdatedPayload payload,
            Api.IDomainEvent domainEvent)
        {
            Assert.Throws<ArgumentNullException>(
                () => payload.CreateFunctionEventRelatedToPropertiesPayload(domainEvent));
        }

        [Theory]
        [MemberData(nameof(DataFunctionEvent))]
        public void PropertiesCreatedPayloadExtensions_FunctionCreated(
            PropertiesUpdatedPayload payload,
            Api.IDomainEvent domainEvent,
            FunctionPropertiesChangedEvent expectedResult)
        {
            // ToDo: Think about test
            FunctionPropertiesChangedEvent result =
                payload.CreateFunctionEventRelatedToPropertiesPayload(domainEvent);

            expectedResult.Should()
                          .BeEquivalentTo(
                              result,
                              opt => opt.Excluding(op => op.EventId)
                                        .Excluding(op => op.MetaData.Batch)
                                        .Excluding(op => op.Timestamp)
                                        .Excluding(op => op.OldFunction)
                                        .Excluding(op => op.Payload.Properties)
                                        .RespectingRuntimeTypes());
        }
    }
}
