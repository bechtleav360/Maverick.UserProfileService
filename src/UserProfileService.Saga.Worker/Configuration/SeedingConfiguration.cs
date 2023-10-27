using System.Collections.Generic;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Saga.Worker.Configuration;

/// <summary>
///     The seeding configuration that is used to configure the seeding objects.
/// </summary>
public class SeedingConfiguration
{
    /// <summary>
    ///     Specifies whether seeding should be performed when the service is started.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    ///     All functions to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateFunctionRequest> Functions { get; set; } =
        new Dictionary<string, CreateFunctionRequest>();

    /// <summary>
    ///     All groups to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateGroupRequest> Groups { get; set; } = new Dictionary<string, CreateGroupRequest>();

    /// <summary>
    ///     Identifier of the service used as initiator for all seeding operations.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     All organizations to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateOrganizationRequest> Organizations { get; set; } =
        new Dictionary<string, CreateOrganizationRequest>();

    /// <summary>
    ///     All roles to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateRoleRequest> Roles { get; set; } = new Dictionary<string, CreateRoleRequest>();

    /// <summary>
    ///     All tags to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateTagRequest> Tags { get; set; } = new Dictionary<string, CreateTagRequest>();

    /// <summary>
    ///     All users to be seeded into the database.
    /// </summary>
    public Dictionary<string, CreateUserRequest> Users { get; set; } = new Dictionary<string, CreateUserRequest>();
}
