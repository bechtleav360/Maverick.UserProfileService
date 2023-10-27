using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Projection.Handlers;
using UserProfileService.Sync.Projection.UnitTests.Utilities;
using Xunit;
using EntityDeleted = Maverick.UserProfileService.AggregateEvents.Resolved.V1.EntityDeleted;

namespace UserProfileService.Sync.Projection.UnitTests.EventHandlerTests
{
    public class EntityDeletedTests
    {
        [Theory]
        [AutoData]
        public void Handle_message_with_bad_arguments_should_throw(EntityDeleted entityDeleted)
        {
            entityDeleted.Id = "";

            //arrange
            Mock<IProfileService> profileServiceMock = HandlerTestsPreparationHelper.GetProfileServiceMock();

            var sourceSystemFactoryMock = new Mock<ISyncSourceSystemFactory>();


            IServiceProvider services = HandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s => { s.AddSingleton(profileServiceMock.Object); s.AddSingleton(sourceSystemFactoryMock.Object); s.AddSingleton(new SyncConfiguration()); });

            var sut = ActivatorUtilities.CreateInstance<EntityDeletedEventHandler>(services);
            
            // act
            Func<Task> act = async () =>
                             await sut.HandleEventAsync(
                                 entityDeleted.AddDefaultMetadata(services),
                                 entityDeleted.GenerateEventHeader(14));

            act.Should().ThrowAsync<ArgumentException>();

        }
    }
}
