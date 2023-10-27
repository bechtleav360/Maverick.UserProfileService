using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Describes the factory to create instances of
/// </summary>
public interface ISyncModelComparerFactory
{
    /// <summary>
    ///     Create an instance of <see cref="ISyncModelComparer{TSyncModel}" /> for given sync model type.
    /// </summary>
    /// <typeparam name="TSyncModel">Type of sync model to create the comparer for.</typeparam>
    /// <returns>Instance of <see cref="ISyncModelComparer{TSyncModel}" /></returns>
    ISyncModelComparer<TSyncModel> CreateComparer<TSyncModel>() where TSyncModel : ISyncModel;
}
