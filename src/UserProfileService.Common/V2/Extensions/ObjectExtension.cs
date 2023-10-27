using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="object" />s.
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    ///     The given object is converted to the given type.
    /// </summary>
    /// <typeparam name="T">Type of the target object into which the source object should be converted. </typeparam>
    /// <param name="object">Object to be converted.</param>
    /// <param name="convertedObject">The converted object if no conversion could be performed will return null.</param>
    /// <returns>True if object was converted, otherwise false.</returns>
    public static bool TryConvertObject<T>(this object @object, out T convertedObject)
    {
        convertedObject = @object.TryConvertObject<T>();

        return convertedObject != null;
    }

    /// <summary>
    ///     The given object is converted to the given type.
    /// </summary>
    /// <typeparam name="T">Type of the target object into which the source object should be converted. </typeparam>
    /// <param name="object">Object to be converted.</param>
    /// <returns>The converted object if no conversion could be performed will return null.</returns>
    public static T TryConvertObject<T>(this object @object, ILogger logger = null)
    {
        logger?.EnterMethod();

        if (@object == null)
        {
            throw new ArgumentNullException(nameof(@object));
        }

        try
        {
            Type conversionType = typeof(T);

            switch (@object)
            {
                // Check if value is an object like JArray and is convertible to property type.
                case JContainer jsonValue:
                    logger?.LogTraceMessage("Object to convert is of type 'JContainer'.", LogHelpers.Arguments());

                    return jsonValue.ToObject<T>();
                case IConvertible ic:
                    logger?.LogTraceMessage("Object to convert is of type 'IConvertible'.", LogHelpers.Arguments());

                    return (T)Convert.ChangeType(@object, conversionType);
            }

            if (@object.GetType() == conversionType)
            {
                logger?.LogTraceMessage(
                    "Object to convert is of type '{type}'.",
                    LogHelpers.Arguments(conversionType.Name));

                return (T)@object;
            }

            return default;
        }
        catch (Exception e)
        {
            logger?.LogErrorMessage(
                e,
                "An error occurred while converting object to type {type}.",
                LogHelpers.Arguments(typeof(T).Name));

            return default;
        }
    }

    /// <summary>
    ///     Clone a object by using json converter.
    /// </summary>
    /// <typeparam name="T">Type of object to clone.</typeparam>
    /// <param name="object">Object to clone.</param>
    /// <returns>Cloned object.</returns>
    public static T CloneJson<T>(this T @object)
    {
        string jsonStr = JsonConvert.SerializeObject(@object);

        return JsonConvert.DeserializeObject<T>(jsonStr);
    }
}
