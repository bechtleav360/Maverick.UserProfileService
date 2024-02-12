using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Maverick.Client.ArangoDb.Protocol;
using Maverick.Client.ArangoDb.Public;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.Client.ArangoDb.Public.Models.Document;
using Maverick.Client.ArangoDb.Public.Models.Transaction;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.UnitTests.Comparer;
using UserProfileService.Arango.UnitTests.V2.Helpers;
using UserProfileService.Arango.UnitTests.V2.TestModels;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using Xunit;
using AVMember = Maverick.UserProfileService.Models.Models.Member;

namespace UserProfileService.Arango.UnitTests.V2.Repository
{
    public class ArangoSecondLevelProjectionRepositoryTests
    {
        private const string CollectionPrefix = "Service";

        [Theory]
        [MemberData(nameof(ProfileData))]
        private async Task Get_Profile_should_work(IProfileEntityModel profile)
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());
            (ISecondLevelProjectionRepository, Mock<IArangoDbClient>) caller = GetConfiguredRepoAndArangoClient();
            ISecondLevelProjectionRepository repo = caller.Item1;
            Mock<IArangoDbClient> arangoClientMock = caller.Item2;
            string profileId = profile.Id;

            arangoClientMock.Setup(
                    c => c.GetDocumentAsync<IProfileEntityModel>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                .ReturnsAsync(
                    new GetDocumentResponse<IProfileEntityModel>(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.OK
                        },
                        profile));

            // Act
            ISecondLevelProjectionProfile result = await repo.GetProfileAsync(profileId, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.GetDocumentAsync<IProfileEntityModel>(
                    It.IsRegex($"{profileId}\\b"),
                    transaction.TransactionId,
                    false),
                Times.Once);

