using System;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.Common.Extensions;

/// <summary>
///     Extends model classes related to projections.
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    ///     Generate the function name using the <paramref name="function" /> instance. It won't use the name property itself.
    /// </summary>
    /// <param name="function">The function whose name should be returned.</param>
    /// <returns>The generated function name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="function" /> is <c>null</c></exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="function" /> contains a role property that is <c>null</c><br />-or-<br />
    ///     <paramref name="function" /> contains a organization property that is <c>null</c>.
    /// </exception>
    public static string GenerateFunctionName(this SecondLevelProjectionFunction function)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (function.Role == null)
        {
            throw new ArgumentException(
                "The role in function must not be null.",
                nameof(function));
        }

        if (function.Organization == null)
        {
            throw new ArgumentException(
                "The organization in function must not be null.",
                nameof(function));
        }

        return $"{function.Organization.Name} {function.Role.Name}";
    }

    /// <summary>
    ///     Generate the function name using the <paramref name="function" /> instance. It won't use the name property itself.
    /// </summary>
    /// <param name="function">The function whose name should be returned.</param>
    /// <returns>The generated function name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="function" /> is <c>null</c></exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="function" /> contains a role property that is <c>null</c><br />-or-<br />
    ///     <paramref name="function" /> contains a organization property that is <c>null</c>.
    /// </exception>
    public static string GenerateFunctionName(this Function function)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (function.Role == null)
        {
            throw new ArgumentException(
                "The role in function must not be null.",
                nameof(function));
        }

        if (function.Organization == null)
        {
            throw new ArgumentException(
                "The organization in function must not be null.",
                nameof(function));
        }

        return $"{function.Organization.Name} {function.Role.Name}";
    }

    /// <summary>
    ///     Generate the function name using the <paramref name="functionCreated" /> instance. It won't use the name property
    ///     itself.
    /// </summary>
    /// <param name="functionCreated">The function whose name should be returned.</param>
    /// <returns>The generated function name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="functionCreated" /> is <c>null</c></exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="functionCreated" /> contains a role property that is <c>null</c><br />-or-<br />
    ///     <paramref name="functionCreated" /> contains a organization property that is <c>null</c>.
    /// </exception>
    public static string GenerateFunctionName(this FunctionCreated functionCreated)
    {
        if (functionCreated == null)
        {
            throw new ArgumentNullException(nameof(functionCreated));
        }

        if (functionCreated.Role == null)
        {
            throw new ArgumentException(
                "The role in function created must not be null.",
                nameof(functionCreated));
        }

        if (functionCreated.Organization == null)
        {
            throw new ArgumentException(
                "The organization in function created must not be null.",
                nameof(functionCreated));
        }

        return $"{functionCreated.Organization.Name} {functionCreated.Role.Name}";
    }
}
