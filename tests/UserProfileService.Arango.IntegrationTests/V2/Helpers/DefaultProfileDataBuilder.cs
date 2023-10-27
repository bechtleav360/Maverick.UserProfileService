using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    internal class DefaultProfileDataBuilder : IProfileDataBuilder
    {
        private readonly ProfileDataOptions _cachedOptions = new ProfileDataOptions();

        private readonly List<IProfileEntityModel> _profiles = new List<IProfileEntityModel>();

        private readonly Dictionary<string, HashSet<string>> _storedProfileProfileRelations =
            new Dictionary<string, HashSet<string>>();

        private readonly Dictionary<string, HashSet<string>> _storedProfileToFunctionRelations =
            new Dictionary<string, HashSet<string>>();

        private readonly Dictionary<string, HashSet<string>> _storedProfileToRoleRelations =
            new Dictionary<string, HashSet<string>>();

        public IProfileDataBuilder AddRelationProfileToProfile(
            string parentId,
            string childId)
        {
            if (!_storedProfileProfileRelations.ContainsKey(parentId))
            {
                _storedProfileProfileRelations.Add(parentId, new HashSet<string>());
            }

            if (!_storedProfileProfileRelations[parentId].Contains(childId))
            {
                _storedProfileProfileRelations[parentId].Add(childId);
            }

            return this;
        }

        public IProfileDataBuilder AddProfiles(IEnumerable<IProfileEntityModel> profiles)
        {
            _profiles.AddRange(profiles);

            return this;
        }

        public IProfileDataBuilder AddProfile(IProfileEntityModel profile)
        {
            _profiles.Add(profile);

            return this;
        }

        public IProfileDataBuilder AddFunctions(IEnumerable<FunctionObjectEntityModel> functions)
        {
            _cachedOptions.FunctionsAndRoles.AddRange(functions);

            return this;
        }

        public IProfileDataBuilder AddRoles(IEnumerable<RoleObjectEntityModel> roles)
        {
            _cachedOptions.FunctionsAndRoles.AddRange(roles);

            return this;
        }

        public IProfileDataBuilder AddRole(RoleObjectEntityModel role)
        {
            _cachedOptions.FunctionsAndRoles.Add(role);

            return this;
        }

        public IProfileDataBuilder AddRelationProfileToRole(
            string profileId,
            string roleId)
        {
            if (!_storedProfileToRoleRelations.ContainsKey(profileId))
            {
                _storedProfileToRoleRelations.Add(profileId, new HashSet<string>());
            }

            if (!_storedProfileToRoleRelations[profileId].Contains(roleId))
            {
                _storedProfileToRoleRelations[profileId].Add(roleId);
            }

            return this;
        }

        public IProfileDataBuilder AddRelationProfileToFunction(
            string profileId,
            string functionId)
        {
            if (!_storedProfileToFunctionRelations.ContainsKey(profileId))
            {
                _storedProfileToFunctionRelations.Add(profileId, new HashSet<string>());
            }

            if (!_storedProfileToFunctionRelations[profileId].Contains(functionId))
            {
                _storedProfileToFunctionRelations[profileId].Add(functionId);
            }

            return this;
        }

        public ProfileDataOptions Build()
        {
            _profiles.ForEach(
                p =>
                {
                    if (!_storedProfileProfileRelations.ContainsKey(p.Id) || !(p is GroupEntityModel group))
                    {
                        return;
                    }

                    List<IProfileEntityModel> children = _profiles
                        .Where(
                            c => _storedProfileProfileRelations[p.Id]
                                .Contains(c.Id))
                        .ToList();

                    if (group.Members == null)
                    {
                        group.Members = children
                            .Select(
                                SampleDataTestHelper.GetDefaultTestMapper()
                                    .Map<Member>)
                            .ToList();
                    }
                    else
                    {
                        group.Members = group.Members
                            .Concat(
                                children
                                    .Select(
                                        SampleDataTestHelper
                                            .GetDefaultTestMapper()
                                            .Map<Member>))
                            .GroupBy(
                                m => m.Id,
                                m => m,
                                (_, elements) => elements.FirstOrDefault())
                            .ToList();
                    }

                    children.ForEach(
                        c =>
                        {
                            c.MemberOf ??= new List<Member>();

                            c.MemberOf
                                .Add(
                                    SampleDataTestHelper.GetDefaultTestMapper()
                                        .Map<Member>(group));
                        });
                });

            _cachedOptions.Profiles.AddRange(_profiles);

            return _cachedOptions;
        }
    }
}
