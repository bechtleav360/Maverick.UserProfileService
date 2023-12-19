using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Commands.Attributes;
using UserProfileService.Saga.Events.Contracts;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.StateMachine.Implementations;
using Xunit;
using Xunit.Sdk;

namespace UserProfileService.Saga.Worker.UnitTests.Factories;

public class DefaultSagaCommandFactoryTests
{
    private readonly DefaultSagaCommandFactory _factory;

    public DefaultSagaCommandFactoryTests()
    {
        var mockLogger = new Mock<ILogger<DefaultSagaCommandFactory>>();
        mockLogger.Setup(l => l.Log(It.Is<LogLevel>(
                                                    level => level == LogLevel.Critical || level == LogLevel.Error),
                                    It.IsAny<EventId>(),
                                    new It.IsAnyType(),
                                    It.IsAny<Exception>(),
                                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                  .Throws(new XunitException("No error log messages allowed!"));
        _factory = new DefaultSagaCommandFactory(mockLogger.Object);
    }

    [Theory]
    [MemberData(nameof(GetTestArgumentsWithValidCommandNames))]
    public void ConstructSagaCommand_ValidCommandName_ReturnsSagaCommand(string commandName, Type expectedType)
    {
        // Act
        var result = _factory.ConstructSagaCommand(commandName);
        // Assert
        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.CommandName.Should().Be(commandName);
        result.ExactType.Should().Be(expectedType);
    }

    [Fact]
    public void ConstructSagaCommand_InvalidCommandName_ThrowsException()
    {
        // Arrange
        var commandName = "InvalidCommand";
        // Act & Assert
        _factory.Invoking(y => y.ConstructSagaCommand(commandName))
                .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ConstructSagaCommand_EmptyCommandName_ThrowsException()
    {
        // Arrange
        var commandName = "";
        // Act & Assert
        _factory.Invoking(y => y.ConstructSagaCommand(commandName))
                .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DetermineCommandServiceType_ValidCommandName_ReturnsType()
    {
        // Arrange
        var commandName = CommandConstants.GroupCreate;
        // Act
        var result = _factory.DetermineCommandServiceType(commandName);
        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void DetermineCommandServiceType_InvalidCommandName_ThrowsException()
    {
        // Arrange
        var commandName = "InvalidCommand";
        // Act & Assert
        _factory.Invoking(y => y.DetermineCommandServiceType(commandName))
                .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void DetermineCommandServiceType_EmptyCommandName_ThrowsException()
    {
        // Arrange
        var commandName = "";
        // Act & Assert
        _factory.Invoking(y => y.DetermineCommandServiceType(commandName))
                .Should().Throw<ArgumentException>();
    }

    public static IEnumerable<object[]> GetTestArgumentsWithValidCommandNames()
    {
        var mapping = GetDefaultCommandNameToCommandTypesMapping();

        foreach (var commandName in GetDefaultCommandNames())
        {
            if (!mapping.TryGetValue(commandName, out var commandType))
            {
                continue;
            }

            yield return new object[] { commandName, commandType };
        }
    }

    private static IEnumerable<string> GetDefaultCommandNames()
    {
        return typeof(CommandConstants)
               .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
               .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
               .Select(f => (string)f.GetRawConstantValue());
    }

    private static Dictionary<string, Type> GetDefaultCommandNameToCommandTypesMapping()
    {
        return typeof(GroupCreatedMessage)
               .Assembly
               .GetTypes()
               .Where(t => !string.IsNullOrEmpty(t.GetCustomAttribute<CommandAttribute>()?.Value))
               .ToDictionary(t => t.GetCustomAttribute<CommandAttribute>()!.Value, t => t);
    }
}