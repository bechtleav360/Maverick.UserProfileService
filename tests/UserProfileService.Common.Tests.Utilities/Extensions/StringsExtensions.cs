using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class StringsExtensions
    {
        private static bool TryConvert(
            IEnumerable<string> sequence,
            out List<string> result)
        {
            result = sequence as List<string> ?? sequence?.ToList();

            return result != null;
        }

        private static string Do(IEnumerable enumerable)
        {
            var sb = new StringBuilder();
            var i = 0;

            foreach (object obj in enumerable)
            {
                if (i++ > 0)
                {
                    sb.Append("|");
                }

                sb.Append(obj);
            }

            return sb.Length == 0
                ? "<empty>"
                : sb.ToString();
        }

        public static string ConvertToOutputStringDuringTest(this object obj)
        {
            return obj switch
            {
                null => "<NULL>",
                string s => s,
                DateTime dt => dt.ToString("O"),
                IEnumerable<string> stringSequence => TryConvert(stringSequence, out List<string> stringList)
                    ? !stringList.Any()
                        ? "<empty>"
                        : string.Join("|", stringList)
                    : "",
                IEnumerable enumerable => Do(enumerable),
                _ => obj?.ToString()
            };
        }
    }
}
