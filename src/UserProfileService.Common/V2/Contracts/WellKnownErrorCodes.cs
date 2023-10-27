namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Contains error codes as constant strings.
/// </summary>
public class WellKnownErrorCodes
{
    /// <summary>
    ///     If a child object could not be found.
    /// </summary>
    public const string ChildNotFound = "CHILD_NOT_FOUND";

    /// <summary>
    ///     If a cursor could not be found.
    /// </summary>
    public const string CursorNotFound = "CURSOR_NOT_FOUND";

    /// <summary>
    ///     If a object could not be found.
    /// </summary>
    public const string ObjectNotFound = "OBJECT_NOT_FOUND";

    /// <summary>
    ///     If a parent object could not be found.
    /// </summary>
    public const string ParentNotFound = "PARENT_NOT_FOUND";

    /// <summary>
    ///     If a profile could not be found.
    /// </summary>
    public const string ProfileNotFound = "PROFILE_NOT_FOUND";

    /// <summary>
    ///     If a role could not be found.
    /// </summary>
    public const string RoleNotFound = "ROLE_NOT_FOUND";

    /// <summary>
    ///     If a type object could not be found.
    /// </summary>
    public const string TypeNotFound = "TYPE_NOT_FOUND";
}
