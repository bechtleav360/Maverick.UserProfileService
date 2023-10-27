using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit.Internals;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;
using TagType = Maverick.UserProfileService.AggregateEvents.Common.Enums.TagType;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class ProfileReadTests : ArangoFirstLevelRepoTestBase
    {
        /// <summary>
        ///     Data for <see cref="Get_all_relevant_objects_because_of_property_change_of_profile_should_work" />.
        /// </summary>
        public static IList<object[]> ProfilePropertyChangeData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.RootOrganizationId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.RootOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauOrganizationId,
                            ObjectType.Organization)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.RootOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumLesenFunction,
                            ObjectType.Function)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.AbteilungTiefbauOrganizationId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauOrganizationId,
                            ObjectType.Organization),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauMitarbeitFunction,
                            ObjectType.Function),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MitarbeiterGroupId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MitarbeiterGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged
                                             .AbteilungTiefbauMitarbeitFunction,
                            ObjectType.Function),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MitarbeiterGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            ObjectType.User),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            ObjectType.Group)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                    new[]
                    {
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            ObjectType.Group),
                        new ObjectIdent(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            ObjectType.User)
                    }
                }
            };

        /// <summary>
        ///     Data for <see cref="Get_all_children_of_profile_should_work" />.
        /// </summary>
        public static IList<object[]> GetChildrenData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.PropertyChanged.RootOrganizationId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                            FirstLevelMemberRelation.DirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.AbteilungTiefbauOrganizationId,
                            FirstLevelMemberRelation.IndirectMember)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MinisteriumOrganizationId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.AbteilungTiefbauOrganizationId,
                            FirstLevelMemberRelation.DirectMember)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.AbteilungTiefbauOrganizationId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            FirstLevelMemberRelation.IndirectMember)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.MitarbeiterGroupId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                            FirstLevelMemberRelation.DirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            FirstLevelMemberRelation.IndirectMember)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                            FirstLevelMemberRelation.DirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            FirstLevelMemberRelation.IndirectMember),
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                            FirstLevelMemberRelation.DirectMember)
                    }
                },
                new object[]
                {
                    HierarchyTestData.PropertyChanged.LeitungBrunnenbauGroupId,
                    new[]
                    {
                        new RelationTestModel(
                            HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                            FirstLevelMemberRelation.DirectMember)
                    }
                },
                new object[] { HierarchyTestData.PropertyChanged.HugoGetraenkUserId, Array.Empty<string>() },
                new object[] { HierarchyTestData.PropertyChanged.MatildeSchmerzId, Array.Empty<string>() }
            };

        public static IList<object[]> GetAllMembers = new List<object[]>
        {
            // OrgUnitTest
            new object[] { HierarchyTestData.ContainerMembers.SubPraktikantenGroupId, Array.Empty<ObjectIdent>() },
            new object[]
            {
                HierarchyTestData.ContainerMembers.PraktikantenGroupId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .MarcelKoenigUserId,
                        ObjectType.User),
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .SebastianKaiserUserId,
                        ObjectType.User),
                    new ObjectIdent(
                        HierarchyTestData.ContainerMembers
                                         .SubPraktikantenGroupId,
                        ObjectType.Group)
                }
            },
            new object[] { HierarchyTestData.ContainerMembers.StreberGroupId, Array.Empty<ObjectIdent>() },
            new object[] { HierarchyTestData.ContainerMembers.NixKoennerGroupId, Array.Empty<ObjectIdent>() }
        };

        /// <summary>
        ///     Data for <see cref="Get_calculated_client_settings_of_profile_should_work" />.
        /// </summary>
        public static IList<object[]> GetClientSettingsData =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.ClientSettings.DbUserId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.DbUserId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.PromotedKey,
                            Value = HierarchyTestData.ClientSettings.PromotedValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.PrincipalsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookPremiumValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueStAugustin
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.A365Key,
                            Value = HierarchyTestData.ClientSettings.A365Value
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueStAugustin
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.A365Key,
                            Value = HierarchyTestData.ClientSettings.A365Value
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 3,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 3,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 4,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                },
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 4,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                },
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ClientSettings.AaUserId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.PrincipalsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookPremiumValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueStAugustin
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.A365Key,
                            Value = HierarchyTestData.ClientSettings.A365Value
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 4,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                },
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 4,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                },
                                {
                                    HierarchyTestData.ClientSettings.PrincipalsGroupId, new List<RangeCondition>
                                                                      {
                                                                          new RangeCondition()
                                                                      }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ClientSettings.PrincipalsGroupId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.PrincipalsGroupId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookPremiumValue
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueStAugustin
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.A365Key,
                            Value = HierarchyTestData.ClientSettings.A365Value
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 3,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 3,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.AvsGroupId, new List<RangeCondition>
                                                               {
                                                                   new RangeCondition()
                                                               }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ClientSettings.AvsGroupId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueStAugustin
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.AvsGroupId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.A365Key,
                            Value = HierarchyTestData.ClientSettings.A365Value
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 2,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                },
                                {
                                    HierarchyTestData.ClientSettings.ShBonnGroupId, new List<RangeCondition>
                                                                  {
                                                                      new RangeCondition()
                                                                  }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ClientSettings.ShBonnGroupId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 1,
                            Conditions = new Dictionary<string, IList<RangeCondition>>
                            {
                                {
                                    HierarchyTestData.ClientSettings.BechtleGroupId, new List<RangeCondition>
                                                                   {
                                                                       new RangeCondition()
                                                                   }
                                }
                            },
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ClientSettings.BechtleGroupId,
                    new[]
                    {
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.AddressKey,
                            Value = HierarchyTestData.ClientSettings.AdressValueNeckarsulm
                        },
                        new FirstLevelProjectionsClientSetting
                        {
                            ProfileId = HierarchyTestData.ClientSettings.BechtleGroupId,
                            Hops = 0,
                            Conditions = new Dictionary<string, IList<RangeCondition>>(),
                            SettingsKey = HierarchyTestData.ClientSettings.OutlookKey,
                            Value = HierarchyTestData.ClientSettings.OutlookDefaultValue
                        }
                    }
                }
            };

        /// <summary>
        ///     Data for <see cref="Get_difference_in_parent_tree_of_profile_should_work" />.
        /// </summary>
        public static IList<object[]> GetDifferenceInTree =
            new List<object[]>
            {
                new object[]
                {
                    HierarchyTestData.ParentTreesTest.RootOrganizationId,
                    new[] { HierarchyTestData.ParentTreesTest.MinisteriumOrganizationId },
                    new[]
                    {
                        new TestParentsTreeDifferenceResult
                        {
                            ProfileId = HierarchyTestData.ParentTreesTest.RootOrganizationId,
                            ReferenceProfileId = HierarchyTestData.ParentTreesTest.MinisteriumOrganizationId,
                            ParentTags = new FirstLevelProjectionTagAssignment[] { },
                            MissingRelations = Array.Empty<TestParentTreeEdgeRelation>()
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ParentTreesTest.BugBustersGroupId,
                    new[]
                    {
                        HierarchyTestData.ParentTreesTest.AusbilderUserId,
                        HierarchyTestData.ParentTreesTest.ElHinzoUserId
                    },
                    new[]
                    {
                        new TestParentsTreeDifferenceResult
                        {
                            ProfileId = HierarchyTestData.ParentTreesTest.BugBustersGroupId,
                            ReferenceProfileId = HierarchyTestData.ParentTreesTest.AusbilderUserId,
                            ParentTags = new FirstLevelProjectionTagAssignment[] { },
                            MissingRelations = new[]
                            {
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags =
                                        new[]
                                        {
                                            new FirstLevelProjectionTagAssignment
                                            {
                                                IsInheritable = true,
                                                TagId = HierarchyTestData.ParentTreesTest
                                                                         .BechtleTagId
                                            }
                                        },
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BugBustersGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.AvsGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.AvsGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BechtleBonnGroupId,
                                        ObjectType.Group)
                                }
                            }
                        },
                        new TestParentsTreeDifferenceResult
                        {
                            ProfileId = HierarchyTestData.ParentTreesTest.BugBustersGroupId,
                            ReferenceProfileId = HierarchyTestData.ParentTreesTest.ElHinzoUserId,
                            ParentTags = new FirstLevelProjectionTagAssignment[] { },
                            MissingRelations = new[]
                            {
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags =
                                        new[]
                                        {
                                            new FirstLevelProjectionTagAssignment
                                            {
                                                IsInheritable = true,
                                                TagId = HierarchyTestData.ParentTreesTest
                                                                         .BechtleTagId
                                            }
                                        },
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BugBustersGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.AvsGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.AvsGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BechtleBonnGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BechtleBonnGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.MinisteriumLesenFunction,
                                        ObjectType.Function)
                                }
                            }
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ParentTreesTest.AhFanclubGroupId,
                    new[] { HierarchyTestData.ParentTreesTest.BbMemberUserId },
                    new[]
                    {
                        new TestParentsTreeDifferenceResult
                        {
                            ProfileId = HierarchyTestData.ParentTreesTest.AhFanclubGroupId,
                            ReferenceProfileId = HierarchyTestData.ParentTreesTest.BbMemberUserId,
                            ParentTags = new FirstLevelProjectionTagAssignment[] { },
                            MissingRelations = new[]
                            {
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.AhFanclubGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.FanclubGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.FanclubGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.RoleWriteId,
                                        ObjectType.Role)
                                }
                            }
                        }
                    }
                },
                new object[]
                {
                    HierarchyTestData.ParentTreesTest.LeitungBonnGroupId,
                    new[] { HierarchyTestData.ParentTreesTest.MarkusMaderUserId },
                    new[]
                    {
                        new TestParentsTreeDifferenceResult
                        {
                            ProfileId = HierarchyTestData.ParentTreesTest.LeitungBonnGroupId,
                            ReferenceProfileId = HierarchyTestData.ParentTreesTest.MarkusMaderUserId,
                            ParentTags = new FirstLevelProjectionTagAssignment[] { },
                            MissingRelations = new[]
                            {
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.LeitungBonnGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.LeitungGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags =
                                        new[]
                                        {
                                            new FirstLevelProjectionTagAssignment
                                            {
                                                IsInheritable = true,
                                                TagId = HierarchyTestData.ParentTreesTest
                                                                         .EmpireTagId
                                            }
                                        },
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.LeitungGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.FitnessstudioGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.FitnessstudioGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.HinzoEmpireGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = Array.Empty<FirstLevelProjectionTagAssignment>(),
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.LeitungBonnGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BonnGroupId,
                                        ObjectType.Group)
                                },
                                new TestParentTreeEdgeRelation
                                {
                                    Conditions = new[] { new RangeCondition() },
                                    ParentTags = new[]
                                    {
                                        new FirstLevelProjectionTagAssignment
                                        {
                                            IsInheritable = true,
                                            TagId = HierarchyTestData.ParentTreesTest
                                                                     .EmpireTagId
                                        }
                                    },
                                    Child = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.BonnGroupId,
                                        ObjectType.Group),
                                    Parent = new ObjectIdent(
                                        HierarchyTestData.ParentTreesTest.FitnessstudioGroupId,
                                        ObjectType.Group)
                                }
                            }
                        }
                    }
                }
            };

        /// <summary>
        ///     Data for <see cref="Get_parents_of_profile_should_work" />.
        /// </summary>
        public static IList<object[]> GetParentsData = new[]
        {
            new object[] { HierarchyTestData.PropertyChanged.RootOrganizationId, Array.Empty<ObjectIdent>() },
            new object[]
            {
                HierarchyTestData.PropertyChanged
                                 .MinisteriumOrganizationId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .RootOrganizationId,
                        ObjectType.Organization)
                }
            },
            new object[] { HierarchyTestData.PropertyChanged.MitarbeiterGroupId, Array.Empty<ObjectIdent>() },
            new object[]
            {
                HierarchyTestData.PropertyChanged.BrunnenbauGroupId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .MitarbeiterGroupId,
                        ObjectType.Group),
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .AbteilungTiefbauMitarbeitFunction,
                        ObjectType.Function)
                }
            },
            new object[]
            {
                HierarchyTestData.PropertyChanged
                                 .LeitungBrunnenbauGroupId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .BrunnenbauGroupId,
                        ObjectType.Group)
                }
            },
            new object[]
            {
                HierarchyTestData.PropertyChanged.MatildeSchmerzId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .LeitungBrunnenbauGroupId,
                        ObjectType.Group)
                }
            },
            new object[]
            {
                HierarchyTestData.PropertyChanged.HugoGetraenkUserId,
                new[]
                {
                    new ObjectIdent(
                        HierarchyTestData.PropertyChanged
                                         .BrunnenbauGroupId,
                        ObjectType.Group)
                }
            }
        };

        private readonly FirstLevelProjectionFixture _fixture;

        public ProfileReadTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<FirstLevelProjectionParentsTreeDifferenceResult> Convert(
            TestParentsTreeDifferenceResult input)
        {
            var result = new FirstLevelProjectionParentsTreeDifferenceResult
            {
                Profile =
                    (IFirstLevelProjectionContainer)await
                        GetDocumentObjectAsync<IFirstLevelProjectionProfile>(input.ProfileId),
                ProfileTags = await Task.WhenAll(input.ParentTags.Select(ConvertTagAssignment)),
                ReferenceProfileId = input.ReferenceProfileId,
                MissingRelations = await Task.WhenAll(input.MissingRelations.Select(ConvertRelation))
            };

            return result;
        }

        private async Task<FirstLevelProjectionTreeEdgeRelation> ConvertRelation(TestParentTreeEdgeRelation relation)
        {
            var result = new FirstLevelProjectionTreeEdgeRelation
            {
                Child = relation.Child,
                Conditions = relation.Conditions,
                ParentTags = await Task.WhenAll(relation.ParentTags.Select(ConvertTagAssignment)),
                Parent = await GetContainer(relation.Parent)
            };

            return result;
        }

        private async Task<TagAssignment> ConvertTagAssignment(FirstLevelProjectionTagAssignment assignment)
        {
            var tag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(assignment.TagId);

            return new TagAssignment
            {
                IsInheritable = assignment.IsInheritable,
                TagDetails = new Tag
                {
                    Name = tag.Name,
                    Id = tag.Id,
                    Type = (TagType)tag.Type
                }
            };
        }

        [Fact]
        public async Task Get_user_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile profile = await repo.GetProfileAsync(ProfileReadTestData.ReadUser.Id);

            profile.Should()
                .BeOfType<FirstLevelProjectionUser>()
                .And.BeEquivalentTo(ProfileReadTestData.ReadUser);
        }

        [Fact]
        public async Task Get_group_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile profile = await repo.GetProfileAsync(ProfileReadTestData.ReadGroup.Id);

            profile.Should()
                .BeOfType<FirstLevelProjectionGroup>()
                .And.BeEquivalentTo(ProfileReadTestData.ReadGroup);
        }

        [Fact]
        public async Task Get_organization_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile profile = await repo.GetProfileAsync(ProfileReadTestData.ReadOrganization.Id);

            profile.Should()
                .BeOfType<FirstLevelProjectionOrganization>()
                .And.BeEquivalentTo(ProfileReadTestData.ReadOrganization);
        }

        [Fact]
        public async Task Get_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetProfileAsync("this-profile-does-not-exists"));
        }

        [Theory]
        [MemberData(nameof(ProfilePropertyChangeData))]
        public async Task Get_all_relevant_objects_because_of_property_change_of_profile_should_work(
            string profileId,
            ObjectIdent[] objects)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdentPath> profiles =
                await repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent(profileId, ObjectType.Profile));

            profiles.Should().NotBeNull().And.BeEquivalentTo(objects);
        }

        [Fact]
        public async Task Get_all_relevant_objects_because_of_property_change_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
                    new ObjectIdent("this-profile-does-not-exists", ObjectType.Profile)));
        }

        [Theory]
        [MemberData(nameof(GetChildrenData))]
        public async Task Get_all_children_of_profile_should_work(
            string profileId,
            RelationTestModel[] expectedChildren)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionProfile[] expectedProfiles = await Task.WhenAll(
                expectedChildren.Select(c => GetDocumentObjectAsync<IFirstLevelProjectionProfile>(c.ProfileId)));

            List<FirstLevelRelationProfile> expectedProfileRelation = expectedChildren.Select(
                    child => new FirstLevelRelationProfile(
                        expectedProfiles.FirstOrDefault(pr => pr.Id == child.ProfileId),
                        child.Relation))
                .ToList();

            IList<FirstLevelRelationProfile> profiles =
                await repo.GetAllChildrenAsync(new ObjectIdent(profileId, ObjectType.Profile));

            profiles.Should().NotBeNull().And.BeEquivalentTo(expectedProfileRelation);
        }

        [Theory]
        [MemberData(nameof(GetAllMembers))]
        public async Task Get_direct_members_should_work(
            string profileId,
            ObjectIdent[] expectedMembers)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<ObjectIdent> profiles =
                await repo.GetContainerMembersAsync(new ObjectIdent(profileId, ObjectType.Profile));

            profiles.Should().NotBeNull().And.BeEquivalentTo(expectedMembers);
        }

        [Fact]
        public async Task Get_all_children_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetAllChildrenAsync(new ObjectIdent("this-profile-does-not-exists", ObjectType.Profile)));
        }

        [Theory]
        [MemberData(nameof(GetClientSettingsData))]
        public async Task Get_calculated_client_settings_of_profile_should_work(
            string profileId,
            FirstLevelProjectionsClientSetting[] expectedSettings)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            IList<FirstLevelProjectionsClientSetting> clientSettings =
                await repo.GetCalculatedClientSettingsAsync(profileId);

            clientSettings.Should()
                .NotBeNull()
                .And.BeEquivalentTo(
                    expectedSettings,
                    o => o.Excluding(s => s.Weight).Excluding(s => s.UpdatedAt));
        }

        [Fact]
        public async Task Get_calculated_client_settings_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetCalculatedClientSettingsAsync("this-profile-does-not-exists"));
        }

        [Theory]
        [MemberData(nameof(GetDifferenceInTree))]
        public async Task Get_difference_in_parent_tree_of_profile_should_work(
            string profileId,
            string[] parentIds,
            TestParentsTreeDifferenceResult[] expectedTree)
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            FirstLevelProjectionParentsTreeDifferenceResult[] expectedTrees =
                await Task.WhenAll(expectedTree.Select(Convert));

            IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> result =
                repo.GetDifferenceInParentsTreesAsync(profileId, parentIds);

            IList<FirstLevelProjectionParentsTreeDifferenceResult> trees = await result.ToListAsync();
            trees.Should().NotBeNull().And.BeEquivalentTo(expectedTrees);
        }

        [Fact]
        public async Task Get_difference_in_parent_tree_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                async () =>
                {
                    IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> list =
                        repo.GetDifferenceInParentsTreesAsync(
                            "this-profile-does-not-exists",
                            new[] { ProfileReadTestData.ReadGroup.Id });

                    await list.ToListAsync();
                });
        }

        [Fact]
        public async Task Get_difference_in_parent_tree_with_not_existing_parent_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                async () =>
                {
                    IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> differences =
                        repo.GetDifferenceInParentsTreesAsync(
                            ProfileReadTestData.ReadGroup.Id,
                            new[] { "this-profile-does-not-exists" });

                    await differences.ToListAsync();
                });
        }

        [Theory]
        [MemberData(nameof(GetParentsData))]
        public async Task Get_parents_of_profile_should_work(
            string
                profileId,
            ObjectIdent[] expectedParentIds)
        {
            IFirstLevelProjectionRepository repo =
                await _fixture.GetFirstLevelRepository();

            IFirstLevelProjectionContainer[] expectedContainers =
                await Task.WhenAll(expectedParentIds.Select(p => GetContainer(p)));

            IList<IFirstLevelProjectionContainer> response = await repo.GetParentsAsync(profileId);

            response.Should().BeEquivalentTo(expectedContainers);
        }

        [Fact]
        public async Task
            Get_parents_of_not_existing_profile_should_throw()
        {
            IFirstLevelProjectionRepository repo =
                await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => repo.GetParentsAsync("this-profile-does-not-exists"));
        }

        public class TestParentsTreeDifferenceResult
        {
            public string ProfileId { get; set; }
            public string ReferenceProfileId { get; set; }
            public TestParentTreeEdgeRelation[] MissingRelations { get; set; }
            public FirstLevelProjectionTagAssignment[] ParentTags { get; set; }
        }

        public class TestParentTreeEdgeRelation
        {
            public ObjectIdent Parent { get; set; }
            public ObjectIdent Child { get; set; }
            public RangeCondition[] Conditions { get; set; }
            public FirstLevelProjectionTagAssignment[] ParentTags { get; set; }
        }
    }
}
