using System.Reflection;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Implementation of <see cref="ISyncModelComparer{TSyncModel}" />.
/// </summary>
public class GroupSyncComparer : BaseSyncComparer<GroupSync, GroupBasic>
{
    /// <summary>
    ///     Create an instance of <see cref="GroupSyncComparer" />
    /// </summary>
    /// <param name="loggerFactory">
    ///     Represents a type used to configure the logging system and create instances of
    ///     <see cref="ILogger" />.
    /// </param>
    public GroupSyncComparer(ILoggerFactory loggerFactory) : base(
        loggerFactory)
    {
    }

    /// <inheritdoc />
    protected override bool ComparePropertyValue(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue)
    {
        if (propertyInfo.Name == nameof(GroupBasic.Weight) || propertyInfo.Name == nameof(GroupBasic.IsSystem))
        {
            if (targetValue == null)
            {
                return true;
            }
        }

        return Equals(sourceValue, targetValue);
    }
}
