using System.Collections.Generic;

namespace UserProfileService.Common.Tests.Utilities.FluentApi
{
    /// <summary>
    ///     The default builder implementation.
    /// </summary>
    public class TestArgumentBuilder : ITestArgumentsBuilder
    {
        private readonly List<object[]> _testCaseArguments = new List<object[]>();

        /// <summary>
        ///     Creates a new argument builder to start the fluent api of building process.
        /// </summary>
        /// <returns></returns>
        public static ITestArgumentsBuilder CreateArguments()
        {
            return new TestArgumentBuilder();
        }

        /// <inheritdoc />
        public ITestArgumentsBuilder AddTestCaseParameters(
            params object[] parameters)
        {
            _testCaseArguments.Add(parameters);

            return this;
        }

        /// <inheritdoc />
        public IEnumerable<object[]> Build()
        {
            return _testCaseArguments;
        }
    }
}
