using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     Represents a repository of the first-level-projection and contains methods to read/write
///     profile/functions/role/tag related data.
/// </summary>
public interface IFirstLevelProjectionRepository : IProjectionStateRepository
{
    /// <summary>
    ///     Aborts an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task AbortTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a tag to an existing function.
    /// </summary>
    /// <param name="tag">The tag assignment that should be add to the profile.</param>
    /// <param name="functionId">The function that the tag should be added to.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddTagToFunctionAsync(
        FirstLevelProjectionTagAssignment tag,
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a tag to an existing profile.
    /// </summary>
    /// <param name="tag">The tag assignment that should be add to the profile.</param>
    /// <param name="profileId">The profile to that the tag should be added to.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddTagToProfileAsync(
        FirstLevelProjectionTagAssignment tag,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a tag to an existing role.
    /// </summary>
    /// <param name="tag">The tag assignment that should be add to the profile.</param>
    /// <param name="roleId">The role to that the tag should be added to.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddTagToRoleAsync(
        FirstLevelProjectionTagAssignment tag,
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits an existing <paramref name="transaction" />.
    /// </summary>
    /// <param name="transaction">The object including information about the transaction to commit.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task CommitTransactionAsync(
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a role with with the specified <paramref name="function" /> data.
    /// </summary>
    /// <param name="function">The function that should be created.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateFunctionAsync(
        FirstLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an assignment from a target parent to a profile.
    /// </summary>
    /// <param name="parentId">The id of the parent whose profile should assign to.</param>
    /// <param name="parentType">The type of the parent that the profile is assigned to.</param>
    /// <param name="profileId">The id of the profile that should be assigned to the parent</param>
    /// <param name="conditions"> Defines the date time condition for object assignments.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateProfileAssignmentAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a profile with with the specified <paramref name="profile" />  data.
    /// </summary>
    /// <param name="profile">The profile that should be created.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateProfileAsync(
        IFirstLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a role with with the specified <paramref name="role" />  data.
    /// </summary>
    /// <param name="role">The role that should be crated.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateRoleAsync(
        FirstLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a tag with with the specified <paramref name="tag" /> data.
    /// </summary>
    /// <param name="tag">The tag that should be created.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateTag(
        FirstLevelProjectionTag tag,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a function by a given id <paramref name="functionId" />.
    /// </summary>
    /// <param name="functionId">The id of the function that should be deleted.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task DeleteFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete an existing profile assignment.
    /// </summary>
    /// <param name="parentId">The id of the parent whose profile should unassign from.</param>
    /// <param name="profileId">The id of the profile that should be unassigned from the parent</param>
    /// <param name="parentType">The type of the parent that the profile is unassigned from.</param>
    /// <param name="conditions"> Defines the date time condition for object assignments.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task DeleteProfileAssignmentAsync(
        string parentId,
        ContainerType parentType,
        string profileId,
        IList<RangeCondition> conditions,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a profile by a given id <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId">The id of the profile that should be deleted.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task DeleteProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a role by a given id <paramref name="roleId" />.
    /// </summary>
    /// <param name="roleId">The id of the role that should be deleted.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task DeleteRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a tag by a given id <paramref name="tagId" />.
    /// </summary>
    /// <param name="tagId">The id of the tag that should be deleted.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task DeleteTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return the children and their children recursively from a given parent.
    ///     The children will be returned distinctly.
    /// </summary>
    /// <param name="parentId">
    ///     The <see cref="ObjectIdent" />  that includes the id and object type from whose parent the children
    ///     should be retrieved recursively.
    /// </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="IFirstLevelProjectionProfile" />.
    /// </returns>
    Task<IList<FirstLevelRelationProfile>> GetAllChildrenAsync(
        ObjectIdent parentId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get all related objects that are affected by a change of a property from an object.
    ///     If a property change is triggered, all relevant objects should be notified.
    ///     That includes direct children (i.e. organization children, group children) AND
    ///     a function plus their virtual children, if this function is a direct virtual relative of the changed object.
    ///     An interesting case would be a name change of an organization mapped with a "non-empty" function.
    ///     Non-empty functions are ones that are not assigned to a group or user.
    ///     The original object whose property has changed will be returned as well.
    /// </summary>
    /// <param name="idOfModifiedObject">The id of the modified object.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of <see cref="ObjectIdentPath" />. The list
    ///     represents all affected objects with id and type.
    /// </returns>
    Task<IList<ObjectIdentPath>> GetAllRelevantObjectsBecauseOfPropertyChangedAsync(
        ObjectIdent idOfModifiedObject,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns all client settings that the profile has. The direct assigned client settings
    ///     as well as the inherited will be returned for the <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId">The id of the profile.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="FirstLevelProjectionsClientSetting" />.
    /// </returns>
    Task<IList<FirstLevelProjectionsClientSetting>>
        GetCalculatedClientSettingsAsync(
            string profileId,
            IDatabaseTransaction transaction = default,
            CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return all parent relations of the profile identified by <paramref name="profileId" />
    ///     that are <b>not</b> part of parent relations identified by <paramref name="referenceProfileId" />.
    /// </summary>
    /// <remarks>
    ///     The method only returns the relations that are part of the tree of <paramref name="profileId" />.
    ///     No content from the tree of the <paramref name="referenceProfileId" /> will be delivered.
    /// </remarks>
    /// <param name="profileId">
    ///     The profile id whose parent relation should be return without the
    ///     <paramref name="referenceProfileId" /> relations.
    /// </param>
    /// <param name="referenceProfileId">
    ///     The reference profile id whose parents relations are used to filter the parent
    ///     relations from <paramref name="profileId" />.
    /// </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="FirstLevelProjectionParentsTreeDifferenceResult" />.
    ///     The tree that contains only the parents relations from the profile <paramref name="profileId" /> without
    ///     the parents relations form the <paramref name="referenceProfileId" />. It also includes TagAssignment that
    ///     are different to the <paramref name="referenceProfileId"/>.
    /// </returns>
    IAsyncEnumerable<FirstLevelProjectionParentsTreeDifferenceResult> GetDifferenceInParentsTreesAsync(
        string profileId,
        IList<string> referenceProfileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a function by a given id <paramref name="functionId" />.
    /// </summary>
    /// <param name="functionId">The id of the function that should be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps a <see cref="FirstLevelProjectionFunction" />.</returns>
    Task<FirstLevelProjectionFunction> GetFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Obtains the list of <see cref="FirstLevelProjectionTagAssignment" /> between the profileId and the given TagIds.
    ///     Missing assignments will not be returned.
    /// </summary>
    /// <param name="tagIds">The tag ids that should be assigned to a given profile.</param>
    /// <param name="profileId">
    ///     The profile id that should be checked, if an assignment exists between the profile and the
    ///     given tag ids.
    /// </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="FirstLevelProjectionTagAssignment" />s.
    /// </returns>
    Task<IList<FirstLevelProjectionTagAssignment>> GetTagsAssignmentsFromProfileAsync(
        string[] tagIds,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns the direct members from a container. As result the method returns
    ///     a list of <see cref="ObjectIdent" />s.
    /// </summary>
    /// <param name="containerId">The id of the container whose members you want to get.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellation">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="ObjectIdent" />s.
    /// </returns>
    Task<IList<ObjectIdent>> GetContainerMembersAsync(
        ObjectIdent containerId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellation = default);

    /// <summary>
    ///     Return all direct parents from a given child entity.
    /// </summary>
    /// <param name="childId">The child id whose parents should be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="IFirstLevelProjectionContainer" />s.
    /// </returns>
    Task<IList<IFirstLevelProjectionContainer>> GetParentsAsync(
        string childId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Return all objects that are assigned to the given tag recursively.
    /// </summary>
    /// <param name="tagId">The id of the tag.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of <see cref="ObjectIdent" />s to which
    ///     objects the tag is assign to.
    /// </returns>
    Task<IList<ObjectIdent>> GetAssignedObjectsFromTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a profile by a given id.
    /// </summary>
    /// <param name="profileId">The profile id that should be used to retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="IFirstLevelProjectionProfile" />.
    /// </returns>
    Task<IFirstLevelProjectionProfile> GetProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a profile by a given id.
    /// </summary>
    /// <param name="profileId">The profile id that should be used to retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task that represent the asynchronous read operation. It wraps a list of
    ///     <see cref="IFirstLevelProjectionProfile" />.
    /// </returns>
    Task<TProfileType> GetProfileAsync<TProfileType>(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default) where TProfileType : class, IFirstLevelProjectionProfile;

    /// <summary>
    ///     Returns a role by a given id <paramref name="roleId" />.
    /// </summary>
    /// <param name="roleId">The id of the role that should be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A task that represent the asynchronous read operation. It wraps a <see cref="RoleBasic" />.</returns>
    Task<FirstLevelProjectionRole> GetRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns functions created with the given organization id <paramref name="organizationId" />.
    /// </summary>
    /// <param name="organizationId">The id of the organization for the related roles which should be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A task that represent the asynchronous read operation. It wraps a list of <see cref="RoleBasic" />.</returns>
    Task<ICollection<FirstLevelProjectionFunction>> GetFunctionsOfOrganizationAsync(
        string organizationId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a role by a given id <paramref name="tagId" />.
    /// </summary>
    /// <param name="tagId">The id of the tag that should be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps a <see cref="Tag" />.</returns>
    Task<FirstLevelProjectionTag> GetTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove a tag from an existing function.
    /// </summary>
    /// <param name="tagId">The id of tag that should be removed from a function.</param>
    /// <param name="functionId">The id of the function from which the tag should removed from.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task RemoveTagFromFunctionAsync(
        string tagId,
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove a tag from an existing profile.
    /// </summary>
    /// <param name="tagId">The id of tag that should be removed from a profile.</param>
    /// <param name="profileId">The id of the profile from which the tag should removed from.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task RemoveTagFromProfileAsync(
        string tagId,
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove a tag from an existing Role.
    /// </summary>
    /// <param name="tagId">The id of tag that should be removed from a role.</param>
    /// <param name="roleId">The id of the role from which the tag should removed from.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task RemoveTagFromRoleAsync(
        string tagId,
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Set the client setting for an existing profile.
    /// </summary>
    /// <param name="profileId">The id of profile for that the client setting should be set.</param>
    /// <param name="clientSetting">The client setting that should be set.</param>
    /// <param name="key">The key of the client setting.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task SetClientSettingsAsync(
        string profileId,
        string clientSetting,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Starts a transaction and returns an object containing information about the new created transaction.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous operation that wraps the <see cref="IDatabaseTransaction" />.</returns>
    Task<IDatabaseTransaction> StartTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Unset the client setting for an existing profile.
    /// </summary>
    /// <param name="profileId">The id of profile for that the client setting should be set.</param>
    /// <param name="key">The key of the client setting that should be unset.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task UnsetClientSettingsAsync(
        string profileId,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing function and overwrite it's properties with the specified <paramref name="function" /> data.
    /// </summary>
    /// <param name="function">The new state of the function to be modified.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task UpdateFunctionAsync(
        FirstLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing profile and overwrite it's properties with the specified <paramref name="profile" /> data.
    /// </summary>
    /// <param name="profile">The new state of the profile to be modified.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task UpdateProfileAsync(
        IFirstLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing role and overwrite it's properties with the specified <paramref name="role" /> data.
    /// </summary>
    /// <param name="role">The new state of the role to be modified.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task UpdateRoleAsync(
        FirstLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the value of updatedAt for all given entities.
    /// </summary>
    /// <param name="updatedAt">The new value for updatedAt.</param>
    /// <param name="objects">The <see cref="ObjectIdent" /> of the objects to update.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task SetUpdatedAtAsync(
        DateTime updatedAt,
        IList<ObjectIdent> objects,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns temporary assignments that have to be activated/deactivated.
    /// </summary>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of
    ///     <see cref="FirstLevelProjectionTemporaryAssignment" /> elements.
    /// </returns>
    Task<IList<FirstLevelProjectionTemporaryAssignment>> GetTemporaryAssignmentsAsync(
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves the <paramref name="desiredStates" /> of temporary assignments.
    /// </summary>
    /// <param name="desiredStates">State of temporary assignments to be saved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task UpdateTemporaryAssignmentStatesAsync(
        IList<FirstLevelProjectionTemporaryAssignment> desiredStates,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously checks if a user exists based on the provided parameters.
    /// </summary>
    /// <param name="externalId">The external ID of the user. Must be provided along with either display name or email.</param>
    /// <param name="displayName">The display name of the user. Must be provided along with either external ID or email.</param>
    /// <param name="email">The email of the user. Must be provided along with either external ID or display name.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation (optional).</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the user exists, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown if none of externalId, displayName, or email is provided.</exception>
    Task<bool> UserExistAsync(
        string externalId,
        string displayName,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously checks if a group with the specified external ID, name, and display name exists.
    /// </summary>
    /// <param name="externalId">The external ID of the group to search for.</param>
    /// <param name="name">The name of the group to search for.</param>
    /// <param name="displayName">The display name of the group to search for.</param>
    /// <param name="ignoreCase">Specifies whether the comparison should be case-insensitive.</param>
    /// <param name="cancellationToken">A token to cancel the operation, if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the group exists,
    /// and <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is canceled via the <paramref name="cancellationToken"/>.
    /// </exception>
    Task<bool> GroupExistAsync(
        string externalId,
        string name,
        string displayName,
        bool ignoreCase,
        CancellationToken cancellationToken = default);
}
