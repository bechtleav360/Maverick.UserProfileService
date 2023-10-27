using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.Models.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="object" />s.
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    ///     Returns the properties of the object associated with the properties of the given type.
    /// </summary>
    /// <typeparam name="TTarget">Type of the object with which the source object is compared.</typeparam>
    /// <param name="object">Object from which the properties are to be returned.</param>
    /// <returns>Dictionary with the properties of the source object. Key is the property name.</returns>
    public static Dictionary<string, object> GetTargetPropertiesAsDictionary<TTarget>(this object @object)
    {
        Dictionary<string, object> properties = @object
            .GetType()
            .GetProperties()
            .Where(p => { return p.GetCustomAttributeValue<ModifiableAttribute, bool>(cp => cp?.AllowEdit ?? false); })
            .ToDictionary(p => p.Name, p => p.GetValue(@object));

        return properties;
    }

    /// <summary>
    ///     Extract a list of properties from the current object.
    /// </summary>
    /// <param name="object">Object to extract the properties from.</param>
    /// <param name="propertyKeys">Property keys to extract from object.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>Dictionary of property key and value.</returns>
    public static IDictionary<string, object> ExtractProperties(
        this object @object,
        IEnumerable<string> propertyKeys,
        ILogger logger = null)
    {
        logger?.EnterMethod();

        var properties = new Dictionary<string, object>();

        foreach (string propertyKey in propertyKeys)
        {
            logger?.LogDebugMessage(
                "Get property value for key {key} and {type}",
                LogHelpers.Arguments(propertyKey, @object.GetType().Name));

            PropertyInfo propertyInfo = @object.GetType().GetProperty(propertyKey);

            if (propertyInfo == null)
            {
                logger?.LogDebugMessage(
                    "No property value for key {key} and {type} found.",
                    LogHelpers.Arguments(propertyKey, @object.GetType().Name));

                continue;
            }

            object propertyValue = propertyInfo.GetValue(@object);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
            {
                logger?.LogDebugMessage(
                    "Found property value for key {key} and {type}. Value: {value}",
                    LogHelpers.Arguments(
                        propertyKey,
                        @object.GetType().Name,
                        JsonConvert.SerializeObject(propertyValue)));
            }
            else
            {
                logger?.LogDebugMessage(
                    "Found property value for key {key} and {type}.",
                    LogHelpers.Arguments(propertyKey, @object.GetType().Name));
            }

            properties.Add(propertyKey, propertyValue);
        }

        return logger.ExitMethod(properties);
    }
}
