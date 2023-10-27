using System;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Validation.Abstractions;

namespace UserProfileService.Sync.Projection.Abstractions;

/// <summary>
///     Defines the service to handle function operations.
/// </summary>
public interface IFunctionService
{
    /// <summary>
    ///     Get all functions.
    /// </summary>
    /// <remarks>
    ///     All found instances of functions will be converted to their estimated types<br />
    ///     (given by <typeparamref name="TFunction" />)
    /// </remarks>
    /// <typeparam name="TFunction">
    ///     The type of each element in the result set (either <see cref="FunctionSync" /> or
    ///     inherited from <see cref="FunctionSync" />).
    /// </typeparam>
    /// <param name="options">
    ///     Options to refine the search request and to set up pagination and sorting. If <c>null</c>, the
    ///     default values of pagination will be used.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps a list of found
    ///     <typeparamref name="TFunction" />s. If no functions have been found, an empty list will be returned.
    /// </returns>
    /// <exception cref="ValidationException">If <paramref name="options" /> is not valid.</exception>
    Task<IPaginatedList<TFunction>> GetFunctionsAsync<TFunction>(
        AssignmentQueryObject options = null,
        CancellationToken cancellationToken = default)
        where TFunction : FunctionSync;

    /// <summary>
    ///     Creates the provided function in the database
    /// </summary>
    /// <param name="function">   The <see cref="FunctionSync" /> that is being created in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <see cref="FunctionSync" />.
    /// </returns>
    Task<FunctionSync> CreateFunctionAsync(FunctionSync function, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the provided function in the database
    /// </summary>
    /// <param name="function">   The <see cref="FunctionSync" /> that is being updated in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <see cref="FunctionSync" />.
    /// </returns>
    Task<FunctionSync> UpdateFunctionAsync(FunctionSync function, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the function corresponding to the provided id in the database
    /// </summary>
    /// <param name="functionId">   The id of the <see cref="FunctionSync" /> that is being deleted in the system</param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>A task representing the asynchronous read operation.</returns>
    Task DeleteFunctionAsync(string functionId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get the function corresponding to  the provided id in the database
    /// </summary>
    /// <param name="functionId">   The id of the <see cref="FunctionSync" /></param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests. The default value is
    ///     <see cref="CancellationToken.None" />
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous read operation, containing the created operation of type
    ///     <see cref="FunctionSync" />.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="functionId" /> is empty or contains only whitespace characters.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="functionId" /> is <c>null</c>.</exception>
    /// <exception cref="InstanceNotFoundException">No function can be found whose key equals <paramref name="functionId" />.</exception>
    Task<FunctionSync> GetFunctionAsync(string functionId, CancellationToken cancellationToken = default);
}
