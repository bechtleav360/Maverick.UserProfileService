using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Utilities;

namespace UserProfileService.Common.Tests.V2.Mocks
{
    public class FakeReadService : IReadService
    {
        private readonly List<Group> _groups;
        private readonly List<UserView> _users;

        public FakeReadService()
        {
            (List<Group> groups, List<UserView> users, List<FunctionView> _, List<RoleView> _,
                    List<Tag> _)
                = SampleDataHelper.GenerateSampleData();

            _groups = groups;
            _users = users;
        }

        public FakeReadService(List<Group> groups, List<UserView> users)
        {
            _groups = groups;
            _users = users;
        }

        public Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
            RequestedProfileKind expectedKind = RequestedProfileKind.All,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default)
            where TUser : UserBasic
            where TGroup : GroupBasic
            where TOrgUnit : OrganizationBasic
        {
            List<IProfile> profileList = Enumerable.Empty<IProfile>()
                .Concat(_users)
                .Concat(_groups)
                .ToList();

            return Task.FromResult<IPaginatedList<IProfile>>(
                new PaginatedList<IProfile>(
                    profileList,
                    profileList.Count));
        }

        public Task<IPaginatedList<IProfile>> GetProfilesWithTagAsync<TUser, TGroup, TOrgUnit>(
            string tag,
            RequestedProfileKind expectedKind = RequestedProfileKind.All,
            QueryObject options = null,
            CancellationToken cancellationToken = default)
            where TUser : UserBasic where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<IProfile>> GetProfilesAsync<TUser, TGroup, TOrgUnit>(
            IEnumerable<string> profileIds,
            RequestedProfileKind expectedKind = RequestedProfileKind.All,
            CancellationToken cancellationToken = default)
            where TUser : UserBasic where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
        {
            throw new NotImplementedException();
        }

        public Task<TProfile> GetProfileAsync<TProfile>(
            string profileId,
            RequestedProfileKind expectedKind,
            bool includeInvalidAssignments = true,
            CancellationToken cancellationToken = default) where TProfile : IProfile
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<T>> SearchAsync<T>(
            QueryObject options = null,
            CancellationToken cancellationToken = default) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<IContainerProfile>> GetRootProfilesAsync<TGroup, TOrgUnit>(
            RequestedProfileKind expectedKind,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default) where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<IContainerProfile>> GetParentsOfProfileAsync<TGroup, TOrgUnit>(
            string profileId,
            RequestedProfileKind expectedKind,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default) where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
        {
            throw new NotImplementedException();
        }

        public Task<List<IContainerProfile>> GetAllParentsOfProfileAsync(
            string profileId,
            RequestedProfileKind expectedKind,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<IProfile>> GetChildrenOfProfileAsync<TUser, TGroup, TOrgUnit>(
            string profileId,
            ProfileContainerType expectedParentType,
            RequestedProfileKind expectedChildrenKind = RequestedProfileKind.All,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default)
            where TUser : UserBasic where TGroup : GroupBasic where TOrgUnit : OrganizationBasic
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetImageProfileAsync(string profileId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCustomPropertyOfProfileAsync(
            string profileId,
            string customPropertyKey,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<string>> GetFunctionalAccessRightsOfProfileAsync(
            string profileId,
            bool includeInherited = false,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckFunctionalAccessRightOfProfileAsync(
            string profileId,
            string functionalName,
            bool includeInherited = false,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<IAssignmentObject>> GetLinksForProfileAsync(
            string profileId,
            QueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<Member>> GetAssignedProfiles(
            string roleOrFunctionId,
            QueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<TRole>> GetRolesAsync<TRole>(
            QueryObject options = null,
            CancellationToken cancellationToken = default) where TRole : RoleBasic
        {
            throw new NotImplementedException();
        }

        public Task<RoleView> GetRoleAsync(string roleId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default) where TFunction : FunctionBasic
        {
            throw new NotImplementedException();
        }

        public Task<TFunction> GetFunctionAsync<TFunction>(
            string functionId,
            CancellationToken cancellationToken = default) where TFunction : FunctionView
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<CalculatedTag>> GetTagsOfProfileAsync(
            string profileOrObjectId,
            RequestedTagType tagType,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<LinkedRoleObject>> GetRolesOfProfileAsync(
            string profileId,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<LinkedFunctionObject>> GetFunctionsOfProfileAsync(
            string profileId,
            bool returnFunctionsRecursively = false,
            AssignmentQueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<JObject> GetSettingsOfProfileAsync(
            string profileId,
            ProfileKind profileKind,
            string settingsKey,
            bool includeInherited = true,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ObjectIdent>> GetAllAssignedIdsOfUserAsync(
            string profileId,
            bool includeInactiveAssignments,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<Tag>> GetTagsAsync(
            QueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<Tag>> GetTagsAsync(
            IEnumerable<string> tagIds,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> GetTagAsync(string tagId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetExistentTagsAsync(
            IEnumerable<string> tagIds,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IList<IProfile>> GetAllProfilesAsync(
            RequestedProfileKind profileKindFilter = RequestedProfileKind.All,
            string sortingPropertyName = "id",
            SortOrder sortOrder = SortOrder.Asc,
            CancellationToken cancellationToken = default)
        {
            List<IProfile> profileList = Enumerable.Empty<IProfile>()
                .Concat(_users)
                .Concat(_groups)
                .ToList();

            return Task.FromResult<IList<IProfile>>(
                new PaginatedList<IProfile>(
                    profileList,
                    profileList.Count));
        }

        public Task<IList<IAssignmentObject>> GetAllAssignmentObjectsAsync(
            RequestedAssignmentObjectType typeFilter = RequestedAssignmentObjectType.Function,
            string sortingPropertyName = "id",
            SortOrder sortOrder = SortOrder.Asc,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IPaginatedList<ActivityLogEntry>> GetActivityLogsAsync(
            ObjectType entityType,
            IEnumerable<string> objectIds = null,
            QueryObject options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<IProfile>> GetProfileByExternalOrInternalIdAsync<TUser, TGroup, TOrgUnit>(
            string profileId,
            bool allowExternalIds = true,
            string source = null,
            CancellationToken cancellationToken = default)
            where TUser : User where TGroup : Group where TOrgUnit : Organization
        {
            throw new NotImplementedException();
        }

        public Task<IProfile> GetProfileByExternalAndInternalIdAsync(
            string profileId,
            bool allowExternalIds = true,
            string source = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IProfile> GetProfileByIdOrExternalIdAsync<TUser, TGroup, TOrgUnit>(
            string idOrExternalId,
            CancellationToken cancellationToken = default)
            where TUser : IProfile
            where TGroup : IContainerProfile
            where TOrgUnit : IContainerProfile
        {
            throw new NotImplementedException();
        }
    }
}
