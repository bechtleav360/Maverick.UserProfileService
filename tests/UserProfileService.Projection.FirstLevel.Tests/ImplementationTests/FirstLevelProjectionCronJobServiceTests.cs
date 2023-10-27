using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Services;
using UserProfileService.Projection.FirstLevel.Tests.Mocks;
using UserProfileService.Projection.FirstLevel.Tests.Utilities;
using Xunit;

namespace UserProfileService.Projection.FirstLevel.Tests.ImplementationTests
{
    public class FirstLevelProjectionCronJobServiceTests
    {
        [Fact]
        public async Task Execute_should_work()
        {
            var cancellationToken = new CancellationToken();
            Mock<ITemporaryAssignmentsExecutor> mock = MockProvider.GetDefaultMock<ITemporaryAssignmentsExecutor>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                    s.AddSingleton<ICronJobService, FirstLevelProjectionCronJobService>()
                        .AddSingleton(mock.Object));

            var sut = services.GetRequiredService<ICronJobService>();

            await sut.ExecuteAsync(cancellationToken);

            mock.Verify(
                o => o
                    .CheckTemporaryAssignmentsAsync(ItShould.BeEquivalentTo(cancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task Execute_but_cancelled_should_work()
        {
            var cancellationToken = new CancellationToken(true);
            Mock<ITemporaryAssignmentsExecutor> mock = MockProvider.GetDefaultMock<ITemporaryAssignmentsExecutor>();

            IServiceProvider services = FirstLevelHandlerTestsPreparationHelper.GetWithDefaultTestSetup(
                s =>
                    s.AddSingleton<ICronJobService, FirstLevelProjectionCronJobService>()
                        .AddSingleton(mock.Object));

            var sut = services.GetRequiredService<ICronJobService>();

            await sut.ExecuteAsync(cancellationToken);

            mock.Verify(
                o => o
                    .CheckTemporaryAssignmentsAsync(ItShould.BeEquivalentTo(cancellationToken)),
                Times.Never);
        }
    }
}
