using System;
using System.Collections.Generic;
using System.Linq;
using UserProfileService.Common.Tests.Utilities.Utilities;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class ParameterExtensions
    {
        public static ParameterCollection<TValue> ToParameterCollection<TValue>(
            this Dictionary<string, TValue> dictionary)
        {
            return new ParameterCollection<TValue>(dictionary);
        }

        public static ParameterCollection<TValue> ToParameterCollection<TInput, TValue>(
            this IEnumerable<TInput> elements,
            Func<TInput, string> keySelector,
            Func<TInput, TValue> valueSelector)
        {
            return new ParameterCollection<TValue>(elements?.ToDictionary(keySelector, valueSelector));
        }
    }
}
