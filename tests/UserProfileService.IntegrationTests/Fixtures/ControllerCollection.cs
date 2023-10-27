using Xunit;

namespace UserProfileService.IntegrationTests.Fixtures
{
    [CollectionDefinition(nameof(ControllerCollection), DisableParallelization = true)]
    public class ControllerCollection : ICollectionFixture<ControllerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
