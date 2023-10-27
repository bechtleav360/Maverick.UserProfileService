using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.UnitTests.MockDataBuilder
{
    /// <summary>
    ///     An interface containing a method to build an object of sync model type <see cref="TBuilder" />
    /// </summary>
    /// <typeparam name="TBuilder">Type of the concrete builder class</typeparam>
    public interface ISyncModelBuilder<TBuilder> where TBuilder : ISyncModel
    {
        /// <summary>
        ///     Build an object of basic type <see cref="TBuilder" />
        /// </summary>
        /// <returns>An object of type <see cref="TBuilder" /></returns>
        TBuilder BuildSyncModel();
    }
}
