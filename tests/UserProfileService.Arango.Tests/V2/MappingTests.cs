using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Arango.Tests.V2.Helpers;
using Xunit;

namespace UserProfileService.Arango.Tests.V2
{
    public class MappingTests
    {
        [Fact]
        public void ConvertGroupEntityToGroupView()
        {
            GroupEntityModel entity = SampleDataTestHelper
                .GetGroupFaker(
                    true,
                    true,
                    1,
                    1,
                    1,
                    1)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<GroupView>();

            GroupEntityToGroupComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertGroupEntityToGroupBasic()
        {
            GroupEntityModel entity = SampleDataTestHelper
                .GetGroupFaker(
                    true,
                    true,
                    1,
                    1,
                    1,
                    1)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<GroupBasic>();

            GroupEntityToGroupComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertGroupEntityToGroup()
        {
            GroupEntityModel entity = SampleDataTestHelper
                .GetGroupFaker(
                    true,
                    true,
                    1,
                    1,
                    1,
                    1)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<Group>();

            GroupEntityToGroupComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertUserEntityToUserBasic()
        {
            UserEntityModel entity = SampleDataTestHelper
                .GetUserFaker(true, 2, 2)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<UserBasic>();

            UserEntityToUserComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertUserEntityToUserView()
        {
            UserEntityModel entity = SampleDataTestHelper
                .GetUserFaker(true, 2, 2)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<UserView>();

            UserEntityToUserComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertUserEntityToUser()
        {
            UserEntityModel entity = SampleDataTestHelper
                .GetUserFaker(true, 2, 2)
                .Generate(1)
                .First();

            var actualConverted =
                entity.ToSpecifiedProfileModel<User>();

            UserEntityToUserComparer.Compare(entity, actualConverted);
        }

        [Fact]
        public void ConvertTaggedParentGroupsToGroupView()
        {
            IList<GroupEntityModel> referenceValues = SampleDataTestHelper.GetGroupsWithCorrectAndInvalidTags("my#tag");

            List<GroupView> unused = referenceValues
                .Select(g => g.ToSpecifiedProfileModel<GroupView>())
                .ToList();
        }

        [Fact]
        public void ConvertOrganizationEntityToBasic()
        {
            List<OrganizationEntityModel> orgUnits =
                SampleDataTestHelper.GetOrganizationFaker(true, true, 0, 2)
                    .Generate(5);

            List<Tuple<OrganizationEntityModel, OrganizationBasic>> converted =
                orgUnits.Select(
                        o =>
                            new Tuple<OrganizationEntityModel, OrganizationBasic>(
                                o,
                                o.ToSpecifiedProfileModel<OrganizationBasic>()))
                    .ToList();

            converted.ForEach(tuple => OrganizationEntityToOrganizationComparer.Compare(tuple.Item1, tuple.Item2));
        }

        [Fact]
        public void ConvertOrganizationEntityToView()
        {
            List<OrganizationEntityModel> orgUnits =
                SampleDataTestHelper.GetOrganizationFaker(true, true, 0, 2)
                    .Generate(5);

            List<Tuple<OrganizationEntityModel, OrganizationView>> converted =
                orgUnits.Select(
                        o =>
                            new Tuple<OrganizationEntityModel, OrganizationView>(
                                o,
                                o.ToSpecifiedProfileModel<OrganizationView>()))
                    .ToList();

            converted.ForEach(tuple => OrganizationEntityToOrganizationComparer.Compare(tuple.Item1, tuple.Item2));
        }

        [Fact]
        public void ConvertOrganizationEntityToOrganization()
        {
            List<OrganizationEntityModel> orgUnits =
                SampleDataTestHelper.GetOrganizationFaker(true, true, 0, 2)
                    .Generate(5);

            List<Tuple<OrganizationEntityModel, Organization>> converted =
                orgUnits.Select(
                        o =>
                            new Tuple<OrganizationEntityModel, Organization>(
                                o,
                                o.ToSpecifiedProfileModel<Organization>()))
                    .ToList();

            converted.ForEach(tuple => OrganizationEntityToOrganizationComparer.Compare(tuple.Item1, tuple.Item2));
        }
    }
}
