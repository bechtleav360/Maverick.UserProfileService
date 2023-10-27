using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;
using UserProfileService.Common.V2.Models;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using AggregateTag = Maverick.UserProfileService.AggregateEvents.Common.Models.Tag;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using SyncProfileKind = UserProfileService.Sync.Abstraction.Models.ProfileKind;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class DefaultModelConstellation
{
    internal const string ProjectionStateCollection = "projectionState";

    private const string ProfileAssignmentsCollection = "assignments";

    private static IList<Action<IModelBuilder>> ModelCustomizer =
        new List<Action<IModelBuilder>>();

    internal ModelBuilderOptions ModelsInfo { get; }

    private DefaultModelConstellation(IModelBuilder modelBuilder, string prefix, string queryPrefix = null)
    {
        ModelsInfo = modelBuilder.BuildOptions(prefix, queryPrefix ?? prefix);

    }

    /// <summary>
    ///     Register further database collection.
    /// </summary>
    /// <param name="addModelFunction"> action used to register further database collections.</param>
    public static void AddModelToUserProfileStorage(
        Action<IModelBuilder> addModelFunction)
    {
        if (addModelFunction == null)
        {
            return;
        }

        ModelCustomizer.Add(addModelFunction);
    }

    private static DefaultModelConstellation NewUserProfileStorage(
        string prefix,
        string queryCollectionPrefix = null)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;


        foreach (Action<IModelBuilder> act in ModelCustomizer)
        {
            act.Invoke(modelBuilder);
        }

        modelBuilder.Entity<IProfileEntityModel>()
            .HasKeyIdentifier(g => g.Id)
            .HasAlias<IProfile>()
            .HasAlias<ISecondLevelProjectionProfile>()
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.User),
                typeof(User),
                typeof(UserEntityModel),
                typeof(UserBasic),
                typeof(UserView))
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Group),
                typeof(Group),
                typeof(GroupEntityModel),
                typeof(GroupBasic),
                typeof(GroupView))
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Organization),
                typeof(OrganizationEntityModel),
                typeof(Organization),
                typeof(OrganizationBasic),
                typeof(OrganizationView))
            .Collection("profilesQuery")
            .QueryCollection("profilesQuery");

        modelBuilder.Entity<IContainerProfileEntityModel>()
            .HasKeyIdentifier(g => g.Id)
            .HasAlias<IContainerProfile>()
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Group),
                typeof(Group),
                typeof(GroupEntityModel),
                typeof(GroupBasic),
                typeof(GroupView))
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Organization),
                typeof(OrganizationEntityModel),
                typeof(Organization),
                typeof(OrganizationBasic),
                typeof(OrganizationView))
            .Collection("profilesQuery")
            .QueryCollection("profilesQuery");

        modelBuilder.Entity<IAssignmentObjectEntity>()
            .HasKeyIdentifier(o => o.Id)
            .HasAlias<ILinkedObject>()
            .HasAlias(
                o => o.Type,
                nameof(RoleType.Role),
                typeof(RoleObjectEntityModel),
                typeof(RoleView),
                typeof(RoleBasic))
            .HasAlias(
                o => o.Type,
                nameof(RoleType.Function),
                typeof(FunctionObjectEntityModel),
                typeof(FunctionView),
                typeof(FunctionBasic))
            .Collection("rolesFunctionsQuery")
            .QueryCollection("rolesFunctionsQuery");

        modelBuilder.Entity<Tag>()
            .HasKeyIdentifier(t => t.Id)
            .HasAlias<AggregateTag>()
            .Collection("tagsQuery")
            .QueryCollection("tagsQuery");

        modelBuilder.Entity<ClientSettingsEntityModel>()
            .HasAlias<ClientSettingsBasic>()
            .Collection("clientSettingsQuery")
            .QueryCollection("clientSettingsQuery");

        modelBuilder.Entity<SecondLevelProjectionAssignmentsUser>()
            .NoCollection()
            .QueryCollection("assignments");

        modelBuilder.Entity<ActivityLogEntry>()
            .HasKeyIdentifier(t => t.Id)
            .Collection("activityLogs")
            .QueryCollection("activityLogs");

        modelBuilder.Entity<SecondLevelProjectionProfileVertexData>()
            .AddChildRelation<SecondLevelProjectionProfileVertexData>(b => b.WithCollectionName("pathTreeEdges"))
            .Collection("pathTree");

        modelBuilder.Entity<SecondLevelProjectionAssignmentsUser>()
            .NoCollection()
            .QueryCollection(ProfileAssignmentsCollection);

        modelBuilder.Entity<ProjectionState>().Collection(ProjectionStateCollection);

        return new DefaultModelConstellation(modelBuilder, prefix, queryCollectionPrefix);
    }

    private static void SetupFirstLevelAssignmentsEdge<TContainer>(
        IRelationBuilder<IFirstLevelProjectionProfile, TContainer> builder)
        where TContainer : IFirstLevelProjectionContainer
    {
        builder.WithFromProperty(u => u.Name)
            .WithToProperty(g => g.ContainerType)
            .WithCollectionName("assignments");
    }

    private static void SetupFirstLevelTagLinkEdge<TObject>(
        IRelationBuilder<TObject, FirstLevelProjectionTag> builder)
        where TObject : IFirstLevelProjectionSimplifier
    {
        builder.WithToProperty(u => u.Name)
            .WithCollectionName("tagLinks");
    }

    private static void SetupFirstLevelClientSettingsEdge(
        IRelationBuilder<IFirstLevelProjectionProfile, FirstLevelProjectionsClientSetting> builder)
    {
        builder.WithCollectionName("settingsLinks");
    }

    private static void SetupFirstLevelFunctionLinkEdge<TContainer>(
        IRelationBuilder<FirstLevelProjectionFunction, TContainer> builder)
        where TContainer : IFirstLevelProjectionSimplifier
    {
        // names are used because we know that this method only should be called with functions and organizations.
        builder.WithToProperty(nameof(IFirstLevelProjectionContainer.ContainerType))
            .WithToProperty(nameof(FirstLevelProjectionRole.Name))
            .WithCollectionName("functionLinks");
    }

    private static DefaultModelConstellation NewFirstLevelProjectionStorage(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<IFirstLevelProjectionProfile>()
            .HasKeyIdentifier(g => g.Id)
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.User),
                typeof(FirstLevelProjectionUser))
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Group),
                typeof(FirstLevelProjectionGroup))
            .HasAlias(
                p => p.Kind,
                nameof(ProfileKind.Organization),
                typeof(FirstLevelProjectionOrganization))
            .AddChildRelation<FirstLevelProjectionGroup>(SetupFirstLevelAssignmentsEdge)
            .AddChildRelation<FirstLevelProjectionOrganization>(SetupFirstLevelAssignmentsEdge)
            .AddChildRelation<FirstLevelProjectionRole>(SetupFirstLevelAssignmentsEdge)
            .AddChildRelation<FirstLevelProjectionFunction>(SetupFirstLevelAssignmentsEdge)
            .AddChildRelation<FirstLevelProjectionTag>(SetupFirstLevelTagLinkEdge)
            .AddChildRelation<FirstLevelProjectionsClientSetting>(SetupFirstLevelClientSettingsEdge)
            .AddParentRelation<FirstLevelProjectionFunction>(SetupFirstLevelFunctionLinkEdge)
            .Collection("profiles");

        modelBuilder.Entity<FirstLevelProjectionFunction>()
            .HasKeyIdentifier(o => o.Id)
            .AddChildRelation<FirstLevelProjectionTag>(SetupFirstLevelTagLinkEdge)
            .AddChildRelation<FirstLevelProjectionOrganization>(SetupFirstLevelFunctionLinkEdge)
            .AddChildRelation<FirstLevelProjectionRole>(SetupFirstLevelFunctionLinkEdge)
            .AddParentRelation<IFirstLevelProjectionProfile>(SetupFirstLevelAssignmentsEdge)
            .Collection("functions");

        modelBuilder.Entity<FirstLevelProjectionRole>()
            .HasKeyIdentifier(o => o.Id)
            .AddChildRelation<FirstLevelProjectionTag>(SetupFirstLevelTagLinkEdge)
            .AddParentRelation<IFirstLevelProjectionProfile>(SetupFirstLevelAssignmentsEdge)
            .AddParentRelation<FirstLevelProjectionFunction>(SetupFirstLevelFunctionLinkEdge)
            .Collection("roles");

        modelBuilder.Entity<FirstLevelProjectionTag>()
            .HasKeyIdentifier(t => t.Id)
            .AddParentRelation<IFirstLevelProjectionProfile>(SetupFirstLevelTagLinkEdge)
            .AddParentRelation<FirstLevelProjectionRole>(SetupFirstLevelTagLinkEdge)
            .AddParentRelation<FirstLevelProjectionFunction>(SetupFirstLevelTagLinkEdge)
            .Collection("tags");

        modelBuilder.Entity<FirstLevelProjectionsClientSetting>()
            .HasAlias<FirstLevelProjectionClientSettingsBasic>()
            .AddParentRelation<IFirstLevelProjectionProfile>(SetupFirstLevelClientSettingsEdge)
            .Collection("clientSettings");

        modelBuilder.Entity<FirstLevelProjectionTemporaryAssignment>()
            .HasKeyIdentifier(a => a.Id)
            .Collection("temporaryAssignments");

        // TODO Key & ProfileId index

        modelBuilder.Entity<ProjectionState>().Collection(ProjectionStateCollection);

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    private static DefaultModelConstellation NewEventCollectorStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<EventData>()
            .Collection("eventCollectorData")
            .QueryCollection("eventCollectorData");

        modelBuilder.Entity<StartCollectingEventData>()
            .Collection("startEventCollectorData")
            .QueryCollection("startEventCollectorData");

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    private static DefaultModelConstellation NewSyncScheduleStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<SyncSchedule>()
            .Collection("schedule")
            .QueryCollection("schedule");

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    private static DefaultModelConstellation NewSyncEntityStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<ISyncProfile>()
            .HasKeyIdentifier(g => g.Id)
            .HasAlias(
                p => p.Kind,
                nameof(SyncProfileKind.User),
                typeof(UserSync))
            .HasAlias(
                p => p.Kind,
                nameof(SyncProfileKind.Group),
                typeof(GroupSync))
            .HasAlias(
                p => p.Kind,
                nameof(SyncProfileKind.Organization),
                typeof(OrganizationSync))
            .Collection("profiles")
            .QueryCollection("profiles");

        modelBuilder.Entity<RoleSync>()
            .Collection("roles")
            .QueryCollection("roles");

        modelBuilder.Entity<FunctionSync>()
            .Collection("functions")
            .QueryCollection("functions");

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    private static DefaultModelConstellation NewTicketStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<TicketBase>()
            .Collection("tickets")
            .QueryCollection("tickets");

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    private static DefaultModelConstellation NewSyncStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<ProjectionState>()
            .Collection(ProjectionStateCollection);

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    internal static DefaultModelConstellation CreateNew(
        string prefix = WellKnownDatabaseKeys.CollectionPrefixUserProfileService,
        string queryPrefix = null)
    {
        return NewUserProfileStorage(prefix, queryPrefix);
    }

    internal static DefaultModelConstellation CreateNewTicketStore(string prefix)
    {
        return NewTicketStore(prefix);
    }

    internal static DefaultModelConstellation CreateNewSyncStore(string prefix)
    {
        return NewSyncStore(prefix);
    }

    internal static DefaultModelConstellation CreateNewSyncEntityStore(string prefix)
    {
        return NewSyncEntityStore(prefix);
    }

    internal static DefaultModelConstellation CreateNewFirstLevelProjection(string prefix)
    {
        return NewFirstLevelProjectionStorage(prefix);
    }

    internal static DefaultModelConstellation CreateNewSecondLevelProjection(string prefix)
    {
        //return NewSecondLevelProjectionStorage(prefix);
        return NewUserProfileStorage(prefix);
    }

    internal static DefaultModelConstellation CreateNewEventCollectorStore(string prefix)
    {
        return NewEventCollectorStore(prefix);
    }

    internal static DefaultModelConstellation CreateSyncScheduleStore(string prefix)
    {
        return NewSyncScheduleStore(prefix);
    }

    internal static DefaultModelConstellation CreateSyncEntityStore(string prefix)
    {
        return NewSyncEntityStore(prefix);
    }

    internal static DefaultModelConstellation NewEventLogStore(string prefix)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<EventLogTuple>()
            .Collection("batchEventLog")
            .QueryCollection("batchEventLog");

        modelBuilder.Entity<EventBatch>()
            .AddChildRelation<EventLogTuple>(b => b.WithCollectionName("batchEventLinks"))
            .Collection("batchLog")
            .QueryCollection("batchLog");

        return new DefaultModelConstellation(modelBuilder, prefix);
    }

    public static DefaultModelConstellation NewAssignmentsProjectionRepository(string prefix, string queryPrefix = null)
    {
        IModelBuilder modelBuilder = ModelBuilder.NewOne;

        modelBuilder.Entity<ProjectionState>()
            .Collection(ProjectionStateCollection);

        modelBuilder.Entity<SecondLevelProjectionAssignmentsUser>()
            .NoCollection()
            .QueryCollection(ProfileAssignmentsCollection);

        return new DefaultModelConstellation(modelBuilder, prefix, queryPrefix);
    }
}
