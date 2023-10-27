using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Models;

namespace UserProfileService.Adapter.Marten.Options;

/// <summary>
///     Validates an parses an query options to another internal query options object.
/// </summary>
public interface IOptionsValidateParser
{
    /// <summary>
    ///     Validates an parses a <see cref="QueryOptions" /> to an internal query options mode
    ///     <see cref="QueryOptionsVolatileModel" /> that can be used to query the database.
    /// </summary>
    /// <param name="options">The query options that should be parser and validated.</param>
    /// <typeparam name="TResult">The result object that is used to validate if a property is part of the object.</typeparam>
    /// <returns>Returns a <see cref="QueryOptionsVolatileModel" /> that is used to filter the result set of volatile date.</returns>
    QueryOptionsVolatileModel ParseAndValidateQueryOptions<TResult>(QueryOptions options);
}
