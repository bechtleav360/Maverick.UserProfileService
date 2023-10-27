using System.Collections.Generic;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.Public;

/// <summary>
///     Arango User model class
/// </summary>
public class AUser
{
    /// <summary>
    ///     A flag indicating whether the user account should be activated or not.
    ///     The default value is true. If set to false, the user won't be able to
    ///     log into the database
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    ///     A JSON object with extra user information. The data contained in extra
    ///     will be stored for the user but not be interpreted further by ArangoDB
    /// </summary>
    public Dictionary<string, object> Extra { get; set; }

    /// <summary>
    ///     he user password as a string. If not specified, it will default to an empty string.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    ///     Login name of the user to be created
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    ///     Creates a new Arango User
    /// </summary>
    public AUser()
    {
        Active = true;
    }
}
