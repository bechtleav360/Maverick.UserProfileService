using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Factories;
using UserProfileService.StateMachine.Services;
using Xunit;

namespace UserProfileService.Saga.Worker.UnitTests.Factories
{
    public class CommandServiceFactoryTests
    {
        [Theory]
        [InlineData("group-created", typeof(GroupCreatedMessageService))]
        [InlineData("function-created", typeof(FunctionCreatedMessageService))]
        public void CreateCommandServiceSuccess(string command, Type type)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(new Mock<IProjectionReadService>().Object)
                .AddSingleton(new Mock<IValidationService>().Object)
                .AddLogging()
                .BuildServiceProvider();

            var factory = new CommandServiceFactory(serviceProvider);

            ICommandService service = factory.CreateCommandService(command);

            Assert.IsType(type, service);
        }
    }
}
