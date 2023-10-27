using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    internal class ProfileDataOptions
    {
        internal List<IProfileEntityModel> Profiles { get; } = new List<IProfileEntityModel>();
        internal List<IAssignmentObjectEntity> FunctionsAndRoles { get; } = new List<IAssignmentObjectEntity>();
    }
}
