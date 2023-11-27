namespace UserProfileService.Adapter.Arango.V2.Contracts;

public static class ArangoConstants
{
    /// <summary>
    ///     The regular expression pattern to check if a string is a valid ArangoDb key.
    /// </summary>
    internal const string ArangoKeyPattern = @"^[A-Za-z0-9_\-:.@\(\)+,=;$!*'%]{1,245}$";

    public const string ArangoClientName = "Arango_DB_Client";
    public const string ArangoFirstLevelLogStore = "Arango_First_Level_Projection_Log_Writer";
    public const string ArangoFirstLevelProjectionName = "Arango_First_Level_Projection_Name";

    /// <summary>
    ///     Defines the client name of the event collector.
    /// </summary>
    public const string DatabaseClientNameEventCollector = "EventCollector";

    /// <summary>
    ///     Defines the client name of the saga worker projection.
    /// </summary>
    public const string DatabaseClientNameSagaWorker = "SagaWorker";

    /// <summary>
    ///     Defines the client name of the saga worker projection.
    /// </summary>
    public const string DatabaseClientNameSync = "Sync";

    /// <summary>
    ///     Defines the client name of the ticket-store.
    /// </summary>
    public const string DatabaseClientNameTicketStore = "UpsTickets";

    /// <summary>
    ///     Defines the client name of the user profile service (master repository).
    /// </summary>
    public const string DatabaseClientNameUserProfileStorage = "UpsStorage";

    public const string SecondLevelArangoClientName = "Arango_Second_Level_Client";
    public const string SecondLevelAssignmentsArangoClientName = "Arango_Second_Level_Assignment_Client";

}
