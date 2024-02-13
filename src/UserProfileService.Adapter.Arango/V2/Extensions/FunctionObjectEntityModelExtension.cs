using System;
using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Extension method for the <see cref="FunctionObjectEntityModel" />.
/// </summary>
public static class FunctionObjectEntityModelExtension
{
    /// <summary>
    ///     Updates <see cref="FunctionObjectEntityModel" /> with the <see cref="SecondLevelProjectionFunction" />.
    ///     Only the Organization, Role, Source, Name, ExternalIds and UpdateAt property has to be updated.
    /// </summary>
    /// <param name="functionModel">The model that has to be updated.</param>
    /// <param name="function">
    ///     The function that should update the
    ///     <paramref name="functionModel"/>
    /// </param>
    /// <param name="mapper">The mapper is used to map complex objects.</param>
    /// <returns>The updated <see cref="FunctionObjectEntityModel" />.</returns>
    internal static FunctionObjectEntityModel UpdateFunctionModel(
        this FunctionObjectEntityModel functionModel,
        SecondLevelProjectionFunction function,
        IMapper mapper)
    {
        if (functionModel == null)
        {
            throw new ArgumentNullException(nameof(functionModel));
        }

        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (mapper == null)
        {
            throw new ArgumentNullException(nameof(mapper));
        }

        functionModel.Organization = mapper.Map<OrganizationBasic>(function.Organization);
        functionModel.Role = mapper.Map<RoleBasic>(function.Role);
        functionModel.Source = function.Source;
        functionModel.Name = $"{function.Organization.Name} {function.Role.Name}";
        functionModel.ExternalIds = mapper.Map<ExternalIdentifier[]>(function.ExternalIds);
        functionModel.UpdatedAt = function.UpdatedAt;

        return functionModel;
    }

    /// <summary>
    ///     The Method is used to generate the name for the <see cref="FunctionObjectEntityModel" />.
    /// </summary>
    /// <param name="function">The function whose name has to be generated.</param>
    /// <returns>The generate function name.</returns>
    /// <exception cref="ArgumentNullException">If the function, the organization, or the role of the function is null.</exception>
    internal static string GenerateFunctionModelName(
        this FunctionObjectEntityModel function)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (function.Role == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        if (function.Organization == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        return $"{function.Organization.Name} {function.Role.Name}";
    }
}
