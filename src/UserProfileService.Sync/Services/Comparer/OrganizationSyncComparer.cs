using System.Reflection;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Implementation of <see cref="ISyncModelComparer{TSyncModel}" />.
/// </summary>
public class OrganizationSyncComparer : BaseSyncComparer<OrganizationSync, OrganizationBasic>
{
    /// <summary>
    ///     Create an instance of <see cref="OrganizationSyncComparer" />
    /// </summary>
    /// <param name="loggerFactory">
    ///     Represents a type used to configure the logging system and create instances of
    ///     <see cref="ILogger" />.
    /// </param>
    public OrganizationSyncComparer(ILoggerFactory loggerFactory) : base(
        loggerFactory)
    {
    }

    /// <inheritdoc />
    protected override bool ComparePropertyValue(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue)
    {
        if (propertyInfo.Name == nameof(OrganizationBasic.Weight)
            || propertyInfo.Name == nameof(OrganizationBasic.IsSystem))
        {
            if (targetValue == null)
            {
                return true;
            }
        }

        return Equals(sourceValue, targetValue);
    }
}
