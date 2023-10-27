using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.Tests.Utilities.Extensions;
using ArangoProfiles = UserProfileService.Adapter.Arango.V2.Helpers.MappingProfiles;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Implementations
{
    internal class ReflectionTestData : ITestData
    {
        private readonly List<PossibleMember> _assignments;
        private readonly List<FunctionObjectEntityModel> _functions = new List<FunctionObjectEntityModel>();
        private readonly List<GroupEntityModel> _groups = new List<GroupEntityModel>();
        private readonly IMapper _mapper;

        private readonly List<OrganizationEntityModel> _organizations =
            new List<OrganizationEntityModel>();

        private readonly List<RoleObjectEntityModel> _roles = new List<RoleObjectEntityModel>();
        private readonly Dictionary<string, List<AggregateModels.TagAssignment>> _tagged;
        private readonly List<AggregateModels.Tag> _tags = new List<AggregateModels.Tag>();
        private readonly List<UserEntityModel> _users = new List<UserEntityModel>();
        private  List<ExtendedProfileEdgeData> _objectTreeEdges = new List<ExtendedProfileEdgeData>();
        private  List<ExtendedProfileVertexData> _objectTreeVertices = new List<ExtendedProfileVertexData>();

        private IDictionary<string, TreeNode> _relationTree;

        public IList<FunctionObjectEntityModel> Functions => _functions;

        public IList<GroupEntityModel> Groups => _groups;

        public IList<OrganizationEntityModel> Organizations => _organizations;

        public IList<RoleObjectEntityModel> Roles => _roles;

        public IList<AggregateModels.Tag> Tags => _tags;
        public IList<UserEntityModel> Users => _users;
        public IList<ExtendedProfileEdgeData> EdgeData => _objectTreeEdges;
        public IList<ExtendedProfileVertexData> VertexData => _objectTreeVertices;

        public ReflectionTestData(IList<SeedObject> data)
        {
            _mapper = new Mapper(
                new MapperConfiguration(
                    cfg =>
                    {
                        cfg.AddProfile(typeof(MappingProfiles));
                        cfg.AddProfile(typeof(ArangoProfiles));
                    }));

            foreach (SeedObject o in data)
             {
                AddData(o);
            }

            _assignments =
                data.Where(d => d.Assignments.Any())
                    .SelectMany(s => s.Assignments)
                    .GroupBy(
                        a => new
                        {
                            a.ChildProfileId,
                            a.ParentId
                        },
                        a => a,
                        (_, items) =>
                            new PossibleMember(items))
                    .ToList();

            _tagged =
                data
                    .Where(p => p.Entity is not ExtendedProfileVertexData && p.Entity is not ExtendedProfileEdgeData)
                    .ToDictionary(
                        d => d.Id,
                        d => d.AssignedTags
                              .Select(
                                  t => new AggregateModels.TagAssignment
                                       {
                                           TagDetails = _tags.FirstOrDefault(t2 => t2.Id == t.TagId),
                                           IsInheritable = t.IsInheritable
                                       })
                              .Where(t => t.TagDetails != null)
                              .ToList());

            AdjustRelationsAndBuildRelationsTree();
            AdjustPathsAndTags();
            CalculateProfileTreeData();
        }

        private void AddData(SeedObject obj)
        {
            if (obj.Entity is UserView userView)
            {
                _users.Add(_mapper.Map<UserEntityModel>(userView));

                return;
            }

            if (obj.Entity is GroupView groupView)
            {
                _groups.Add(_mapper.Map<GroupEntityModel>(groupView));

                return;
            }

            if (obj.Entity is Organization org)
            {
                _organizations.Add(_mapper.Map<OrganizationEntityModel>(org));

                return;
            }

            if (obj.Entity is OrganizationView orgView)
            {
                _organizations.Add(_mapper.Map<OrganizationEntityModel>(orgView));

                return;
            }

            if (obj.Entity is RoleView roleView)
            {
                _roles.Add(_mapper.Map<RoleObjectEntityModel>(roleView));

                return;
            }

            if (obj.Entity is FunctionView functionView)
            {
                _functions.Add(_mapper.Map<FunctionObjectEntityModel>(functionView));

                return;
            }

            if (obj.Entity is CalculatedTag calcTag)
            {
                _tags.Add(_mapper.Map<AggregateModels.Tag>(calcTag));

                return;
            }

            if (obj.Entity is Tag tag)
            {
                _tags.Add(_mapper.Map<AggregateModels.Tag>(tag));
            }

            if (obj.Entity is ExtendedProfileVertexData profileNode)
            {
                _objectTreeVertices.Add(_mapper.Map<ExtendedProfileVertexData>(profileNode));
            }

            if (obj.Entity is ExtendedProfileEdgeData profileEdge)
            {
                _objectTreeEdges.Add(_mapper.Map<ExtendedProfileEdgeData>(profileEdge));
            }
        }

        private void AdjustRelationsAndBuildRelationsTree()
        {
            List<IProfileEntityModel> allProfiles = _users
                .Cast<IProfileEntityModel>()
                .Concat(_groups)
                .Concat(_organizations)
                .ToList();

            // child relations
            List<List<PossibleMember>> childAssignments = _assignments
                .GroupBy(
                    a => a.ChildId,
                    a => a,
                    (_, items) => items.ToList())
                .Where(items => items.Any())
                .ToList();

            var list = new ConcurrentDictionary<string, TreeNode>();

            foreach (List<PossibleMember> mInfo in childAssignments)
            {
                // only valid for parent == profile (function/role are no profiles)
                if (mInfo[0].ChildKind == ProfileKind.User
                    && TryGet(
                        _users,
                        u => u.Id == mInfo[0].ChildId,
                        out UserEntityModel user)
                    && TryGetMember(
                        allProfiles,
                        mInfo,
                        m => m.ParentId,
                        out IList<ExtendedMember> uMembers))
                {
                    user.MemberOf = uMembers.ToApiMembers();

                    UpdateNode(
                        list,
                        user,
                        uMembers);

                    continue;
                }

                // only valid for parent == profile (function/role are no profiles)
                if (mInfo[0].ChildKind == ProfileKind.Group
                    && TryGet(
                        _groups,
                        u => u.Id == mInfo[0].ChildId,
                        out GroupEntityModel group)
                    && TryGetMember(
                        allProfiles,
                        mInfo,
                        m => m.ParentId,
                        out IList<ExtendedMember> gMembers))
                {
                    group.MemberOf = gMembers.ToApiMembers();
                    UpdateNode(list, group, gMembers);

                    continue;
                }

                // only valid for function/
                if (mInfo[0].ChildKind == ProfileKind.User
                    && TryGet(
                        _users,
                        u => u.Id == mInfo[0].ChildId,
                        out UserEntityModel userInFunction)
                    && TryGetLinkedObjects(
                        _functions,
                        mInfo,
                        f => f.ParentId,
                        out IList<ILinkedObject> functionLinks))
                {
                    userInFunction.Functions = functionLinks;

                    UpdateNode(
                        list,
                        userInFunction,
                        functionLinks);

                    continue;
                }

                // only valid for function/
                if (mInfo[0].ChildKind == ProfileKind.Group
                    && TryGet(
                        _groups,
                        u => u.Id == mInfo[0].ChildId,
                        out GroupEntityModel groupInFunction)
                    && TryGetLinkedObjects(
                        _functions,
                        mInfo,
                        f => f.ParentId,
                        out IList<ILinkedObject> functionGroupLinks))
                {
                    groupInFunction.SecurityAssignments = functionGroupLinks;

                    UpdateNode(
                        list,
                        groupInFunction,
                        functionGroupLinks);
                }
            }

            // parent relations
            List<List<PossibleMember>> parentAssignments = _assignments
                .GroupBy(
                    a => a.ParentId,
                    a => a,
                    (_, items) => items.ToList())
                .Where(items => items.Any())
                .ToList();

            foreach (List<PossibleMember> mInfo in parentAssignments)
            {
                if (mInfo[0].ParentType == ContainerType.Group
                    && TryGet(
                        _groups,
                        u => u.Id == mInfo[0].ParentId,
                        out GroupEntityModel group)
                    && TryGetMember(
                        allProfiles,
                        mInfo,
                        m => m.ChildId,
                        out IList<ExtendedMember> gMembers))
                {
                    group.Members = gMembers.ToApiMembers();

                    group.ChildrenCount = group.Members.Count(
                        m =>
                            m.Kind == Maverick.UserProfileService.Models.EnumModels.ProfileKind.Group
                            || m.Kind == Maverick.UserProfileService.Models.EnumModels.ProfileKind.User);

                    group.HasChildren = group.ChildrenCount > 0;
                }

                if (mInfo[0].ParentType == ContainerType.Function
                    && TryGet(
                        _functions,
                        f => f.Id == mInfo[0].ParentId,
                        out FunctionObjectEntityModel function)
                    && TryGetMember(
                        allProfiles,
                        mInfo,
                        m => m.ChildId,
                        out IList<ExtendedMember> fMembers))
                {
                    function.LinkedProfiles = fMembers.ToApiMembers();
                }

                if (mInfo[0].ParentType == ContainerType.Role
                    && TryGet(
                        _roles,
                        r => r.Id == mInfo[0].ParentId,
                        out RoleObjectEntityModel role)
                    && TryGetMember(
                        allProfiles,
                        mInfo,
                        m => m.ChildId,
                        out IList<ExtendedMember> rMembers))
                {
                    role.LinkedProfiles = rMembers.ToApiMembers();
                }
            }

            _relationTree = list;
        }

        private void AdjustTags(IProfileEntityModel profile)
        {
            List<string> relatedObjectIds = GetSubPathsOfRelationTreeNode(_relationTree, profile.Id)
                .SelectMany(id => id)
                .Distinct()
                .Except(new[] { profile.Id })
                .ToList();

            IEnumerable<CalculatedTag> ownTags =
                _tagged.TryGetValue(profile.Id, out List<AggregateModels.TagAssignment> value)
                    ? value
                        .Select(
                            ta => new CalculatedTag
                            {
                                Id = ta.TagDetails.Id,
                                Name = ta.TagDetails.Name,
                                IsInherited = false
                            })
                    : Enumerable.Empty<CalculatedTag>();

            profile.Tags = ownTags
                .Concat(
                    relatedObjectIds
                        .Select(
                            id => _tagged.TryGetValue(id, out List<AggregateModels.TagAssignment> aggregateTag)
                                ? aggregateTag
                                : null)
                        .Where(ta => ta != null)
                        .SelectMany(ta => ta)
                        .Where(ta => ta.IsInheritable)
                        .Select(
                            ta => new CalculatedTag
                            {
                                Id = ta.TagDetails.Id,
                                Name = ta.TagDetails.Name,
                                IsInherited = true
                            }))
                .Distinct(new CalculatedTagIdEqualityComparer())
                .ToList();
        }

        private bool TryGet<TObject>(
            IEnumerable<TObject> list,
            Func<TObject, bool> itemSelector,
            out TObject obj)
        {
            obj = list.FirstOrDefault(itemSelector);

            return obj != null;
        }

        private bool TryGetLinkedObjects(
            IEnumerable<IAssignmentObjectEntity> functionsOrRoles,
            IEnumerable<PossibleMember> matchingProfiles,
            Func<PossibleMember, string> matchingIdPropertySelector,
            out IList<ILinkedObject> elements)
        {
            ILinkedObject UpdateMember(
                ILinkedObject linkedObject,
                PossibleMember possibleMember)
            {
                linkedObject.Conditions = possibleMember.Conditions.ToApiRangeConditions().ToList();

                return linkedObject;
            }

            elements = functionsOrRoles
                .Select(
                    p => new
                    {
                        PossibleMember = matchingProfiles.FirstOrDefault(
                            o =>
                                matchingIdPropertySelector.Invoke(o) == p.Id),
                        Member = _mapper.Map<ILinkedObject>(p)
                    })
                .Where(o => o.PossibleMember != null)
                .Select(o => UpdateMember(o.Member, o.PossibleMember))
                .ToList();

            return elements.Count > 0;
        }

        private bool TryGetMember(
            IEnumerable<IProfileEntityModel> profiles,
            IEnumerable<PossibleMember> matchingProfiles,
            Func<PossibleMember, string> matchingIdPropertySelector,
            out IList<ExtendedMember> elements)
        {
            ExtendedMember UpdateMember(
                ExtendedMember member,
                PossibleMember possibleMember)
            {
                member.RangeConditions = possibleMember.Conditions;

                return member;
            }

            elements = profiles
                .Select(
                    p => new
                    {
                        PossibleMember = matchingProfiles.FirstOrDefault(
                            o =>
                                matchingIdPropertySelector.Invoke(o) == p.Id),
                        Member = new ExtendedMember(_mapper.Map<Member>(p))
                    })
                .Where(o => o.PossibleMember != null)
                .Select(o => UpdateMember(o.Member, o.PossibleMember))
                .ToList();

            return elements.Count > 0;
        }

        private static void UpdateNode(
            ConcurrentDictionary<string, TreeNode> source,
            IProfileEntityModel profile,
            IList<ExtendedMember> members)
        {
            source.AddOrUpdate(
                profile.Id,
                _ =>
                {
                    var node = new TreeNode(profile);

                    node.Parents.AddRange(
                        members.Select(
                            m => new TreeNodeCondition(
                                m.RangeConditions,
                                source.GetOrAdd(
                                    m.Original.Id,
                                    _ => new TreeNode(m.Original)))));

                    return node;
                },
                (_, current) =>
                {
                    current.Parents.AddRange(
                        members.Select(
                            m => new TreeNodeCondition(
                                m.RangeConditions,
                                source.GetOrAdd(
                                    m.Original.Id,
                                    _ => new TreeNode(m.Original)))));

                    return current;
                });
        }

        private static void UpdateNode(
            ConcurrentDictionary<string, TreeNode> source,
            IProfileEntityModel profile,
            IList<ILinkedObject> members)
        {
            source.AddOrUpdate(
                profile.Id,
                _ =>
                {
                    var node = new TreeNode(profile);

                    node.Parents.AddRange(
                        members.Select(
                            m => new TreeNodeCondition(
                                m.Conditions,
                                source.GetOrAdd(
                                    m.Id,
                                    _ => new TreeNode(m)))));

                    return node;
                },
                (_, current) =>
                {
                    current.Parents.AddRange(
                        members.Select(
                            m => new TreeNodeCondition(
                                m.Conditions,
                                source.GetOrAdd(
                                    m.Id,
                                    _ => new TreeNode(m)))));

                    return current;
                });
        }

        private void AdjustPathsAndTags()
        {
            // users
            foreach (UserEntityModel user in _users)
            {
                user.Paths = GetSubPathsOfRelationTreeNode(_relationTree, user.Id)
                    .Select(p => string.Join("/", p))
                    .Append(user.Id)
                    .Distinct()
                    .ToList();

                AdjustTags(user);
            }

            // groups
            foreach (GroupEntityModel group in _groups)
            {
                group.Paths = GetSubPathsOfRelationTreeNode(_relationTree, group.Id)
                    .Select(p => string.Join("/", p))
                    .Append(group.Id)
                    .Distinct()
                    .ToList();

                AdjustTags(group);
            }

            // organizations
            foreach (OrganizationEntityModel organization in _organizations)
            {
                organization.Paths = GetSubPathsOfRelationTreeNode(_relationTree, organization.Id)
                    .Select(p => string.Join("/", p))
                    .Append(organization.Id)
                    .Distinct()
                    .ToList();

                AdjustTags(organization);
            }
        }

        // inspired by BreadthFirstSearch
        private static List<List<string>> GetSubPathsOfRelationTreeNode(
            IDictionary<string, TreeNode> tree,
            string start,
            bool ignoreConditions = false,
            bool ignoreSimulationFlag = false)
        {
            if (!tree.ContainsKey(start))
            {
                return new List<List<string>>
                {
                    new List<string>
                    {
                        start
                    }
                };
            }

            var paths = new List<List<string>>();

            var queue = new Queue<Tuple<string, List<string>>>();

            queue.Enqueue(
                new Tuple<string, List<string>>(
                    start,
                    new List<string>
                    {
                        start
                    }));

            while (queue.Count > 0)
            {
                (string current, List<string> currentList) = queue.Dequeue();

                List<TreeNode> parents = tree[current]
                    .Parents
                    .Where(p => ignoreConditions || p.IsRelationActive(ignoreSimulationFlag))
                    .Select(p => p.Node)
                    .ToList();

                if (parents.Count == 0)
                {
                    paths.Add(currentList);
                }

                foreach (TreeNode parent in parents)
                {
                    List<string> nextList = currentList.ToList();
                    nextList.Add(parent.RegardingObjectId);
                    queue.Enqueue(new Tuple<string, List<string>>(parent.RegardingObjectId, nextList));
                }
            }

            return paths.Select(subPath => subPath.ReverseAndReturn()).ToList();
        }

        private void CalculateProfileTreeData()
        {
            var objectTreeVertices = new List<ExtendedProfileVertexData>();
            var objectTreeEdges = new List<ExtendedProfileEdgeData>();

            foreach (string start in _relationTree.Keys.ToList())
            {
                List<List<string>> paths =
                    GetSubPathsOfRelationTreeNode(_relationTree, start, true, true);

                foreach (List<string> path in paths)
                {
                    for (var i = 1; i < path.Count; i++)
                    {
                        (IList<ExtendedProfileVertexData> vertices, ExtendedProfileEdgeData edge)
                            = ProfileTreeDataBuilder.CreateTreeData(
                                start,
                                path[i - 1],
                                path[i],
                                _tagged.TryGetValue(path[i - 1], out List<AggregateModels.TagAssignment> parentTags)
                                    ? parentTags
                                    : null,
                                _tagged.TryGetValue(path[i], out List<AggregateModels.TagAssignment> childTags)
                                    ? childTags
                                    : null,
                                GetConditions(path[i], path[i - 1], true));

                        objectTreeEdges.Add(edge);
                        objectTreeVertices.AddRange(vertices);
                    }
                }
            }

            _objectTreeVertices.AddRange(objectTreeVertices.Distinct().ToList());
            _objectTreeEdges.AddRange(objectTreeEdges.Distinct().ToList());
        }

        private IList<AggregateModels.RangeCondition> GetConditions(
            string childId,
            string parentId,
            bool ignoreSimulationFlag)
        {
            List<AggregateModels.RangeCondition> conditions =
                _assignments
                    .SingleOrDefault(a => a.ChildId == childId && a.ParentId == parentId)
                    ?.Conditions
                    ?.Where(
                        c => ignoreSimulationFlag
                            || !c.OnlyValidForSimulation)
                    .Select(c => c.ToAggregateRangeCondition())
                    .ToList();

            return conditions;
        }
    }
}
