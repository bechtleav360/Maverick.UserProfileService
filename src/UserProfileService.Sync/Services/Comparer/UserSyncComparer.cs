using System;
using System.Reflection;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Implementation of <see cref="ISyncModelComparer{TSyncModel}" />.
/// </summary>
public class UserSyncComparer : BaseSyncComparer<UserSync, UserBasic>
{
    /// <summary>
    ///     Create an instance of <see cref="UserSyncComparer" />
    /// </summary>
    /// <param name="loggerFactory">
    ///     Represents a type used to configure the logging system and create instances of
    ///     <see cref="ILogger" />.
    /// </param>
    public UserSyncComparer(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    /// <inheritdoc />
    protected override bool ComparePropertyValue(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue)
    {
        // Email check to lower case if both values are string
        if (propertyInfo.Name == nameof(UserBasic.Email)
            && sourceValue is string sourceStr
            && targetValue is string targetStr)
        {
            return sourceStr.Equals(targetStr, StringComparison.OrdinalIgnoreCase);
        }

        return Equals(sourceValue, targetValue);
    }
}
