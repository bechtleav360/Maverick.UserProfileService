using System.Reflection;
using Maverick.UserProfileService.Models.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using UserProfileService.Api.Common.Exceptions;


namespace UserProfileService.Api.Common.Extensions;

/// <summary>
///     Extensions to generate urls
/// </summary>
public static class UrlExtensions
{
    private static string RemoveControllerFromUrl(this string url)
    {
        const string apiPrefix = "/api/v2/";
        var uri = new Uri(url);
        int indexOfApi = uri.AbsolutePath.IndexOf(apiPrefix, StringComparison.InvariantCulture);

        if (indexOfApi == -1)
        {
            throw new NotSupportedException($"Url must contain {apiPrefix} but is {url}");
        }

        string path = uri.AbsolutePath[..(indexOfApi + apiPrefix.Length)];

        var uriBuilder = new UriBuilder(uri)
        {
            Path = path,
            Query = string.Empty
        };

        return uriBuilder.Uri.ToString();
    }

    /// <summary>
    ///     Fills every property of type <see cref="string" /> or <see cref="Uri" /> in the object if marked with an
    ///     <see cref="UriRedirectionAttribute" />
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <param name="o">The source object</param>
    /// <param name="context">The related action context for url resolving</param>
    /// <returns>Returns source object with resolved url properties</returns>
    public static T ResolveUrlProperties<T>(this T o, ActionContext context)
    {
        if (o == null)
        {
            throw new ArgumentNullException(nameof(o));
        }

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        Type type = o.GetType();

        List<PropertyInfo> redirectionProperties = type.GetProperties()
            .Where(
                x => x.CustomAttributes.Any(
                    y => y.AttributeType
                        == typeof(UriRedirectionAttribute)))
            .ToList();

        if (!redirectionProperties.Any())
        {
            return o;
        }

        IUrlHelper urlHelper = new UrlHelperFactory().GetUrlHelper(context);

        string routeUrl = urlHelper.RouteUrl(
            new UrlRouteContext
            {
                Host = urlHelper.ActionContext.HttpContext.Request.Host.Value,
                Protocol = urlHelper.ActionContext.HttpContext.Request.Scheme
            })!;

        Uri baseUrl = new Uri(routeUrl.RemoveControllerFromUrl(), UriKind.Absolute);

        foreach (PropertyInfo redirectionProperty in redirectionProperties)
        {
            string relativeUrl =
                redirectionProperty.GetCustomAttribute<UriRedirectionAttribute>()?.Pattern.FormatWith(o)
                ?? string.Empty;

            if (redirectionProperty.PropertyType == typeof(string))
            {
                redirectionProperty.SetValue(o, new Uri(baseUrl, relativeUrl).AbsoluteUri);
            }
            else if (redirectionProperty.PropertyType == typeof(Uri))
            {
                redirectionProperty.SetValue(o, new Uri(baseUrl, relativeUrl));
            }
            else
            {
                throw new InvalidFieldTypeException(
                    $"Unable to Resolve Url for Property {redirectionProperty.Name} (Invalid type {redirectionProperty.PropertyType})");
            }
        }

        return o;
    }

    /// <summary>
    ///     Fills every property of type <see cref="string" /> or <see cref="Uri" /> in the object if marked with an
    ///     <see cref="UriRedirectionAttribute" />
    /// </summary>
    /// <typeparam name="T">Source type</typeparam>
    /// <param name="o">The source object</param>
    /// <param name="context">The related http context for url resolving</param>
    /// <returns>Returns source object with resolved url properties</returns>
    public static T ResolveUrlProperties<T>(this T o, HttpContext context)
    {
        return o.ResolveUrlProperties(new ActionContext(context, context.GetRouteData(), new ActionDescriptor()));
    }
}
