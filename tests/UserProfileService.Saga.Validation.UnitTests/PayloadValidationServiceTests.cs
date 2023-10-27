using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.DependencyInjection;
using UserProfileService.Validation.Abstractions;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests
{
    public class PayloadValidationServiceTests
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPayloadValidationService _validationService;

        public PayloadValidationServiceTests()
        {
            _loggerFactory = new LoggerFactory();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => _loggerFactory.CreateLogger<IPayloadValidationService>());

            serviceCollection.AddPayloadValidation();

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            _validationService = new PayloadValidationService(
                serviceProvider,
                _loggerFactory.CreateLogger<PayloadValidationService>());
        }

        [Fact]
        public void ValidateUpdateObjectProperties_Should_ReturnTrue_IfGroupMemberExists()
        {
            var propertiesUpdatedPayload = new PropertiesUpdatedPayload
            {
                Id = Guid.NewGuid().ToString(),
                Properties = new Dictionary<string, object>
                {
                    { nameof(GroupBasic.DisplayName), "Group 1" },
                    { nameof(GroupBasic.IsSystem), true },
                    { nameof(GroupBasic.Weight), 500 }
                }
            };

            ValidationResult result =
                _validationService.ValidateUpdateObjectProperties<GroupBasic>(propertiesUpdatedPayload);

            Assert.NotNull(result);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateUpdateObjectProperties_Should_ReturnTrue_IfUserMemberExists()
        {
            var propertiesUpdatedPayload = new PropertiesUpdatedPayload
            {
                Id = Guid.NewGuid().ToString(),
                Properties = new Dictionary<string, object>
                {
                    { nameof(UserBasic.DisplayName), "Group 1" },
                    { nameof(UserBasic.Email), "max.mueller@bechtle.com" },
                    { nameof(UserBasic.FirstName), "Max" },
                    { nameof(UserBasic.LastName), "Mueller" }
                }
            };

            ValidationResult result =
                _validationService.ValidateUpdateObjectProperties<UserBasic>(propertiesUpdatedPayload);

            Assert.NotNull(result);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("4711")]
        [InlineData("displayName")]
        [InlineData("Displayname")]
        [InlineData("displayname")]
        public void ValidateUpdateObjectProperties_Should_ReturnInvalidProperties_IfMemberNotExists(
            string invalidMember)
        {
            var propertiesUpdatedPayload = new PropertiesUpdatedPayload
            {
                Id = Guid.NewGuid().ToString(),
                Properties = new Dictionary<string, object>
                {
                    { invalidMember, "Group 1" },
                    { nameof(GroupBasic.IsSystem), true },
                    { nameof(GroupBasic.Weight), 500 }
                }
            };

            ValidationResult result =
                _validationService.ValidateUpdateObjectProperties<GroupBasic>(propertiesUpdatedPayload);

            Assert.False(result.IsValid);

            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors, r => r.Member == invalidMember);
        }

        [Theory]
        [MemberData(nameof(GetInvalidGroupMemberValues))]
        public void ValidateUpdateObjectProperties_Should_ReturnInvalidProperties_IfMemberValueIsInvalid(
            Dictionary<string, object> properties)
        {
            var propertiesUpdatedPayload = new PropertiesUpdatedPayload
            {
                Id = Guid.NewGuid().ToString(),
                Properties = properties
            };

            ValidationResult result =
                _validationService.ValidateUpdateObjectProperties<GroupBasic>(propertiesUpdatedPayload);

            Assert.False(result.IsValid);

            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors, r => r.Member == properties.First().Key);
        }

        public static IEnumerable<object[]> GetInvalidGroupMemberValues()
        {
            var allData = new List<object[]>
            {
                new object[]
                {
                    new Dictionary<string, object>
                    {
                        { nameof(GroupBasic.DisplayName), new List<string>() }
                    }
                },
                new object[]
                {
                    new Dictionary<string, object>
                    {
                        { nameof(GroupBasic.IsSystem), null }
                    }
                },
                new object[]
                {
                    new Dictionary<string, object>
                    {
                        { nameof(GroupBasic.IsSystem), "invalid" }
                    }
                }
            };

            return allData;
        }
    }
}
