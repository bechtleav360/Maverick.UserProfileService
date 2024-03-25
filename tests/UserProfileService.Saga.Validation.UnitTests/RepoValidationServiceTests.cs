using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Validation.Abstractions;
using Xunit;

namespace UserProfileService.Saga.Validation.UnitTests
{
    public class RepoValidationServiceTests
    {
        private readonly Mock<IValidationReadService> _mockReadService;
        private readonly IRepoValidationService _service;

        public RepoValidationServiceTests()
        {
            _mockReadService = new Mock<IValidationReadService>();
            ILogger<RepoValidationService> logger = new LoggerFactory().CreateLogger<RepoValidationService>();

            _service = new RepoValidationService(_mockReadService.Object, logger);
        }

        #region ValidateDuplicateFunctionAsync

        [Fact]
        public async Task ValidateDuplicateFunctionAsync_Success_ReturnInValid_IfFunctionsExists()
        {
            // Arrange
            var roleId = "role 1";
            var oeId = "oe 1";
            List<FunctionBasic> functions = MockDataGenerator.GenerateFunctionBasicInstances(2);

            _mockReadService.Setup(t => t.GetFunctionsAsync(roleId, oeId)).ReturnsAsync(functions);

            // Act
            ValidationResult result = await _service.ValidateDuplicateFunctionAsync(roleId, oeId);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(t => t.GetFunctionsAsync(roleId, oeId), Times.Once);
        }

        [Fact]
        public async Task ValidateDuplicateFunctionAsync_Success_ReturnValid_IfFunctionsNotExists()
        {
            // Arrange
            var roleId = "role 1";
            var oeId = "oe 1";

            _mockReadService.Setup(t => t.GetFunctionsAsync(roleId, oeId)).ReturnsAsync(new List<FunctionBasic>());

            // Act
            ValidationResult result = await _service.ValidateDuplicateFunctionAsync(roleId, oeId);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(t => t.GetFunctionsAsync(roleId, oeId), Times.Once);
        }

        [Theory]
        [InlineData("test", null)]
        [InlineData(null, "test")]
        public async Task ValidateDuplicateFunctionAsync_Should_Throw_ArgumentNullException_IfRoleOrOrganizationIdIsNull(
            string roleId,
            string oeId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateDuplicateFunctionAsync(roleId, oeId));
        }

