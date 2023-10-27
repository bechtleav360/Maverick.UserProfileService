using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.IntegrationTests.Utilities
{
    internal enum ObjectOperation
    {
        Key,
        Create,
        Update,
        Object
    }

    internal static class TestDataKeyUtility
    {
        public static string GenerateKey(ObjectType type, ObjectOperation operation, int index = 0)
        {
            return $"{type}-{operation}-{index}";
        }

        public static string GenerateConfigKey(ObjectType type, ObjectOperation operation, int index = 0)
        {
            return $"config-{type}-{operation}-{index}";
        }
    }
}
