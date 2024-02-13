namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     Defines an enumeration for the scope of collection (query or command).
/// </summary>
public enum CollectionScope
{
    /// <summary>
    ///     Represents a query collection scope.
    /// </summary>
    Query,

    /// <summary>
    ///     Represents a command collection scope.
    /// </summary>
    Command
}
