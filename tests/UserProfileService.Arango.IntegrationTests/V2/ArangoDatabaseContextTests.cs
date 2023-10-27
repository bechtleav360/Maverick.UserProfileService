using MassTransit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging.Abstractions;
using UserProfileService.Sync.States;
using Xunit;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using System.Linq.Expressions;
using System;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public.Extensions;
using UserProfileService.Messaging.ArangoDb.Configuration;
using UserProfileService.Messaging.ArangoDb.Saga;

namespace UserProfileService.Arango.IntegrationTests.V2
{
    public class ArangoDatabaseContextTests : ArangoDbTestBase
    {
        [Theory]
        [InlineData(20,10,0)]
        [InlineData(30, 10, 0)]
        [InlineData(15, 10, 10)]
        [InlineData(7, 7, 7)]
        [InlineData(11, 10, 10)]

        public async Task CountParameter_Return_by_LoadAsync_should_match(int totalAmount, int limit, int offset)
        {
           // Arrange
           IList<ProcessState> testData = GenerateProcessStates(totalAmount);
           var fixture = new ArangoDatabaseContextFixture(testData);

           IArangoDbClient arangoClient = await fixture.GetClientAsync();

           var databaseContext = new ArangoDatabaseContext<ProcessState>(arangoClient
              ,
               new ArangoSagaRepositoryOptions<ProcessState>(
                   ConcurrencyMode.Optimistic,
                   "",
                   fixture.CollectionName,
                  fixture.GetArangoDbClientName()),
               new NullLoggerFactory());

           // Act
           (int count, IList<ProcessState> results) exceptedResult = await databaseContext.LoadAsync(limit, offset);

           // Assert
           exceptedResult.Should().NotBeNull();
           exceptedResult.count.Should().Be(totalAmount);
           exceptedResult.results.Count.Should().Be((totalAmount - offset) >= limit ? limit : totalAmount - offset);

        }

        [Fact]
        public async Task TotalAmount_return_by_LoadAsync_should_be_right()
        {
            const int totalAmount = 10;
            const int limit = 5;
            const int offset = 3;

            // Arrange
            IList<ProcessState> testData = GenerateProcessStates(totalAmount);
            var fixture = new ArangoDatabaseContextFixture(testData);

            IArangoDbClient arangoClient = await fixture.GetClientAsync();

            var databaseContext = new ArangoDatabaseContext<ProcessState>(arangoClient
                                                                          ,
                                                                          new ArangoSagaRepositoryOptions<ProcessState>(
                                                                              ConcurrencyMode.Optimistic,
                                                                              "",
                                                                              fixture.CollectionName,
                                                                              fixture.GetArangoDbClientName()),
                                                                          new NullLoggerFactory());

            // Act
            (int totalAmountStateEntries, IList<ProcessState> resultSet) = await databaseContext.LoadAsync(
                limit,
                offset, 
                entry => entry.Process.FinishedAt);

            // Assert
            totalAmountStateEntries.Should().Be(totalAmount);
            resultSet.Count.Should().Be(limit);

        }

        private static IList<ProcessState> GenerateProcessStates(int amount)
        {
            var dataGenerator = new Fixture();

            return dataGenerator.Build<ProcessState>()
                                .Without(p => p.Exception)
                                .CreateMany(amount)
                                .ToList();
        }
    }
}
