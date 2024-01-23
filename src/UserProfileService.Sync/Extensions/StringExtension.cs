
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Class that contains string extensions.
/// </summary>
public static class StringExtension
{
    /// <summary>
    ///     Returns for the string the right object type.
    /// </summary>
    /// <param name="entity">The entity that should be returned as <see cref="ObjectType" />.</param>
    /// <returns>The object type <see cref="ObjectType" />.</returns>
    public static ObjectType GetObjectType(this string entity)
    {
        return entity switch
               {
                   SyncConstants.SagaStep.GroupStep => ObjectType.Group,
                   SyncConstants.SagaStep.RoleStep => ObjectType.Role,
                   SyncConstants.SagaStep.UserStep => ObjectType.User,
                   SyncConstants.SagaStep.OrgUnitStep => ObjectType.Organization,
                   SyncConstants.SagaStep.FunctionStep => ObjectType.Function,
                   _ => ObjectType.Unknown
               };
    }
}
