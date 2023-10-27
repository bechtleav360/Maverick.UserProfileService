namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     An interface containing a method to build an object of basic type <see cref="TBuilder" />
    /// </summary>
    /// <typeparam name="TBuilder">Type of the concrete builder class</typeparam>
    public interface IBasicBuilder<out TBuilder>
    {
        /// <summary>
        ///     Build an object of basic type <see cref="TBuilder" />
        /// </summary>
        /// <returns>An object of type <see cref="TBuilder" /></returns>
        TBuilder BuildBasic();
    }
}
