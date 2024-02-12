namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

internal class TestsDataStore
{
    private static TestsDataStore _cache;
    internal PropertiesChangedTestsData DataForPropertiesChangedTests { get; }

    internal static TestsDataStore Instance => _cache ??= new TestsDataStore();

    private TestsDataStore()
    {
        DataForPropertiesChangedTests = new PropertiesChangedTestsData();
    }
}