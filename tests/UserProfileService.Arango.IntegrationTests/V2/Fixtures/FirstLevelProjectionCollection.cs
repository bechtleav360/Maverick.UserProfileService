using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    [CollectionDefinition(nameof(FirstLevelProjectionCollection), DisableParallelization = true)]
    public class FirstLevelProjectionCollection : ICollectionFixture<FirstLevelProjectionFixture>,
        ICollectionFixture<FirstLevelProjectionReadFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
