using System;
using System.Linq.Expressions;

namespace UserProfileService.Adapter.Arango.V2.Abstractions;

/// <summary>
///     Contains information to filter data for virtual properties.
/// </summary>
internal interface IVirtualPropertyResolver
{
    /// <summary>
    ///     The name of the conversion method (like 'Count').
    /// </summary>
    string Conversion { get; }

    /// <summary>
    ///     Get the appropriate filter expression valid in the context of the resolver.
    /// </summary>
    /// <returns>The appropriate lambda expression that contains the filtering information.</returns>
    LambdaExpression GetFilterExpression();

    /// <summary>
    ///     The type of the returned object (in a conversion it will be different than the original property).
    /// </summary>
    /// <returns></returns>
    Type GetReturnType();
}
