using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Maverick.UserProfileService.Models.BasicModels;
using Microsoft.Extensions.Logging;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Services.Comparer;

/// <summary>
///     Implementation of <see cref="ISyncModelComparer{TSyncModel}" />.
/// </summary>
public class FunctionSyncComparer : BaseSyncComparer<FunctionSync, FunctionBasic>
{
    /// <summary>
    ///     Create an instance of <see cref="FunctionSyncComparer" />
    /// </summary>
    /// <param name="loggerFactory">
    ///     Represents a type used to configure the logging system and create instances of
    ///     <see cref="ILogger" />.
    /// </param>
    public FunctionSyncComparer(ILoggerFactory loggerFactory) : base(
        loggerFactory)
    {
    }

    /// <inheritdoc />
    protected override bool ComparePropertyValue(
        PropertyInfo propertyInfo,
        object sourceValue,
        object targetValue)
    {
        if (propertyInfo.Name == nameof(FunctionBasic.OrganizationId))
        {
            IOrderedEnumerable<string> sourceList = ((IList<string>)sourceValue).OrderBy(k => k);
            IOrderedEnumerable<string> targetList = ((IList<string>)targetValue).OrderBy(k => k);

            return sourceList.SequenceEqual(targetList);
        }

        return Equals(sourceValue, targetValue);
    }
}
