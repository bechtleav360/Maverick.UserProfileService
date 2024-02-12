using System;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Mocks;

public class MockDatabaseTransaction : IDatabaseTransaction
{
    public string Id { get; } = Guid.NewGuid().ToString();

    public override string ToString()
    {
        return Id;
    }

    public CallingServiceContext CallingService { get; set; }
}