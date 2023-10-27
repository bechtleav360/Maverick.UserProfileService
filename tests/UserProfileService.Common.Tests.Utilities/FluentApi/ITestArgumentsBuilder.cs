using System.Collections.Generic;

namespace UserProfileService.Common.Tests.Utilities.FluentApi
{
    /// <summary>
    ///     Defines methods of a test argument builder.
    /// </summary>
    public interface ITestArgumentsBuilder
    {
        /// <summary>
        ///     Adds a parameter list as complete set of arguments of a test case.
        /// </summary>
        /// <param name="parameters">The parameters for the test case.</param>
        /// <returns>The builder itself to be used in a fluent way.</returns>
        ITestArgumentsBuilder AddTestCaseParameters(params object[] parameters);

        /// <summary>
        ///     Returns the complete set of parameter lists as <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <returns>All previous "registered" test argument lists.</returns>
        IEnumerable<object[]> Build();
    }
}
