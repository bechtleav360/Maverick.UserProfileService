using System;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HasClientSettings : Attribute
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public HasClientSettings(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
