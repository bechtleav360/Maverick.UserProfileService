namespace UserProfileService.Common.Tests.Utilities.MockDataBuilder
{
    /// <summary>
    ///     Abstract class used to build model objects
    /// </summary>
    /// <typeparam name="TBuilder">Type of the concrete builder class</typeparam>
    /// <typeparam name="TObject">Type of the object that should be mocked</typeparam>
    public abstract class AbstractMockBuilder<TBuilder, TObject>
    {
        /// <summary>
        ///     The mocked object
        /// </summary>
        protected TObject Mockedobject;

        /// <summary>
        ///     Method use to generate an <see cref="TObject" /> object
        /// </summary>
        /// <returns>The built <see cref="TObject" /> on the fluent way</returns>
        public TObject Build()
        {
            return Mockedobject;
        }

        /// <summary>
        ///     Method to initialize all the fields of the <see cref="TObject" /> that is being generated
        /// </summary>
        /// <returns>
        ///     <see cref="TBuilder" />
        /// </returns>
        public abstract TBuilder GenerateSampleData();
    }
}
