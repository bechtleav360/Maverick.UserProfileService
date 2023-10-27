using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     The implementation of <see cref="ISyncModel" /> for functions.
/// </summary>
[Model(SyncConstants.Models.Function)]
public class FunctionSync : ISyncModel
{
    /// <summary>
    ///     The ending date on which the function is valid.
    /// </summary>
    public DateTime End { set; get; }

    /// <inheritdoc />
    public IList<KeyProperties> ExternalIds { get; set; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     The Id of the organization <see cref="OrganizationBasic" />
    /// </summary>
    public string OrganizationId { get; set; }

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; }

    /// <summary>
    ///     Describes the role that is related to the function.
    /// </summary>
    public string Role { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The start date on which the function is valid.
    /// </summary>
    public DateTime Start { set; get; }
}
