using Maverick.UserProfileService.FilterUtility.Abstraction;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserProfileService.FilterHelper;

/// <summary>
///     The model binder for <see cref="Filter" />
/// </summary>
public class QueryFilterBinder : IModelBinder
{
    private readonly IFilterUtility<Filter> _filterSerializer;

    /// <summary>
    ///     Initializes a model binder for <see cref="Filter" />
    /// </summary>
    /// <param name="filterSerializer">Filter serializer for <see cref="Filter" /></param>
    public QueryFilterBinder(IFilterUtility<Filter> filterSerializer)
    {
        _filterSerializer = filterSerializer;
    }

    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        ValueProviderResult filterString = bindingContext.ValueProvider.GetValue(
            bindingContext.ModelMetadata.Name ?? string.Empty);

        if (string.IsNullOrEmpty(filterString.ToString()))
        {
            return Task.CompletedTask;
        }

        Filter filter;

        try
        {
            filter = _filterSerializer.Deserialize(filterString.ToString());
        }
        catch (AggregateException aggEx)
        {
            if (aggEx.InnerExceptions.Count == 1)
            {
                bindingContext.ModelState.TryAddModelError("Query filter", aggEx.InnerExceptions[0].Message);

                return Task.CompletedTask;
            }

            var number = 1;

            foreach (Exception innerException in aggEx.InnerExceptions)
            {
                bindingContext.ModelState.TryAddModelError($"Query filter #{number++}", innerException.Message);
            }

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            bindingContext.ModelState.TryAddModelError("Query filter", e.Message);

            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(filter);

        return Task.CompletedTask;
    }
}
