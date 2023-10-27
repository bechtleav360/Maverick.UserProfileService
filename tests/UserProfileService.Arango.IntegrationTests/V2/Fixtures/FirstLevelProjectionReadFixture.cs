namespace UserProfileService.Arango.IntegrationTests.V2.Fixtures
{
    public class FirstLevelProjectionReadFixture : FirstLevelProjectionFixtureBase
    {
        protected override string GetFirstLevelProjectionPrefix()
        {
            return FirstLevelProjectionReadPrefix;
        }
    }
}
