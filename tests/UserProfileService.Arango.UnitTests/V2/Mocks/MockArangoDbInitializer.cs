using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Enums;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Arango.UnitTests.V2.Mocks
{
    public class MockArangoDbInitializer : IDbInitializer
    {
        /// <inheritdoc />
        public Task<SchemaInitializationResponse> EnsureDatabaseAsync(
            bool forceRecreation = false,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SchemaInitializationResponse(SchemaInitializationStatus.Checked));
        }
    }
}
