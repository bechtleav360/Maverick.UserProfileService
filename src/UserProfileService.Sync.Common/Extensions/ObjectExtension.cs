using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Sync.Common.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="object" />s.
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    ///     Overwrites the properties of the entity with the given properties.
    /// </summary>
    /// <param name="object">Entity for which the properties are to be overwritten</param>
    /// <param name="properties">Properties that has been changed.</param>
    public static void UpdateProperties(this object @object, IDictionary<string, object> properties)
    {
        foreach (KeyValuePair<string, object> property in properties)
        {
            // The same mechanism for checking properties is used in the saga worker.
            PropertyInfo propertyInfo = @object.GetType().GetProperty(property.Key);

            if (propertyInfo == null)
            {
                continue;
            }

            if (property.Value is JContainer jsonValue)
            {
                propertyInfo.SetValue(@object, jsonValue.ToObject(propertyInfo.PropertyType), null);

                continue;
            }

            propertyInfo.SetValue(@object, Convert.ChangeType(property.Value, propertyInfo.PropertyType), null);
        }
    }
}
