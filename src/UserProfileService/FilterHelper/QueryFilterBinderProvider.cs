using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace UserProfileService.FilterHelper;

/// <summary>
///     Represents a model binder provider that will provide instances of <see cref="QueryFilterBinder" />. These bind
///     incoming <see cref="Filter" /> model instances.
/// </summary>
public class QueryFilterBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.BindingInfo.BindingSource != null && context.BindingInfo.BindingSource != BindingSource.Query)
        {
            return null;
        }

        return context.Metadata.ModelType == typeof(Filter)
            ? new BinderTypeModelBinder(typeof(QueryFilterBinder))
            : null;
    }
}
