using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json.Linq;
using UserProfileService.Common;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.MockDataBuilder;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.UnitTests.Extensions;
using EventInitiatorApi = UserProfileService.EventSourcing.Abstractions.Models.EventInitiator;
using EventInitiatorResolved = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using ExternalIdentifierApi = Maverick.UserProfileService.Models.Models.ExternalIdentifier;
using ExternalIdentifierResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using InitiatorTypeResolved = Maverick.UserProfileService.AggregateEvents.Common.InitiatorType;
using GroupResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using InitiatorType = UserProfileService.EventSourcing.Abstractions.Models.InitiatorType;
using OrganizationResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using MemberResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using RangeConditionResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using ObjectTypeResolved = Maverick.UserProfileService.AggregateEvents.Common.Enums.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using ProfileKindResolved = Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind;
using RangeCondition = Maverick.UserProfileService.Models.Models.RangeCondition;
using TagApi = Maverick.UserProfileService.Models.RequestModels.Tag;
using TagType = Maverick.UserProfileService.AggregateEvents.Common.Enums.TagType;

namespace UserProfileService.Projection.FirstLevel.UnitTests
{
    internal static class InputSagaWorkerEventsOutputEventTuple
    {
        internal const string ClientSettingsKeyToDelete = "delete-me";
        internal const string ClientSettingsValueToDelete = "will be deleted";
        internal const string ClientSettingsKeyToKeep = "valid-key";
        internal const string ClientSettingsValueToKeep = "don't delete me";
        internal const string ClientSettingDoubleKey = "double-key";
        internal const string ProcessId = UserProfileServiceEventExtensions.DefaultProcessId;
        private static readonly DateTime _createdAt = DateTime.UtcNow.AddYears(-1);
        private static readonly DateTime _updatedAt = DateTime.UtcNow;
        
        #region ObjectAssignmentHandler

        #region ObjectAssingmentEvents_Groups_Assingmets

        // test includes to add root to the firstLevel group
        // the firstLevelGroup is already assinged to the secondLevelGroups
        internal static ObjectAssignmentEvent AddRootToFirstLevelGroupAssignment = new ObjectAssignmentEvent
        {
            CorrelationId =
                "127C1732-5658-4517-BCC8-8501010CCD98",
            EventId =
                "E4E129F2-EADD-4FC2-A2A0-1B21FABD3141",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            RequestSagaId =
                "E236035B-5D2A-4CE3-86A5-09865E1E988A",
            Timestamp = DateTime.UtcNow,
            VersionInformation = 2,
            Payload = new AssignmentPayload
            {
                Added = new[]
                {
                    new ConditionObjectIdent
                    {
                        Conditions = new[] { new RangeCondition() },
                        Id = "firstLevel",
                        Type = ObjectType.Group
                    }
                },
                Removed = Array.Empty<ConditionObjectIdent>(),
                IsSynchronized = false,
                Resource = new ObjectIdent("root", ObjectType.Group),
                Type = AssignmentType.ChildrenToParent
            },

            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId = "127C1732-5658-4517-BCC8-8501010CCD98",
                Initiator = new EventInitiatorResolved
                {
                    Id = "System",
                    Type = InitiatorTypeResolved.System
                },
                ProcessId = "E236035B-5D2A-4CE3-86A5-09865E1E988A",
                VersionInformation = 2
            }
        };

