using System.Collections.Generic;

namespace UserProfileService.Projection.SecondLevel.Tests.Extensions;

public static class DictionaryExtensions
{
    public static IDictionary<string, object> AddChange(
        this IDictionary<string, object> target,
        string propertyName,
        object newValue)
    {
        if (target.ContainsKey(propertyName))
        {
            return target;
        }

        target.Add(propertyName, newValue);

        return target;
    }
}