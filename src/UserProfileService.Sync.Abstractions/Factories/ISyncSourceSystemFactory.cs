using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Systems;

namespace UserProfileService.Sync.Abstraction.Factories;

/// <summary>
///     Describes the implementation of a factory to create instances of <see cref="ISynchronizationSourceSystem{T}" />.
/// </summary>
public interface ISyncSourceSystemFactory
{
    /// <summary>
    ///     Creates an instance of <see cref="ISynchronizationSourceSystem{T}" /> for the given type.
    /// </summary>
    /// <typeparam name="T">Type to create an instance of <see cref="ISynchronizationSourceSystem{T}" /> for.</typeparam>
    /// <param name="sourceSystem"></param>
    /// <returns>Instance of <see cref="ISynchronizationSourceSystem{T}" /></returns>
    ISynchronizationSourceSystem<T> Create<T>(string sourceSystem)
        where T : ISyncModel;
}