        internal static FirstLevelProjectionGroup RootGroup = new FirstLevelProjectionGroup
        {
            Id = "root",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName = "Root-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "root-External",
                    Source = "Bonnea"
                }
            },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "root-Bitch"
        };

        internal static FirstLevelProjectionGroup FirstLevelGroup = new FirstLevelProjectionGroup
        {
            Id = "firstLevel",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 2.0,
            DisplayName = "firstLevel-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "firstLevel#External",
                    Source = "Bonnea"
                }
            },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "firstLevel-Bitch"
        };

        internal static FirstLevelProjectionGroup SecondLevelGroup = new FirstLevelProjectionGroup
        {
            Id = "secondLevel",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName = "secondLevel-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "secondLevel#External",
                    Source = "Bonnea"
                }
            },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "secondLevel-Bitch"
        };

        internal static EventMetaData MetaDataGroupsAssignments = new EventMetaData
        {
            CorrelationId =
                AddRootToFirstLevelGroupAssignment
                    .CorrelationId,
            ProcessId =
                AddRootToFirstLevelGroupAssignment
                    .RequestSagaId,
            Timestamp =
                AddRootToFirstLevelGroupAssignment
                    .Timestamp,
            HasToBeInverted = false,
            Initiator = new EventInitiatorResolved
            {
                Id = "Api",
                Type = InitiatorTypeResolved.User
            },
            VersionInformation = 1,
            RelatedEntityId =
                AddRootToFirstLevelGroupAssignment.Payload
                    .Resource.Id
        };

        internal static WasAssignedToGroup FirstGroupAssignToRoot = new WasAssignedToGroup
        {
            ProfileId = FirstLevelGroup.Id,
            Conditions = new[]
            {
                new RangeConditionResolved
                {
                    End = null,
                    Start = null
                }
            },
            Target = new GroupResolved
            {
                Id = RootGroup.Id,
                UpdatedAt = RootGroup.UpdatedAt,
                CreatedAt = RootGroup.CreatedAt,
                Weight = RootGroup.Weight,
                DisplayName = RootGroup.DisplayName,
                ExternalIds =
                    new List<ExternalIdentifierResolved>
                    {
                        new ExternalIdentifierResolved
                        {
                            Id = "root-External",
                            Source = "Bonnea"
                        }
                    },
                Source = RootGroup.Source,
                IsMarkedForDeletion =
                    RootGroup.IsMarkedForDeletion,
                IsSystem = RootGroup.IsSystem,
                Name = RootGroup.Name
            },
            MetaData = MetaDataGroupsAssignments
                .CloneEventDate()
        };

        internal static WasAssignedToGroup FirstGroupAssignToRootSecondStream = new WasAssignedToGroup
        {
            ProfileId = FirstLevelGroup.Id,
            Conditions = new[]
            {
                new RangeConditionResolved
                {
                    End = null,
                    Start = null
                }
            },
            Target = new GroupResolved
            {
                Id = RootGroup.Id,
                UpdatedAt = RootGroup.UpdatedAt,
                CreatedAt = RootGroup.CreatedAt,
                Weight = RootGroup.Weight,
                DisplayName = RootGroup.DisplayName,
                ExternalIds =
                    new List<
                        ExternalIdentifierResolved>
                    {
                        new ExternalIdentifierResolved
                        {
                            Id = "root-External",
                            Source = "Bonnea"
                        }
                    },
                Source = RootGroup.Source,
                IsMarkedForDeletion =
                    RootGroup.IsMarkedForDeletion,
                IsSystem = RootGroup.IsSystem,
                Name = RootGroup.Name
            },
            MetaData = MetaDataGroupsAssignments.CloneEventDate()
        };

        internal static MemberAdded MemberAddedToRoot = new MemberAdded
        {
            Member = new MemberResolved
            {
                Id = FirstLevelGroup.Id,
                DisplayName = FirstLevelGroup.DisplayName,
                ExternalIds = new List<
                    ExternalIdentifierResolved>
                {
                    new ExternalIdentifierResolved
                    {
                        Id = "firstLevel#External",
                        Source = "Bonnea"
                    }
                },
                Kind = ProfileKindResolved
                    .Group,
                Name = FirstLevelGroup.Name,
                Conditions = new List<RangeConditionResolved>
                {
                    new RangeConditionResolved
                    {
                        End = null,
                        Start = null
                    }
                }
            },
            ParentId = RootGroup.Id,
            ParentType = ContainerType.Group,
            MetaData = MetaDataGroupsAssignments.CloneEventDate()
        };

        internal static TagAssignment[] TagAssignmentGroup =
        {
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "First-Level-Tag-Group",
                    Type = TagType.Custom
                },
                IsInheritable = true
            },
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "Second-Id-Tag-Group",
                    Type = TagType.Custom
                },
                IsInheritable = true
            },
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "Third-Id-Tag-Group",
                    Type = TagType.Custom
                },
                IsInheritable = true
            }
        };

        internal static TagsAdded TagAddToFirstGroup = new TagsAdded
        {
            Id = RootGroup.Id,
            ObjectType = ObjectTypeResolved.Group,
            Tags = TagAssignmentGroup,
            MetaData = MetaDataGroupsAssignments.CloneEventDate()
        };

        internal static TagsAdded TagAddToSecondGroup = new TagsAdded
        {
            Id = RootGroup.Id,
            ObjectType = ObjectTypeResolved.Group,
            Tags = TagAssignmentGroup,
            MetaData = MetaDataGroupsAssignments.CloneEventDate()
        };

        internal static IEnumerable<EventTuple> ResolvedEventsGroupsAssignments = new[]
        {
            new EventTuple
            {
                TargetStream = "Mock-Group-root",
                Event = MemberAddedToRoot.SetRelatedEntityId("Mock-Group-root")
            },
            new EventTuple
            {
                TargetStream = "Mock-Group-firstLevel",
                Event = FirstGroupAssignToRoot.SetRelatedEntityId("Mock-Group-firstLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Group-firstLevel",
                Event = TagAddToFirstGroup.SetRelatedEntityId("Mock-Group-firstLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Group-secondLevel",
                Event = FirstGroupAssignToRootSecondStream.SetRelatedEntityId("Mock-Group-secondLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Group-secondLevel",
                Event = TagAddToSecondGroup.SetRelatedEntityId("Mock-Group-secondLevel")
            }
        };

        #endregion

        #region ObjectAssingmentEvents_Organization_Assingmets

        internal static TagAssignment[] TagAssignmentOrganization =
        {
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "First-Level-Tag-Org",
                    Type = TagType.Custom
                },
                IsInheritable = true
            },
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "Second-Id-Tag-Org",
                    Type = TagType.Custom
                },
                IsInheritable = true
            },
            new TagAssignment
            {
                TagDetails = new Tag
                {
                    Id = "Third-Id-Tag-Org",
                    Type = TagType.Custom
                },
                IsInheritable = true
            }
        };

        // test includes to add root to the firstLevel organization
        // the firstLevelOrganization is already assigned to the secondLevelOrganization
        internal static ObjectAssignmentEvent AddRootToFirstLevelOrganizationAssignment = new ObjectAssignmentEvent
        {
            CorrelationId =
                "2165F8D9-F022-4330-92C0-D12A03859070",
            EventId =
                "760184C4-BE1A-4CFC-A058-4F004A314729",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            RequestSagaId =
                "E92130B8-7982-47B7-A4B5-D36E637E3586",
            Timestamp = DateTime.UtcNow,
            VersionInformation = 2,
            Payload = new AssignmentPayload
            {
                Added = new[]
                {
                    new ConditionObjectIdent
                    {
                        Conditions = new[] { new RangeCondition() },
                        Id = "firstLevel",
                        Type = ObjectType.Organization
                    }
                },
                Removed = Array.Empty<ConditionObjectIdent>(),
                IsSynchronized = false,
                Type = AssignmentType.ChildrenToParent,
                Resource = new ObjectIdent("root", ObjectType.Organization)
            },

            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId = "2165F8D9-F022-4330-92C0-D12A03859070",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved.User
                },
                ProcessId = "E92130B8-7982-47B7-A4B5-D36E637E3586",
                VersionInformation = 2
            }
        };

        internal static EventMetaData MetaDataOrganizationAssignments = new EventMetaData
        {
            CorrelationId =
                AddRootToFirstLevelOrganizationAssignment
                    .CorrelationId,
            ProcessId =
                AddRootToFirstLevelOrganizationAssignment
                    .RequestSagaId,
            Timestamp =
                AddRootToFirstLevelOrganizationAssignment
                    .Timestamp,
            HasToBeInverted = false,
            Initiator = new EventInitiatorResolved
            {
                Id = "Api",
                Type = InitiatorTypeResolved.User
            },
            VersionInformation = 1,
            RelatedEntityId =
                AddRootToFirstLevelOrganizationAssignment
                    .Payload.Resource.Id
        };

        internal static FirstLevelProjectionOrganization RootOrganization = new FirstLevelProjectionOrganization
        {
            Id = "root",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName = "Root-Display",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id = "root-External",
                        Source = "Bonnea"
                    }
                },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "root-Bitch",
            IsSubOrganization = false
        };

        internal static FirstLevelProjectionOrganization FirstLevelOrganization = new FirstLevelProjectionOrganization
        {
            Id = "firstLevel",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 2.0,
            DisplayName = "firstLevel-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "firstLevel#External",
                    Source = "Bonnea"
                }
            },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "firstLevel-Bitch",
            IsSubOrganization = false
        };

        internal static FirstLevelProjectionOrganization SecondLevelOrganization = new FirstLevelProjectionOrganization
        {
            Id = "secondLevel",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName = "secondLevel-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "secondLevel#External",
                    Source = "Bonnea"
                }
            },
            Source = "who knows",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "secondLevel-Bitch",
            IsSubOrganization = false
        };

        internal static TagsAdded TagAddToFirstOrganization = new TagsAdded
        {
            Id = RootOrganization.Id,
            ObjectType = ObjectTypeResolved.Organization,
            Tags = TagAssignmentOrganization,
            MetaData = MetaDataOrganizationAssignments
                .CloneEventDate()
        };

        internal static TagsAdded TagAddToSecondOrganization = new TagsAdded
        {
            Id = RootOrganization.Id,
            ObjectType = ObjectTypeResolved.Organization,
            Tags = TagAssignmentOrganization,
            MetaData = MetaDataOrganizationAssignments
                .CloneEventDate()
        };

        internal static WasAssignedToOrganization FirstOrganizationAssignToRoot = new WasAssignedToOrganization
        {
            Conditions =
                new[] { new RangeConditionResolved() },
            ProfileId = FirstLevelGroup.Id,
            Target = new OrganizationResolved
            {
                Id = RootOrganization.Id,
                UpdatedAt = RootOrganization.UpdatedAt,
                CreatedAt = RootOrganization.CreatedAt,
                Weight = RootOrganization.Weight,
                DisplayName = RootOrganization.DisplayName,
                ExternalIds =
                    new List<ExternalIdentifierResolved>
                    {
                        new ExternalIdentifierResolved
                        {
                            Id = "root-External",
                            Source = "Bonnea"
                        }
                    },
                Source = RootOrganization.Source,
                IsMarkedForDeletion =
                    RootOrganization.IsMarkedForDeletion,
                IsSystem = RootOrganization.IsSystem,
                Name = RootOrganization.Name,
                IsSubOrganization = false
            },
            MetaData = MetaDataOrganizationAssignments.CloneEventDate()
        };

        internal static WasAssignedToOrganization FirstOrganizationAssignToRootSecondStream =
            new WasAssignedToOrganization
            {
                Conditions =
                    new[] { new RangeConditionResolved() },
                ProfileId = FirstLevelGroup.Id,
                Target = new OrganizationResolved
                {
                    Id = RootOrganization.Id,
                    UpdatedAt = RootOrganization.UpdatedAt,
                    CreatedAt = RootOrganization.CreatedAt,
                    Weight = RootOrganization.Weight,
                    DisplayName = RootOrganization.DisplayName,
                    ExternalIds =
                        new List<ExternalIdentifierResolved>
                        {
                            new ExternalIdentifierResolved
                            {
                                Id = "root-External",
                                Source = "Bonnea"
                            }
                        },
                    Source = RootOrganization.Source,
                    IsMarkedForDeletion =
                        RootOrganization.IsMarkedForDeletion,
                    IsSystem = RootOrganization.IsSystem,
                    Name = RootOrganization.Name,
                    IsSubOrganization = false
                },
                MetaData = MetaDataOrganizationAssignments.CloneEventDate()
            };

        internal static MemberAdded MemberAddedToRootOrganization = new MemberAdded
        {
            Member = new MemberResolved
            {
                Id = FirstLevelOrganization.Id,
                DisplayName = FirstLevelOrganization
                    .DisplayName,
                ExternalIds = new List<
                    ExternalIdentifierResolved>
                {
                    new ExternalIdentifierResolved
                    {
                        Id = "firstLevel#External",
                        Source = "Bonnea"
                    }
                },
                Kind = ProfileKindResolved
                    .Organization,
                Conditions =
                    new List<RangeConditionResolved>
                    {
                        new RangeConditionResolved()
                    },
                Name = FirstLevelOrganization.Name
            },
            MetaData =
                MetaDataOrganizationAssignments
                    .CloneEventDate(),
            ParentId = RootGroup.Id,
            ParentType = ContainerType.Organization
        };

        internal static IEnumerable<EventTuple> ResolvedEventsOrganizationsAssignments = new[]
        {
            new EventTuple
            {
                TargetStream = "Mock-Organization-root",
                Event = MemberAddedToRootOrganization.SetRelatedEntityId("Mock-Organization-root")
            },
            new EventTuple
            {
                TargetStream = "Mock-Organization-firstLevel",
                Event = FirstOrganizationAssignToRoot.SetRelatedEntityId("Mock-Organization-firstLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Organization-firstLevel",
                Event = TagAddToFirstOrganization.SetRelatedEntityId("Mock-Organization-firstLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Organization-secondLevel",
                Event = TagAddToSecondOrganization.SetRelatedEntityId("Mock-Organization-secondLevel")
            },
            new EventTuple
            {
                TargetStream = "Mock-Organization-secondLevel",
                Event = FirstOrganizationAssignToRootSecondStream.SetRelatedEntityId("Mock-Organization-secondLevel")
            }
        };

        #endregion

        #region ObjectAssignmentEvents_Function_To_Group_Assignments

        internal static ObjectAssignmentEvent AddRootToFunctionToGroupAssignment = new ObjectAssignmentEvent
        {
            CorrelationId =
                "4251EA14-EF23-49E2-9234-AF4EAF97C26E",
            EventId =
                "CA4A9F76-2DC5-4E82-B162-474AFDF115DC",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            RequestSagaId =
                "2DA17804-EEDD-460C-9206-51B868F564A0",
            Timestamp = DateTime.UtcNow,
            VersionInformation = 2,
            Payload = new AssignmentPayload
            {
                Added = new[]
                {
                    new ConditionObjectIdent
                    {
                        Conditions = new[] { new RangeCondition() },
                        Id = "FirstGroup",
                        Type = ObjectType.Group
                    }
                },
                Removed = Array.Empty<ConditionObjectIdent>(),
                IsSynchronized = false,
                Resource = new ObjectIdent("RootFunction", ObjectType.Function),
                Type = AssignmentType.ChildrenToParent
            },

            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId = "4251EA14-EF23-49E2-9234-AF4EAF97C26E",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved.User
                },
                ProcessId = "2DA17804-EEDD-460C-9206-51B868F564A0",
                VersionInformation = 2
            }
        };

        internal static EventMetaData MetaDataFunctionToGroupAssignment = new EventMetaData
        {
            CorrelationId =
                AddRootToFunctionToGroupAssignment
                    .CorrelationId,
            ProcessId =
                AddRootToFunctionToGroupAssignment
                    .RequestSagaId,
            Timestamp =
                AddRootToFunctionToGroupAssignment
                    .Timestamp,
            HasToBeInverted = false,
            Initiator = new EventInitiatorResolved
            {
                Id = "Api",
                Type = InitiatorTypeResolved.User
            },
            VersionInformation = 1
        };

        internal static FirstLevelProjectionGroup FirstGroup = new FirstLevelProjectionGroup
        {
            Id = "FirstGroup",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 21.0,
            DisplayName = "firstGroup-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "firstGroup#External",
                    Source = "Bonnea"
                }
            },
            Source = "NotUsedField - FirstGroup",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "firstGroup-Name"
        };

        internal static FirstLevelProjectionGroup SecondGroup = new FirstLevelProjectionGroup
        {
            Id = "SecondGroup",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 712.0,
            DisplayName = "SecondGroup-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "SecondGroup#External",
                    Source = "Bonnea"
                }
            },
            Source = "NotUsedField - SecondGroup",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "SecondGroup-Name"
        };

        internal static FirstLevelProjectionUser FirstUser = new FirstLevelProjectionUser
        {
            Id = "FirstUser",
            Name = "Michael, Hohns",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "Hohns, Michael",
            Email = "michael.hohns@KLAUKE.de",
            FirstName = "Michael",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#FirstUser",
                    Source = "Bonnea"
                }
            },
            LastName = "Hohns",
            UserName = "MichaelSpontaneous"
        };

        private static readonly FirstLevelProjectionRole _rootFunctionRole = new FirstLevelProjectionRole
        {
            Name = "NeedRoleForRootFunction",
            Id = "FunctionRole",
            Description =
                "This role is only user in Test",
            Source = "NotUsedField - FunctionRole",
            IsSystem = false,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DeniedPermissions =
                new List<string>
                {
                    "DELETE",
                    "WRITE"
                },
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#DE960881-3393-41B3-866E-60FAEE84FBA6",
                        Source = "Bonnea"
                    }
                },
            Permissions =
                new List<string>
                {
                    "READ",
                    "LOOKI LOOKI"
                },
            SynchronizedAt = null
        };

        private static readonly FirstLevelProjectionOrganization _rootFunctionOrganization =
            new FirstLevelProjectionOrganization
            {
                Id = "RootFunctionOrganization",
                UpdatedAt = _updatedAt,
                CreatedAt = _createdAt,
                Weight = 7.0,
                DisplayName = "RootFunctionOrganization-Display",
                ExternalIds =
                    new List<ExternalIdentifierApi>
                    {
                        new ExternalIdentifierApi
                        {
                            Id = "RootFunctionOrganization#External",
                            Source = "Bonnea"
                        }
                    },
                Source = "NotUsedField-RootFunctionOrganization",
                IsMarkedForDeletion = false,
                IsSystem = false,
                Name = "RootFunctionOrganization-Bitch",
                IsSubOrganization = false
            };

        internal static FirstLevelProjectionFunction RootFunction = new FirstLevelProjectionFunction
        {
            Id = "RootFunction",
            Role = _rootFunctionRole,
            Organization = _rootFunctionOrganization,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            ExternalIds = new List<ExternalIdentifierApi>(),
            Source = "NotUsedField-RootFunction",
            SynchronizedAt = null
        };

        private static readonly Function _functionForWasAssignedToEvent = new Function
        {
            Id = RootFunction.Id,
            RoleId = _rootFunctionRole.Id,
            OrganizationId =
                _rootFunctionOrganization.Id,
            UpdatedAt = RootFunction
                .UpdatedAt,
            CreatedAt = RootFunction
                .CreatedAt,
            ExternalIds =
                new List<
                    ExternalIdentifierResolved>(),
            Source = RootFunction.Source,
            Role = new Role
            {
                Name =
                    "NeedRoleForRootFunction",
                Id = "FunctionRole",
                Description =
                    "This role is only user in Test",
                Source =
                    "NotUsedField - FunctionRole",
                IsSystem = false,
                CreatedAt =
                    _createdAt,
                UpdatedAt =
                    _updatedAt,
                DeniedPermissions =
                    new List<string>
                    {
                        "DELETE",
                        "WRITE"
                    },
                ExternalIds =
                    new List<
                        ExternalIdentifierResolved>
                    {
                        new
                            ExternalIdentifierResolved
                            {
                                Id =
                                    "external#DE960881-3393-41B3-866E-60FAEE84FBA6",
                                Source =
                                    "Bonnea"
                            }
                    },
                Permissions =
                    new List<string>
                    {
                        "READ",
                        "LOOKI LOOKI"
                    },
                SynchronizedAt = null
            },
            Organization =
                new OrganizationResolved
                {
                    Id =
                        "RootFunctionOrganization",
                    UpdatedAt =
                        _updatedAt,
                    CreatedAt =
                        _createdAt,
                    Weight = 7.0,
                    DisplayName =
                        "RootFunctionOrganization-Display",
                    ExternalIds =
                        new List<
                            ExternalIdentifierResolved>
                        {
                            new
                                ExternalIdentifierResolved
                                {
                                    Id =
                                        "RootFunctionOrganization#External",
                                    Source =
                                        "Bonnea"
                                }
                        },
                    Source =
                        "NotUsedField-RootFunctionOrganization",
                    IsMarkedForDeletion =
                        false,
                    IsSystem = false,
                    Name =
                        "RootFunctionOrganization-Bitch",
                    IsSubOrganization =
                        false
                }
        };

        // produced events
        internal static WasAssignedToFunction FirstGroupAssignToRootFunction = new WasAssignedToFunction
        {
            Conditions =
                new[] { new RangeConditionResolved() },
            ProfileId = FirstGroup.Id,
            Target =
                _functionForWasAssignedToEvent,
            MetaData =
                MetaDataFunctionToGroupAssignment
                    .CloneEventDate()
        };

        internal static WasAssignedToFunction SecondGroupAssignToRootFunction = new WasAssignedToFunction
        {
            Conditions =
                new[] { new RangeConditionResolved() },
            ProfileId = FirstGroup.Id,
            Target = _functionForWasAssignedToEvent,
            MetaData =
                MetaDataFunctionToGroupAssignment.CloneEventDate()
        };

        internal static WasAssignedToFunction FirstUserAssignToRootFunction = new WasAssignedToFunction
        {
            Conditions =
                new[] { new RangeConditionResolved() },
            ProfileId = FirstGroup.Id,
            Target =
                _functionForWasAssignedToEvent,
            MetaData =
                MetaDataFunctionToGroupAssignment
                    .CloneEventDate()
        };

        internal static MemberAdded MemberAddedToRootFunction = new MemberAdded
        {
            Member = new MemberResolved
            {
                Id = FirstGroup.Id,
                DisplayName = FirstGroup
                    .DisplayName,
                ExternalIds = new List<
                    ExternalIdentifierResolved>
                {
                    new ExternalIdentifierResolved
                    {
                        Id = "firstGroup#External",
                        Source = "Bonnea"
                    }
                },
                Kind = ProfileKindResolved
                    .Group,
                Conditions =
                    new List<RangeConditionResolved>
                    {
                        new RangeConditionResolved()
                    },
                Name = FirstGroup.Name
            },
            MetaData =
                MetaDataFunctionToGroupAssignment
                    .CloneEventDate(),
            ParentId = RootFunction.Id,
            ParentType = ContainerType.Function
        };

        internal static IEnumerable<EventTuple> ResolvedEventsFunctionToGroupAssignments = new[]
        {
            new EventTuple
            {
                TargetStream = $"Mock-Function-{RootFunction.Id}",
                Event = MemberAddedToRootFunction.SetRelatedEntityId($"Mock-Function-{RootFunction.Id}")
            },
            new EventTuple
            {
                TargetStream = $"Mock-Group-{FirstGroup.Id}",
                Event = FirstGroupAssignToRootFunction.SetRelatedEntityId($"Mock-Group-{FirstGroup.Id}")
            },
            new EventTuple
            {
                TargetStream = $"Mock-Group-{SecondGroup.Id}",
                Event = SecondGroupAssignToRootFunction.SetRelatedEntityId($"Mock-Group-{SecondGroup.Id}")
            },
            new EventTuple
            {
                TargetStream = $"Mock-User-{FirstUser.Id}",
                Event = FirstUserAssignToRootFunction.SetRelatedEntityId($"Mock-User-{FirstUser.Id}")
            }
        };

        #endregion

        #region TagDeletedEvent

        internal static TagDeletedEvent TagDeletedEvent = new TagDeletedEvent
        {
            CorrelationId = "F3936391-1709-4570-95BB-54FB858943AC",
            EventId = "7EFD2FD9-9671-4474-9718-95D7AFB07DCC",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            OldTag = new TagApi
            {
                Id = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
                Name = string.Empty
            },
            Payload = new IdentifierPayload
            {
                Id = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
                IsSynchronized = true
            },
            RequestSagaId = "60342F67-14D0-40FA-A546-77633525777D",
            Timestamp = _createdAt,
            VersionInformation = 2,
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved.User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 2
            }
        };

        internal static FirstLevelProjectionTag FirstLevelTag = new FirstLevelProjectionTag
        {
            Id = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            Type = Maverick.UserProfileService.Models.EnumModels.TagType.Custom,
            Name = string.Empty
        };

        internal static List<ObjectIdent> TagToObjectIdents = new List<ObjectIdent>
        {
            new ObjectIdent(
                "60342F67-14D0-40FA-A546-77633525777D",
                ObjectType.Organization),
            new ObjectIdent(
                "12407A90-DF54-4386-A6C8-026ECBAE1502",
                ObjectType.Group),
            new ObjectIdent(
                "6A905A3B-0F88-4ABE-A2AC-ECD028708017",
                ObjectType.Function),
            new ObjectIdent(
                "A0DFD0BA-C89C-44D9-9953-B6E31AAD59B2",
                ObjectType.User),
            new ObjectIdent(
                "D6F3EB69-3424-4B85-94A8-03FB4E0CC87D",
                ObjectType.Role)
        };

        internal static TagDeleted TagResolvedOwn = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved.User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-Tag-5757A99F-AAEC-42D1-8B82-78A45E117D9D"
            }
        };

        internal static TagDeleted TagResolvedOrganization = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved
                        .User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-Organization-60342F67-14D0-40FA-A546-77633525777D"
            }
        };

        internal static TagDeleted TagResolvedGroup = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved
                        .User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-Group-12407A90-DF54-4386-A6C8-026ECBAE1502"
            }
        };

        internal static TagDeleted TagResolvedFunction = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved
                        .User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-Function-6A905A3B-0F88-4ABE-A2AC-ECD028708017"
            }
        };

        internal static TagDeleted TagResolvedUser = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved
                        .User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-User-A0DFD0BA-C89C-44D9-9953-B6E31AAD59B2"
            }
        };

        internal static TagDeleted TagResolvedRole = new TagDeleted
        {
            TagId = "5757A99F-AAEC-42D1-8B82-78A45E117D9D",
            MetaData = new EventMetaData
            {
                Batch = null,
                CorrelationId =
                    "F3936391-1709-4570-95BB-54FB858943AC",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved
                        .User
                },
                ProcessId =
                    "60342F67-14D0-40FA-A546-77633525777D",
                VersionInformation = 1,
                RelatedEntityId =
                    "Mock-Role-D6F3EB69-3424-4B85-94A8-03FB4E0CC87D"
            }
        };

        internal static EntityDeleted TagEntityDeleted = new EntityDeleted
        {
            Id = TagResolvedOwn.TagId,
            MetaData = TagResolvedOwn.MetaData
        };

        internal static List<EventTuple> ResolvedTagDeletedTuple = new List<EventTuple>
        {
            new EventTuple(
                "Mock-Organization-60342F67-14D0-40FA-A546-77633525777D",
                TagResolvedOrganization),
            new EventTuple(
                "Mock-Group-12407A90-DF54-4386-A6C8-026ECBAE1502",
                TagResolvedGroup),
            new EventTuple(
                "Mock-Function-6A905A3B-0F88-4ABE-A2AC-ECD028708017",
                TagResolvedFunction),
            new EventTuple(
                "Mock-User-A0DFD0BA-C89C-44D9-9953-B6E31AAD59B2",
                TagResolvedUser),
            new EventTuple(
                "Mock-Role-D6F3EB69-3424-4B85-94A8-03FB4E0CC87D",
                TagResolvedRole),
            new EventTuple(
                "Mock-Tag-5757A99F-AAEC-42D1-8B82-78A45E117D9D",
                TagEntityDeleted)
        };

        #endregion

        #endregion

        #region ProfileSetClientSettingsHandler

        internal static FirstLevelProjectionUser ClientSettingsUser = new FirstLevelProjectionUser
        {
            Id = "E579788B-E4A6-439D-8EE5-EF519CD671AF",
            Name = "Andreas",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "Hinz, Andreas",
            Email = "andreas.hinz@testGuru.de",
            FirstName = "Andreas",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external-E579788B-E4A6-439D-8EE5-EF519CD671AF",
                    Source = "Bonnea"
                }
            },
            LastName = "Hinz",
            UserName = "TrendyAndy"
        };

        internal static ProfileClientSettingsSetEvent SetClientSettings = new ProfileClientSettingsSetEvent
        {
            CorrelationId =
                "668C0C21-176D-47E3-A922-4E1CC2C5B0AC",
            EventId =
                "785FFEE8-E4C3-4A8C-90B7-9D2ED4CBEE09",
            Initiator = new EventInitiatorApi
            {
                Id =
                    "2B3D6C62-3E7E-4F25-9773-7AEEE60F2AB0",
                Type = InitiatorType
                    .System
            },
            Timestamp = _createdAt,
            RequestSagaId =
                "5740EC0E-E773-400C-8E1A-1A965B0DB4FE",
            Payload = new ClientSettingsSetPayload
            {
                Key = "Outlook",
                Resource = new ProfileIdent(
                    ClientSettingsUser.Id,
                    ProfileKind.User),
                Settings = JObject.Parse("{\"Value\":\"O365Premium\"}"),
                IsSynchronized = true
            },
            VersionInformation = 2,
            MetaData = new EventMetaData
            {
                Initiator =
                    new EventInitiatorResolved
                    {
                        Id =
                            "2B3D6C62-3E7E-4F25-9773-7AEEE60F2AB0",
                        Type =
                            InitiatorTypeResolved
                                .System
                    },
                VersionInformation = 2,
                Batch = null,
                CorrelationId =
                    "668C0C21-176D-47E3-A922-4E1CC2C5B0AC",
                HasToBeInverted = false,
                ProcessId =
                    "5740EC0E-E773-400C-8E1A-1A965B0DB4FE",
                RelatedEntityId =
                    ClientSettingsUser.Id,
                Timestamp = _createdAt
            }
        };

        internal static EventMetaData ClientSettingsMetaData = new EventMetaData
        {
            Initiator =
                new EventInitiatorResolved
                {
                    Id =
                        "2B3D6C62-3E7E-4F25-9773-7AEEE60F2AB0",
                    Type =
                        InitiatorTypeResolved
                            .System
                },
            VersionInformation = 1,
            Batch = null,
            CorrelationId =
                "668C0C21-176D-47E3-A922-4E1CC2C5B0AC",
            HasToBeInverted = false,
            ProcessId =
                "5740EC0E-E773-400C-8E1A-1A965B0DB4FE",
            RelatedEntityId =
                ClientSettingsUser.Id,
            Timestamp = _createdAt
        };

        internal static ProfileClientSettingsSet ClientSettingsSetResolved = new ProfileClientSettingsSet
        {
            MetaData = ClientSettingsMetaData,
            ClientSettings =
                "{\"Value\":\"O365Premium\"}",
            Key = "Outlook",
            ProfileId = ClientSettingsUser.Id,
            EventId =
                "A0AA48F4-D1EF-4F26-B23B-6226D7CEA459"
        };

        internal static ClientSettingsCalculated ClientSettingsCalculatedResolved = new ClientSettingsCalculated
        {
            Key = "Outlook",
            CalculatedSettings =
                "{\"Value\":\"O365Premium\"}",
            MetaData = ClientSettingsMetaData,
            ProfileId = ClientSettingsUser.Id,
            EventId =
                "E52366E6-666B-4CF0-92A8-E9BE2CB989B2",
            IsInherited = false
        };

        internal static ClientSettingsCalculated ClientSettingsInheritedCalculatedResolved = new ClientSettingsCalculated
            {
                Key = "Outlook",
                CalculatedSettings =
                    "{\"Value\":\"O365Premium\"}",
                MetaData = ClientSettingsMetaData,
                ProfileId = ClientSettingsUser.Id,
                EventId =
                    "E52366E6-666B-4CF0-92A8-E9BE2CB989B2",
                IsInherited = true
            };

        internal static ClientSettingsInvalidated ClientSettingsInvalidated = new ClientSettingsInvalidated
        {
            MetaData = ClientSettingsMetaData,
            EventId =
                "4B3538CB-ECBB-4CB8-A5BE-C0FD45FD15B2",
            Keys = new[] { "Outlook" },
            ProfileId = ClientSettingsUser.Id
        };

        internal static List<EventTuple> ResolvedClientSettingsUserEventTuple = new List<EventTuple>
        {
            new EventTuple(
                $"Mock-User-{ClientSettingsUser.Id}",
                ClientSettingsSetResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}")),

            new EventTuple(
                $"Mock-User-{ClientSettingsUser.Id}",
                ClientSettingsInvalidated.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}")),
            new EventTuple(
                $"Mock-User-{ClientSettingsUser.Id}",
                ClientSettingsCalculatedResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}"))
        };

        internal static List<EventTuple> ResolvedClientSettingsInheritedUserEventTuple = new List<EventTuple>
            {
                new EventTuple(
                    $"Mock-User-{ClientSettingsUser.Id}",
                    ClientSettingsSetResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}")),

                new EventTuple(
                    $"Mock-User-{ClientSettingsUser.Id}",
                    ClientSettingsInvalidated.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}")),
                new EventTuple(
                    $"Mock-User-{ClientSettingsUser.Id}",
                    ClientSettingsInheritedCalculatedResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsUser.Id}"))
            };

        internal static List<EventTuple> ResolvedClientSettingsOfRootGroupEventTuple(
            string groupId,
            string childUserId,
            IStreamNameResolver streamNameResolver)
        {
            string groupStreamName =
                streamNameResolver.GetStreamName(new ObjectIdent(groupId, ObjectType.Group));

            string userStreamName =
                streamNameResolver.GetStreamName(new ObjectIdent(childUserId, ObjectType.User));

            return new List<EventTuple>
            {
                new EventTuple(
                    groupStreamName,
                    new ProfileClientSettingsUnset
                        {
                            Key = ClientSettingsKeyToDelete,
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = groupId
                        }
                        .AddDefaultMetadata(
                            groupStreamName,
                            DateTime.UtcNow)),
                new EventTuple(
                    groupStreamName,
                    new ClientSettingsInvalidated
                        {
                            Keys = new[] { ClientSettingsKeyToKeep },
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = groupId
                        }
                        .AddDefaultMetadata(
                            groupStreamName,
                            DateTime.UtcNow)),
                new EventTuple(
                    userStreamName,
                    new ClientSettingsInvalidated
                        {
                            Keys = new[] { ClientSettingsKeyToKeep },
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = childUserId
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow))
            };
        }

        internal static List<EventTuple> ResolvedClientSettingsOfLonelyUserEventTuple(
            string userId,
            IStreamNameResolver streamNameResolver)
        {
            string userStreamName =
                streamNameResolver.GetStreamName(new ObjectIdent(userId, ObjectType.User));

            return new List<EventTuple>
            {
                new EventTuple(
                    userStreamName,
                    new ProfileClientSettingsUnset
                        {
                            Key = ClientSettingsKeyToDelete,
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = userId
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow)),
                new EventTuple(
                    userStreamName,
                    new ClientSettingsInvalidated
                        {
                            Keys = new[] { ClientSettingsKeyToKeep },
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = userId
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow))
            };
        }

        internal static List<EventTuple> ResolvedClientSettingsOfUserInsideGroupWithSameKeyEventTuple(
            string userId,
            IStreamNameResolver streamNameResolver)
        {
            string userStreamName =
                streamNameResolver.GetStreamName(new ObjectIdent(userId, ObjectType.User));

            return new List<EventTuple>
            {
                new EventTuple(
                    userStreamName,
                    new ProfileClientSettingsUnset
                        {
                            Key = ClientSettingDoubleKey,
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = userId
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow)),
                new EventTuple(
                    userStreamName,
                    new ClientSettingsCalculated
                        {
                            Key = ClientSettingDoubleKey,
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = userId,
                            CalculatedSettings = ClientSettingsValueToKeep
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow)),
                new EventTuple(
                    userStreamName,
                    new ClientSettingsInvalidated
                        {
                            Keys = new[] { ClientSettingDoubleKey },
                            EventId = MockedSagaWorkerEventsBuilder.DefaultEventId,
                            ProfileId = userId
                        }
                        .AddDefaultMetadata(
                            userStreamName,
                            DateTime.UtcNow))
            };
        }

        #endregion

        #region ClientSettingsSetBatchHandler

        internal static FirstLevelProjectionUser ClientSettingsBatchUser = new FirstLevelProjectionUser
        {
            Id =
                "559D7193-CC31-454E-BD31-BCA29CC5AD27",
            Name = "Randy",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "Candy, Randy",
            Email = "randy.candy@testGuru.de",
            FirstName = "Randy",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external-559D7193-CC31-454E-BD31-BCA29CC5AD27",
                        Source = "Bonnea"
                    }
                },
            LastName = "Candy",
            UserName = "CandyShop"
        };

        internal static ProfileClientSettingsSetBatchEvent SetClientSettingsBatchEvent =
            new ProfileClientSettingsSetBatchEvent
            {
                CorrelationId =
                    "40607617-BEDB-4957-AD44-EA9FFD2443BC",
                EventId =
                    "0BF9DAE6-EB39-4433-8DBF-89CEB9F9F171",
                Initiator = new EventInitiatorApi
                {
                    Id =
                        "3169EE49-498C-4A47-9590-1E4CF2DED61B",
                    Type = InitiatorType
                        .User
                },
                Timestamp = _createdAt,
                RequestSagaId =
                    "7513D7A9-C05D-4AB3-BD3D-927A35FD79E3",
                Payload = new ClientSettingsSetBatchPayload
                {
                    Key = "OutlookBatch",
                    Resources = new[] { new ProfileIdent(ClientSettingsBatchUser.Id, ProfileKind.User) },
                    Settings = JObject.Parse("{\"Value\":\"O365Premium-Batch\"}"),
                    IsSynchronized = true
                },
                VersionInformation = 2,
                MetaData = new EventMetaData
                {
                    Initiator =
                        new EventInitiatorResolved
                        {
                            Id =
                                "3169EE49-498C-4A47-9590-1E4CF2DED61B",
                            Type =
                                InitiatorTypeResolved
                                    .User
                        },
                    VersionInformation = 2,
                    Batch = null,
                    CorrelationId =
                        "40607617-BEDB-4957-AD44-EA9FFD2443BC",
                    HasToBeInverted = false,
                    ProcessId =
                        "7513D7A9-C05D-4AB3-BD3D-927A35FD79E3",
                    RelatedEntityId =
                        ClientSettingsBatchUser.Id,
                    Timestamp = _createdAt
                }
            };

        internal static EventMetaData ClientSettingsBatchMetaData = new EventMetaData
        {
            Initiator =
                new EventInitiatorResolved
                {
                    Id =
                        "3169EE49-498C-4A47-9590-1E4CF2DED61B",
                    Type =
                        InitiatorTypeResolved
                            .User
                },
            VersionInformation = 1,
            Batch = null,
            CorrelationId =
                "40607617-BEDB-4957-AD44-EA9FFD2443BC",
            HasToBeInverted = false,
            ProcessId =
                "7513D7A9-C05D-4AB3-BD3D-927A35FD79E3",
            RelatedEntityId =
                ClientSettingsBatchUser.Id,
            Timestamp = _createdAt
        };

        internal static ClientSettingsCalculated ClientSettingsBatchResolved = new ClientSettingsCalculated
        {
            Key = "OutlookBatch",
            CalculatedSettings =
                "{\"Value\":\"O365Premium-Batch\"}",
            MetaData =
                ClientSettingsBatchMetaData,
            ProfileId = ClientSettingsBatchUser
                .Id,
            EventId =
                "E52366E6-666B-4CF0-92A8-E9BE2CB989B2",
            IsInherited = false
        };

        internal static ClientSettingsCalculated ClientSettingsInheritedBatchResolved = new ClientSettingsCalculated
        {
            Key = "OutlookBatch",
            CalculatedSettings =
               "{\"Value\":\"O365Premium-Batch\"}",
            MetaData =
               ClientSettingsBatchMetaData,
            ProfileId = ClientSettingsBatchUser
               .Id,
            EventId =
               "E52366E6-666B-4CF0-92A8-E9BE2CB989B2",
            IsInherited = true
        };

        internal static ProfileClientSettingsSet ClientSettingsBatchSetResolved = new ProfileClientSettingsSet
        {
            MetaData = ClientSettingsBatchMetaData,
            ClientSettings =
                "{\"Value\":\"O365Premium-Batch\"}",
            Key = "OutlookBatch",
            ProfileId = ClientSettingsBatchUser.Id,
            EventId =
                "A0AA48F4-D1EF-4F26-B23B-6226D7CEA459"
        };

        internal static ClientSettingsInvalidated ClientSettingsBatchInvalidated = new ClientSettingsInvalidated
        {
            MetaData = ClientSettingsBatchMetaData,
            EventId =
                "AED8BF36-30B5-4C31-9248-1EBBE818962C",
            Keys = new[] { "OutlookBatch" },
            ProfileId = ClientSettingsBatchUser.Id
        };

        internal static List<EventTuple> ResolvedClientSettingsUserBatchEventTuple = new List<EventTuple>
        {
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsBatchSetResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}")),
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsBatchResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}")),
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsBatchInvalidated.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}"))
        };

        internal static List<EventTuple> ResolvedClientSettingsInheritedUserBatchEventTuple = new List<EventTuple>
        {
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsBatchSetResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}")),
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsInheritedBatchResolved.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}")),
            new EventTuple(
                $"Mock-User-{ClientSettingsBatchUser.Id}",
                ClientSettingsBatchInvalidated.SetRelatedEntityId($"Mock-User-{ClientSettingsBatchUser.Id}"))
        };

        #endregion

        #region RoleDeletedHander

        #region OnlyRoleShouldBeDeleted

        internal static FirstLevelProjectionRole RoleToDelete = new FirstLevelProjectionRole
        {
            Id = "7853F228-4EED-43E5-B49D-5D6F44013666"
        };

        internal static RoleDeletedEvent RoleDeletedEvent = new RoleDeletedEvent
        {
            Payload = new IdentifierPayload
            {
                Id = RoleToDelete.Id
            },
            CorrelationId = "4F6BE7A8-62C9-4AD1-AE7F-9BFF93A7C0AE",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            RequestSagaId = "1E449B27-AC23-4BD2-823E-639F1B6FBE9C",
            Timestamp = _createdAt,
            MetaData = new EventMetaData
            {
                CorrelationId =
                    "4F6BE7A8-62C9-4AD1-AE7F-9BFF93A7C0AE",
                Initiator = new EventInitiatorResolved
                {
                    Id = "Api",
                    Type = InitiatorTypeResolved.User
                },
                Timestamp = _createdAt,
                HasToBeInverted = false,
                ProcessId =
                    "1E449B27-AC23-4BD2-823E-639F1B6FBE9C",
                VersionInformation = 1
            }
        };

        private static readonly EntityDeleted _enityDeleteEvent = new EntityDeleted
        {
            Id = RoleToDelete.Id,
            MetaData = RoleDeletedEvent.MetaData
        };

        internal static List<EventTuple> ResolvedOnlyRoleShouldBeDelete = new List<EventTuple>
        {
            new EventTuple
            {
                Event = _enityDeleteEvent
                    .SetRelatedEntityId($"Mock-Role-{RoleToDelete.Id}"),
                TargetStream =
                    $"Mock-Role-{RoleToDelete.Id}"
            }
        };

        #endregion

        #region Complex test run

        internal static FirstLevelProjectionRole RootRole = new FirstLevelProjectionRole
        {
            Name = "RootRole-Name",
            Id = "RootRole",
            Description =
                "This role is only user in Test",
            Source = "NotUsedField - RootRole",
            IsSystem = false,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DeniedPermissions =
                new List<string>
                {
                    "DELETE",
                    "WRITE"
                },
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#RootRole",
                        Source = "Bonnea"
                    }
                },
            Permissions =
                new List<string>
                {
                    "READIND",
                    "WATCHING"
                },
            SynchronizedAt = null
        };

        internal static ObjectIdent FirstRoleFunction = new ObjectIdent
        {
            Id = "FirstRoleFunction",
            Type = ObjectType.Function
        };

        internal static ObjectIdent SecondRoleFunction = new ObjectIdent
        {
            Id = "SecondRoleFunction",
            Type = ObjectType.Function
        };

        internal static RoleDeletedEvent RoleDeletedEventComplexCase = new RoleDeletedEvent
        {
            Payload = new IdentifierPayload
            {
                Id = RootRole.Id
            },
            CorrelationId =
                "2FBA5C99-C31D-475A-9B78-6371490A3D55",
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            RequestSagaId =
                "BA1AA253-AF59-43E1-AD6B-E6E68D5F73E3",
            Timestamp = _createdAt,
            MetaData = new EventMetaData
            {
                CorrelationId =
                    "2FBA5C99-C31D-475A-9B78-6371490A3D55",
                Initiator =
                    new EventInitiatorResolved
                    {
                        Id = "Api",
                        Type = InitiatorTypeResolved
                            .User
                    },
                Timestamp = _createdAt,
                HasToBeInverted = false,
                ProcessId =
                    "BA1AA253-AF59-43E1-AD6B-E6E68D5F73E3",
                VersionInformation = 1
            }
        };

        internal static FirstLevelProjectionGroup RoleRootGroup = new FirstLevelProjectionGroup
        {
            Id = "RoleGroup",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName = "RoleGroup-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "external#RoleGroup",
                    Source = "Bonnea"
                }
            },
            Source = "MotUserField - RoleGroup",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "RoleGroup-Name"
        };

        internal static FirstLevelProjectionGroup SecondRoleFunctionGroup = new FirstLevelProjectionGroup
        {
            Id = "SecondRoleFunctionGroup",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName =
                "SecondRoleFunctionGroup-Display",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#SecondRoleFunctionGroup",
                        Source = "Bonnea"
                    }
                },
            Source =
                "MotUserField - SecondRoleFunctionGroup",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "SecondRoleFunctionGroup-Name"
        };

        internal static FirstLevelProjectionGroup SubGroup = new FirstLevelProjectionGroup
        {
            Id = "SubGroup",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 7.0,
            DisplayName =
                "SubGroup-Display",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#SecondRoleFunctionGroup",
                        Source = "Bonnea"
                    }
                },
            Source =
                "MotUserField - SubGroup",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "SubGroup-Name"
        };

        internal static FirstLevelProjectionUser RoleFirstUser = new FirstLevelProjectionUser
        {
            Id = "RoleFirstUser",
            Name = "RoleFirstUser-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "RoleFirstUser-DisplayName",
            Email = "RoleFirstUser@bechtle.de",
            FirstName = "RoleFirstUser-FirstName",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#RoleFirstUser",
                    Source = "Bonnea"
                }
            },
            LastName = "RoleFirstUser-LastName",
            UserName = "RoleFirstUser-UserName"
        };

        internal static FirstLevelProjectionUser RoleSecondUser = new FirstLevelProjectionUser
        {
            Id = "RoleSecondUser",
            Name = "RoleSecondUser-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "RoleSecondUser-DisplayName",
            Email = "RoleSecondUser@bechtle.de",
            FirstName = "RoleSecondUser-FirstName",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#RoleFirstUser",
                    Source = "Bonnea"
                }
            },
            LastName = "RoleSecondUser-LastName",
            UserName = "RoleSecondUser-UserName"
        };

        internal static FirstLevelProjectionUser FirstUserRoleFunction = new FirstLevelProjectionUser
        {
            Id = "FirstUserRoleFunction",
            Name = "FirstUserRoleFunction-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName =
                "FirstUserRoleFunction-DisplayName",
            Email = "FirstUserRoleFunction@bechtle.de",
            FirstName =
                "FirstUserRoleFunction-FirstName",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#FirstUserRoleFunction",
                        Source = "Bonnea"
                    }
                },
            LastName =
                "FirstUserRoleFunction-LastName",
            UserName = "FirstUserRoleFunction-UserName"
        };

        internal static FirstLevelProjectionUser SecondRoleFunctionUser = new FirstLevelProjectionUser
        {
            Id = "SecondRoleFunctionUser",
            Name = "SecondRoleFunctionUser-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName =
                "SecondRoleFunctionUser-DisplayName",
            Email =
                "SecondRoleFunctionUser@bechtle.de",
            FirstName =
                "SecondRoleFunctionUser-FirstName",
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#SecondRoleFunctionUser",
                        Source = "Bonnea"
                    }
                },
            LastName =
                "SecondRoleFunctionUser-LastName",
            UserName =
                "SecondRoleFunctionUser-UserName"
        };

        internal static FirstLevelProjectionUser SubUserOne = new FirstLevelProjectionUser
        {
            Id = "SubUserOne",
            Name = "SubUserOne-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "SubUserOne-DisplayName",
            Email = "SubUserOne@bechtle.de",
            FirstName = "SubUserOne-FirstName",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#SubUserOne",
                    Source = "Bonnea"
                }
            },
            LastName = "SubUserOne-LastName",
            UserName = "SubUserOne-UserName"
        };

        internal static FirstLevelProjectionUser SubUserTwo = new FirstLevelProjectionUser
        {
            Id = "SubUserTwo",
            Name = "SubUserTwo-Name",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "SubUserTwo-DisplayName",
            Email = "SubUserTwo@bechtle.de",
            FirstName = "SubUserTwo-FirstName",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#SubUserOne",
                    Source = "Bonnea"
                }
            },
            LastName = "SubUserTwo-LastName",
            UserName = "SubUserTwo-UserName"
        };

        // Delete events
        internal static EntityDeleted RootRoleDeletedEventResolved = new EntityDeleted
        {
            Id = RootRole.Id,
            MetaData = RoleDeletedEventComplexCase.MetaData
                .CloneEventDate()
        };

        internal static EntityDeleted FirstRoleFunctionDeletedEventResolved = new EntityDeleted
        {
            Id = FirstRoleFunction.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData
                .CloneEventDate()
        };

        internal static EntityDeleted SecondRoleFunctionDeletedEventResolved = new EntityDeleted
        {
            Id = SecondRoleFunction.Id,
            MetaData =
                RoleDeletedEventComplexCase
                    .MetaData
                    .CloneEventDate()
        };

        // Container deleted events for deleted root role
        internal static ContainerDeleted RoleGroupContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = RootRole.Id,
            ContainerType = ContainerType.Role,
            MemberId = RoleRootGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted RoleFirstUserContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = RootRole.Id,
            ContainerType = ContainerType.Role,
            MemberId = RoleRootGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted RoleSecondUserContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = RootRole.Id,
            ContainerType = ContainerType.Role,
            MemberId = RoleRootGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        // FirstRoleFunction was deleted
        internal static ContainerDeleted FirstUserRoleFunctionContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = FirstRoleFunction.Id,
            ContainerType = ContainerType.Function,
            MemberId = FirstUserRoleFunction.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        // SecondRoleFunction was deleted
        internal static ContainerDeleted SecondRoleFunctionGroupContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = SecondRoleFunction.Id,
            ContainerType = ContainerType.Function,
            MemberId = SecondRoleFunctionGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted SecondRoleFunctionUserContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = SecondRoleFunction.Id,
            ContainerType = ContainerType.Function,
            MemberId = SecondRoleFunctionGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted SubGroupContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = SecondRoleFunction.Id,
            ContainerType = ContainerType.Function,
            MemberId = SecondRoleFunctionGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted SubUserOneContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = SecondRoleFunction.Id,
            ContainerType =
                ContainerType.Function,
            MemberId = SecondRoleFunctionGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static ContainerDeleted SubUserTwoContainerDeletedResolved = new ContainerDeleted
        {
            ContainerId = SecondRoleFunction.Id,
            ContainerType =
                ContainerType.Function,
            MemberId = SecondRoleFunctionGroup.Id,
            MetaData = RoleDeletedEventComplexCase
                .MetaData.CloneEventDate()
        };

        internal static List<EventTuple> ResolvedRoleDeletedComplexCaseEventTuple = new List<EventTuple>
        {
            // deleted events
            new EventTuple
            {
                Event = RootRoleDeletedEventResolved.SetRelatedEntityId($"Mock-Role-{RootRole.Id}"),
                TargetStream = $"Mock-Role-{RootRole.Id}"
            },
            new EventTuple
            {
                Event = FirstRoleFunctionDeletedEventResolved.SetRelatedEntityId(
                    $"Mock-Function-{FirstRoleFunction.Id}"),
                TargetStream = $"Mock-Function-{FirstRoleFunction.Id}"
            },
            new EventTuple
            {
                Event = SecondRoleFunctionDeletedEventResolved.SetRelatedEntityId(
                    $"Mock-Function-{SecondRoleFunction.Id}"),
                TargetStream = $"Mock-Function-{SecondRoleFunction.Id}"
            },
            // container deleted event for rootRoleDeleted
            new EventTuple
            {
                Event = RoleGroupContainerDeletedResolved.SetRelatedEntityId($"Mock-Group-{RoleRootGroup.Id}"),
                TargetStream = $"Mock-Group-{RoleRootGroup.Id}"
            },
            new EventTuple
            {
                Event = RoleFirstUserContainerDeletedResolved.SetRelatedEntityId($"Mock-User-{RoleFirstUser.Id}"),
                TargetStream = $"Mock-User-{RoleFirstUser.Id}"
            },
            new EventTuple
            {
                Event = RoleSecondUserContainerDeletedResolved.SetRelatedEntityId($"Mock-User-{RoleSecondUser.Id}"),
                TargetStream = $"Mock-User-{RoleSecondUser.Id}"
            },
            // container deleted first function
            new EventTuple
            {
                Event = FirstUserRoleFunctionContainerDeletedResolved.SetRelatedEntityId(
                    $"Mock-User-{FirstUserRoleFunction.Id}"),
                TargetStream = $"Mock-User-{FirstUserRoleFunction.Id}"
            },
            // container deleted second function
            new EventTuple
            {
                Event = SecondRoleFunctionGroupContainerDeletedResolved.SetRelatedEntityId(
                    $"Mock-Group-{SecondRoleFunctionGroup.Id}"),
                TargetStream = $"Mock-Group-{SecondRoleFunctionGroup.Id}"
            },
            new EventTuple
            {
                Event = SecondRoleFunctionUserContainerDeletedResolved.SetRelatedEntityId(
                    $"Mock-User-{SecondRoleFunctionUser.Id}"),
                TargetStream = $"Mock-User-{SecondRoleFunctionUser.Id}"
            },
            new EventTuple
            {
                Event = SubGroupContainerDeletedResolved.SetRelatedEntityId($"Mock-Group-{SubGroup.Id}"),
                TargetStream = $"Mock-Group-{SubGroup.Id}"
            },
            new EventTuple
            {
                Event = SubUserOneContainerDeletedResolved.SetRelatedEntityId($"Mock-User-{SubUserOne.Id}"),
                TargetStream = $"Mock-User-{SubUserOne.Id}"
            },
            new EventTuple
            {
                Event = SubUserTwoContainerDeletedResolved.SetRelatedEntityId($"Mock-User-{SubUserTwo.Id}"),
                TargetStream = $"Mock-User-{SubUserTwo.Id}"
            }
        };

        #endregion

        #endregion

        #region ProfileDeletedEventHandler

        #region deleted group

        internal static FirstLevelProjectionGroup GroupToDeleteInProfileDeleted = new FirstLevelProjectionGroup
        {
            Id = "GroupToDelteInProfileDeletedEvent",
            UpdatedAt = _updatedAt,
            CreatedAt = _createdAt,
            Weight = 2.0,
            DisplayName = "GroupToDelteInProfileDeletedEvent-Display",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id = "GroupToDelteInProfileDeletedEvent#External",
                    Source = "Bonnea"
                }
            },
            Source = "GroupToDelteInProfileDeletedEvent-Source",
            IsMarkedForDeletion = false,
            IsSystem = false,
            Name = "GroupToDelteInProfileDeletedEvent-Name"
        };

        internal static EventMetaData MetaDataProfileDeletedEvent = new EventMetaData
        {
            Initiator = new EventInitiatorResolved
            {
                Id = "Api",
                Type = InitiatorTypeResolved.User
            },
            Timestamp = _createdAt,
            CorrelationId =
                "19600FA6-2159-42C9-98F3-515C0048D8E1",
            ProcessId =
                "5E8C883E-E853-4B90-BCEF-51DFEACE04DE",
            VersionInformation = 1
        };

        internal static ProfileDeletedEvent ProfileDeletedEventTest = new ProfileDeletedEvent
        {
            Payload = new ProfileIdentifierPayload
            {
                Id = GroupToDeleteInProfileDeleted.Id,
                ExternalIds =
                    GroupToDeleteInProfileDeleted
                        .ExternalIds,
                ProfileKind = ProfileKind.Group
            },
            Initiator = new EventInitiatorApi
            {
                Id = "Api",
                Type = InitiatorType.User
            },
            CorrelationId =
                "19600FA6-2159-42C9-98F3-515C0048D8E1",
            RequestSagaId =
                "5E8C883E-E853-4B90-BCEF-51DFEACE04DE",
            Timestamp = _createdAt,
            EventId =
                "272F053F-7A4F-4F1C-82D7-E06CD955FA50",
            VersionInformation = 2,
            MetaData = MetaDataProfileDeletedEvent
                .CloneEventDate()
        };

        internal static FirstLevelProjectionUser UserChildToDeleteGroupProfile = new FirstLevelProjectionUser
        {
            Id = "UserChildDeleteProfileProperty",
            Name = "Karl-Heinz, Schneider",
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DisplayName = "Karl-Heinz, Schneider",
            Email = "Karl-Heinz.Schneider@KLAUKE.de",
            FirstName = "Karl-Heinz",
            ExternalIds = new List<ExternalIdentifierApi>
            {
                new ExternalIdentifierApi
                {
                    Id =
                        "external#UserChildDeleteProfileProperty",
                    Source = "LDAP"
                }
            },
            LastName = "Schneider",
            UserName = "KHS"
        };

        private static readonly FirstLevelProjectionRole _parentRoleForFunction = new FirstLevelProjectionRole
        {
            Name = "NeedRoleForRootFunction",
            Id = "FunctionRole",
            Description =
                "This role is only user in Test",
            Source = "NotUsedField - FunctionRole",
            IsSystem = false,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            DeniedPermissions =
                new List<string>
                {
                    "DELETE",
                    "WRITE"
                },
            ExternalIds =
                new List<ExternalIdentifierApi>
                {
                    new ExternalIdentifierApi
                    {
                        Id =
                            "external#DE960881-3393-41B3-866E-60FAEE84FBA6",
                        Source = "Bonnea"
                    }
                },
            Permissions =
                new List<string>
                {
                    "READ",
                    "LOOKI LOOKI"
                },
            SynchronizedAt = null
        };

        private static readonly FirstLevelProjectionOrganization _parentOrganizationForFunction =
            new FirstLevelProjectionOrganization
            {
                Id = "ParentOrganizationForFunction",
                UpdatedAt = _updatedAt,
                CreatedAt = _createdAt,
                Weight = 20,
                DisplayName = "ParentOrganizationForFunction-Display",
                ExternalIds =
                    new List<ExternalIdentifierApi>
                    {
                        new ExternalIdentifierApi
                        {
                            Id = "ParentOrganizationForFunction#External",
                            Source = "Bonnea"
                        }
                    },
                Source = "NotUsedField-ParentOrganizationForFunction",
                IsMarkedForDeletion = false,
                IsSystem = false,
                Name = "ParentOrganizationForFunction-Name",
                IsSubOrganization = false
            };

        internal static FirstLevelProjectionFunction FunctionParentFromDeletedProfileGroup =
            new FirstLevelProjectionFunction
            {
                Id = "FunctionParentFromDeletedProfileGroup",
                Role = _parentRoleForFunction,
                Organization = _parentOrganizationForFunction,
                CreatedAt = _createdAt,
                UpdatedAt = _updatedAt,
                ExternalIds = new List<ExternalIdentifierApi>(),
                Source = "NotUsedField-FunctionParentFromDeletedProfileGroup",
                SynchronizedAt = null
            };

        internal static EntityDeleted ProfileGroupDeleteResolved = new EntityDeleted
        {
            Id = GroupToDeleteInProfileDeleted.Id,
            MetaData = MetaDataProfileDeletedEvent
                .CloneEventDate()
        };

        internal static ContainerDeleted ContainerGroupDeleted = new ContainerDeleted
        {
            ContainerId = GroupToDeleteInProfileDeleted.Id,
            ContainerType = ContainerType.Group,
            MemberId = UserChildToDeleteGroupProfile.Id,
            MetaData = MetaDataProfileDeletedEvent
                .CloneEventDate()
        };

        internal static MemberDeleted MemberGroupDeletedResolved = new MemberDeleted
        {
            ContainerId =
                FunctionParentFromDeletedProfileGroup.Id,
            MemberId = GroupToDeleteInProfileDeleted.Id,
            MetaData = MetaDataProfileDeletedEvent
                .CloneEventDate()
        };

        internal static List<EventTuple> ProfileGroupDeletedEventTuple = new List<EventTuple>
        {
            new EventTuple
            {
                Event = ProfileGroupDeleteResolved
                    .SetRelatedEntityId($"Mock-Group-{GroupToDeleteInProfileDeleted.Id}"),
                TargetStream =
                    $"Mock-Group-{GroupToDeleteInProfileDeleted.Id}"
            },
            new EventTuple
            {
                Event = ContainerGroupDeleted
                    .SetRelatedEntityId($"Mock-User-{UserChildToDeleteGroupProfile.Id}"),
                TargetStream =
                    $"Mock-User-{UserChildToDeleteGroupProfile.Id}"
            },
            new EventTuple
            {
                Event = MemberGroupDeletedResolved
                    .SetRelatedEntityId($"Mock-Function-{FunctionParentFromDeletedProfileGroup.Id}"),
                TargetStream =
                    $"Mock-Function-{FunctionParentFromDeletedProfileGroup.Id}"
            }
        };

        #endregion

        #endregion
    }
}
