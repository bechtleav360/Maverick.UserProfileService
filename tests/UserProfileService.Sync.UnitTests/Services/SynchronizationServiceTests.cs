using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using Maverick.UserProfileService.Models.EnumModels;
using FluentAssertions;
using MassTransit;
using MassTransit.Saga;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserProfileService.Messaging.ArangoDb.Saga;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.Messages.Responses;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.Views;
using UserProfileService.Sync.Services;
using UserProfileService.Sync.States;
using UserProfileService.Sync.Utilities;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Services
{
    public class SynchronizationServiceTests
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task StartSynchronizationAsync_Success(bool schedule, bool isActive)
        {
            // Arrange
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();
            var synchronizer = new Mock<ISyncProcessSynchronizer>();

            scheduleService.Setup(s => s.GetScheduleAsync(default))
                           .ReturnsAsync(
                               new SyncSchedule
                               { IsActive = isActive });

            var messageBroker = new Mock<IBus>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var service = new SynchronizationService(
                messageBroker.Object,
                logger,
                scheduleService.Object,
                synchronizer.Object,
                null,
                null);

            // Act
            await service.StartSynchronizationAsync("correlation-id", schedule, default);

            // Assert
            messageBroker.Verify(
                s => s.Publish(It.IsAny<StartSyncCommand>(), default),
                Times.Once);
        }

        [Fact]
        public async Task
            StartSynchronizationAsync_Should_Throw_OperationInvalidException_IfScheduleDeactivated_And_ScheduleIsDeactivated()
        {
            // Arrange
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            scheduleService.Setup(s => s.GetScheduleAsync(default))
                           .ReturnsAsync(
                               new SyncSchedule
                               { IsActive = false });

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var service = new SynchronizationService(null, logger, scheduleService.Object,null, null, null);

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.StartSynchronizationAsync("correlation-id", true, default));
        }

        [Fact]
        public async Task GetDetailedProcess_Should_CallRepo_with_right_parameter()
        {
            Guid id = Guid.NewGuid();
            // Arrange
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);
            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            mock.Setup(
                    m => m.Execute(
                        It.IsAny<Func<SagaRepositoryContext<ProcessState>, Task<ProcessState>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessState());

            //Act 
            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object,null, mock.Object, GetMapper());
            await service.GetDetailedProcessAsync(id);

            //Assert
            service.IdOfLoadedEntity.Should().Be(id);
        }

        [Fact]
        public async Task GetDetailedProcess_Should_Throw_By_Empty_Id()
        {
            Guid id = Guid.Empty;
            // Arrange
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);
            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            mock.Setup(
                    m => m.Execute(
                        It.IsAny<Func<SagaRepositoryContext<ProcessState>, Task<ProcessState>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessState());
            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object,null, mock.Object, GetMapper());
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetDetailedProcessAsync(id));
        }

        [Theory]
        [MemberData(nameof(GetTestDataForSyncStatus))]
        public async Task GetStatus_Should_work(Tuple<int, IList<ProcessState>> processData, bool isSyncLockAvailable, SyncStatus expectedStatus)
        {
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();
            
            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var synchronizerMock = new Mock<ISyncProcessSynchronizer>();

            synchronizerMock.Setup(s => s.IsSyncLockAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(isSyncLockAvailable);

            var mockRepo = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();


            mockRepo.Setup(
                    m => m.ExecuteQuery(
                        It.IsAny<Func<ISagaRepositoryQueryContext<ProcessState>,
                            Task<Tuple<int, IList<ProcessState>>>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(processData);

            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object, synchronizerMock.Object, mockRepo.Object, GetMapper());

            // Act
            var status = await service.GetSyncStatusAsync();

            // Assert
            synchronizerMock.Verify(s => s.IsSyncLockAvailableAsync(It.IsAny<CancellationToken>()), Times.Once);
            status.Should().BeEquivalentTo(expectedStatus, options => options.Excluding(c => c.Process ));
        }

        /// <summary>
        ///     This test also implicitly tests the mapping from ProcessState to ProcessView.
        /// </summary>
        [Theory,AutoData]
        public async Task GetDetailedProcess_Should_Return_Expected_Output(Guid id, ProcessState process)
        {
            ProcessDetail expectedProcess = GetProcessView(process);

            // Arrange
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);
            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            mock.Setup(
                    m => m.Execute(
                        It.IsAny<Func<SagaRepositoryContext<ProcessState>, Task<ProcessState>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(process);
            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object, null, mock.Object, GetMapper());

            //Act
            ProcessDetail loadedProcess = await service.GetDetailedProcessAsync(id);
            loadedProcess.Should().BeEquivalentTo(expectedProcess);

        }

        [Fact]
        public async Task GetRunningSyncProcess_should_return_empty_list_by_no_running_process()
        {
            // Arrange
            var mapper = GetMapper();
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            var testTuple = new Tuple<int, IList<ProcessState>>(1, new List<ProcessState>());

            mock.Setup(
                    m => m.ExecuteQuery(
                        It.IsAny<Func<ISagaRepositoryQueryContext<ProcessState>,
                            Task<Tuple<int, IList<ProcessState>>>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(testTuple);

            var service = new SynchronizationTestService(
                null,
                logger,
                syncOptions,
                scheduleService.Object,
                null,
                mock.Object,
                mapper);

            //Act
            IList<ProcessView> runningProcess = (await service.GetRunningSyncProcessAsync())
                .ToList();

            //Assert
            runningProcess.Should().NotBeNull();
            runningProcess.Should().BeEmpty();
        }

        [Theory]
        [AutoData]
        public async Task GetRunningSyncProcess_should_return_running_process(ProcessState process)
        {
            // Arrange
            var mapper = GetMapper();
           

            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            var testTuple = new Tuple<int, IList<ProcessState>>(1, new List<ProcessState>{process});

            mock.Setup(
                    m => m.ExecuteQuery(
                        It.IsAny<Func<ISagaRepositoryQueryContext<ProcessState>,
                            Task<Tuple<int, IList<ProcessState>>>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(testTuple);

            var service = new SynchronizationTestService(
                null,
                logger,
                syncOptions,
                scheduleService.Object,
                null,
                mock.Object,
                mapper);

            var expectedResult = new List<ProcessView>{mapper.Map<ProcessView>(process)};

            //Act
            IList<ProcessView> runningProcess = (await service.GetRunningSyncProcessAsync())
                .ToList();

            //Assert
            runningProcess.Should().NotBeNull();
            runningProcess.Should().BeEquivalentTo(expectedResult);
        }


        [Theory]
        [MemberData(nameof(GetAllProcessData))]
        public async Task GetAllProcesses_Should_work(
           QueryObject query, Expression<Func<ProcessState, object>> expectedExpression)
        {
            // Arrange
            var mapper = GetMapper();
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();
            
            var testTuple = new Tuple<int, IList<ProcessState>>(1, new List<ProcessState>());

            mock.Setup(
                    m => m.ExecuteQuery(
                        It.IsAny<Func<ISagaRepositoryQueryContext<ProcessState>,
                            Task<Tuple<int, IList<ProcessState>>>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    testTuple);

            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object,null, mock.Object, mapper);
            //Act
            await service.GetAllProcessesAsync(query);

            //Assert
            service.SortOrder.Should().Be(query.SortOrder);
            service.Limit.Should().Be(query.PageSize);
            service.Offset.Should().Be((query.Page - 1) * query.PageSize);
            service.SortExpression.Should().BeEquivalentTo(expectedExpression);
        }

        [Theory]
        [MemberData(nameof(GetAllProcessDataWithInvalidSortParameter))]
        public void GetAllProcesses_Should_Throw_By_Unknown_Sorting_Parameter(
            QueryObject query)
        {
            // Arrange
            var mapper = GetMapper();
            ILogger<SynchronizationService> logger = new LoggerFactory().CreateLogger<SynchronizationService>();
            var scheduleService = new Mock<IScheduleService>();

            IOptions<SyncConfiguration> syncOptions = new OptionsWrapper<SyncConfiguration>(null);

            var mock = new Mock<ISagaRepositoryQueryContextFactory<ProcessState>>();

            var testTuple = new Tuple<int, IList<ProcessState>>(1, new List<ProcessState>());

            mock.Setup(
                    m => m.ExecuteQuery(
                        It.IsAny<Func<ISagaRepositoryQueryContext<ProcessState>,
                            Task<Tuple<int, IList<ProcessState>>>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    testTuple);

            var service = new SynchronizationTestService(null, logger, syncOptions, scheduleService.Object,null, mock.Object, mapper);
            //Act & Assert
            Func<Task>act = async () => await service.GetAllProcessesAsync(query);
            act.Should().ThrowAsync<ArgumentException>();
            
        }


        public static IEnumerable<object[]> GetAllProcessData()
        {
            
            Expression<Func<ProcessState, object>> startedAtExpr = c => c.Process.StartedAt; 
            Expression<Func<ProcessState, object>> finishedAtExpr = c => c.Process.FinishedAt;
            Expression<Func<ProcessState, object>> lastActivityExpr = c => c.Process.UpdatedAt;
            Expression<Func<ProcessState, object>> statusExpr = c => c.Process.Status;
            Expression<Func<ProcessState, object>> initiatorDisplayNameExpr = c => c.Initiator.DisplayName;
            Expression<Func<ProcessState, object>> initiatorNameExpr = c => c.Initiator.Name;

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "startedAt",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Asc
                             },
                            startedAtExpr
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "finishedAt",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Asc
                             },
                             finishedAtExpr
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "lastActivity",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Desc
                             },
                             lastActivityExpr
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "status",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Desc

                             },
                             statusExpr
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "displayName",
                                 Page = 1,
                                 PageSize = 5,
                                 SortOrder = SortOrder.Desc

                             },
                             initiatorDisplayNameExpr
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "Name",
                                 Page = 1,
                                 PageSize = 5,
                                 SortOrder = SortOrder.Desc

                             },
                             initiatorNameExpr
                         };
        }

        public static IEnumerable<object[]> GetAllProcessDataWithInvalidSortParameter()
        {
            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "startedAt_",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Asc
                             },
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "finishedAt_",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Asc
                             },
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "lastActivity_",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Desc
                             },
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "status_",
                                 Page = 2,
                                 PageSize = 10,
                                 SortOrder = SortOrder.Desc

                             },
                             
                         };

            yield return new object[]
                         {
                             new QueryObject
                             {
                                 OrderedBy = "initiator_",
                                 Page = 1,
                                 PageSize = 5,
                                 SortOrder = SortOrder.Desc

                             },
                         };
        }

        private static IMapper GetMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>());

            return new Mapper(mapperConfig);
        }

        private ProcessDetail GetProcessView(ProcessState process)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            var result = new ProcessDetail
                         {
                             UpdatedAt = process.Process.UpdatedAt,
                             FinishedAt = process.Process.FinishedAt,
                             Initiator = process.Initiator,
                             Status = process.Process.Status,
                             StartedAt = process.Process.StartedAt,
                             Systems = process.Process.Systems.ToDictionary(
                                 keyValuePair => keyValuePair.Key,
                                 keyValuePair => new SystemView
                                                 {
                                                     IsCompleted = keyValuePair.Value.IsCompleted,
                                                     Steps = keyValuePair.Value.Steps.Select(
                                                                             s => new KeyValuePair<string, StepView>(
                                                                                 s.Key,
                                                                                 new StepView
                                                                                 {
                                                                                     Final = s.Value?.Final,
                                                                                     Handled = s.Value?.Handled,
                                                                                     Temporary = s.Value?.Temporary,
                                                                                     Status = s.Value?.Status
                                                                                 }))
                                                                         .ToDictionary(o => o.Key, o => o.Value)
                                                 })
                         };

            return result;
        }

        public static IEnumerable<object[]> GetTestDataForSyncStatus()
        {
            var testProcess = new Fixture().Build<ProcessState>().Create();
            testProcess.Process.FinishedAt = null;

            yield return new object[] { new Tuple<int, IList<ProcessState>>(0, new List<ProcessState>()), true, new SyncStatus{ IsRunning = false} };
            yield return new object[] { new Tuple<int, IList<ProcessState>>(1, new List<ProcessState>{testProcess}), false, new SyncStatus { IsRunning = true} };

        }

       
    }
}
