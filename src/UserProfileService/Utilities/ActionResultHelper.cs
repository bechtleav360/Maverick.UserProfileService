using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Abstractions;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.TicketStore.Models;
using UserProfileService.Extensions;
using StringExtensions = UserProfileService.Api.Common.Extensions.StringExtensions;

namespace UserProfileService.Utilities;

public static class ActionResultHelper
{
    internal static string GenerateRouteUrl(
        this OperationMap map,
        UserProfileOperationTicket ticket,
        IUrlHelper urlHelper,
        ILogger logger)
    {
        logger.EnterMethod();

        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        if (ticket == null)
        {
            throw new ArgumentNullException(nameof(ticket));
        }

        if (urlHelper == null)
        {
            throw new ArgumentNullException(nameof(urlHelper));
        }

        logger.LogInfoMessage("Generating result url for ticket {ticketId}.", LogHelpers.Arguments(ticket.Id));

        string template = map.Controller?.GetMethod(map.ActionName)
            ?.GetCustomAttribute<HttpGetAttribute>()
            ?.Template;

        logger.LogInfoMessage("Template has the value: {template}", LogHelpers.Arguments(template));

        var dictionary = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(template))
        {
            string[] tokens = Regex.Matches(template, @"\{[a-zA-Z0-9]+\}")
                .Select(x => x.Value.Trim('{', '}'))
                .ToArray();

            logger.LogTraceMessage(
                "Found {length} tokens ({tokens}).",
                LogHelpers.Arguments(tokens.Length, string.Join(',', tokens)));

            for (int tokenCounter = 0, objectIdCounter = 0; tokenCounter < tokens.Length; tokenCounter++)
            {
                if (tokens[tokenCounter] == "controller" || tokens[tokenCounter] == "version")
                {
                    continue;
                }

                logger.LogTraceMessage(
                    "Set [{tokenCounter}] to {objectIds}",
                    LogHelpers.Arguments(tokens[tokenCounter], ticket.ObjectIds[objectIdCounter]));

                dictionary.Add(tokens[tokenCounter], ticket.ObjectIds[objectIdCounter]);
                objectIdCounter++;
            }
        }

        string controllerToken = map.Controller?.Name.TrimEnd("Controller").ToLowerInvariant();
        logger.LogTraceMessage("Set [controller] to {controllerToken}", LogHelpers.Arguments(controllerToken));
        dictionary.AddOrUpdate("controller", controllerToken);
        dictionary.AddOrUpdate("version", "2");

        string fullTemplate = map.Controller?.GetCustomAttribute<RouteAttribute>()
                ?
                .Template.Replace("[controller]", controllerToken)
            + "/"
            + template;

        logger.LogInfoMessage("The template for the url: {fullTemplate}.", LogHelpers.Arguments(fullTemplate));

        string resultUrl = urlHelper.ActionContext.HttpContext.Request.Scheme
            + "://"
            + urlHelper.ActionContext.HttpContext.Request.Host
            + "/"
            + StringExtensions.StringFormat(fullTemplate, dictionary)
            + (ticket.AdditionalQueryParameter == null
                ? string.Empty
                : $"?Filter={ticket.AdditionalQueryParameter}");

        logger.LogInfoMessage("Generated Url {resultUrl}.", LogHelpers.Arguments(resultUrl));

        return logger.ExitMethod<string>(resultUrl);
    }

    /// <summary>
    ///     Generates an <see cref="IActionResult" /> based on <paramref name="value" />.
    ///     If <paramref name="value" /> is <c>null</c> a <see cref="NoContentResult" /> will be returned.
    ///     Otherwise an <see cref="ObjectResult" /> containing <paramref name="value" /> with status
    ///     <paramref name="status" /> will be returned.
    /// </summary>
    /// <typeparam name="T">Specifies the type of <paramref name="value" />.</typeparam>
    /// <param name="value">Contains the value to wrap in an <see cref="IActionResult" />.</param>
    /// <param name="status">Defines the status to use if <paramref name="value" /> is not null.</param>
    /// <returns>An <see cref="IActionResult" /> containing <paramref name="value" />.</returns>
    public static IActionResult ToActionResult<T>(T value, HttpStatusCode status = HttpStatusCode.OK)
    {
        if (value == null)
        {
            return new NoContentResult();
        }

        return new ObjectResult(value)
        {
            StatusCode = (int)status
        };
    }

    public static async Task<IActionResult> GetAcceptedAtStatusResultAsync(
        Func<Task<string>> function,
        ILogger logger,
        [CallerMemberName] string callerName = null)
    {
        logger.EnterMethod();

        string ticketId = await function();

        logger.LogInfoMessage("Created ticket with the ticket id: {ticketId}.", LogHelpers.Arguments(ticketId));

        return new AcceptedAtRouteResult(
            "GetStatus",
            new
            {
                id = ticketId,
                controller = "Status",
                version = "2"
            },
            null);
    }

    /// <summary>
    ///     Runs the task <paramref name="data" /> and returns its value. If an exception occurs the default value will be
    ///     returned.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="data">The function which retrieves the data.</param>
    /// <param name="logger">A <see cref="ILogger" /> which is used in order to log the exception which occurred.</param>
    /// <param name="defaultValue">
    ///     Specifies the value to return if an exception was thrown during the invocation of
    ///     <paramref name="data" />.
    /// </param>
    /// <param name="callerName">Specifies the name of the method which is calling the method.</param>
    /// <returns>Either the value of <paramref name="data" /> or <paramref name="defaultValue" />.</returns>
    public static async Task<T> Save<T>(
        Func<Task<T>> data,
        ILogger logger,
        T defaultValue = default,
        [CallerMemberName] string callerName = null)
    {
        try
        {
            return await data();
        }
        catch (Exception ex)
        {
            logger.LogErrorMessage(ex, "{exceptionMessage}", LogHelpers.Arguments(ex.Message));

            return defaultValue;
        }
    }
}
