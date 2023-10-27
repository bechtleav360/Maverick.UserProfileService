namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data
{
    public static class ClientSettingsTestData
    {
        public const string SettingsKey = "valid-key-good-for-everyone";

        public static class NewSettings
        {
            public const string NewSettingUserId = "2759f6a4-29fd-4f27-bcc3-6757f013c7ba";
            public const string NewSettingsValue = "{ \"you are\": \"my sunshine\" }";
        }

        public static class UnsetSettings
        {
            public const string UnsetSettingsUserId = "d32254d9-c5e2-40e6-bc7e-9706ee89306d";
        }

        public static class UpdateSettings
        {
            public const string UpdateSettingUserId = "6d15bf58-facf-4107-aa0e-06db133c8312";
            public const string UpdateSettingsValue = "{ \"this is\": \"awesome\" }";
        }

        public static class InvalidateSettings
        {
            public const string InvalidateSettingsUserId = "16bbd06f-dcdf-450b-b80b-d5aa0e04b366";

            public const string InvalidateSettingsValue = "{\"doc_id\":\"351\", \"description\":\"bla bla\"}";

            public static string[] InvalidateSettingsRemainingKeys => new[] { "correct one", "this is a cool one" };
        }
    }
}
