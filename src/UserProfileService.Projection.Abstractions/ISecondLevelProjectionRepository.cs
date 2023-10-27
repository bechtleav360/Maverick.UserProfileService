using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     Represents a repository of the second-level-projection and contains methods to read/write
///     profile/functions/role/tag related data.
/// </summary>
public interface ISecondLevelProjectionRepository : IProjectionStateRepository
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
    ///     Adds the specified custom properties to the profile corresponding to the given id.
    /// </summary>
    /// <param name="profileId">The id of the profile which becomes new custom properties.</param>
    /// <param name="customProperties">The custom properties that should be added to the specified profile.</param>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task AddCustomPropertiesToProfile(
        string profileId,
        Dictionary<string, string> customProperties,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove the specified custom properties to the profile corresponding to the given id.
    /// </summary>
    /// <param name="profileId">The id of the profile which becomes new custom properties.</param>
    /// <param name="customPropertiesKeys">The keys of custom properties that should be removed from the specified profile.</param>
    /// <param name="transaction">The object including information about the transaction to aborted.</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public Task RemoveCustomPropertiesFromProfile(
        string profileId,
        IEnumerable<string> customPropertiesKeys,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new <paramref name="member" /> to a container of type <paramref name="containerType" /> and with the
    ///     specified <paramref name="containerId" />.
    /// </summary>
    /// <param name="containerType">The type of the container whose members list should be modified.</param>
    /// <param name="containerId">The id of the container whose members list should be modified.</param>
    /// <param name="member">The new member to be added.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddMemberAsync(
        string containerId,
        ContainerType containerType,
        Member member,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds a new <paramref name="container" /> to a profile with specified <paramref name="member" /> as
    ///     container/memberOf.
    /// </summary>
    /// <param name="relatedProfileId">
    ///     The id of the profile from whose point of view the related event is viewed.<br />
    ///     This can be the same value like <paramref name="memberId" />, if the "own" data set should be modified. But it will
    ///     be different, if just a related profile has been changed and the "own" relation tree must be modified.
    /// </param>
    /// <param name="memberId">The Id of the member whose memberOf list should be modified.</param>
    /// <param name="conditions">
    ///     The conditions of the membership
    ///     <see cref="Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition" />.
    /// </param>
    /// <param name="container">The container to be added as new parent/container of the specified member.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddMemberOfAsync(
        string relatedProfileId,
        string memberId,
        IList<RangeCondition> conditions,
        ISecondLevelProjectionContainer container,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default
    );

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
    ///     Set client settings to the profile corresponding to the given id.
    /// </summary>
    /// <param name="profileId"> The id of the profile whose client settings should be set. </param>
    /// <param name="key"> The key of the client setting that should be set. </param>
    /// <param name="settings"> The client settings as JSON string. </param>
    /// <param name="isInherited"> True if the client settings is owned by the related profile otherwise false. </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns> A task that represent the asynchronous write operation. </returns>
    Task SetClientSettingsAsync(
        string profileId,
        string key,
        string settings,
        bool isInherited,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Remove the specified (with key) client setting from the profile (only not inherited client settings can be unset).
    /// </summary>
    /// <param name="profileId"> The id of the profile whose client settings has been unset </param>
    /// <param name="key"> The key of the client setting that should be unset </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A task that represent the asynchronous write operation. </returns>
    Task UnsetClientSettingFromProfileAsync(
        string profileId,
        string key,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Invalidate client settings from profile. This will remove all client settings
    ///     with keys that are NOT in <paramref name="remainingKeys" /> for the profile with id <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId"> The id of the profile. </param>
    /// <param name="remainingKeys">
    ///     An array containing the remaining client setting keys. If empty, all client settings are
    ///     deleted.
    /// </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A task that represent the asynchronous write operation. </returns>
    Task InvalidateClientSettingsFromProfile(
        string profileId,
        string[] remainingKeys,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new function with specified <paramref name="function" /> data.
    /// </summary>
    /// <param name="function">The function to be created.</param>
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
        SecondLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new profile with specified <paramref name="profile" /> data.
    /// </summary>
    /// <param name="profile">The profile to be created.</param>
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
        ISecondLevelProjectionProfile profile,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new role with specified <paramref name="role" /> data.
    /// </summary>
    /// <param name="role">The role to be created.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// l-o
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateRoleAsync(
        SecondLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a new tag with specified <paramref name="tag" /> data.
    /// </summary>
    /// <param name="tag">The tag to be created.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task CreateTagAsync(
        Tag tag,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Adds tags to object.
    /// </summary>
    /// <param name="relatedObjectId">
    ///     The id of the profile from whose point of view the related event is viewed.<br />
    ///     This can be the same value like <paramref name="objectId" />, if the "own" data set should be modified. But it will
    ///     be different, if just a related profile has been changed and the "own" relation tree must be modified.
    /// </param>
    /// <param name="objectId">The Id of the tagged object.</param>
    /// <param name="objectType">The object type <see cref="ObjectType" /> that has been tagged.</param>
    /// <param name="tags">A collection of tags assignments <see cref="TagAssignment" />.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task AddTagToObjectAsync(
        string relatedObjectId,
        string objectId,
        ObjectType objectType,
        IEnumerable<TagAssignment> tags,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the tag with the given Id.
    /// </summary>
    /// <param name="tagId"> The Id of the tag that should be remove. </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns> >A task that represent the asynchronous write operation. </returns>
    Task RemoveTagAsync(
        string tagId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes tags assignments from the specified object.
    /// </summary>
    /// <param name="relatedProfileId">
    ///     The id of the profile from whose point of view the related event is viewed.<br />
    ///     This can be the same value like <paramref name="member.Id" />, if the "own" data set should be modified. But it
    ///     will
    ///     be different, if just a related profile has been changed and the "own" relation tree must be modified.
    /// </param>
    /// <param name="objectId"> The Id of the tagged object. </param>
    /// <param name="objectType"> The object type <see cref="ObjectType" /> that has been tagged. </param>
    /// <param name="tags"> A collection of tags assignments <see cref="TagAssignment" /> </param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns> A task that represent the asynchronous write operation. </returns>
    Task RemoveTagFromObjectAsync(
        string relatedProfileId,
        string objectId,
        ObjectType objectType,
        IEnumerable<string> tags,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a function with the specified <paramref name="functionId" />.
    /// </summary>
    /// <param name="functionId">The id of the function to be deleted.</param>
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
    ///     Deletes a profile with the specified <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId">The id of the profile to be deleted.</param>
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
    ///     Deletes a role with the specified <paramref name="roleId" />.
    /// </summary>
    /// <param name="roleId">The id of the role to be deleted.</param>
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
    ///     Gets the function with the specified <paramref name="functionId" />.
    /// </summary>
    /// <param name="functionId">The id of the function to be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps the requested function.</returns>
    Task<SecondLevelProjectionFunction> GetFunctionAsync(
        string functionId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the path entries of a profile with the specified <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId">The id of the profile whose path information should be returned.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps the requested paths as string collection.</returns>
    Task<IList<string>> GetPathOfProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a profile with the specified <paramref name="profileId" />.
    /// </summary>
    /// <param name="profileId">The id of the profile to be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps the requested profile.</returns>
    Task<ISecondLevelProjectionProfile> GetProfileAsync(
        string profileId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the role with the specified <paramref name="roleId" />.
    /// </summary>
    /// <param name="roleId">The id of the role to be retrieved.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous read operation. It wraps the requested function.</returns>
    Task<SecondLevelProjectionRole> GetRoleAsync(
        string roleId,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the conditions on a member with the specified <paramref name="memberId" /> from a container of type
    ///     <paramref name="containerType" /> and with the specified <paramref name="containerId" />.
    /// </summary>
    /// <param name="containerType">The type of the container whose members list should be modified.</param>
    /// <param name="containerId">The id of the container whose members list should be modified.</param>
    /// <param name="memberId">The id of the member to be removed.</param>
    /// <param name="conditions">A list of range-condition that should be deleted.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task RemoveMemberAsync(
        string containerId,
        ContainerType containerType,
        string memberId,
        IList<RangeCondition> conditions = null,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the given conditions from a container of type <paramref name="containerType" /> and with the specified
    ///     <paramref name="containerId" /> from the container/memberOf list of a profile with specified
    ///     <paramref name="memberId" />.
    /// </summary>
    /// <param name="relatedProfileId">
    ///     The id of the profile from whose point of view the related event is viewed.<br />
    ///     This can be the same value like <paramref name="memberId" />, if the "own" data set should be modified. But it will
    ///     be different, if just a related profile has been changed and the "own" relation tree must be modified.
    /// </param>
    /// <param name="memberId">The id of the member whose memberOf list should be modified.</param>
    /// <param name="conditions">A list of range-condition that should be deleted.</param>
    /// <param name="containerType">The type of the container whose members list should be modified.</param>
    /// <param name="containerId">The id of the container whose members list should be modified.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task RemoveMemberOfAsync(
        string relatedProfileId,
        string memberId,
        ContainerType containerType,
        string containerId,
        IList<RangeCondition> conditions = null,
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
        SecondLevelProjectionFunction function,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates an existing profile and overwrite it all primitive properties with the specified
    ///     <paramref name="profile" /> data.
    /// </summary>
    /// <remarks>
    ///     Path, Members, MemberOf, etc. are calculated properties, that will be ignored. There are suitable methods to update
    ///     these properties.
    /// </remarks>
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
        ISecondLevelProjectionProfile profile,
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
        SecondLevelProjectionRole role,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to update a <see cref="Member" /> inside the members property of a
    ///     <see cref="ISecondLevelProjectionContainer" />. This
    ///     method will only update properties of the member. It won't add or remove the member from the list or modify the
    ///     list itself.
    /// </summary>
    /// <remarks>
    ///     The method won't threw an exception, if <paramref name="memberIdentifier" /> is not part of
    ///     <see cref="relatedProfileId" />'s set of linked objects. Neither will an error occur, if the property set
    ///     contains irrelevant properties.
    /// </remarks>
    /// <param name="relatedProfileId">The id of the profile whose member should been updated.</param>
    /// <param name="memberIdentifier">
    ///     The id of the member inside the member set of <paramref name="relatedProfileId" /> that
    ///     should been updated.
    /// </param>
    /// <param name="changedPropertySet">A set of property changes.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task TryUpdateMemberAsync(
        string relatedProfileId,
        string memberIdentifier,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to update a <see cref="Member" />Of inside the member-of property of a
    ///     <see cref="ISecondLevelProjectionContainer" />. This
    ///     method will only update properties of the member-of object. It won't add or remove the object from the list or
    ///     modify the
    ///     list itself.
    /// </summary>
    /// <remarks>
    ///     The method won't threw an exception, if <paramref name="memberIdentifier" /> is not part of
    ///     <see cref="relatedProfileId" />'s set of linked objects. Neither will an error occur, if the property set
    ///     contains irrelevant properties.
    /// </remarks>
    /// <param name="relatedProfileId">The id of the profile whose member-of entry should been updated.</param>
    /// <param name="memberIdentifier">
    ///     The id of the member inside the member set of <paramref name="relatedProfileId" /> that
    ///     should been updated.
    /// </param>
    /// <param name="changedPropertySet">A set of property changes.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task TryUpdateMemberOfAsync(
        string relatedProfileId,
        string memberIdentifier,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to update a linked object inside the linked object set of a profile. This
    ///     method will only update properties of the list entry. It won't add or remove the entry from the list or modify the
    ///     list itself.
    /// </summary>
    /// <remarks>
    ///     Linked objects can be functions or roles.<br />
    ///     The method won't threw an exception, if <paramref name="linkedObjectId" /> is not part of
    ///     <see cref="relatedProfileId" />'s set of linked objects. Neither will an error occur, if the property set
    ///     contains irrelevant properties.
    /// </remarks>
    /// <param name="linkedObjectId">
    ///     The id of the role or function whose set of linked profiles contains the linked profile to
    ///     be modified.
    /// </param>
    /// <param name="changedPropertySet">A set of property changes.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <param name="relatedProfileId">
    ///     The id of the profile whose set of linked objects contains the function or role to
    ///     be modified.
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task TryUpdateLinkedObjectAsync(
        string relatedProfileId,
        string linkedObjectId,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tries to update a linked profile inside the linked profile set of a linked object. This
    ///     method will only update properties of the list entry. It won't add or remove the entry from the list or modify the
    ///     list itself.
    /// </summary>
    /// <remarks>
    ///     Linked objects can be functions or roles.<br />
    ///     The method won't threw an exception, if <paramref name="linkedProfileId" /> is not part of
    ///     <see cref="relatedLinkedObjectId" />'s set of linked objects. Neither will an error occur, if the property set
    ///     contains irrelevant properties.
    /// </remarks>
    /// <param name="linkedProfileId">
    ///     The id of the linked profile that is part of the linked object and those properties
    ///     should be changed.
    /// </param>
    /// <param name="changedPropertySet">A set of property changes.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <param name="relatedLinkedObjectId">
    ///     The id of the role or function whose set of linked profiles contains the linked
    ///     profile to be modified.
    /// </param>
    /// <returns>A task that represent the asynchronous write operation.</returns>
    Task TryUpdateLinkedProfileAsync(
        string relatedLinkedObjectId,
        string linkedProfileId,
        IDictionary<string, object> changedPropertySet,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     (Re-)Calculates all properties of <paramref name="relatedProfileId" /> that are influences by inheritance, like
    ///     path or tags, due of changed relationships between
    ///     a inherited profile with id equals <paramref name="profileId" /> and a target with id equals
    ///     <paramref name="targetId" />.
    /// </summary>
    /// <param name="relatedEntity">
    ///     The id of the entity whose calculated properties (like path or tags, members, etc..) should be
    ///     recalculated.
    /// </param>
    /// <param name="profileId">The id of the profile whose relation ship to a target has been changed.</param>
    /// <param name="targetId">
    ///     The id of the target that deals as parent of <paramref name="profileId" /> conditionally and
    ///     whose relationship has been activated oder deactivated.
    /// </param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="assignmentIsActive">A boolean flag indicating whether the relation ship is now active or not.</param>
    /// <param name="transaction">
    ///     An optional <see cref="IDatabaseTransaction" /> instance to execute the underlying method
    ///     inside a transaction.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    /// <exception cref="System.ArgumentNullException">
    ///     <paramref name="relatedProfileId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="profileId" /> is <c>null</c><br />-or-<br />
    ///     <paramref name="targetId" /> is <c>null</c><br />-or-<br />
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     <paramref name="relatedProfileId" /> is empty or whitespace<br />-or-<br />
    ///     <paramref name="profileId" /> is empty or whitespace<br />-or-<br />
    ///     <paramref name="targetId" /> is empty or whitespace<br />-or-<br />
    /// </exception>
    Task RecalculateAssignmentsAsync(
        ObjectIdent relatedEntity,
        string profileId,
        string targetId,
        ObjectType targetType,
        bool assignmentIsActive,
        IDatabaseTransaction transaction = default,
        CancellationToken cancellationToken = default);
}
