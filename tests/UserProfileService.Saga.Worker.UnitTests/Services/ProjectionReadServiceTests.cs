using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.StateMachine.Services;
using Xunit;

namespace UserProfileService.Saga.Worker.UnitTests.Services
{
    public class ProjectionReadServiceTests
    {
        [Theory]
        [ClassData(typeof(CheckGroupNameExistsAsyncTestData))]
        public async Task CheckGroupNameExistsAsync_Success_IfNameEqual(
            GroupNameTestObj testObj,
            IEnumerable<Group> groups)
        {
            // Arrange
            var readServiceMock = new Mock<IReadService>();

            IPaginatedList<IProfile> paginatedGroups = new PaginatedList<IProfile>(groups);

            readServiceMock
                .Setup(
                    s => s.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                        RequestedProfileKind.Group,
                        It.IsAny<AssignmentQueryObject>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(paginatedGroups);

            ILogger<ProjectionReadService> logger = new LoggerFactory().CreateLogger<ProjectionReadService>();

            var projectionReadService = new ProjectionReadService(readServiceMock.Object, logger);

            // Act
            bool duplicate = await projectionReadService.CheckGroupNameExistsAsync(
                testObj.Name,
                testObj.DisplayName,
                true,
                testObj.Id);

            // Assert
            Assert.Equal(testObj.ExpectedResult, duplicate);
        }

        /// <summary>
        ///     Class containing test data for CheckGroupNameExistsAsync.
        /// </summary>
        public class CheckGroupNameExistsAsyncTestData : IEnumerable<object[]>
        {
            private readonly List<Group> _emptyList = new List<Group>();

            private readonly List<Group> _groupTestList = new List<Group>
            {
                new Group
                {
                    Id = "123",
                    Name = "Group Test",
                    DisplayName = "Group Test"
                },
                new Group
                {
                    Id = "234",
                    Name = "Group Tests",
                    DisplayName = "Group Tests"
                }
            };

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <inheritdoc />
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new GroupNameTestObj("456", "Group Test", "Group Test", true), _groupTestList
                };

                yield return new object[] { new GroupNameTestObj("456", "groUp test", "Test", true), _groupTestList };
                yield return new object[] { new GroupNameTestObj("456", "group", "groUp test", true), _groupTestList };

                yield return new object[]
                {
                    new GroupNameTestObj("123", "Group Test", "Group Test", false), _groupTestList
                };

                yield return new object[] { new GroupNameTestObj("123", "groUp test", "Test", false), _groupTestList };
                yield return new object[] { new GroupNameTestObj("123", "group", "groUp test", false), _groupTestList };
                yield return new object[] { new GroupNameTestObj("123", "Group", "Group", false), _groupTestList };
                yield return new object[] { new GroupNameTestObj("123", "Group", "Group", false), _groupTestList };

                // Empty list
                yield return new object[]
                {
                    new GroupNameTestObj("456", "Group Test", "Group Test", false), _emptyList
                };

                yield return new object[] { new GroupNameTestObj("456", "groUp test", "Test", false), _emptyList };
                yield return new object[] { new GroupNameTestObj("456", "group", "groUp test", false), _emptyList };

                yield return new object[]
                {
                    new GroupNameTestObj("123", "Group Test", "Group Test", false), _emptyList
                };

                yield return new object[] { new GroupNameTestObj("123", "groUp test", "Test", false), _emptyList };
                yield return new object[] { new GroupNameTestObj("123", "group", "groUp test", false), _emptyList };
                yield return new object[] { new GroupNameTestObj("123", "Group", "Group", false), _emptyList };
                yield return new object[] { new GroupNameTestObj("123", "Group", "Group", false), _emptyList };
            }
        }

        public class GroupNameTestObj
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string DisplayName { get; set; }

            public bool ExpectedResult { get; set; }

            public GroupNameTestObj(string id, string name, string displayName, bool expectedResult)
            {
                Id = id;
                Name = name;
                DisplayName = displayName;
                ExpectedResult = expectedResult;
            }
        }
    }
}
