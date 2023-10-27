namespace UserProfileService.OpenApiSpec.Examples;

/// <summary>
///     The implementation of <see cref="IExampleProvider" /> that generates data for the CreateUSerSettingSection API
///     method.
/// </summary>
public class CreateUserSettingsSectionExampleData : IExampleProvider
{
    /// <inheritdoc />
    public object GetExample()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "referenceId", "CFC9366A-9EFB-4AE6-AE18-CFC874736E7E" },
                {
                    "links", new List<Dictionary<string, string>>
                    {
                        new Dictionary<string, string>
                        {
                            { "objectId", "43162AA8-E653-473C-AEFC-6141CD7FCCC2" },
                            { "name", "document #1" }
                        },
                        new Dictionary<string, string>
                        {
                            { "objectId", "AC9F3ED2-054F-45AE-8A3D-2E5E0994D488" },
                            { "name", "my item" }
                        }
                    }
                },
                { "relatedUserId", "6474F4C8-0F79-40F6-BA72-0245FBE38A5E" }
            },
            new Dictionary<string, object>
            {
                { "referenceId", "BD6823AC-2D0A-4B1D-8464-A65BAF0EAAD9" },
                { "relatedUserId", null }
            }
        };
    }
}
