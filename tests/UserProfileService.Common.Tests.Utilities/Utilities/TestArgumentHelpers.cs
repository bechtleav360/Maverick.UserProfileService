namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    /// <summary>
    ///     Includes helping methods for argument lists in test methods.
    /// </summary>
    public static class TestArgumentHelpers
    {
        /// <summary>
        ///     Returns 0..n parameters as object[].
        /// </summary>
        /// <param name="parameters">The parameters to be returned as array.</param>
        /// <returns>The array containing <paramref name="parameters" /></returns>
        public static object[] GetArgumentArray(
            params object[] parameters)
        {
            return parameters;
        }
    }
}
