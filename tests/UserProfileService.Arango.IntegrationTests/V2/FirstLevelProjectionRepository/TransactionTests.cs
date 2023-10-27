using System;
using System.Linq;
using System.Threading.Tasks;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class TransactionTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;
        private readonly ITestOutputHelper _output;

        public TransactionTests(FirstLevelProjectionFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _output = outputHelper;
        }

        [Fact]
        public async Task Create_transaction_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IDatabaseTransaction transaction = await repo.StartTransactionAsync();
            Assert.NotNull(transaction);
            Assert.IsAssignableFrom<ArangoTransaction>(transaction);

            var arangoTransaction = (ArangoTransaction)transaction;
            Assert.False(string.IsNullOrWhiteSpace(arangoTransaction.TransactionId));
            Assert.True(arangoTransaction.IsActive);
            Assert.NotEmpty(arangoTransaction.Collections);
        }

        [Fact]
        public async Task Commit_transaction_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IDatabaseTransaction transaction = await repo.StartTransactionAsync();

            await repo.CommitTransactionAsync(transaction);
            var arangoTransaction = (ArangoTransaction)transaction;
            Assert.False(arangoTransaction.IsActive);
        }

        [Fact]
        public async Task Commit_transaction_should_throw()
        {
            ModelBuilderOptions modelsInfo = DefaultModelConstellation
                .CreateNewFirstLevelProjection(FirstLevelProjectionPrefix)
                .ModelsInfo;

            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var transaction = new ArangoTransaction
            {
                Collections = modelsInfo.GetEdgeCollections()
                    .Concat(modelsInfo.GetDocumentCollections())
                    .ToList(),
                TransactionId = Guid.NewGuid().ToString()
            };

            await Assert.ThrowsAsync<DatabaseException>(() => repo.CommitTransactionAsync(transaction));
        }

        [Fact]
        public async Task Abort_transaction_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            IDatabaseTransaction transaction = await repo.StartTransactionAsync();

            await repo.AbortTransactionAsync(transaction);
            var arangoTransaction = (ArangoTransaction)transaction;
            Assert.False(arangoTransaction.IsActive);
        }

        [Fact]
        public async Task Abort_invalid_transaction_should_throw()
        {
            ModelBuilderOptions modelsInfo = DefaultModelConstellation
                .CreateNewFirstLevelProjection(FirstLevelProjectionPrefix)
                .ModelsInfo;

            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var transaction = new ArangoTransaction
            {
                Collections = modelsInfo.GetEdgeCollections()
                    .Concat(modelsInfo.GetDocumentCollections())
                    .ToList(),
                TransactionId = Guid.NewGuid().ToString()
            };

            await Assert.ThrowsAsync<DatabaseException>(() => repo.AbortTransactionAsync(transaction));
        }
    }
}
