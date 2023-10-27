using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.Models.Annotations;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Abstract class to compare objects of type
///     <see>
///         <cref>T</cref>
///     </see>
/// </summary>
/// <typeparam name="T">Type of objects to compare.</typeparam>
/// <typeparam name="TModifiable">
///     Type of modifiable object of type
///     <see>
///         <cref>T</cref>
///     </see>
/// </typeparam>
public abstract class BaseSyncComparer<T, TModifiable> : ISyncModelComparer<T> where T : ISyncModel
{
    /// <summary>
    ///     The logger for logging purposes.
    /// </summary>
    protected readonly ILogger<BaseSyncComparer<T, TModifiable>> Logger;

    /// <summary>
    ///     Create an instance of <see cref="BaseSyncComparer{T,TModifiable}" />
    /// </summary>
    /// <param name="factory">
    ///     Represents a type used to configure the logging system and create instances of
    ///     <see cref="ILogger" />
    /// </param>
    protected BaseSyncComparer(ILoggerFactory factory)
    {
        Logger = factory.CreateLogger<BaseSyncComparer<T, TModifiable>>();
    }

    private static bool IsChangeableProperty(PropertyInfo propertyInfo)
    {
        PropertyInfo property = typeof(TModifiable).GetProperty(propertyInfo.Name);

        return property != null
            && property.CanRead
            && property.GetCustomAttributeValue<ModifiableAttribute, bool>(cp => cp?.AllowEdit ?? false);
    }

    private bool ComparePropertyValueInternal(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue)
    {
        if (propertyInfo.Name == nameof(ISyncModel.ExternalIds))
        {
            IOrderedEnumerable<KeyProperties> sourceList = ((IList<KeyProperties>)sourceValue).OrderBy(k => k.Id);
            IOrderedEnumerable<KeyProperties> targetList = ((IList<KeyProperties>)targetValue).OrderBy(k => k.Id);

            return sourceList.SequenceEqual(targetList, new KeyPropertiesEqualityComparer());
        }

        return ComparePropertyValue(propertyInfo, sourceValue, targetValue);
    }

    /// <summary>
    ///     Compare the source value with the target value.
    /// </summary>
    /// <param name="propertyInfo">The property info for the property name.</param>
    /// <param name="sourceValue">The source value that should be compared.</param>
    /// <param name="targetValue">The target value that should be compared.</param>
    /// <returns>True if the values are equal, otherwise false.</returns>
    protected abstract bool ComparePropertyValue(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue);

    /// <inheritdoc />
    public bool CompareObject(
        T source,
        T target,
        out IDictionary<string, object> modifiedProperties)
    {
        modifiedProperties = new Dictionary<string, object>();

        if (source == null && target == null)
        {
            Logger.LogDebugMessage(
                "Source and target object of type '{type}' are null.",
                LogHelpers.Arguments(nameof(T)));

            return true;
        }

        if (source == null || target == null)
        {
            Logger.LogDebugMessage(
                "Source or target object of type '{type}' is null.",
                LogHelpers.Arguments(nameof(T)));

            return false;
        }

        foreach (PropertyInfo propertyInfo in source.GetType().GetProperties())
        {
            if (IsChangeableProperty(propertyInfo))
            {
                object sourceValue = propertyInfo.GetValue(source, null);
                object targetValue = propertyInfo.GetValue(target, null);

                if (!ComparePropertyValueInternal(propertyInfo, sourceValue, targetValue))
                {
                    modifiedProperties.Add(propertyInfo.Name, targetValue);
                }
            }
        }

        return !modifiedProperties.Keys.Any();
    }
}
