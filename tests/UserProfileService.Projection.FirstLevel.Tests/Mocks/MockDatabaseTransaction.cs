using System;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Mocks
{
    public class MockDatabaseTransaction : IDatabaseTransaction
    {
        public string Id { get; }

        public MockDatabaseTransaction()
        {
            Id = Guid.NewGuid().ToString();
        }

        public MockDatabaseTransaction(string transactionId)
        {
            Id = transactionId;
        }

        public override string ToString()
        {
            return Id;
        }

        public CallingServiceContext CallingService { get; set; }
    }
}
