using System.Collections.Generic;
using FluentAssertions;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Extensions;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Extensions
{
    public class SyncConfigurationExtensionsTests
    {

        [Theory]
        [MemberData(nameof(GetSyncTestData))]
        public void GetSortedSystems_should_work(Dictionary<string, SourceSystemConfiguration> input,
                                                 Dictionary<string, SourceSystemConfiguration> sortedDictionary)
        {
            var syncConfiguration = GenerateSyncConfigurationFromSystems(input);
            var sortedSystems = syncConfiguration.GetSystemSorted();

            sortedSystems.Should().BeEquivalentTo(sortedDictionary, opt => opt.WithStrictOrdering());
        }

        private SyncConfiguration GenerateSyncConfigurationFromSystems(
            Dictionary<string, SourceSystemConfiguration> systems)
        {
            return new SyncConfiguration
            {
                SourceConfiguration = new SourceConfiguration
                {
                    Systems = systems
                }
            };
        }

        public static IEnumerable<object[]> GetSyncTestData()
        {
            yield return new object[]
                         {
                             new Dictionary<string,SourceSystemConfiguration>()
                             {
                                 {
                                     "Bonnea",new SourceSystemConfiguration
                                              {
                                                  Priority = 10
                                              }
                                 },
                                 {
                                     "Ldap",new SourceSystemConfiguration
                                              {
                                                  Priority = 10
                                              }
                                 },
                                 {
                                     "Oracle",new SourceSystemConfiguration
                                            {
                                                Priority = 10
                                            }
                                 }

                             },

                             new Dictionary<string,SourceSystemConfiguration>()
                             {
                                 {
                                     "Oracle",new SourceSystemConfiguration
                                              {
                                                  Priority = 10
                                              }
                                 },
                                 {
                                     "Ldap",new SourceSystemConfiguration
                                            {
                                                Priority = 10
                                            }
                                 },
                                 {
                                     "Bonnea",new SourceSystemConfiguration
                                              {
                                                  Priority = 10
                                              }
                                 }
                             },
                         };

            yield return new object[]
                       {
                             new Dictionary<string,SourceSystemConfiguration>()
                             {
                                 {
                                     "Bonnea",new SourceSystemConfiguration
                                              {
                                                  Priority = 1
                                              }
                                 },
                                 {
                                     "Ldap",new SourceSystemConfiguration
                                              {
                                                  Priority = 10
                                              }
                                 },
                                 {
                                     "Oracle",new SourceSystemConfiguration
                                            {
                                                Priority = 2
                                            }
                                 }

                             },

                             new Dictionary<string,SourceSystemConfiguration>()
                             {
                                 {
                                     "Ldap",new SourceSystemConfiguration
                                            {
                                                Priority = 10
                                            }
                                 },
                                 {
                                     "Oracle",new SourceSystemConfiguration
                                              {
                                                  Priority = 2
                                              }
                                 },
                                 {
                                     "Bonnea",new SourceSystemConfiguration
                                              {
                                                  Priority = 1
                                              }
                                 }
                             },
                       };
        }
    }
}
