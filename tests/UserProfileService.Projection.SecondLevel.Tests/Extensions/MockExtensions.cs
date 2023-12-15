using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Tests.Mocks;

namespace UserProfileService.Projection.SecondLevel.Tests.Extensions;

public static class MockExtensions
{
    /// <summary>
    ///     Apply setup of start and commit transaction methods.
    /// </summary>
    public static Mock<TRepo> ApplyWorkingTransactionSetup<TRepo>(
        this Mock<TRepo> mock,
        MockDatabaseTransaction transaction)
        where TRepo : class, ISecondLevelProjectionRepository
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

        mock.Setup(
                r => r.UpdateProfilePropertiesAsync(
                    It.IsAny<string>(),
                    It.IsNotNull<IDictionary<string, object>>(),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        mock.Setup(
                r => r.UpdateFunctionPropertiesAsync(
                    It.IsAny<string>(),
                    It.IsNotNull<IDictionary<string, object>>(),
                    It.Is<IDatabaseTransaction>(t => ((MockDatabaseTransaction)t).Id == transaction.Id),
                    It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        mock.Setup(
                r => r.UpdateRolePropertiesAsync(
                    It.IsAny<string>(),
                    It.IsNotNull<IDictionary<string, object>>(),
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
        where TRepo : class, ISecondLevelProjectionRepository
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