        [Theory]
        [InlineData("test", "")]
        [InlineData("test", " ")]
        [InlineData("", "test")]
        [InlineData(" ", "test")]
        public async Task ValidateDuplicateFunctionAsync_Should_Throw_ArgumentNullException_IfRoleOrOrganizationIdIsEmpty(
            string roleId,
            string oeId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateDuplicateFunctionAsync(roleId, oeId));
        }

        #endregion

        #region ValidateRoleExistsAsync

        [Fact]
        public async Task ValidateRoleExistsAsync_Success_ReturnValid_IfRoleExists()
        {
            // Arrange
            RoleBasic role = MockDataGenerator.GenerateRoleBasicInstances().First();

            _mockReadService.Setup(t => t.GetRoleAsync(role.Id)).ReturnsAsync(role);

            // Act
            ValidationResult result = await _service.ValidateRoleExistsAsync(role.Id);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(t => t.GetRoleAsync(role.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateRoleExistsAsync_Success_ReturnInValid_IfRoleNotExists()
        {
            // Arrange
            RoleBasic role = MockDataGenerator.GenerateRoleBasicInstances().First();

            _mockReadService.Setup(t => t.GetRoleAsync(role.Id)).ReturnsAsync((RoleBasic)null);

            // Act
            ValidationResult result = await _service.ValidateRoleExistsAsync(role.Id);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(t => t.GetRoleAsync(role.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateRoleExistsAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateRoleExistsAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task  ValidateRoleExistsAsync_Should_Throw_ArgumentNullException_IfIdIsEmpty(string id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateRoleExistsAsync(id));
        }

        #endregion

        #region ValidateOrganizationExistsAsync

        [Fact]
        public async Task ValidateOrganizationExistsAsync_Success_ReturnValid_IfOrganizationExists()
        {
            // Arrange
            OrganizationBasic organization = MockDataGenerator.GenerateOrganizationBasicInstances().First();

            _mockReadService.Setup(t => t.CheckProfileExistsAsync(organization.Id, organization.Kind))
                .ReturnsAsync(true);

            // Act
            ValidationResult result = await _service.ValidateOrganizationExistsAsync(organization.Id);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(t => t.CheckProfileExistsAsync(organization.Id, organization.Kind), Times.Once);
        }

        [Fact]
        public async Task ValidateOrganizationExistsAsync_Success_ReturnInValid_IfOrganizationNotExists()
        {
            // Arrange
            OrganizationBasic organization = MockDataGenerator.GenerateOrganizationBasicInstances().First();

            _mockReadService.Setup(t => t.CheckProfileExistsAsync(organization.Id, organization.Kind))
                .ReturnsAsync(false);

            // Act
            ValidationResult result = await _service.ValidateOrganizationExistsAsync(organization.Id);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(t => t.CheckProfileExistsAsync(organization.Id, organization.Kind), Times.Once);
        }

        [Fact]
        public async Task ValidateOrganizationExistsAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateOrganizationExistsAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateOrganizationExistsAsync_Should_Throw_ArgumentNullException_IfIdIsEmpty(string id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateOrganizationExistsAsync(id));
        }

        #endregion

        #region ValidateFunctionExistsAsync

        [Fact]
        public async Task ValidateFunctionExistsAsync_Success_ReturnValid_IfFunctionExists()
        {
            // Arrange
            FunctionView function = MockDataGenerator.GenerateFunctionViewInstance();

            _mockReadService.Setup(t => t.GetFunctionAsync(function.Id)).ReturnsAsync(function);

            // Act
            ValidationResult result = await _service.ValidateFunctionExistsAsync(function.Id);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(t => t.GetFunctionAsync(function.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateFunctionExistsAsync_Success_ReturnInValid_IfFunctionNotExists()
        {
            // Arrange
            FunctionView function = MockDataGenerator.GenerateFunctionViewInstance();

            _mockReadService.Setup(t => t.GetFunctionAsync(function.Id)).ReturnsAsync((FunctionView)null);

            // Act
            ValidationResult result = await _service.ValidateFunctionExistsAsync(function.Id);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(t => t.GetFunctionAsync(function.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateFunctionExistsAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
           await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateFunctionExistsAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateFunctionExistsAsync_Should_Throw_ArgumentNullException_IfIdIsEmpty(string id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateFunctionExistsAsync(id));
        }

        #endregion

        #region ValidateTagExistsAsync

        [Fact]
        public async Task ValidateTagExistsAsync_Success_ReturnValid_IfTagExists()
        {
            // Arrange
            Tag tag = MockDataGenerator.GenerateTags().First();

            _mockReadService.Setup(t => t.GetTagAsync(tag.Id)).ReturnsAsync(tag);

            // Act
            ValidationResult result = await _service.ValidateTagExistsAsync(tag.Id);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(t => t.GetTagAsync(tag.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateTagExistsAsync_Success_ReturnInValid_IfTagNotExists()
        {
            // Arrange
            Tag tag = MockDataGenerator.GenerateTags().First();

            _mockReadService.Setup(t => t.GetTagAsync(tag.Id)).ReturnsAsync((Tag)null);

            // Act
            ValidationResult result = await _service.ValidateTagExistsAsync(tag.Id);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(t => t.GetTagAsync(tag.Id), Times.Once);
        }

        [Fact]
        public async Task ValidateTagExistsAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateTagExistsAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateTagExistsAsync_Should_Throw_ArgumentNullException_IfIdIsEmpty(string id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateTagExistsAsync(id));
        }

        #endregion

        #region ValidateTagsExistAsync

        [Fact]
        public async Task ValidateTagsExistAsync_Success_ReturnValid_IfTagsNotExists()
        {
            // Arrange
            List<Tag> tagsExists = MockDataGenerator.GenerateTags(2);
            List<Tag> tagsNotExists = MockDataGenerator.GenerateTags(2);

            Dictionary<string, bool> tagsExistsIds = tagsExists.ToDictionary(t => t.Id, _ => true);
            Dictionary<string, bool> tagsNotExistsIds = tagsNotExists.ToDictionary(t => t.Id, _ => false);

            string[] tagIds = tagsExistsIds.Union(tagsNotExistsIds).Select(t => t.Key).ToArray();

            Dictionary<string, bool> tagExists =
                tagsExistsIds.Union(tagsNotExistsIds).ToDictionary(t => t.Key, t => t.Value);

            _mockReadService.Setup(t => t.CheckTagsExistAsync(tagIds)).ReturnsAsync(tagExists);

            // Act
            ValidationResult result = await _service.ValidateTagsExistAsync(tagIds);

            // Assert
            Assert.False(result.IsValid);
            ValidationAttribute error = Assert.Single(result.Errors);

            object ids = Assert.Single(error.AdditionalInformation, t => t.Key == "Ids").Value;

            ids
                .Should()
                .BeEquivalentTo(tagsNotExistsIds.Select(t => t.Key));

            _mockReadService.Verify(t => t.CheckTagsExistAsync(tagIds), Times.Once);
        }

        [Fact]
        public async Task ValidateTagsExistAsync_Success_ReturnValid_IfTagsExists()
        {
            // Arrange
            List<Tag> tagsExists = MockDataGenerator.GenerateTags(2);

            Dictionary<string, bool> tagsExistsIds = tagsExists.ToDictionary(t => t.Id, _ => true);

            string[] tagIds = tagsExistsIds.Select(t => t.Key).ToArray();

            _mockReadService.Setup(t => t.CheckTagsExistAsync(tagIds)).ReturnsAsync(tagsExistsIds);

            // Act
            ValidationResult result = await _service.ValidateTagsExistAsync(tagIds);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateTagsExistAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateTagsExistAsync(null));
        }

        #endregion

        #region ValidateGroupExistsAsync

        [Fact]
        public async Task ValidateGroupExistsAsync_Success_ReturnInValid_IfGroupExists()
        {
            // Arrange
            var groupId = "";
            GroupBasic group = MockDataGenerator.GenerateGroupBasicInstances().First();

            _mockReadService.Setup(t => t.CheckGroupNameExistsAsync(group.Name, group.DisplayName, true, groupId))
                .ReturnsAsync(true);

            // Act
            ValidationResult result = await _service.ValidateGroupExistsAsync(group.Name, group.DisplayName, groupId);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);

            _mockReadService.Verify(
                t => t.CheckGroupNameExistsAsync(group.Name, group.DisplayName, true, groupId),
                Times.Once);
        }

        [Fact]
        public async Task ValidateGroupExistsAsync_Success_ReturnValid_IfGroupNotExists()
        {
            // Arrange
            var groupId = "";
            GroupBasic group = MockDataGenerator.GenerateGroupBasicInstances().First();

            _mockReadService.Setup(t => t.CheckGroupNameExistsAsync(group.Name, group.DisplayName, true, groupId))
                .ReturnsAsync(false);

            // Act
            ValidationResult result = await _service.ValidateGroupExistsAsync(group.Name, group.DisplayName, groupId);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(
                t => t.CheckGroupNameExistsAsync(group.Name, group.DisplayName, true, groupId),
                Times.Once);
        }

        #endregion

        #region ValidateUserEmailExistsAsync

        [Fact]
        public async Task ValidateUserEmailExistsAsync_Success_ReturnInValid_IfEmailExists()
        {
            // Arrange
            UserBasic user = MockDataGenerator.GenerateUserBasicInstances().First();

            _mockReadService.Setup(t => t.CheckUserEmailExistsAsync(user.Email, user.Id))
                .ReturnsAsync(true);

            // Act
            ValidationResult result = await _service.ValidateUserEmailExistsAsync(user.Email, user.Id);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(
                t => t.CheckUserEmailExistsAsync(user.Email, user.Id),
                Times.Once);
        }

        [Fact]
        public async Task ValidateUserEmailExistsAsync_Success_ReturnValid_IfEmailNotExists()
        {
            // Arrange
            UserBasic user = MockDataGenerator.GenerateUserBasicInstances().First();

            _mockReadService.Setup(t => t.CheckUserEmailExistsAsync(user.Email, user.Id))
                .ReturnsAsync(false);

            // Act
            ValidationResult result = await _service.ValidateUserEmailExistsAsync(user.Email, user.Id);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(
                t => t.CheckUserEmailExistsAsync(user.Email, user.Id),
                Times.Once);
        }

        [Fact]
        public async Task ValidateUserEmailExistsAsync_Should_Throw_ArgumentNullException_IfEmailIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateUserEmailExistsAsync(null, "id123"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateUserEmailExistsAsync_Should_Throw_ArgumentNullException_IfEmailIsEmpty(string email)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateUserEmailExistsAsync(email, "id123"));
        }

        #endregion

        #region ValidateObjectExistsAsync

        [Fact]
        public async Task ValidateObjectExistsAsync_Success_ReturnValid_IfObjectExists()
        {
            // Arrange
            ObjectIdent obj = MockDataGenerator.GenerateObjectIdentInstances().First();

            _mockReadService.Setup(t => t.CheckObjectExistsAsync(obj))
                .ReturnsAsync(true);

            // Act
            ValidationResult result = await _service.ValidateObjectExistsAsync(obj);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(
                t => t.CheckObjectExistsAsync(obj),
                Times.Once);
        }

        [Fact]
        public async Task ValidateObjectExistsAsync_Success_ReturnInValid_IfObjectNotExists()
        {
            // Arrange
            ObjectIdent obj = MockDataGenerator.GenerateObjectIdentInstances().First();

            _mockReadService.Setup(t => t.CheckObjectExistsAsync(obj))
                .ReturnsAsync(false);

            // Act
            ValidationResult result = await _service.ValidateObjectExistsAsync(obj);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(
                t => t.CheckObjectExistsAsync(obj),
                Times.Once);
        }

        [Fact]
        public async Task ValidateObjectExistsAsync_Should_Throw_ArgumentNullException_IfObjectIdentIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateObjectExistsAsync(null));
        }

        #endregion

        #region ValidateProfileExistsAsync

        [Fact]
        public async Task ValidateProfileExistsAsync_Success_ReturnValid_IfProfileExists()
        {
            // Arrange
            IProfile profile = MockDataGenerator.GenerateProfileInstances().First();
            var profileIdent = new ProfileIdent(profile.Id, profile.Kind);

            _mockReadService.Setup(t => t.GetProfileAsync(profileIdent.Id, profileIdent.ProfileKind))
                .ReturnsAsync(profile);

            // Act
            ValidationResult<IProfile> result = await _service.ValidateProfileExistsAsync(profileIdent);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Facade);

            profile.Should().BeEquivalentTo(result.Facade);

            _mockReadService.Verify(
                t => t.GetProfileAsync(profileIdent.Id, profileIdent.ProfileKind),
                Times.Once);
        }

        [Fact]
        public async Task ValidateProfileExistsAsync_Success_ReturnInValid_IfProfileNotExists()
        {
            // Arrange
            IProfile profile = MockDataGenerator.GenerateProfileInstances().First();
            var profileIdent = new ProfileIdent(profile.Id, profile.Kind);

            _mockReadService.Setup(t => t.GetProfileAsync(profileIdent.Id, profileIdent.ProfileKind))
                .ReturnsAsync((IProfile)null);

            // Act
            ValidationResult<IProfile> result = await _service.ValidateProfileExistsAsync(profileIdent);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Null(result.Facade);

            _mockReadService.Verify(
                t => t.GetProfileAsync(profileIdent.Id, profileIdent.ProfileKind),
                Times.Once);
        }

        [Fact]
        public async Task ValidateObjectExistsAsync_Should_Throw_ArgumentNullException_IfProfileIdentIsNull()
        {
            // Act & Assert
           await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateProfileExistsAsync(null));
        }

        #endregion

        #region ValidateObjectExistsAsync

        [Fact]
        public async Task ValidateObjectsExistAsync_Success_ReturnValid_IfObjectsExists()
        {
            // Arrange
            List<IObjectIdent> objects = MockDataGenerator.GenerateObjectIdentInstances().ToList<IObjectIdent>();

            foreach (IObjectIdent obj in objects)
            {
                _mockReadService.Setup(t => t.CheckObjectExistsAsync(obj))
                    .ReturnsAsync(true);
            }

            // Act
            ValidationResult result = await _service.ValidateObjectsExistAsync(objects);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            foreach (IObjectIdent obj in objects)
            {
                _mockReadService.Verify(t => t.CheckObjectExistsAsync(obj), Times.Once);
            }
        }

        [Fact]
        public async Task ValidateObjectsExistAsync_Success_ReturnInValid_IfObjectsNotExists()
        {
            // Arrange
            List<IObjectIdent> objects = MockDataGenerator.GenerateObjectIdentInstances().ToList<IObjectIdent>();

            foreach (IObjectIdent obj in objects)
            {
                _mockReadService.Setup(t => t.CheckObjectExistsAsync(obj))
                    .ReturnsAsync(false);
            }

            // Act
            ValidationResult result = await _service.ValidateObjectsExistAsync(objects);

            // Assert
            Assert.False(result.IsValid);

            ValidationAttribute error = Assert.Single(result.Errors);

            object ids = Assert.Single(error.AdditionalInformation, t => t.Key == "Ids").Value;

            ids
                .Should()
                .BeEquivalentTo(objects);

            foreach (IObjectIdent obj in objects)
            {
                _mockReadService.Verify(t => t.CheckObjectExistsAsync(obj), Times.Once);
            }
        }

        [Fact]
        public async Task ValidateObjectsExistAsync_Should_Throw_ArgumentNullException_IfObjectIdentsListIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateObjectsExistAsync(null));
        }

        #endregion

        #region ValidateClientSettingsExistsAsync

        [Fact]
        public async Task ValidateClientSettingsExistsAsync_Success_ReturnValid_IfSettingsExists()
        {
            // Arrange
            var settingsKey = "key-1";
            ProfileIdent profileIdent = MockDataGenerator.GenerateProfileIdent(1, ProfileKind.Organization).First();
            JObject settings = JObject.FromObject(MockDataGenerator.GenerateUserInstances().First());

            _mockReadService.Setup(
                    t => t.GetSettingsOfProfileAsync(
                        profileIdent.Id,
                        profileIdent.ProfileKind,
                        settingsKey))
                .ReturnsAsync(settings);

            // Act
            ValidationResult<JObject> result =
                await _service.ValidateClientSettingsExistsAsync(profileIdent, settingsKey);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.NotNull(result.Facade);

            settings.Should().BeEquivalentTo(result.Facade);

            _mockReadService.Verify(
                t => t.GetSettingsOfProfileAsync(profileIdent.Id, profileIdent.ProfileKind, settingsKey),
                Times.Once);
        }

        [Fact]
        public async Task ValidateClientSettingsExistsAsync_Success_ReturnValid_IfSettingsHasNoValues()
        {
            // Arrange
            var settingsKey = "key-1";
            ProfileIdent profileIdent = MockDataGenerator.GenerateProfileIdent(1, ProfileKind.Organization).First();
            var settings = new JObject();

            _mockReadService.Setup(
                    t => t.GetSettingsOfProfileAsync(
                        profileIdent.Id,
                        profileIdent.ProfileKind,
                        settingsKey))
                .ReturnsAsync(settings);

            // Act
            ValidationResult<JObject> result =
                await _service.ValidateClientSettingsExistsAsync(profileIdent, settingsKey);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(
                t => t.GetSettingsOfProfileAsync(profileIdent.Id, profileIdent.ProfileKind, settingsKey),
                Times.Once);
        }

        [Fact]
        public async Task ValidateClientSettingsExistsAsync_Success_ReturnValid_IfSettingsAreNull()
        {
            // Arrange
            var settingsKey = "key-1";
            ProfileIdent profileIdent = MockDataGenerator.GenerateProfileIdent(1, ProfileKind.Organization).First();

            _mockReadService.Setup(
                    t => t.GetSettingsOfProfileAsync(
                        profileIdent.Id,
                        profileIdent.ProfileKind,
                        settingsKey))
                .ReturnsAsync((JObject)null);

            // Act
            ValidationResult<JObject> result =
                await _service.ValidateClientSettingsExistsAsync(profileIdent, settingsKey);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);

            _mockReadService.Verify(
                t => t.GetSettingsOfProfileAsync(profileIdent.Id, profileIdent.ProfileKind, settingsKey),
                Times.Once);
        }

        [Fact]
        public async Task ValidateClientSettingsExistsAsync_Should_Throw_ArgumentNullException_IfProfileIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateClientSettingsExistsAsync(null, "oe1"));
        }

        [Fact]
        public async Task ValidateClientSettingsExistsAsync_Should_Throw_ArgumentNullException_IfKeyIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.ValidateClientSettingsExistsAsync(new ProfileIdent("id", ProfileKind.User), null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateClientSettingsExistsAsync_Should_Throw_ArgumentNullException_IfKeyIsEmpty(string key)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.ValidateClientSettingsExistsAsync(new ProfileIdent("id", ProfileKind.User), key));
        }

        #endregion

        #region ValidateContainerProfileAssignmentGraphAsync

        [Fact]
        public async Task ValidateContainerProfileAssignmentGraphAsync_Success_IfAssignmentsNoContainerProfile()
        {
            // Arrange
            var objectIdent = new ObjectIdent("id1", ObjectType.Group);

            ICollection<ConditionObjectIdent> assignments = new List<ConditionObjectIdent>
            {
                new ConditionObjectIdent("user1", ObjectType.User),
                new ConditionObjectIdent("user2", ObjectType.User)
            };

            // Act
            ValidationResult result =
                await _service.ValidateContainerProfileAssignmentGraphAsync(objectIdent, assignments);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateContainerProfileAssignmentGraphAsync_Success_IfParentsNotEqualToAssignments()
        {
            // Arrange
            var objectIdent = new ObjectIdent("id1", ObjectType.Group);

            ICollection<ConditionObjectIdent> assignments = new List<ConditionObjectIdent>
            {
                new ConditionObjectIdent("user1", ObjectType.User),
                new ConditionObjectIdent("group1", ObjectType.Group)
            };

            string[] parents = { "group2", "group3" };

            _mockReadService.Setup(t => t.GetAllParentsOfProfile(objectIdent.Id))
                .ReturnsAsync(parents);

            // Act
            ValidationResult result =
                await _service.ValidateContainerProfileAssignmentGraphAsync(objectIdent, assignments);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateContainerProfileAssignmentGraphAsync_Success_IfParentsEqualToAssignments()
        {
            // Arrange
            var objectIdent = new ObjectIdent("id1", ObjectType.Group);

            ICollection<ConditionObjectIdent> assignments = new List<ConditionObjectIdent>
            {
                new ConditionObjectIdent("user1", ObjectType.User),
                new ConditionObjectIdent("group1", ObjectType.Group)
            };

            string[] parents = { "group1", "group2" };

            _mockReadService.Setup(t => t.GetAllParentsOfProfile(objectIdent.Id))
                .ReturnsAsync(parents);

            // Act
            ValidationResult result =
                await _service.ValidateContainerProfileAssignmentGraphAsync(objectIdent, assignments);

            // Assert
            Assert.False(result.IsValid);

            ValidationAttribute error = Assert.Single(result.Errors);

            object ids = Assert.Single(error.AdditionalInformation, t => t.Key == "Ids").Value;

            var expectedInvalidParents = new List<string>
            {
                "group1"
            };

            IEnumerable<string> idList = Assert.IsAssignableFrom<IEnumerable<ConditionObjectIdent>>(ids)
                .Select(i => i.Id);

            expectedInvalidParents
                .Should()
                .BeEquivalentTo(idList);
        }

        [Fact]
        public async Task
            ValidateContainerProfileAssignmentGraphAsync_Should_Throw_ArgumentNullException_IfObjectIdentIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.ValidateContainerProfileAssignmentGraphAsync(null, new List<ConditionObjectIdent>()));
        }

        [Fact]
        public async Task
            ValidateContainerProfileAssignmentGraphAsync_Should_Throw_ArgumentNullException_IfAssignmentListIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.ValidateContainerProfileAssignmentGraphAsync(
                    new ObjectIdent("123", ObjectType.Function),
                    null));
        }

        #endregion

        #region ValidateRoleAssignmentsAsync

        [Fact]
        public async Task ValidateRoleAssignmentsAsync_Success_ReturnValid_IfNoAssignments()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            string[] assignments = Array.Empty<string>();

            _mockReadService.Setup(t => t.GetRoleFunctionAssignmentsAsync(roleId))
                .ReturnsAsync(assignments);

            // Act
            ValidationResult result = await _service.ValidateRoleAssignmentsAsync(roleId);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            _mockReadService.Verify(
                t => t.GetRoleFunctionAssignmentsAsync(roleId),
                Times.Once);
        }

        [Fact]
        public async Task ValidateRoleAssignmentsAsync_Success_ReturnInValid_IfAssignments()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var assignments = new[] { "function 1", "function - 2" };

            _mockReadService.Setup(t => t.GetRoleFunctionAssignmentsAsync(roleId))
                .ReturnsAsync(assignments);

            // Act
            ValidationResult result = await _service.ValidateRoleAssignmentsAsync(roleId);

            // Assert
            Assert.False(result.IsValid);
            ValidationAttribute error = Assert.Single(result.Errors);

            object ids = Assert.Single(error.AdditionalInformation, t => t.Key == "FunctionIds").Value;

            ids
                .Should()
                .BeEquivalentTo(assignments);

            _mockReadService.Verify(
                t => t.GetRoleFunctionAssignmentsAsync(roleId),
                Times.Once);
        }

        [Fact]
        public async Task ValidateRoleAssignmentsAsync_Should_Throw_ArgumentNullException_IfIdIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.ValidateRoleAssignmentsAsync(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ValidateRoleAssignmentsAsync_Should_Throw_ArgumentNullException_IfIdIsEmpty(string id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ValidateRoleAssignmentsAsync(id));
        }

        #endregion
    }
}
