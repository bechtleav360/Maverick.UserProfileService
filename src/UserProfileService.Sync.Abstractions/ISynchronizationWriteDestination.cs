using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstraction;

/// <summary>
///     This interface is used to write objects in the destination target system.
/// </summary>
public interface ISynchronizationWriteDestination<TSource>
{
    /// <summary>
    ///     Create an objects if it does not exists in the destination system.
    /// </summary>
    /// <param name="sourceObject">The source object that should be created.</param>
    /// <param name="collectingId">
    ///     Id of the current collection process. It is used to assign the response to the
    ///     synchronization process.
    /// </param>
    /// <param name="token">The token is used for cancel the stated task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps objects of type
    ///     <cref>TSource.</cref>
    /// </returns>
    Task<CommandResult> CreateObjectAsync(
        Guid collectingId,
        TSource sourceObject,
        CancellationToken token);

    /// <summary>
    ///     Updates an objects if it has change in the destination system.
    /// </summary>
    /// <param name="collectingId">
    ///     Id of the current collection process. It is used to assign the response to the
    ///     synchronization process.
    /// </param>
    /// <param name="sourceObject">The source object that should be updated.</param>
    /// <param name="modifiedProperties">List of modified properties.</param>
    /// <param name="token">The token is used for cancel the stated task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps objects of type
    ///     <cref>TSource.</cref>
    ///     .
    /// </returns>
    Task<CommandResult> UpdateObjectAsync(
        Guid collectingId,
        TSource sourceObject,
        ISet<string> modifiedProperties,
        CancellationToken token);

    /// <summary>
    ///     Delete all objects that are not in the source system.
    /// </summary>
    /// <param name="collectingId">
    ///     Id of the current collection process. It is used to assign the response to the
    ///     synchronization process.
    /// </param>
    /// <param name="objectsToDelete">The objects that should be deleted.</param>
    /// <param name="sourceSystem">The system the source object based on.</param>
    /// <param name="token">The token is used for cancel the stated task.</param>
    /// <returns>
    ///     A task that represents the asynchronous read operation. It wraps objects of type
    ///     <cref>TSource</cref>
    /// </returns>
    Task<IList<CommandResult>> DeleteObjectsAsync(
        Guid collectingId,
        IList<TSource> objectsToDelete,
        string sourceSystem,
        CancellationToken token);

    /// <summary>
    ///     Creates all relations that are found while synchronizing the objects.
    /// </summary>
    /// <param name="collectingId">Id of the current process. It is used to assign the response to the synchronization process. </param>
    /// <param name="relations">A list of relations that has to be created.</param>
    /// <param name="delete">Specifies if the relations has to be deleted or created.</param>
    /// <param name="token">The token is used for cancel the stated Task. </param>
    /// <returns>A task that represents the asynchronous read operation. It wraps objects of type <inheritdoc cref="object" />.</returns>
    Task<IList<RelationProcessingObject>> HandleRelationsAsync(
        Guid collectingId,
        IList<IRelation> relations,
        bool delete = false,
        CancellationToken token = default);
}
