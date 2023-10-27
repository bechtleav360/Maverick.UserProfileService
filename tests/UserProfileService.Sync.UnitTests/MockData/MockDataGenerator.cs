using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.UnitTests.MockData
{
    /// <summary>
    ///     Generates some mock data for the sync.
    /// </summary>
    internal class MockDataGenerator
    {
        internal static IList<ProcessView> GenerateSyncProcess(int number = 1)
        {
            return new Faker<ProcessView>()
                   .RuleFor(faker => faker.StartedAt, faker => faker.Date.Past().ToUniversalTime())
                   .RuleFor(
                       faker => faker.LastActivity,
                       (faker, actualSyncP) => actualSyncP.StartedAt!.Value.AddHours(faker.Random.Int(1, 10)))
                   .RuleFor(
                       faker => faker.FinishedAt,
                       (faker, actualSyncP)
                           => actualSyncP.LastActivity!.Value.AddMinutes(faker.Random.Int(0, 60)).OrNull(faker, 0.15f))
                   .RuleFor(
                       faker => faker.Status,
                       (faker, actualSyncP) => actualSyncP.FinishedAt != null
                           ? ProcessStatus.Success
                           : faker.PickRandomWithout(ProcessStatus.Success, ProcessStatus.Initialize))
                   .RuleFor(faker => faker.Initiator, faker => GenerateInitiator().Single())
                   .RuleFor(
                       faker => faker.SyncOperations,
                       (faker, actualSyncP) => new Operations
                       {
                           Groups = faker.Random.Int(10, 25),
                           Organizations = faker.Random.Int(10, 50),
                           Roles = faker.Random.Int(10, 50),
                           Users = faker.Random.Int(50, 200)
                       })
                   .Generate(number);
        }

        internal static IList<ActionInitiator> GenerateInitiator(int number = 1)
        {
            return new Faker<ActionInitiator>()
                   .RuleFor(faker => faker.Name, faker => faker.Name.LastName())
                   .RuleFor(faker => faker.DisplayName, (faker, actualInitiator) => actualInitiator.Name)
                   .RuleFor(faker => faker.Id, faker => Guid.NewGuid().ToString())
                   .Generate(number);
        }
    }
}
