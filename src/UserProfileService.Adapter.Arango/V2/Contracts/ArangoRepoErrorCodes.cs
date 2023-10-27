// ReSharper disable UnusedMember.Global
namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     Contains error codes of the ArangoDB implementation of the Maverick UPS and SecService mainly as integer values.
/// </summary>
public class ArangoRepoErrorCodes
{
    /// <summary>
    ///     If the custom property key could not be found (string version).
    /// </summary>
    public const string CustomPropertyKeyNotFoundString = "CUSTOMPROPERTYKEY_NOT_FOUND";

    /// <summary>
    ///     If the FROM node of an edge could not be found.
    /// </summary>
    public const int FromNodeNotFound = 30001;

    /// <summary>
    ///     If the FROM node of an edge could not be found (string version).
    /// </summary>
    public const string FromNodeNotFoundString = "FROM_NODE_NOT_FOUND";

    /// <summary>
    ///     Sets the lowest positive error code. Values less than this value are not Maverick ArangoDB error codes.
    /// </summary>
    public const int LowerBoundOfErrorCodes = ProfileNotFound;

    /// <summary>
    ///     If a member could not be found (string version).
    /// </summary>
    public const string MemberNotFoundString = "MEMBERs_NOT_FOUND";

    /// <summary>
    ///     If a profile could not be found (Integer version).
    /// </summary>
    public const int ProfileNotFound = 30000;

    /// <summary>
    ///     If a profile could not be found (string version).
    /// </summary>
    public const string ProfileNotFoundString = "PROFILE_NOT_FOUND";

    /// <summary>
    ///     If a role or function could not be found.
    /// </summary>
    public const int RoleOrFunctionNotFound = 30004;

    /// <summary>
    ///     If a role or function could not be found (string version).
    /// </summary>
    public const string RoleOrFunctionNotFoundString = "ROLEFUNCTION_NOT_FOUND";

    /// <summary>
    ///     If a role or function could not be found.
    /// </summary>
    public const int TagNotFound = 30005;

    /// <summary>
    ///     If a role or function could not be found (string version).
    /// </summary>
    public const string TagNotFoundString = "TAG_NOT_FOUND";

    /// <summary>
    ///     If the TO node of an edge could not be found.
    /// </summary>
    public const int ToNodeNotFound = 30002;

    /// <summary>
    ///     If the TO node of an edge could not be found (string version).
    /// </summary>
    public const string ToNodeNotFoundString = "TO_NODE_NOT_FOUND";
}
