using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.Abstractions.Models;
using AggregatedModels = Maverick.UserProfileService.AggregateEvents.Common.Models;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using TagType = Maverick.UserProfileService.AggregateEvents.Common.Enums.TagType;

namespace UserProfileService.Projection.FirstLevel.UnitTests.HandlerTests.MethodTest
{
    internal static class ArgumentsObjectAssignmentMethods
    {
        private static readonly DateTime _startDate = DateTime.Now;
        private static readonly DateTime _endDate = DateTime.Now.AddDays(32);

        #region GetAssingment-Method-Data

        #region FunctionToProfileData

        private static readonly ObjectAssignmentEvent _assignmentsFunctionToProfile = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "AssingToFunction",
                    ObjectType.Function),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "GroupToAssign",
                        ObjectType.Group,
                        new RangeCondition()),
                    new ConditionObjectIdent(
                        "UserToAssign",
                        ObjectType.User,
                        new RangeCondition()),
                    new ConditionObjectIdent(
                        "SecondGroupToAssign",
                        ObjectType.Group,
                        new RangeCondition())
                },

                Removed = new[]
                {
                    new ConditionObjectIdent(
                        "GroupToAssign",
                        ObjectType.Group,
                        new RangeCondition()),
                    new ConditionObjectIdent(
                        "UserToAssign",
                        ObjectType.User,
                        new RangeCondition()),
                    new ConditionObjectIdent(
                        "SecondGroupToAssign",
                        ObjectType.Group,
                        new RangeCondition())
                },
                Type = AssignmentType
                    .ChildrenToParent
            }
        };

        private static readonly Assignment[] _assignmentsFunctionToProfileResult =
        {
            new Assignment
            {
                Conditions =
                    new[] { new RangeCondition() },
                ProfileId = "GroupToAssign",
                TargetId = "AssingToFunction",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[] { new RangeCondition() },
                ProfileId = "UserToAssign",
                TargetId = "AssingToFunction",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[] { new RangeCondition() },
                ProfileId = "SecondGroupToAssign",
                TargetId = "AssingToFunction",
                TargetType = ObjectType.Function
            }
        };

        #endregion

        #region RoleToProfileData

        private static readonly ObjectAssignmentEvent _assignmentsRoleToProfile = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "AssingToRole",
                    ObjectType.Role),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "GroupToAssign",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))),
                    new ConditionObjectIdent(
                        "UserToAssign",
                        ObjectType.User,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))),
                    new ConditionObjectIdent(
                        "SecondGroupToAssign",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2)))
                },

                Removed = new[]
                {
                    new ConditionObjectIdent(
                        "GroupToAssign",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))),
                    new ConditionObjectIdent(
                        "UserToAssign",
                        ObjectType.User,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))),
                    new ConditionObjectIdent(
                        "SecondGroupToAssign",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2)))
                },
                Type = AssignmentType
                    .ChildrenToParent
            }
        };

        private static readonly Assignment[] _assignmentsRoleToProfileResult =
        {
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))
                    },
                ProfileId = "GroupToAssign",
                TargetId = "AssingToRole",
                TargetType = ObjectType.Role
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))
                    },
                ProfileId = "UserToAssign",
                TargetId = "AssingToRole",
                TargetType = ObjectType.Role
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))
                    },
                ProfileId = "SecondGroupToAssign",
                TargetId = "AssingToRole",
                TargetType = ObjectType.Role
            }
        };

        #endregion

        #region GroupToContainerData

        private static readonly ObjectAssignmentEvent _assignmentsGroupToContainer = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "GroupsToAssingToVariousContainer",
                    ObjectType.Group),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "FunctionOne",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(10))),
                    new ConditionObjectIdent(
                        "FunctionTwo",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(12))),
                    new ConditionObjectIdent(
                        "RoleOne",
                        ObjectType.Role,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2)))
                },

                Removed = new[]
                {
                    new ConditionObjectIdent(
                        "FunctionOne",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(10))),
                    new ConditionObjectIdent(
                        "FunctionTwo",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(12))),
                    new ConditionObjectIdent(
                        "RoleOne",
                        ObjectType.Role,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2)))
                },
                Type = AssignmentType
                    .ChildrenToParent
            }
        };

        private static readonly Assignment[] _assignmentsGroupToContainerResult =
        {
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(10))
                    },
                ProfileId = "GroupsToAssingToVariousContainer",
                TargetId = "FunctionOne",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(12))
                    },
                ProfileId = "GroupsToAssingToVariousContainer",
                TargetId = "FunctionTwo",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddDays(2))
                    },
                ProfileId = "GroupsToAssingToVariousContainer",
                TargetId = "RoleOne",
                TargetType = ObjectType.Role
            }
        };

        #endregion

        #region UserToContainerData

        private static readonly ObjectAssignmentEvent _assignmentsUserToContainer = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "UserToAssingToVariousContainer",
                    ObjectType.User),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "FunctionOne",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(1))),
                    new ConditionObjectIdent(
                        "FunctionTwo",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(12))),
                    new ConditionObjectIdent(
                        "RoleOne",
                        ObjectType.Role,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(23)))
                },

                Removed = new[]
                {
                    new ConditionObjectIdent(
                        "FunctionOne",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(1))),
                    new ConditionObjectIdent(
                        "FunctionTwo",
                        ObjectType.Function,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(12))),
                    new ConditionObjectIdent(
                        "RoleOne",
                        ObjectType.Role,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(23)))
                },
                Type = AssignmentType
                    .ChildrenToParent
            }
        };

        private static readonly Assignment[] _assignmentsUserToContainerResult =
        {
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(1))
                    },
                ProfileId = "UserToAssingToVariousContainer",
                TargetId = "FunctionOne",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(12))
                    },
                ProfileId = "UserToAssingToVariousContainer",
                TargetId = "FunctionTwo",
                TargetType = ObjectType.Function
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(23))
                    },
                ProfileId = "UserToAssingToVariousContainer",
                TargetId = "RoleOne",
                TargetType = ObjectType.Role
            }
        };

        #endregion

        #region OrganizationToOrganizationData

        private static readonly ObjectAssignmentEvent _assignmentsOrganizationToProfile = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "RootOrganization",
                    ObjectType.Organization),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "OrganizationOne",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(120))),
                    new ConditionObjectIdent(
                        "OrganizationTwo",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(120))),
                    new ConditionObjectIdent(
                        "OrganizationThree",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(120)))
                },

                Removed = new[]
                {
                    new ConditionObjectIdent(
                        "OrganizationOne",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(120))),
                    new ConditionObjectIdent(
                        "OrganizationTwo",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(120))),
                    new ConditionObjectIdent(
                        "OrganizationThree",
                        ObjectType.Organization,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(120)))
                },
                Type = AssignmentType
                    .ChildrenToParent
            }
        };

        private static readonly Assignment[] _assignmentsOrganizationToProfileResult =
        {
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(120))
                    },
                ProfileId = "OrganizationOne",
                TargetId = "RootOrganization",
                TargetType = ObjectType.Organization
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(120))
                    },
                ProfileId = "OrganizationTwo",
                TargetId = "RootOrganization",
                TargetType = ObjectType.Organization
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(120))
                    },
                ProfileId = "OrganizationThree",
                TargetId = "RootOrganization",
                TargetType = ObjectType.Organization
            }
        };

        #endregion

        #region GroupsToProfiles

        private static readonly ObjectAssignmentEvent _assignmentsGroupParentsToUser = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "UserAssisgnedAsChild",
                    ObjectType.User),
                Added = new[]
                {
                    new ConditionObjectIdent(
                        "GroupParentsOne",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(11))),
                    new ConditionObjectIdent(
                        "GroupParentsTwo",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(7))),
                    new ConditionObjectIdent(
                        "GroupParentsThree",
                        ObjectType.Group,
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(17)))
                },
                Type = AssignmentType.ParentsToChild
            }
        };

        private static readonly Assignment[] _assignmentsGroupParentsToUserResult =
        {
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddHours(11))
                    },
                ProfileId = "UserAssisgnedAsChild",
                TargetId = "GroupParentsOne",
                TargetType = ObjectType.Group
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddMonths(7))
                    },
                ProfileId = "UserAssisgnedAsChild",
                TargetId = "GroupParentsTwo",
                TargetType = ObjectType.Group
            },
            new Assignment
            {
                Conditions =
                    new[]
                    {
                        new RangeCondition(
                            _startDate,
                            _startDate.AddYears(17))
                    },
                ProfileId = "UserAssisgnedAsChild",
                TargetId = "GroupParentsThree",
                TargetType = ObjectType.Group
            }
        };

        #endregion

        #region AssignmentsBadData

        private static readonly ObjectAssignmentEvent _badAssignmentsData = new ObjectAssignmentEvent
        {
            Payload = new AssignmentPayload
            {
                Resource = new ObjectIdent(
                    "UserAssisgnedAsChild",
                    ObjectType.User),
                Added = new[]
                {
                    new
                        ConditionObjectIdent(
                            "FunctionOne",
                            ObjectType
                                .Function,
                            new
                                RangeCondition(
                                    _startDate,
                                    _startDate
                                        .AddHours(11))),
                    new
                        ConditionObjectIdent(
                            "GroupOne",
                            ObjectType
                                .Group,
                            new
                                RangeCondition(
                                    _startDate,
                                    _startDate
                                        .AddMonths(7))),
                    new
                        ConditionObjectIdent(
                            "RoleOne",
                            ObjectType.Role,
                            new
                                RangeCondition(
                                    _startDate,
                                    _startDate
                                        .AddYears(17))),
                    new
                        ConditionObjectIdent(
                            "OrganizationOne",
                            ObjectType
                                .Organization,
                            new
                                RangeCondition(
                                    _startDate,
                                    _startDate
                                        .AddYears(17)))
                },
                Type = AssignmentType
                    .ParentsToChild
            }
        };

        #endregion

        #region AssignmentsMethodMemberData

        public static IEnumerable<object[]> AddAssignmentsContainerToProfiles =>
            new List<object[]>
            {
                new object[]
                {
                    _assignmentsFunctionToProfile.Payload.Added,
                    _assignmentsFunctionToProfile.Payload.Resource,
                    _assignmentsFunctionToProfile.Payload.Type,
                    _assignmentsFunctionToProfileResult
                },
                new object[]
                {
                    _assignmentsFunctionToProfile.Payload.Removed,
                    _assignmentsFunctionToProfile.Payload.Resource,
                    _assignmentsFunctionToProfile.Payload.Type,
                    _assignmentsFunctionToProfileResult
                },
                new object[]
                {
                    _assignmentsRoleToProfile.Payload.Added,
                    _assignmentsRoleToProfile.Payload.Resource,
                    _assignmentsRoleToProfile.Payload.Type,
                    _assignmentsRoleToProfileResult
                },
                new object[]
                {
                    _assignmentsRoleToProfile.Payload.Removed,
                    _assignmentsRoleToProfile.Payload.Resource,
                    _assignmentsRoleToProfile.Payload.Type,
                    _assignmentsRoleToProfileResult
                }
            };

        public static IEnumerable<object[]> AddAssignmentsOrganizationToProfiles =>
            new List<object[]>
            {
                new object[]
                {
                    _assignmentsOrganizationToProfile.Payload.Added,
                    _assignmentsOrganizationToProfile.Payload.Resource,
                    _assignmentsOrganizationToProfile.Payload.Type,
                    _assignmentsOrganizationToProfileResult
                },
                new object[]
                {
                    _assignmentsOrganizationToProfile.Payload.Removed,
                    _assignmentsOrganizationToProfile.Payload.Resource,
                    _assignmentsOrganizationToProfile.Payload.Type,
                    _assignmentsOrganizationToProfileResult
                }
            };

        public static IEnumerable<object[]> AddAssignmentsGroupsParentsToUser =>
            new List<object[]>
            {
                new object[]
                {
                    _assignmentsGroupParentsToUser.Payload.Added,
                    _assignmentsGroupParentsToUser.Payload.Resource,
                    _assignmentsGroupParentsToUser.Payload.Type,
                    _assignmentsGroupParentsToUserResult
                }
            };

        public static IEnumerable<object[]> EmptyAndNullAssignmentsParameter =>
            new List<object[]>
            {
                new object[] { null },
                new object[] { new ConditionObjectIdent[] { } }
            };

        public static IEnumerable<object[]> BadObjectionAssignmentParameter =>
            new List<object[]>
            {
                new object[]
                {
                    _badAssignmentsData.Payload.Added,
                    _badAssignmentsData.Payload.Resource,
                    _badAssignmentsData.Payload.Type
                }
            };

        public static IEnumerable<object[]> AddAssignmentsProfileToContainer =>
            new List<object[]>
            {
                new object[]
                {
                    _assignmentsGroupToContainer.Payload.Added,
                    _assignmentsGroupToContainer.Payload.Resource,
                    _assignmentsGroupToContainer.Payload.Type,
                    _assignmentsGroupToContainerResult
                },
                new object[]
                {
                    _assignmentsGroupToContainer.Payload.Removed,
                    _assignmentsGroupToContainer.Payload.Resource,
                    _assignmentsGroupToContainer.Payload.Type,
                    _assignmentsGroupToContainerResult
                },
                new object[]
                {
                    _assignmentsUserToContainer.Payload.Removed,
                    _assignmentsUserToContainer.Payload.Resource,
                    _assignmentsUserToContainer.Payload.Type,
                    _assignmentsUserToContainerResult
                },
                new object[]
                {
                    _assignmentsUserToContainer.Payload.Added,
                    _assignmentsUserToContainer.Payload.Resource,
                    _assignmentsUserToContainer.Payload.Type,
                    _assignmentsUserToContainerResult
                }
            };

        #endregion

        #endregion

        #region WasAssingedTo-Method-Data

        #region wasAssinged-To-Group-Date

        private static readonly List<RangeCondition> _rangeConditionsGroup = new List<RangeCondition>
        {
            new RangeCondition(
                _startDate,
                _startDate.AddYears(23)),
            new RangeCondition(
                _startDate.AddMonths(2),
                _startDate.AddMonths(5)),
            new RangeCondition(
                _startDate,
                _endDate)
        };

        private static readonly FirstLevelProjectionGroup _groupToAssign = new FirstLevelProjectionGroup
        {
            Id =
                "990672EA-C360-4B8A-9395-7A3D361FE18B",
            Name = "WasAssignedEvent",
            CreatedAt = _startDate,
            DisplayName = "WasAssignedEvent",
            ExternalIds =
                new List<ExternalIdentifier>
                {
                    new ExternalIdentifier(
                        "External_990672EA-C360-4B8A-9395-7A3D361FE18B",
                        "Bonnea")
                },
            IsSystem = true,
            SynchronizedAt = _startDate.AddDays(2),
            UpdatedAt = _startDate.AddDays(2),
            Weight = 2.0,
            Source = "Dream_Land",
            IsMarkedForDeletion = false
        };

        private static readonly List<AggregatedModels.TagAssignment> _groupTagAssignAssignments =
            new List<AggregatedModels.TagAssignment>
            {
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "B280ADE9-7E35-4603-9534-382704449E39",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = true
                },
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "24D80E9B-F490-485F-8961-9BD84D8D3620",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = false
                }
            };

        private static readonly FirstLevelProjectionTreeEdgeRelation _assignToGroup =
            new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = _groupToAssign,
                Conditions = _rangeConditionsGroup,
                Child = new ObjectIdent("5FB3047E-C01D-4535-87A2-DA570210E5DA", ObjectType.User),
                ParentTags = _groupTagAssignAssignments
            };

        private static readonly WasAssignedToGroup _resolvedGroupAssignment = new WasAssignedToGroup
        {
            Conditions = new[]
            {
                new AggregatedModels.RangeCondition
                {
                    Start = _startDate,
                    End = _startDate.AddYears(23)
                },
                new AggregatedModels.RangeCondition
                {
                    Start =
                        _startDate.AddMonths(2),
                    End = _startDate
                        .AddMonths(5)
                },
                new AggregatedModels.RangeCondition
                {
                    Start = _startDate,
                    End = _endDate
                }
            },
            ProfileId =
                "5FB3047E-C01D-4535-87A2-DA570210E5DA",
            Target = new ResolvedModels.Group
            {
                Id =
                    "990672EA-C360-4B8A-9395-7A3D361FE18B",
                Name = "WasAssignedEvent",
                CreatedAt = _startDate,
                DisplayName =
                    "WasAssignedEvent",
                ExternalIds =
                    new List<AggregatedModels.ExternalIdentifier>
                    {
                        new AggregatedModels.ExternalIdentifier(
                            "External_990672EA-C360-4B8A-9395-7A3D361FE18B",
                            "Bonnea")
                    },
                IsSystem = true,
                SynchronizedAt =
                    _startDate.AddDays(2),
                UpdatedAt =
                    _startDate.AddDays(2),
                Weight = 2.0,
                Source = "Dream_Land",
                IsMarkedForDeletion = false
            }
        };

        #endregion

        #region WasAssinged-To-Organization-Data

        private static readonly List<RangeCondition> _rangeConditionsOrganization = new List<RangeCondition>
        {
            new RangeCondition(
                _startDate,
                _startDate.AddYears(3)),
            new RangeCondition(
                _startDate.AddMonths(222),
                _startDate.AddMonths(511)),
            new RangeCondition(
                _startDate,
                _endDate),
            new RangeCondition(
                _startDate,
                _endDate.AddHours(2323))
        };

        private static readonly FirstLevelProjectionOrganization _organizationToAssign =
            new FirstLevelProjectionOrganization
            {
                Id =
                    "29C5C3DF-5F79-49C2-A061-392E5FEAFD56",
                Name = "WasAssignedEvent_Organization",
                CreatedAt = _startDate,
                DisplayName = "WasAssignedEvent_Organization",
                ExternalIds =
                    new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            "External_29C5C3DF-5F79-49C2-A061-392E5FEAFD56",
                            "Bonnea")
                    },
                IsSystem = true,
                SynchronizedAt = _startDate.AddDays(2),
                UpdatedAt = _startDate.AddDays(2),
                Weight = 2.0,
                Source = "Dream_Land",
                IsMarkedForDeletion = false,
                IsSubOrganization = false
            };

        private static readonly List<AggregatedModels.TagAssignment> _organizationTagAssignAssignments =
            new List<AggregatedModels.TagAssignment>
            {
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "B280ADE9-7E35-4603-9534-382704449E39",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = true
                },
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "24D80E9B-F490-485F-8961-9BD84D8D3620",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = false
                }
            };

        private static readonly FirstLevelProjectionTreeEdgeRelation _assignToOrganization =
            new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = _organizationToAssign,
                Conditions = _rangeConditionsOrganization,
                Child = new ObjectIdent("84D8A215-80B9-44A7-82AD-BC0D447D15E5", ObjectType.Organization),
                ParentTags = _groupTagAssignAssignments
            };

        private static readonly WasAssignedToOrganization _resolvedOrganizationAssignment =
            new WasAssignedToOrganization
            {
                Conditions = new[]
                {
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _startDate.AddYears(3)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start =
                            _startDate.AddMonths(222),
                        End = _startDate
                            .AddMonths(511)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate.AddHours(2323)
                    }
                },
                ProfileId =
                    "84D8A215-80B9-44A7-82AD-BC0D447D15E5",
                Target = new ResolvedModels.Organization
                {
                    Id =
                        "29C5C3DF-5F79-49C2-A061-392E5FEAFD56",
                    Name = "WasAssignedEvent_Organization",
                    CreatedAt = _startDate,
                    DisplayName = "WasAssignedEvent_Organization",
                    ExternalIds =
                        new List<AggregatedModels.ExternalIdentifier>
                        {
                            new AggregatedModels.ExternalIdentifier(
                                "External_29C5C3DF-5F79-49C2-A061-392E5FEAFD56",
                                "Bonnea")
                        },
                    IsSystem = true,
                    SynchronizedAt = _startDate.AddDays(2),
                    UpdatedAt = _startDate.AddDays(2),
                    Weight = 2.0,
                    Source = "Dream_Land",
                    IsMarkedForDeletion = false,
                    IsSubOrganization = false
                }
            };

        #endregion

        #region WasAssinged-To-Role-Data

        private static readonly List<RangeCondition> _rangeConditionsRole = new List<RangeCondition>
        {
            new RangeCondition(
                _startDate,
                _startDate.AddYears(12)),
            new RangeCondition(
                _startDate.AddMonths(12),
                _startDate.AddMonths(12)),
            new RangeCondition(
                _startDate,
                _endDate),
            new RangeCondition(
                _startDate,
                _endDate.AddHours(12))
        };

        private static readonly FirstLevelProjectionRole _roleToAssign =
            new FirstLevelProjectionRole
            {
                Id =
                    "001DD1DA-7E87-4B08-9115-BCE4C017746F",
                Name = "WasAssignedEvent_Role",
                CreatedAt = _startDate,
                ExternalIds =
                    new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            "External_001DD1DA-7E87-4B08-9115-BCE4C017746F",
                            "Bonnea")
                    },
                IsSystem = true,
                SynchronizedAt = _startDate.AddDays(12),
                UpdatedAt = _startDate.AddDays(12),
                Source = "Dream_Land",
                DeniedPermissions = new List<string>
                {
                    "FREEZE",
                    "CREATE"
                },
                Permissions = new List<string>
                {
                    "READ",
                    "EDIT",
                    "COPY"
                },
                Description = "Just a role"
            };

        private static readonly List<AggregatedModels.TagAssignment> _roleTagAssignAssignments =
            new List<AggregatedModels.TagAssignment>
            {
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "B280ADE9-7E35-4603-9534-382704449E39",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = true
                },
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "24D80E9B-F490-485F-8961-9BD84D8D3620",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = false
                }
            };

        private static readonly FirstLevelProjectionTreeEdgeRelation _assignToRole =
            new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = _roleToAssign,
                Conditions = _rangeConditionsRole,
                Child = new ObjectIdent("68963E8E-7525-454C-AA55-280F27420BCC", ObjectType.User),
                ParentTags = _roleTagAssignAssignments
            };

        private static readonly WasAssignedToRole _resolvedRoleAssignment =
            new WasAssignedToRole
            {
                Conditions = new[]
                {
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _startDate.AddYears(12)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start =
                            _startDate.AddMonths(12),
                        End = _startDate
                            .AddMonths(12)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate.AddHours(12)
                    }
                },
                ProfileId =
                    "68963E8E-7525-454C-AA55-280F27420BCC",
                Target = new ResolvedModels.Role
                {
                    Id =
                        "001DD1DA-7E87-4B08-9115-BCE4C017746F",
                    Name = "WasAssignedEvent_Role",
                    CreatedAt = _startDate,
                    ExternalIds =
                        new List<AggregatedModels.ExternalIdentifier>
                        {
                            new AggregatedModels.ExternalIdentifier(
                                "External_001DD1DA-7E87-4B08-9115-BCE4C017746F",
                                "Bonnea")
                        },
                    IsSystem = true,
                    SynchronizedAt = _startDate.AddDays(12),
                    UpdatedAt = _startDate.AddDays(12),
                    Source = "Dream_Land",
                    DeniedPermissions = new List<string>
                    {
                        "FREEZE",
                        "CREATE"
                    },
                    Permissions = new List<string>
                    {
                        "READ",
                        "EDIT",
                        "COPY"
                    },
                    Description = "Just a role"
                }
            };

        #endregion

        #region WasAssinged-To-Function-Data

        private static readonly List<RangeCondition> _rangeConditionsFunction = new List<RangeCondition>
        {
            new RangeCondition(
                _startDate,
                _startDate.AddYears(7)),
            new RangeCondition(
                _startDate.AddMonths(7),
                _startDate.AddMonths(7)),
            new RangeCondition(
                _startDate,
                _endDate),
            new RangeCondition(
                _startDate,
                _endDate.AddHours(77))
        };

        private static readonly FirstLevelProjectionFunction _functionToAssign =
            new FirstLevelProjectionFunction
            {
                Id =
                    "55599C2F-4C8C-4000-B722-DF52B753B7FA",
                CreatedAt = _startDate,
                ExternalIds =
                    new List<ExternalIdentifier>
                    {
                        new ExternalIdentifier(
                            "External_55599C2F-4C8C-4000-B722-DF52B753B7FA",
                            "Bonnea")
                    },

                SynchronizedAt = _startDate.AddDays(7),
                UpdatedAt = _startDate.AddDays(7),
                Source = "Dream_Land",
                Organization = _organizationToAssign,
                Role = _roleToAssign
            };

        private static readonly List<AggregatedModels.TagAssignment> _functionTagAssignAssignments =
            new List<AggregatedModels.TagAssignment>
            {
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "B280ADE9-7E35-4603-9534-382704449E39",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = true
                },
                new AggregatedModels.TagAssignment
                {
                    TagDetails = new AggregatedModels.Tag
                    {
                        Id = "24D80E9B-F490-485F-8961-9BD84D8D3620",
                        Type = TagType.Custom,
                        Name = string.Empty
                    },
                    IsInheritable = false
                }
            };

        private static readonly FirstLevelProjectionTreeEdgeRelation _assignToFunction =
            new FirstLevelProjectionTreeEdgeRelation
            {
                Parent = _functionToAssign,
                Conditions = _rangeConditionsFunction,
                Child = new ObjectIdent("8ACF9D07-1EF3-473E-8240-C1BA23B7D8F0", ObjectType.Group),
                ParentTags = _functionTagAssignAssignments
            };

        private static readonly WasAssignedToFunction _resolvedFunctionAssignment =
            new WasAssignedToFunction
            {
                Conditions = new[]
                {
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _startDate.AddYears(7)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start =
                            _startDate.AddMonths(7),
                        End = _startDate
                            .AddMonths(7)
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate
                    },
                    new AggregatedModels.RangeCondition
                    {
                        Start = _startDate,
                        End = _endDate.AddHours(77)
                    }
                },
                ProfileId =
                    "8ACF9D07-1EF3-473E-8240-C1BA23B7D8F0",
                Target = new ResolvedModels.Function
                {
                    Id =
                        "55599C2F-4C8C-4000-B722-DF52B753B7FA",
                    CreatedAt = _startDate,
                    ExternalIds =
                        new List<AggregatedModels.ExternalIdentifier>
                        {
                            new AggregatedModels.ExternalIdentifier(
                                "External_55599C2F-4C8C-4000-B722-DF52B753B7FA",
                                "Bonnea")
                        },
                    SynchronizedAt = _startDate.AddDays(7),
                    UpdatedAt = _startDate.AddDays(7),
                    Source = "Dream_Land",
                    Organization = _resolvedOrganizationAssignment.Target,
                    Role = _resolvedRoleAssignment.Target,
                    OrganizationId = _resolvedOrganizationAssignment.Target.Id,
                    RoleId = _resolvedRoleAssignment.Target.Id
                }
            };

        #endregion

        #region MemberDateWasAssingedToData

        public static IEnumerable<object[]> WasAssignedToData =>
            new List<object[]>
            {
                new object[] { _assignToGroup, _resolvedGroupAssignment },
                new object[] { _assignToOrganization, _resolvedOrganizationAssignment },
                new object[] { _assignToRole, _resolvedRoleAssignment },
                new object[] { _assignToFunction, _resolvedFunctionAssignment }
            };

        #endregion

        #endregion
    }
}
