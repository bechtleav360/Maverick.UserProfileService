using System.Threading;
using System.Threading.Tasks;
using Moq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Utilities
{
    public static class DataBaseMockExtensions
    {
        /// <summary>
        ///     Apply setup of start and commit transaction methods.
        /// </summary>
        public static Mock<TRepo> ApplyWorkingTransactionSetup<TRepo>(
            this Mock<TRepo> mock,
            MockDatabaseTransaction transaction)
            where TRepo : class, IFirstLevelProjectionRepository
        {
            mock.Setup(r => r.StartTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction)
                .Verifiable();

            mock.Setup(
                    r => r.CommitTransactionAsync(
                        It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            return mock;
        }

        /// <summary>
        ///     Verifies calls of start and commit transaction methods.
        /// </summary>
        public static void VerifyWorkingTransactionMethods<TRepo>(
            this Mock<TRepo> mock,
            MockDatabaseTransaction transaction)
            where TRepo : class, IFirstLevelProjectionRepository
        {
            mock.Verify(
                r => r.StartTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Exactly(1));

            mock.Verify(
                r => r.CommitTransactionAsync(
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(1));
        }
    }
}