            result.Should().BeEquivalentTo(GetMapper().MapProfile(profile));
        }

        private static (ISecondLevelProjectionRepository, Mock<IArangoDbClient>) GetConfiguredRepoAndArangoClient(
            string transactionId = "my-transaction-Id")
        {
            var serviceProvider = new Mock<IServiceProvider>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();

            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            var arangoClient = new Mock<IArangoDbClient>();

            var arangoClientFactory = new Mock<IArangoDbClientFactory>();
            arangoClientFactory.Setup(c => c.Create(It.IsAny<string>())).Returns(arangoClient.Object);

            serviceProvider.Setup(c => c.GetService(typeof(IArangoDbClientFactory)))
                .Returns(arangoClientFactory.Object);

            var transactionResponse = new TransactionOperationResponse(
                new Response
                {
                    IsSuccessStatusCode = true,
                    StatusCode = HttpStatusCode.Accepted
                },
                new TransactionEntity
                {
                    Id = transactionId
                },
                new Connection("", "endpoints=http://localhost:8529;UserName=UPS;Password=1;database=upsv2"));

            arangoClient
                .Setup(c => c.AbortTransactionAsync(It.IsAny<string>()))
                .ReturnsAsync(transactionResponse);

            arangoClient
                .Setup(c => c.CommitTransactionAsync(It.IsAny<string>()))
                .ReturnsAsync(transactionResponse);

            arangoClient
                .Setup(
                    c => c.BeginTransactionAsync(
                        It.IsAny<IList<string>>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<TransactionOptions>()))
                .ReturnsAsync(transactionResponse);

            var repository = new ArangoSecondLevelProjectionRepository(
                new NullLogger<ArangoSecondLevelProjectionRepository>(),
                serviceProvider.Object,
                GetMapper(),
                CollectionPrefix);

            return (repository, arangoClient);
        }

        private static ArangoTransaction GetTestTransaction(string transactionId = null)
        {
            ModelBuilderOptions modelsInfo =
                DefaultModelConstellation.CreateNewSecondLevelProjection(CollectionPrefix).ModelsInfo;

            IList<string> collections = modelsInfo.GetDocumentCollections()
                .Union(modelsInfo.GetEdgeCollections())
                .Where(c => c != null)
                .ToList();

            return new ArangoTransaction
            {
                Collections = collections,
                TransactionId = transactionId
            };
        }

        private static IMapper GetMapper()
        {
            var config = new MapperConfiguration(
                cfg =>
                {
                    cfg.CreateMap<ISecondLevelProjectionProfile, IProfileEntityModel>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionUser, UserEntityModel>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionGroup, GroupEntityModel>().ReverseMap();

                    cfg.CreateMap<SecondLevelProjectionFunction, FunctionObjectEntityModel>()
                       .ForMember(
                           f => f.Name,
                           expression =>
                               expression.MapFrom((function, model) => model.Name = function.GenerateFunctionName()))
                       .ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionRole, RoleObjectEntityModel>().ReverseMap();
                    cfg.CreateMap<Role, RoleBasic>().ReverseMap();

                    cfg.CreateMap<ExternalIdentifier,
                            Maverick.UserProfileService.Models.Models.ExternalIdentifier>()
                        .ReverseMap();

                    cfg.CreateMap<Member, AVMember>().ReverseMap();

                    cfg.CreateMap<RangeCondition, Maverick.UserProfileService.Models.Models.RangeCondition>()
                        .ReverseMap();

                    cfg.CreateMap<Organization, OrganizationBasic>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionGroup, AVMember>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionFunction, AVMember>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionRole, AVMember>().ReverseMap();
                    cfg.CreateMap<SecondLevelProjectionOrganization, AVMember>().ReverseMap();
                    cfg.CreateMap<Member, AVMember>();
                });

            return new Mapper(config);
        }

        [Fact]
        public async Task StartTransaction_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository, Mock<IArangoDbClient>) caller =
                GetConfiguredRepoAndArangoClient(transaction?.TransactionId);

            ISecondLevelProjectionRepository repo = caller.Item1;
            Mock<IArangoDbClient> arangoClientMock = caller.Item2;

            // Act
            IDatabaseTransaction result = await repo.StartTransactionAsync();

            // Assert
            arangoClientMock.Verify(
                client => client.BeginTransactionAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<TransactionOptions>()),
                Times.Once);

            result.Should()
                .BeEquivalentTo(transaction, options => options.Excluding(r => r.TransactionLock));
        }

        [Fact]
        public async Task CommitTransaction_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            // Act
            await repo.CommitTransactionAsync(transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.CommitTransactionAsync(transaction.TransactionId),
                Times.Once);
        }

        [Fact]
        public async Task CommitTransaction_with_bad_type_should_throw()
        {
            // Arrange
            var transaction = new BadTransaction
            {
                TransactionId = Guid.NewGuid().ToString()
            };

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => repo.CommitTransactionAsync(transaction));
            Assert.Equal(nameof(transaction), result.ParamName);

            // Assert
            arangoClientMock.Verify(
                client => client.CommitTransactionAsync(transaction.TransactionId),
                Times.Never);
        }

        [Fact]
        public async Task AbortTransaction_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            // Act
            await repo.AbortTransactionAsync(transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.AbortTransactionAsync(transaction.TransactionId),
                Times.Once);
        }

        [Fact]
        public async Task AbortTransaction_with_bad_type_should_throw()
        {
            // Arrange
            var transaction = new BadTransaction
            {
                TransactionId = Guid.NewGuid().ToString()
            };

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            // Act & Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => repo.AbortTransactionAsync(transaction));
            Assert.Equal(nameof(transaction), result.ParamName);

            // Assert
            arangoClientMock.Verify(
                client => client.AbortTransactionAsync(transaction.TransactionId),
                Times.Never);
        }
        
        [Fact]
        public async Task Get_Function_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            FunctionObjectEntityModel function =
                SampleDataTestHelper.GetSampleFunctionEntityModel(Guid.NewGuid().ToString());

            arangoClientMock.Setup(
                    c => c.GetDocumentAsync<FunctionObjectEntityModel>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                .ReturnsAsync(
                    new GetDocumentResponse<FunctionObjectEntityModel>(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.OK
                        },
                        function));

            // Act
            SecondLevelProjectionFunction result = await repo.GetFunctionAsync(function.Id, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.GetDocumentAsync<FunctionObjectEntityModel>(
                    It.IsRegex($"{function.Id}\\b"),
                    transaction.TransactionId,
                    false),
                Times.Once);

            result.Should().BeEquivalentTo(GetMapper().Map<SecondLevelProjectionFunction>(function));
        }

        [Fact]
        public async Task Get_Role_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            var roleId = Guid.NewGuid().ToString();

            var role = new RoleObjectEntityModel
            {
                Id = roleId
            };

            arangoClientMock.Setup(
                    c => c.GetDocumentAsync<RoleObjectEntityModel>(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<bool>()))
                .ReturnsAsync(
                    new GetDocumentResponse<RoleObjectEntityModel>(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.OK
                        },
                        role));

            // Act
            SecondLevelProjectionRole result = await repo.GetRoleAsync(roleId, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.GetDocumentAsync<RoleObjectEntityModel>(
                    It.IsRegex($"{roleId}\\b"),
                    transaction.TransactionId,
                    false),
                Times.Once);

            result.Should().BeEquivalentTo(GetMapper().Map<SecondLevelProjectionRole>(role));
        }

        [Fact]
        public async Task Create_Profile_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            SecondLevelProjectionUser profile = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            JObject jDoc = GetMapper().MapProfile(profile).InjectDocumentKey(p => p.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await repo.CreateProfileAsync(profile, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    It.Is($"{CollectionPrefix}profilesQuery", StringComparer.OrdinalIgnoreCase),
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    It.Is(transaction.TransactionId, StringComparer.OrdinalIgnoreCase)),
                Times.Once);
        }

        [Fact]
        public async Task Create_Profile_with_null_id_should_throw()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            SecondLevelProjectionUser profile = MockDataGenerator.GenerateSecondLevelProjectionUser().Single();
            profile.Id = null;
            JObject jDoc = GetMapper().MapProfile(profile).InjectDocumentKey(p => p.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateProfileAsync(profile, transaction));

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    It.Is($"{CollectionPrefix}profilesQuery", StringComparer.OrdinalIgnoreCase),
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    It.Is(transaction.TransactionId, StringComparer.OrdinalIgnoreCase)),
                Times.Never);
        }

        [Fact]
        public async Task Create_Function_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            JObject jDoc = GetMapper()
                .Map<FunctionObjectEntityModel>(function)
                .InjectDocumentKey(profile => profile.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await repo.CreateFunctionAsync(function, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    It.Is($"{CollectionPrefix}rolesFunctionsQuery", StringComparer.OrdinalIgnoreCase),
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    It.Is(transaction.TransactionId, StringComparer.OrdinalIgnoreCase)),
                Times.Once);
        }

        [Fact]
        public async Task Create_Function_with_null_id_should_throw()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            SecondLevelProjectionFunction
                function = MockDataGenerator.GenerateSecondLevelProjectionFunctions().Single();

            function.Id = null;

            JObject jDoc = GetMapper()
                .Map<FunctionObjectEntityModel>(function)
                .InjectDocumentKey(profile => profile.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateFunctionAsync(function, transaction));

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    It.Is($"{CollectionPrefix}rolesFunctionsQuery", StringComparer.OrdinalIgnoreCase),
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    It.Is(transaction.TransactionId, StringComparer.OrdinalIgnoreCase)),
                Times.Never);
        }

        [Fact]
        public async Task Create_Role_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            SecondLevelProjectionRole role = MockDataGenerator.GenerateSecondLevelProjectionRoles().Single();
            JObject jDoc = GetMapper().Map<RoleObjectEntityModel>(role).InjectDocumentKey(r => r.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await repo.CreateRoleAsync(role, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    $"{CollectionPrefix}rolesFunctionsQuery",
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    transaction.TransactionId),
                Times.Once);
        }

        [Fact]
        public async Task Create_Tag_should_work()
        {
            // Arrange
            ArangoTransaction transaction = GetTestTransaction(Guid.NewGuid().ToString());

            (ISecondLevelProjectionRepository repo, Mock<IArangoDbClient> arangoClientMock) =
                GetConfiguredRepoAndArangoClient();

            Tag tag = MockDataGenerator.GenerateTagAggregateModels().Single();
            JObject jDoc = tag.InjectDocumentKey(t => t.Id);

            var creationOptions = new CreateDocumentOptions
            {
                Overwrite = false,
                ReturnNew = true,
                ReturnOld = true,
                OverWriteMode = AOverwriteMode.Conflict,
                WaitForSync = true
            };

            arangoClientMock.Setup(
                    c => c.CreateDocumentAsync(
                        It.IsAny<string>(),
                        It.IsAny<JObject>(),
                        It.IsAny<CreateDocumentOptions>(),
                        It.IsAny<string>()))
                .ReturnsAsync(
                    new CreateDocumentResponse(
                        new Response
                        {
                            IsSuccessStatusCode = true,
                            StatusCode = HttpStatusCode.Created
                        },
                        new DocumentResponseEntity()));

            // Act
            await repo.CreateTagAsync(tag, transaction);

            // Assert
            arangoClientMock.Verify(
                client => client.CreateDocumentAsync(
                    $"{CollectionPrefix}tagsQuery",
                    It.Is(jDoc, new JObjectComparer()),
                    It.Is(creationOptions, new CreateDocumentOptionsComparer()),
                    transaction.TransactionId),
                Times.Once);
        }

        public static IEnumerable<object[]> ProfileData()
        {
            IEnumerable<object[]> data = SampleDataTestHelper.GetTestGroupEntities()
                .Select(profile => new object[] { profile });

            return data;
        }
    }
}
