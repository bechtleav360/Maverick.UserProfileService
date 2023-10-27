using System;
using System.Linq.Expressions;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.Abstractions;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Resolves the count property of children of a group. Organizations should be filtered out in this context.
/// </summary>
internal class GroupChildrenCountVirtualPropertyResolver : IVirtualPropertyResolver
{
    /// <summary>
    ///     The name of the conversion method. The original data will be a list of objects, the virtual property is a an
    ///     integer. That's why it is set.
    /// </summary>
    public string Conversion => WellKnownFilterProperties.CountProperty;

    /// <inheritdoc cref="IVirtualPropertyResolver" />
    public LambdaExpression GetFilterExpression()
    {
        Expression<Func<IProfile, bool>> temp = p => p.Kind == ProfileKind.Group || p.Kind == ProfileKind.User;

        return temp;
    }

    /// <inheritdoc cref="IVirtualPropertyResolver" />
    public Type GetReturnType()
    {
        return typeof(int);
    }
}
