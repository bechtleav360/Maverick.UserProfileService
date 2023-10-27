namespace UserProfileService.Sync.Projection.Services;

internal class ErrorCodes
{
    /// <summary>
    ///     If a function could not be found.
    /// </summary>
    public const string FunctionNotFoundString = "FUNCTION_NOT_FOUND";

    /// <summary>
    ///     If a profile could not be found.
    /// </summary>
    public const string ProfileNotFoundString = "PROFILE_NOT_FOUND";

    /// <summary>
    ///     If a role could not be found.
    /// </summary>
    public const string RoleNotFoundString = "ROLE_NOT_FOUND";
}
