namespace UserProfileService.Projection.Common.Utilities;

/// <summary>
///     Contains the name of the projections used inside the ups.
/// </summary>
public static class ProjectionNameConstants
{
    /// <summary>
    ///     Name used to register the first level facade projection by marten event store.
    /// </summary>
    public const string FirstLevelFacadeProjection = "FirstLevelFacadeProjection";

    /// <summary>
    ///     Name used to register the first level projection by marten event store.
    /// </summary>
    public const string FirstLevelProjection = "FirstLevelProjection";

    /// <summary>
    ///     Name used to register the second level projection by marten event store.
    /// </summary>
    public const string SecondLevelApiProjection = "SecondLevelProjection";

    /// <summary>
    ///     Name used to register the second level assignment projection by marten event store.
    /// </summary>
    public const string SecondLevelAssignmentsProjection = "SecondLevelAssignmentsProjection";

    /// <summary>
    ///     Name used to register the second level opa projection by marten event store.
    /// </summary>
    public const string SecondLevelOpaProjection = "SecondLevelOpaProjection";

    /// <summary>
    ///     Name used to register the second level volatile data projection by marten event store.
    /// </summary>
    public const string SecondLevelVolatileDataProjection = "SecondLevelVolatileDataProjection";
}
