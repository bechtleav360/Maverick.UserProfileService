using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    public static class TestArgumentsHelper
    {
        private static IEnumerable<object[]> GetFunctionalAndRolesTestArguments<TEntity, TFuncRole>(
            IEnumerable<TEntity> entities,
            Func<TEntity, bool> filter,
            IEnumerable<TFuncRole> funcOrRole,
            int howMany = 2)
            where TFuncRole : IAssignmentObjectEntity
            where TEntity : IProfile
        {
            return entities.Where(filter).Take(howMany).Select(e => GetTestArgs(e, funcOrRole));
        }

        private static object[] GetTestArgs<TEntity, TFuncRole>(
            TEntity entity,
            IEnumerable<TFuncRole> funcOrRole,
            Type expectedExceptionType = null)
            where TFuncRole : IAssignmentObjectEntity
            where TEntity : IProfile
        {
            string profileId = entity.Id;

            TFuncRole function = funcOrRole
                .FirstOrDefault(
                    f => f.LinkedProfiles != null
                        && f.LinkedProfiles.Any(lp => lp.Id.EndsWith($"/{profileId}")));

            return new object[] { profileId, expectedExceptionType, function?.Name ?? "unknownName" };
        }

        public static IEnumerable<object[]> GetFunctionAndRoleOfProfileArgumentParts()
        {
            return GetFunctionalAndRolesTestArguments(
                    SampleDataTestHelper.GetTestUserEntities(),
                    p => p.SecurityAssignments != null
                        && p.SecurityAssignments.All(link => link.Type == nameof(LinkedRoleObject)),
                    SampleDataTestHelper.GetTestRoleEntities())
                .Concat(
                    GetFunctionalAndRolesTestArguments(
                        SampleDataTestHelper.GetTestUserEntities(),
                        p => p.SecurityAssignments != null
                            && p.SecurityAssignments.All(link => link.Type == nameof(LinkedFunctionObject)),
                        DatabaseFixture.TestFunctions))
                .Concat(
                    GetFunctionalAndRolesTestArguments(
                        SampleDataTestHelper.GetTestUserEntities(),
                        p => p.SecurityAssignments != null
                            && p.SecurityAssignments.Any(link => link.Type == nameof(LinkedRoleObject))
                            && p.SecurityAssignments.Any(link => link.Type == nameof(LinkedFunctionObject)),
                        DatabaseFixture.TestFunctions
                            .Cast<IAssignmentObjectEntity>()
                            .Concat(SampleDataTestHelper.GetTestRoleEntities())))
                .Concat(
                    GetFunctionalAndRolesTestArguments(
                        SampleDataTestHelper.GetTestGroupEntities(),
                        p => p.SecurityAssignments != null
                            && p.SecurityAssignments.All(link => link.Type == nameof(LinkedRoleObject)),
                        SampleDataTestHelper.GetTestRoleEntities()))
                .Concat(
                    GetFunctionalAndRolesTestArguments(
                        SampleDataTestHelper.GetTestGroupEntities(),
                        p => p.SecurityAssignments != null
                            && p.SecurityAssignments.All(link => link.Type == nameof(LinkedFunctionObject)),
                        DatabaseFixture.TestFunctions))
                .Concat(
                    GetFunctionalAndRolesTestArguments(
                        SampleDataTestHelper.GetTestGroupEntities(),
                        p => p.SecurityAssignments != null
                            && p.SecurityAssignments.Any(link => link.Type == nameof(LinkedRoleObject))
                            && p.SecurityAssignments.Any(link => link.Type == nameof(LinkedFunctionObject)),
                        SampleDataTestHelper.GetTestRoleEntities()
                            .Cast<IAssignmentObjectEntity>()
                            .Concat(DatabaseFixture.TestFunctions)));
        }
    }
}
