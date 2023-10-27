using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    public class ParameterCollection<TVal> : Dictionary<string, TVal>
    {
        public new TVal this[string key] =>
            !string.IsNullOrWhiteSpace(key) && ContainsKey(key)
                ? base[key]
                : default;

        public ParameterCollection(IEnumerable<KeyValuePair<string, TVal>> original)
            : base(original, StringComparer.OrdinalIgnoreCase)
        {
        }

        public ParameterCollection(IDictionary<string, TVal> original)
            : base(original ?? new Dictionary<string, TVal>(), StringComparer.OrdinalIgnoreCase)
        {
        }

        public ParameterCollection(IEnumerable<TVal> values, Func<TVal, string> keySelector)
            : base(values.ToDictionary(keySelector, val => val), StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
