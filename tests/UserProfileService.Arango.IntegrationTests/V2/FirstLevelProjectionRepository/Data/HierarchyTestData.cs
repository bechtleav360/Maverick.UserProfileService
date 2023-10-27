using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Seeding.Attributes;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data
{
    [SeedData]
    public static class HierarchyTestData
    {
        public static class PropertyChanged
        {
            [Profile(ProfileKind.Organization, Name = "Root")]
            public const string RootOrganizationId = "418dcd60-35b8-4199-9a97-d42735fdfb48";

            [Profile(ProfileKind.Organization, Name = "Ministerium")]
            [AssignedTo(RootOrganizationId, ContainerType.Organization)]
            public const string MinisteriumOrganizationId = "c46acf5b-b08d-42fa-9e73-b1a78a41c45c";

            [Profile(ProfileKind.Organization, Name = "Abteilung Tiefbau")]
            [AssignedTo(MinisteriumOrganizationId, ContainerType.Organization)]
            public const string AbteilungTiefbauOrganizationId = "45af920d-8fe8-43b6-978e-e9c22a6c03d9";

            [Role(Name = "Lesen")]
            public const string LesenRole = "c140e181-7989-4517-99d8-cf05ed69cd93";

            [Role(Name = "Mitarbeiter")]
            public const string MitarbeiterRole = "ab94d934-57ff-45b3-9446-9e099b2e246d";

            [Function(LesenRole, MinisteriumOrganizationId)]
            public const string MinisteriumLesenFunction = "1ee91c13-ebea-48bb-820f-7560486aa7cb";

            [Function(MitarbeiterRole, AbteilungTiefbauOrganizationId)]
            public const string AbteilungTiefbauMitarbeitFunction = "4e8ad30a-5341-4253-8b49-6b4ec02476f3";

            [Profile(ProfileKind.Group, Name = "Mitarbeiter")]
            public const string MitarbeiterGroupId = "327d1b25-5e47-4c26-8351-a44d810b32f3";

            [Profile(ProfileKind.Group, Name = "Brunnenbau Gruppe")]
            [AssignedTo(MitarbeiterGroupId, ContainerType.Group)]
            [AssignedTo(AbteilungTiefbauMitarbeitFunction, ContainerType.Function)]
            public const string BrunnenbauGroupId = "45d70184-bdf0-48fd-8c6d-4923f3df5fe0";

            [Profile(ProfileKind.Group, Name = "Leitung Brunnenbau Gruppe")]
            [AssignedTo(BrunnenbauGroupId, ContainerType.Group)]
            public const string LeitungBrunnenbauGroupId = "d0ed48a6-e971-4629-9861-3e75f57cf2cf";

            [Profile(ProfileKind.User, Name = "Hugo Getränk")]
            [AssignedTo(BrunnenbauGroupId, ContainerType.Group)]
            public const string HugoGetraenkUserId = "8d6670fd-a959-4502-94f2-ceb5832f0ae6";

            [Profile(ProfileKind.User, Name = "Matilde Schmerz")]
            [AssignedTo(LeitungBrunnenbauGroupId, ContainerType.Group)]
            public const string MatildeSchmerzId = "507005e3-cde4-44ea-81a4-30e2201d2a14";
        }

        public static class ContainerMembers
        {
            // Groups
            [Profile(ProfileKind.Group, Name = "Praktikanten Gruppe")]
            [AssignedTo(LernenRoleId, ContainerType.Role)]
            public const string PraktikantenGroupId = "46E2DA68-EAC7-4A1E-84AA-F31ED797AA6B";

            [Profile(ProfileKind.Group, Name = "Streber Gruppe")]
            [AssignedTo(EinrabeitenLernenFunctionId, ContainerType.Function)]
            public const string StreberGroupId = "3463F299-036B-4DFD-B456-9B128C1F2AAE";

            [Profile(ProfileKind.Group, Name = "NixKoenner Gruppe")]
            [AssignedTo(EinrabeitenLernenFunctionId, ContainerType.Function)]
            public const string NixKoennerGroupId = "499BA9E2-32F2-4AF9-BB02-21D93AC96649";

            [Profile(ProfileKind.Group, Name = "SubPraktikanten Gruppe")]
            [AssignedTo(PraktikantenGroupId, ContainerType.Group)]
            public const string SubPraktikantenGroupId = "260AF6CA-44BC-4D0D-8790-DE62443C3508";

            //OrgUnits
            [Profile(ProfileKind.Organization, Name = "Abteilung Einarbeiten")]
            [AssignedTo(AbteilungSuperOrgUnitId, ContainerType.Organization)]
            public const string AbteilungEinarbeitenOrgUnitId = "FE889ABD-D588-4B83-B0DB-AC5DC6480A31";

            [Profile(ProfileKind.Organization, Name = "Abteilung Einarbeiten")]
            public const string AbteilungSuperOrgUnitId = "47D8D6A5-112B-4F96-BF50-CDB70F204A7D";

            [Profile(ProfileKind.Organization, Name = "Abteilung Einarbeiten")]
            [AssignedTo(AbteilungSuperOrgUnitId, ContainerType.Organization)]
            public const string AbteilungErfahrungOrgId = "F838135A-4FCA-4B19-9929-3015D95C8E68";

            [Profile(ProfileKind.Organization, Name = "Abteilung NixFuerNix")]
            public const string AbteilungNixFuerNixOrgUnitId = "C2F3717B-8AF5-4547-AECA-6B6FEDF49137";

            //User
            [Profile(ProfileKind.User, Name = "Marcel König")]
            [AssignedTo(PraktikantenGroupId, ContainerType.Group)]
            public const string MarcelKoenigUserId = "257471AC-5E97-427B-8D34-0291315182F9";

            [Profile(ProfileKind.User, Name = "Sebastian Kaiser")]
            [AssignedTo(PraktikantenGroupId, ContainerType.Group)]
            public const string SebastianKaiserUserId = "4A6BD503-E150-45DB-8737-7A90D717239C";

            [Profile(ProfileKind.User, Name = "Michael Rooky")]
            [AssignedTo(LernenRoleId, ContainerType.Role)]
            public const string MichaelRookyUserId = "5C8D9E20-87BF-45C5-B7C2-F9C19333E6D8";

            [Profile(ProfileKind.User, Name = "Gregor Schnell Lerner")]
            [AssignedTo(EinrabeitenLernenFunctionId, ContainerType.Function)]
            public const string GregorSchnellLernerUserId = "16C641DF-A1E9-4807-8C20-1503FECAC4ED";

            [Profile(ProfileKind.User, Name = "Michael Dickschaedel")]
            [AssignedTo(EinrabeitenLernenFunctionId, ContainerType.Function)]
            public const string MichaelDickschaedelUserId = "FEB9972B-11AE-4E37-8FA7-0B3B3DAF77DD";

            // Role
            [Role(Name = "Lernen")]
            public const string LernenRoleId = "665A7ACE-B5F4-4021-AF72-11116992BD22";

            // Role
            [Role(Name = "Nix")]
            public const string NixRoleId = "47B4DBD6-073B-42FC-A378-BB15EED577AE";

            // Function
            [Function(LernenRoleId, AbteilungEinarbeitenOrgUnitId)]
            public const string EinrabeitenLernenFunctionId = "180D9E06-64FE-4ADF-B4C0-4E3031A01A7D";

            [Function(NixRoleId, AbteilungNixFuerNixOrgUnitId)]
            public const string NixFuerNixenFunctionId = "DDC50BE7-DCA6-48DF-9C57-10BBB91DF1E0";
        }

        public static class ClientSettings
        {
            [Profile(ProfileKind.Group)]
            [HasClientSettings(AddressKey, AdressValueNeckarsulm)]
            [HasClientSettings(OutlookKey, OutlookDefaultValue)]
            public const string BechtleGroupId = "479e58c5-8ea8-4c4b-a93b-417799d1c682";

            [Profile(ProfileKind.Group)]
            [AssignedTo(BechtleGroupId, ContainerType.Group)]
            public const string ShBonnGroupId = "8a5eac3c-f991-481b-a682-52232694239e";

            [Profile(ProfileKind.Group)]
            [AssignedTo(ShBonnGroupId, ContainerType.Group)]
            [AssignedTo(RoleId, ContainerType.Role)]
            [HasClientSettings(AddressKey, AdressValueStAugustin)]
            [HasClientSettings(A365Key, A365Value)]
            public const string AvsGroupId = "9323535c-50ed-4755-81a8-580b84b6bbcb";

            [Profile(ProfileKind.Group)]
            [AssignedTo(AvsGroupId, ContainerType.Group)]
            [HasClientSettings(OutlookKey, OutlookPremiumValue)]
            public const string PrincipalsGroupId = "4f75f24f-2669-4a94-b644-94dd52dcccd5";

            [Profile(ProfileKind.User)]
            [AssignedTo(PrincipalsGroupId, ContainerType.Group)]
            public const string AaUserId = "d7282c19-b60f-440d-a4fc-aae65b35fa0a";

            [Profile(ProfileKind.User)]
            [AssignedTo(PrincipalsGroupId, ContainerType.Group)]
            [AssignedTo(AvsGroupId, ContainerType.Group)]
            [HasClientSettings(PromotedKey, PromotedValue)]
            public const string DbUserId = "acc205ee-8195-4570-854b-e58b66bdf39a";

            [Role]
            public const string RoleId = "b2f77bce-7284-443d-a9c1-ae773d122249";

            public const string AddressKey = "Address";
            public const string AdressValueNeckarsulm = "Neckasulm";
            public const string AdressValueStAugustin = "St Augustin";

            public const string OutlookKey = "Outlook";
            public const string OutlookDefaultValue = "O365";
            public const string OutlookPremiumValue = "O365 PRemium";

            public const string A365Key = "A365Enabled";
            public const string A365Value = "true";

            public const string PromotedKey = "Promoted";
            public const string PromotedValue = "Boss";
        }

        public static class ParentTreesTest
        {
            [Role(Name = "Rolle Lesen")]
            public const string RoleReadId = "3e4b2f32-d3d1-44de-ae76-8e0adced034a";

            [Role(Name = "Rolle Schreiben")]
            public const string RoleWriteId = "1c55c580-3991-4f03-8b6e-dd5e84ad0e13";

            [Profile(ProfileKind.Organization, Name = "Root")]
            public const string RootOrganizationId = "2db55195-f799-4af9-b91f-007dbecebce7";

            [Profile(ProfileKind.Organization, Name = "Ministerium")]
            [AssignedTo(RootOrganizationId, ContainerType.Organization)]
            public const string MinisteriumOrganizationId = "10816d70-f28e-4e7a-8f21-7eb994f08b00";

            [Tag("Organization")]
            public const string OrganizationTagId = "93b8356a-106a-491e-8a69-103ca141784b";

            [Tag("Bechtle Test")]
            public const string BechtleTagId = "3dcf3ca0-7141-46db-bda5-6df69dd2e059";

            [Tag("Empire Test")]
            public const string EmpireTagId = "3bb25368-7913-4450-9eb6-7f34cc20cab9";

            [Function(RoleReadId, MinisteriumOrganizationId)]
            public const string MinisteriumLesenFunction = "988ae1fc-64b9-491e-b1e9-dfb143bf9600";

            [Profile(ProfileKind.Group, Name = "Bechtle Bonn")]
            [AssignedTo(MinisteriumLesenFunction, ContainerType.Function)]
            public const string BechtleBonnGroupId = "d06e1598-1ed6-4772-a1b2-48b020ab899b";

            [Profile(ProfileKind.Group, Name = "Ausbildung")]
            [AssignedTo(BechtleBonnGroupId, ContainerType.Group)]
            public const string AusbildungGroupId = "0d96e635-f278-4973-95c1-1b7f89ec4191";

            [Profile(ProfileKind.Group, Name = "AVS")]
            [HasTag(BechtleTagId, true)]
            [AssignedTo(BechtleBonnGroupId, ContainerType.Group)]
            public const string AvsGroupId = "5106583a-402d-4cca-ae1f-2c50bc789ebf";

            [Profile(ProfileKind.Group, Name = "Bug Busters")]
            [AssignedTo(AvsGroupId, ContainerType.Group)]
            public const string BugBustersGroupId = "d3f172dd-25e2-4022-9b2c-fd4778882ec3";

            [Profile(ProfileKind.Group, Name = "Fanclubs")]
            [AssignedTo(RoleWriteId, ContainerType.Role)]
            public const string FanclubGroupId = "399749ab-07dc-4e0f-86ad-1d077d7caa26";

            [Profile(ProfileKind.Group, Name = "AH Fanclub")]
            [AssignedTo(FanclubGroupId, ContainerType.Group)]
            public const string AhFanclubGroupId = "ad6c3b18-8409-45c6-882e-07a951906e22";

            [Profile(ProfileKind.Group, Name = "Hinzo Empire")]
            public const string HinzoEmpireGroupId = "23505c7c-119e-46f0-b63b-7c2f588611fc";

            [Profile(ProfileKind.Group, Name = "Fitnessstudio")]
            [HasTag(EmpireTagId, true)]
            [AssignedTo(HinzoEmpireGroupId, ContainerType.Group)]
            public const string FitnessstudioGroupId = "1ba4c9e5-7a35-491d-9192-e9890a70c1fa";

            [Profile(ProfileKind.Group, Name = "Bonn")]
            [AssignedTo(FitnessstudioGroupId, ContainerType.Group)]
            public const string BonnGroupId = "1cb6a738-82e2-469b-a15a-6fea8f40fe10";

            [Profile(ProfileKind.Group, Name = "Leitung")]
            [AssignedTo(FitnessstudioGroupId, ContainerType.Group)]
            public const string LeitungGroupId = "9ef53155-2b40-482c-85ba-57c2bba24b46";

            [Profile(ProfileKind.Group, Name = "Leitung Bonn")]
            [AssignedTo(LeitungGroupId, ContainerType.Group)]
            [AssignedTo(BonnGroupId, ContainerType.Group)]
            public const string LeitungBonnGroupId = "20255963-a546-4156-863b-53e1a5150e02";

            [Profile(ProfileKind.User, Name = "Ausbilder")]
            [AssignedTo(AusbildungGroupId, ContainerType.Group)]
            public const string AusbilderUserId = "1b58b599-96a4-433e-b9cc-938f7d430b64";

            [Profile(ProfileKind.User, Name = "BB Member")]
            [AssignedTo(BugBustersGroupId, ContainerType.Group)]
            public const string BbMemberUserId = "3f0921b9-414c-4221-bb9b-61014bbe2657";

            [Profile(ProfileKind.User, Name = "El Hinzo")]
            [AssignedTo(AhFanclubGroupId, ContainerType.Group)]
            public const string ElHinzoUserId = "f55424d2-88b8-4a04-91d0-966ef4d057ac";

            [Profile(ProfileKind.User, Name = "Markus Mader")]
            public const string MarkusMaderUserId = "10ad2f88-8d9a-46fb-b215-782279b8a971";
        }
    }
}
