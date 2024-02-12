using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Common.UnitTests.Helpers
{
    public class MockDatabaseTransaction : IDatabaseTransaction
    {
        public CallingServiceContext CallingService { get; set; }
    }
}
