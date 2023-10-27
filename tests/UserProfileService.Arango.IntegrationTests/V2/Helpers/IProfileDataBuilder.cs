using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    internal interface IProfileDataBuilder
    {
        IProfileDataBuilder AddProfile(IProfileEntityModel profile);
        IProfileDataBuilder AddProfiles(IEnumerable<IProfileEntityModel> profiles);
        IProfileDataBuilder AddFunctions(IEnumerable<FunctionObjectEntityModel> functions);
        IProfileDataBuilder AddRoles(IEnumerable<RoleObjectEntityModel> roles);
        IProfileDataBuilder AddRole(RoleObjectEntityModel role);

        IProfileDataBuilder AddRelationProfileToRole(
            string secOId,
            string profileId);

        IProfileDataBuilder AddRelationProfileToFunction(
            string profileId,
            string functionId);

        ProfileDataOptions Build();

        IProfileDataBuilder AddRelationProfileToProfile(
            string parentId,
            string childId);
    }
}
