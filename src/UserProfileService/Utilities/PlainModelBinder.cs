using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UserProfileService.Utilities;

/// <summary>
///     Model binder to bind the whole request body to a string parameter
/// </summary>
public class PlainModelBinder : IModelBinder
{
    /// <summary>
    ///     Binds the whole request body to the bound parameter which has to be of type string
    /// </summary>
    /// <param name="bindingContext">
    ///     <see cref="ModelBindingContext" />
    /// </param>
    /// <returns></returns>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.ModelType != typeof(string))
        {
            throw new InvalidCastException($"Cannot bind plain to type {bindingContext.ModelType}");
        }

        bindingContext.HttpContext.Request.EnableBuffering();
        using var sr = new StreamReader(bindingContext.HttpContext.Request.Body, Encoding.UTF8);
        bindingContext.Result = ModelBindingResult.Success(await sr.ReadToEndAsync());
    }
}
