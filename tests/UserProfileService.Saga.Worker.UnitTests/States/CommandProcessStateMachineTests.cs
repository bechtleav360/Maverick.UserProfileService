using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.Models.RequestModels;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using UserProfileService.Commands;
using UserProfileService.Commands.Attributes;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.EventSourcing.Abstractions.Stores;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Definitions;
using UserProfileService.StateMachine.Services;
using UserProfileService.Utilities;
using UserProfileService.Validation.Abstractions;
using UserProfileService.Validation.Abstractions.Configuration;
using UserProfileService.Validation.Abstractions.Message;
using Xunit;
using ValidationResult = UserProfileService.Validation.Abstractions.ValidationResult;

namespace UserProfileService.Saga.Worker.UnitTests.States
{
    public class CommandProcessStateMachineTests
    {
        /// <summary>
        ///     Submit Command
        ///     - ModifySubmitCommand: CommandService.ModifyAsync modify data display name to lower
        ///     - During-Submitted-When-ValidateCommand
        ///     - ThenAsync-ValidateSubmitCommand: CommandService.ValidateAsync return valid response and saved to saga.
        ///     - IfElse-ExternalValidationToTrigger: Result is true then publish ValidationTriggered
        ///     - Finish
        /// </summary>
        [Fact]
        public async Task Initially_When_SubmitCommand_ThenAsync_Success_IfExternalValidationEnabled()
        {
            // Arrange
            StateMachineFacade facade = await SetupStateMachine(true);

            // Act
            await facade.Harness.Bus.Publish(facade.Command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<SubmitCommand>());
            Assert.True(await facade.Harness.Consumed.Any<ValidateCommand>());

            ISagaStateMachineTestHarness<CommandProcessStateMachine, CommandProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<CommandProcessStateMachine, CommandProcessState>();

            Assert.True(await sagaHarness.Consumed.Any<SubmitCommand>());
            Assert.True(await sagaHarness.Consumed.Any<ValidateCommand>());

            CommandProcessState saga = AssertSagaData(facade, sagaHarness);

            Assert.True(await facade.Harness.Published.Any<ValidateCommand>());
            Assert.True(await facade.Harness.Published.Any<ValidationTriggered>());

            facade.MockCommandServiceFactory.Verify(
                m => m.CreateCommandService(facade.Command.Command),
                Times.Exactly(2));
            facade.MockCommandService.Verify(
                m => m.ModifyAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once);
            facade.MockCommandService.Verify(
                m => m.ValidateAsync(
                    It.IsAny<object>(),
                    It.Is<CommandInitiator>(c => c.Id == facade.Command.Initiator.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            string modifiedDataRaw = JsonConvert.SerializeObject(facade.ModifiedData);

            Assert.Equal(modifiedDataRaw, saga.Data);
        }

        /// <summary>
        ///     Submit Command
        ///     - ModifySubmitCommand: CommandService.ModifyAsync modify data display name to lower
        ///     - During-Submitted-When-ValidateCommand
        ///     - ThenAsync-ValidateSubmitCommand: CommandService.ValidateAsync return valid response and saved to saga.
        ///     - IfElse-ExternalValidationToTrigger: Result is false then publish ValidationCompositeResponse
        ///     - During-InternalValidated-When-ValidateCompositeResponse: response result is valid -> publish projection event
        ///     - Finish for this test
        /// </summary>
        [Fact]
        public async Task
            Initially_When_SubmitCommand_ThenAsync_Success__IfExternalValidationDisabled_WithValidInternalValidation()
        {
            // Arrange
            StateMachineFacade facade = await SetupStateMachine(false);

            // Act
            await facade.Harness.Bus.Publish(facade.Command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<SubmitCommand>());
            Assert.True(await facade.Harness.Consumed.Any<SubmitCommand>());

            ISagaStateMachineTestHarness<CommandProcessStateMachine, CommandProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<CommandProcessStateMachine, CommandProcessState>();

            Assert.True(await sagaHarness.Consumed.Any<SubmitCommand>());
            Assert.True(await sagaHarness.Consumed.Any<ValidateCommand>());

            CommandProcessState saga = AssertSagaData(facade, sagaHarness);

            Assert.True(await facade.Harness.Published.Any<ValidateCommand>());
            Assert.False(await facade.Harness.Published.Any<ValidationTriggered>());
            Assert.True(await facade.Harness.Published.Any<ValidationCompositeResponse>());
            Assert.False(await facade.Harness.Published.Any<SubmitCommandFailure>());

            facade.MockCommandServiceFactory.Verify(
                m => m.CreateCommandService(facade.Command.Command),
                Times.Exactly(3));

            facade.MockCommandService.Verify(
                m => m.ModifyAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once);

            facade.MockCommandService.Verify(
                m => m.ValidateAsync(
                    It.IsAny<object>(),
                    It.Is<CommandInitiator>(c => c.Id == facade.Command.Initiator.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            facade.MockEventStore.Verify(
                m => m.WriteEventAsync(
                    It.IsAny<IUserProfileServiceEvent>(),
                    It.IsAny<string>(),
                    default),
                Times.Never);

            facade.MockEventPublisherFactory.Verify(
                p => p.GetPublisher(It.Is<IUserProfileServiceEvent>(
                    e => e.MetaData.CorrelationId == "correlation-id")),
                Times.Once);

            facade.MockEventPublisher.Verify(
                p => p.PublishAsync(
                    It.Is<IUserProfileServiceEvent>(e => e.MetaData.CorrelationId == "correlation-id"),
                    It.IsAny<EventPublisherContext>(),
                    It.Is<CancellationToken>(ct => !ct.IsCancellationRequested)),
                Times.Once);

            string modifiedDataRaw = JsonConvert.SerializeObject(facade.ModifiedData);

            Assert.Equal(modifiedDataRaw, saga.Data);
        }

        /// <summary>
        ///     Submit Command
        ///     - ModifySubmitCommand: CommandService.ModifyAsync modify data display name to lower
        ///     - During-Submitted-When-ValidateCommand
        ///     - ThenAsync-ValidateSubmitCommand: CommandService.ValidateAsync return valid response and saved to saga.
        ///     - IfElse-ExternalValidationToTrigger: Result is false then publish ValidationCompositeResponse
        ///     - During-InternalValidated-When-ValidateCompositeResponse: response result is valid -> publish projection event
        ///     - Finish for this test
        /// </summary>
        [Fact]
        public async Task
            Initially_When_SubmitCommand_ThenAsync_Success__IfExternalValidationDisabled_WithInValidInternalValidation()
        {
            // Arrange
            const bool validInternalValidation = false;
            StateMachineFacade facade = await SetupStateMachine(false, validInternalValidation);

            // Act
            await facade.Harness.Bus.Publish(facade.Command);

            // Assert
            Assert.True(await facade.Harness.Consumed.Any<SubmitCommand>());
            Assert.True(await facade.Harness.Consumed.Any<SubmitCommand>());

            ISagaStateMachineTestHarness<CommandProcessStateMachine, CommandProcessState> sagaHarness =
                facade.Harness.GetSagaStateMachineHarness<CommandProcessStateMachine, CommandProcessState>();

            Assert.True(await sagaHarness.Consumed.Any<SubmitCommand>());
            Assert.True(await sagaHarness.Consumed.Any<ValidateCommand>());

            CommandProcessState saga = AssertSagaData(facade, sagaHarness);

            Assert.True(await facade.Harness.Published.Any<ValidateCommand>());
            Assert.False(await facade.Harness.Published.Any<ValidationTriggered>());
            Assert.True(await facade.Harness.Published.Any<SubmitCommandFailure>());
            Assert.True(await facade.Harness.Published.Any<ValidationCompositeResponse>());

            IEnumerable<IPublishedMessage> messages = facade.Harness.Published.Select(_ => true);
            IPublishedMessage message =
                messages.FirstOrDefault(c => c.GetType() == typeof(PublishedMessage<ValidationCompositeResponse>));

            Assert.NotNull(message);
            var response = Assert.IsType<ValidationCompositeResponse>(message.MessageObject);
            Assert.Equal(validInternalValidation, response.IsValid);

            facade.MockCommandServiceFactory.Verify(
                m => m.CreateCommandService(facade.Command.Command),
                Times.Exactly(2));

            facade.MockCommandService.Verify(
                m => m.ModifyAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.Once);

            facade.MockCommandService.Verify(
                m => m.ValidateAsync(
                    It.IsAny<object>(),
                    It.Is<CommandInitiator>(c => c.Id == facade.Command.Initiator.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            facade.MockEventStore.Verify(
                m => m.WriteEventAsync(
                    It.Is<IUserProfileServiceEvent>(e => e.MetaData.CorrelationId == "correlation-id"),
                    It.IsAny<string>(),
                    default),
                Times.Never);

            string modifiedDataRaw = JsonConvert.SerializeObject(facade.ModifiedData);

            Assert.Equal(modifiedDataRaw, saga.Data);
        }

        private CommandProcessState AssertSagaData(
            StateMachineFacade facade,
            ISagaStateMachineTestHarness<CommandProcessStateMachine, CommandProcessState> sagaHarness)
        {
            List<CommandProcessState> sagas = sagaHarness.Sagas.Select(_ => true).Select(s => s.Saga).ToList();
            CommandProcessState saga = Assert.Single(sagas);

            // Check if data parsed
            Assert.NotNull(saga.Data);
            Assert.NotEqual(saga.Data, facade.Command.Data); // because of modification
            Assert.NotNull(saga.EntityId);
            Assert.Equal(saga.Command, facade.Command.Command);
            Assert.NotEqual(saga.CorrelationId.ToString(), facade.Command.Id.Id);
            saga.CommandIdentifier.Should().BeEquivalentTo(facade.Command.Id);
            facade.Command.Initiator.Should().BeEquivalentTo(saga.Initiator);
            facade.ValidationResult.Should().BeEquivalentTo(saga.ValidationResult);

            return saga;
        }

        private async Task<StateMachineFacade> SetupStateMachine(
            bool externalValidationEnabled,
            bool validInternalValidation = true)
        {
            var facade = new StateMachineFacade();

            IMapper mapper = new MapperConfiguration(
                c => { c.AddProfile<MappingProfiles>(); }).CreateMapper();

            var mockedEventPublisher = new Mock<IEventPublisher>();

            mockedEventPublisher.Setup(
                p => p.PublishAsync(
                    It.IsAny<IUserProfileServiceEvent>(),
                    It.IsAny<EventPublisherContext>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            facade.MockEventPublisher = mockedEventPublisher;

            var mockedEventPublisherFactory = new Mock<IEventPublisherFactory>();

            mockedEventPublisherFactory.Setup(f 
                    => f.GetPublisher(It.IsAny<IUserProfileServiceEvent>()))
                .Returns(mockedEventPublisher.Object);

            facade.MockEventPublisherFactory = mockedEventPublisherFactory;

            CreateGroupRequest request = MockDataGeneratorRequests.CreateGroup();
            GroupCreatedMessage message = mapper.Map<CreateGroupRequest, GroupCreatedMessage>(request);
            string rawMessage = JsonConvert.SerializeObject(message);
            string commandStr = message.GetType().GetCustomAttribute<CommandAttribute>()?.Value;

            facade.Command = new SubmitCommand
                             {
                                 Data = rawMessage,
                                 Command = commandStr,
                                 Initiator = new CommandInitiator("test", CommandInitiatorType.System),
                                 Id = new CommandIdentifier(Guid.NewGuid().ToString(), Guid.NewGuid())
                             };

            facade.MockCommandServiceFactory = new Mock<ICommandServiceFactory>();
            facade.MockCommandService = new Mock<ICommandService>();

            var groupCreatedMessageService = new GroupCreatedMessageService(null, null);
            facade.ModifiedData = await groupCreatedMessageService.ModifyAsync(message);

            facade.MockCommandService
                  .Setup(m => m.ModifyAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(facade.ModifiedData);

            facade.ValidationResult = new ValidationResult();

            if (!validInternalValidation)
            {
                var validationAttribute = new ValidationAttribute("member", "error");
                facade.ValidationResult.Errors.Add(validationAttribute);
            }

            facade.MockCommandService
                  .Setup(
                      m => m.ValidateAsync(
                          It.IsAny<object>(),
                          It.Is<CommandInitiator>(i => i.Id == facade.Command.Initiator.Id),
                          It.IsAny<CancellationToken>()))
                  .ReturnsAsync(facade.ValidationResult);

            facade.MockCommandService.Setup(
                      m => m.CreateAsync(
                          It.IsAny<GroupCreatedMessage>(),
                          It.IsAny<string>(),
                          It.IsAny<string>(),
                          It.Is<CommandInitiator>(i => i.Id == facade.Command.Initiator.Id),
                          It.IsAny<CancellationToken>()))
                  .Returns(
                      groupCreatedMessageService.CreateAsync(
                          message,
                          "correlation-id",
                          "correlation-id",
                          facade.Command.Initiator));

            facade.MockCommandServiceFactory
                  .Setup(m => m.CreateCommandService(commandStr))
                  .Returns(facade.MockCommandService.Object);

            var validationConfiguration = new ValidationConfiguration
                                          {
                                              Commands = new CommandValidationConfiguration
                                                         {
                                                             External = new Dictionary<string, bool>
                                                                        {
                                                                            { commandStr ?? string.Empty, externalValidationEnabled }
                                                                        }
                                                         }
                                          };

            IOptions<ValidationConfiguration> validationOptions = Options.Create(validationConfiguration);
            var mockEventStore = new Mock<IEventStorageClient>();
            facade.MockEventStore = mockEventStore;
            

            ServiceProvider provider = new ServiceCollection()
                                       .AddLogging()
                                       .AddSingleton(facade.MockCommandServiceFactory.Object)
                                       .AddSingleton(validationOptions)
                                       .AddSingleton(mockEventStore.Object)
                                       .AddSingleton(mockedEventPublisherFactory.Object)
                                       .AddMassTransitTestHarness(
                                           cfg =>
                                           {
                                               cfg.AddSagaStateMachine<CommandProcessStateMachine,
                                                   CommandProcessState>();
                                           })
                                       .BuildServiceProvider(true);

            facade.Harness = provider.GetRequiredService<ITestHarness>();

            await facade.Harness.Start();

            return facade;
        }
    }

    public class StateMachineFacade
    {
        public ITestHarness Harness { get; set; }

        public SubmitCommand Command { get; set; }

        public ValidationResult ValidationResult { get; set; }

        public Mock<ICommandServiceFactory> MockCommandServiceFactory { get; set; }

        public Mock<ICommandService> MockCommandService { get; set; }

        public Mock<IEventStorageClient> MockEventStore { get; set; }

        public GroupCreatedMessage ModifiedData { get; set; }

        public Mock<IEventPublisher> MockEventPublisher { get; set; }

        public Mock<IEventPublisherFactory> MockEventPublisherFactory { get; set; }
    }
}
