using System;
using System.Linq;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using Xunit;

namespace UserProfileService.Projection.Common.Tests
{
    public class ModelExtensionsTests
    {
        [Fact]
        public void Generate_function_name_using_event_should_work()
        {
            FunctionCreated createdEvent = ResolvedEventFakers.NewFunctionCreated
                .Generate(1)
                .Single();

            Assert.Equal(
                $"{createdEvent.Organization.Name} {createdEvent.Role.Name}",
                createdEvent.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_event_missing_organization_should_fail()
        {
            FunctionCreated createdEvent = ResolvedEventFakers.NewFunctionCreated
                .Generate(1)
                .Single();

            createdEvent.Organization = null;

            Assert.Throws<ArgumentException>(() => createdEvent.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_event_missing_role_should_fail()
        {
            FunctionCreated createdEvent = ResolvedEventFakers.NewFunctionCreated
                .Generate(1)
                .Single();

            createdEvent.Role = null;

            Assert.Throws<ArgumentException>(() => createdEvent.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_null_as_event_should_fail()
        {
            Assert.Throws<ArgumentNullException>(() => ((FunctionCreated)null).GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_resolved_model_should_work()
        {
            Function function = ResolvedEventsModelFakers.NewFunction
                .Generate(1)
                .Single();

            Assert.Equal(
                $"{function.Organization.Name} {function.Role.Name}",
                function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_resolved_model_missing_organization_should_fail()
        {
            Function function = ResolvedEventsModelFakers.NewFunction
                .Generate(1)
                .Single();

            function.Organization = null;

            Assert.Throws<ArgumentException>(() => function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_resolved_model_missing_role_should_fail()
        {
            Function function = ResolvedEventsModelFakers.NewFunction
                .Generate(1)
                .Single();

            function.Role = null;

            Assert.Throws<ArgumentException>(() => function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_null_as_resolved_model_should_fail()
        {
            Assert.Throws<ArgumentNullException>(() => ((Function)null).GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_second_level_projection_model_should_work()
        {
            SecondLevelProjectionFunction function = MockDataGenerator.GenerateSecondLevelProjectionFunctions()
                .Single();

            Assert.Equal(
                $"{function.Organization.Name} {function.Role.Name}",
                function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_second_level_projection_model__missing_role_should_fail()
        {
            SecondLevelProjectionFunction function = MockDataGenerator.GenerateSecondLevelProjectionFunctions()
                .Single();

            function.Role = null;

            Assert.Throws<ArgumentException>(() => function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_second_level_projection_model_missing_organization_should_fail()
        {
            SecondLevelProjectionFunction function = MockDataGenerator.GenerateSecondLevelProjectionFunctions()
                .Single();

            function.Organization = null;

            Assert.Throws<ArgumentException>(() => function.GenerateFunctionName());
        }

        [Fact]
        public void Generate_function_name_using_null_as_second_level_projection_model_should_fail()
        {
            Assert.Throws<ArgumentNullException>(() => ((SecondLevelProjectionFunction)null).GenerateFunctionName());
        }
    }
}
