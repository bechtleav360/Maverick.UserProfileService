using System;
using System.Linq;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstractions;

namespace UserProfileService.Sync.Extensions;

internal static class TypeAttributeExtension
{
    /// <summary>
    ///     Returns the next saga message type that has to be send
    ///     to reach the next saga step.
    /// </summary>
    /// <param name="sagaStepName">The type for the saga step.</param>
    /// <returns>The saga message to a specific saga step.</returns>
    public static Type GetSagaMessageType(this string sagaStepName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(ISyncMessage).IsAssignableFrom(p))
            .FirstOrDefault(
                t =>
                {
                    return string.Equals(
                        t.GetCustomAttributeValue<StateStepAttribute, string>(ssa => ssa.SagaStepName),
                        sagaStepName,
                        StringComparison.InvariantCultureIgnoreCase);
                });
    }

    /// <summary>
    ///     Return the <see cref="ModelAttribute" /> of given type.
    /// </summary>
    /// <param name="type">Type to retrieve attribute for.</param>
    /// <returns> <see cref="ModelAttribute" /> of given type.</returns>
    public static ModelAttribute GetSyncModelAttribute(this Type type)
    {
        return type
            .GetCustomAttributeValue<ModelAttribute, ModelAttribute>(t => t);
    }
}
