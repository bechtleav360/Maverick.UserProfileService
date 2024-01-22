using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Abstraction.Contracts;

/// <summary>
///     Contains constants related to sync applications.
/// </summary>
public static class SyncConstants
{
    /// <summary>
    ///     Defines the values with which external as well as internal systems can be identified.
    /// </summary>
    public static class System
    {
        /// <summary>
        ///     Defines the global system value for the synchronization system.
        /// </summary>
        public const string InitiatorId = "Sync";

        /// <summary>
        ///     Defines the global system value for ldap system.
        /// </summary>
        public const string Ldap = "Ldap";

        /// <summary>
        ///     Defines the global system value for the user profile service internal system.
        /// </summary>
        public const string UserProfileService = "UserProfileService";

        /// <summary>
        ///     An array of strings containing all global system values available in the sync system.
        /// </summary>
        public static string[] All => new[] { Ldap, UserProfileService };
    }

    /// <summary>
    ///     Defines the well known trigger systems for synchronization
    /// </summary>
    public static class Initiator
    {
        /// <summary>
        ///     Defines the identifier used to recognize synchronization processes started via the API (but not initiated by
        ///     scheduler).
        /// </summary>
        public const string Api = "API";
    }

    /// <summary>
    ///     Defines the well known key used to synchronize sync processes.
    /// </summary>
    public static class SynchronizationKeys
    {
        /// <summary>
        ///     Key used to identify a sync process start request
        /// </summary>
        public const string SyncStart = "sync-start";

        /// <summary>
        ///     Key used to identify a sync process start request from scheduler
        /// </summary>
        public const string SyncStartByScheduler = "scheduler-sync-start";

        /// <summary>
        ///     Key used to allocate a lock object to a running sync-process
        /// </summary>
        public const string SynLockObject = "sync-start-command";
    }

    /// <summary>
    ///     Defines the well known tags for entities.
    /// </summary>
    public static class Tags
    {
        /// <summary>
        ///     Defines the tag name to specify groups as organizations
        /// </summary>
        public const string Organization = "OU";
    }

    /// <summary>
    ///     Defines the values with which the individual sync entities are recognized and mapped.
    /// </summary>
    public static class Models
    {
        /// <summary>
        ///     Identifier for <see cref="Models.AddedRelation" />.
        /// </summary>
        public const string AddedRelation = "addedRelation";

        /// <summary>
        ///     Identifier for <see cref="Models.DeletedRelation" />.
        /// </summary>
        public const string DeletedRelation = "deletedRelation";

        /// <summary>
        ///     Identifier for <see cref="FunctionSync" />.
        /// </summary>
        public const string Function = "function";

        /// <summary>
        ///     Identifier for <see cref="GroupSync" />.
        /// </summary>
        public const string Group = "group";

        /// <summary>
        ///     Identifier for <see cref="OrganizationSync" />.
        /// </summary>
        public const string Organization = "organization";

        /// <summary>
        ///     Identifier for <see cref="RoleSync" />.
        /// </summary>
        public const string Role = "role";

        /// <summary>
        ///     Identifier for <see cref="UserSync" />.
        /// </summary>
        public const string User = "user";
    }

    /// <summary>
    ///     Identifies the saga step that are mapped to the saga messages.
    /// </summary>
    public static class SagaStep
    {
        /// <summary>
        ///     Identifies the added relation step message.
        /// </summary>
        public const string AddedRelationStep = "AddedRelationStep";

        /// <summary>
        ///     Identifies the deleted relation step message.
        /// </summary>
        public const string DeletedRelationStep = "DeletedRelationStep";

        /// <summary>
        ///     Identifies the function step message.
        /// </summary>
        public const string FunctionStep = "functions";

        /// <summary>
        ///     Identifies the groups step message.
        /// </summary>
        public const string GroupStep = "groups";

        /// <summary>
        ///     Identifies the orgUnits step message.
        /// </summary>
        public const string OrgUnitStep = "organizations";

        /// <summary>
        ///     Identifies the role step message.
        /// </summary>
        public const string RoleStep = "roles";

        /// <summary>
        ///     Identifies the users step message.
        /// </summary>
        public const string UserStep = "users";

        /// <summary>
        /// All unary sync steps that can be done.
        /// </summary>
        public static List<string> AllAtomareSteps = new List<string>()
                                                     {
                                                         RoleStep,
                                                         UserStep,
                                                         OrgUnitStep,
                                                         GroupStep,
                                                         FunctionStep,
                                                     };

    }
}
