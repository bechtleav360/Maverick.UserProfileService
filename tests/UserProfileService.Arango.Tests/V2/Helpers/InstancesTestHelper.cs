using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.Tests.V2.Constants;
using UserProfileService.Common.Tests.Utilities.Extensions;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    // to create test samples
    internal static class InstancesTestHelper
    {
        private static readonly IProfile[] _testProfiles =
        {
            new User
            {
                Kind = ProfileKind.User,
                Id = "9B8451A7-10B1-4B13-8C42-AC17C2EDE305",
                Name = "Mathias Schmidt",
                FirstName = "Mathias",
                LastName = "Schmidt",
                ExternalIds = "22348".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "m.schmidt@mail.com",
                DisplayName = "Schmidt, Mathias"
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "E4BA2BBB-5F91-4F9C-97A6-E4036F2FD867",
                Name = "Test group #1",
                DisplayName = "Test group number 1",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "G651.12".CreateSimpleExternalIdentifiers()
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "2336E56A-8207-49C1-8857-D713433062B0",
                Name = "Test group #2",
                DisplayName = "Test group number 2",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "G651.15".CreateSimpleExternalIdentifiers()
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "73D8C3D2-8994-49F8-8C34-9DEF3DE986F2",
                Name = "Markus Philipp",
                FirstName = "Markus",
                LastName = "Philipp",
                ExternalIds = "5485671".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "markusph123@web.de",
                DisplayName = "Philipp, Markus"
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "863550E6-819D-4DBA-A49E-90054835C017",
                Name = "Stefan Rösle",
                FirstName = "Stefan",
                LastName = "Rösle",
                ExternalIds = "5485671".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "st_roesled@myservice.de",
                DisplayName = "Rösle, Stefan"
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "16F05AB0-BC43-45FA-AD84-BC83409E4828",
                Name = "Alexandra Kuvits",
                FirstName = "Alexandra",
                LastName = "Kuvits",
                ExternalIds = "5485671".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "heinz.winghard@gmail.com",
                DisplayName = "Kuvits, Alexandra"
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "7BE60DBD-144D-4094-9BBB-E831914379DC",
                Name = "developer college",
                DisplayName = "Developer college days",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "BKU#14/2%".CreateSimpleExternalIdentifiers()
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "3CFE120F-7C6E-4C32-850F-50324DB24A9B",
                Name = "Shubash Shabush",
                FirstName = "Shubash",
                LastName = "Shabush",
                ExternalIds = "89-12-89/01".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "shusha42@company.com",
                DisplayName = "Shabush, Shubash"
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "72C5C967-3F46-4309-A525-4BEE56397472",
                Name = "Ellerby Caddric",
                FirstName = "Caddric",
                LastName = "Ellerby",
                ExternalIds = "zzdev_0123".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "@company.com",
                DisplayName = "Ellerby, Caddric"
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "B1AC8226-03D8-4D87-9FED-1452011D6CE3",
                Name = "Berri Connell",
                FirstName = "Berri",
                LastName = "Connell",
                ExternalIds = "d72c2e72-6c7a-42da-b06a-f2d84234cc9c".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "berri.connell@company.com",
                DisplayName = "Connell, Berri"
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "57FD0CF6-2891-4B68-ADA9-DB17FDA81951",
                Name = "MaverickAcademy",
                DisplayName = "Maverick academy 2021",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "4711".CreateSimpleExternalIdentifiers()
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "BB53F823-ABED-4FC6-AF68-F210AA00A760",
                Name = "Arie Trotman",
                FirstName = "Arie",
                LastName = "Trotman",
                ExternalIds = "".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "ArieTr0212@mail.uk",
                DisplayName = "Trotman, Arie"
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "5A1F886E-F11F-487B-A4D9-5E0639CE29F0",
                Name = "Lucy Bosco",
                FirstName = "Lucy",
                LastName = "Bosco",
                ExternalIds = "no.1".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "the_lucy@bosco-inc.com",
                DisplayName = "Bosco, Lucy"
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "0E3ED9C3-1933-404C-BA3A-72C035939CEA",
                Name = "Test group #42",
                DisplayName = "Test group number 42 - bonn",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "G551.29".CreateSimpleExternalIdentifiers()
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "B81CBCCE-1255-4A5B-9CAB-EB811258E440",
                Name = "ES.Q3/21",
                DisplayName = "Einsatzsteuerung Q3/21",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "ES.Q3/21".CreateSimpleExternalIdentifiers()
            },
            new Group
            {
                Kind = ProfileKind.Group,
                Id = "EF50E749-69FA-44C0-ACFB-511CC6728A02",
                Name = "MSTeams_support",
                DisplayName = "MS Teams support",
                CreatedAt = DateTime.UtcNow,
                ExternalIds = "uios/jaus01".CreateSimpleExternalIdentifiers()
            },
            new User
            {
                Kind = ProfileKind.User,
                Id = "7ECA9888-4FAE-447D-A407-A407279E5CD2",
                Name = "I'timad Oluwaseyi",
                FirstName = "I'timad",
                LastName = "Oluwaseyi",
                ExternalIds = "34219HU-982HJA".CreateSimpleExternalIdentifiers(),
                CreatedAt = DateTime.UtcNow,
                Email = "Oluwaseyi.iltimad@myaddress.org",
                DisplayName = "Oluwaseyi"
            }
        };

        internal static IProfile[] GetProfiles(
            ProfileKind profileKind,
            int count)
        {
            return _testProfiles.Where(p => p.Kind == profileKind).Take(count).ToArray();
        }

        internal static IProfile[] GetProfiles(int count)
        {
            return _testProfiles.Take(count).ToArray();
        }

        internal static GroupEntityModel GetGroupEntity()
        {
            return GetSamples<GroupEntityModel>(WellKnownFiles.GroupEntitiesSampleData).First();
        }

        private static List<TElem> GetSamples<TElem>(string filepath)
        {
            return JsonConvert.DeserializeObject<List<TElem>>(File.ReadAllText(filepath));
        }
    }
}
