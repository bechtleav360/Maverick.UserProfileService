using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.FilterUtility.Configuration;

namespace Maverick.UserProfileService.FilterUtility.Extensions
{
    internal static class ConfigurationExtensions
    {
        private static string GetComplementLimiter(this FilterUtilityConfiguration config, string limiter)
        {
            if (limiter == config.DefinitionContainerMarkerEnd)
            {
                return config.DefinitionContainerMarkerStart;
            }

            if (limiter == config.FilterContainerMarkerEnd)
            {
                return config.FilterContainerMarkerStart;
            }

            if (limiter == config.CollectionContainerMarkerEnd)
            {
                return config.CollectionContainerMarkerStart;
            }

            return null;
        }

        internal static void ValidateLimiter(this FilterUtilityConfiguration config, string serializedFilter)
        {
            var stack = new Stack<string>();

            string pattern = "("
                + "("
                + config.DefinitionContainerMarkerStart.ToSafeRegexPatternString()
                + ")|"
                + "("
                + config.DefinitionContainerMarkerEnd.ToSafeRegexPatternString()
                + ")|"
                + "("
                + config.FilterContainerMarkerStart.ToSafeRegexPatternString()
                + ")|"
                + "("
                + config.FilterContainerMarkerEnd.ToSafeRegexPatternString()
                + ")|"
                + "("
                + config.CollectionContainerMarkerStart.ToSafeRegexPatternString()
                + ")|"
                + "("
                + config.CollectionContainerMarkerEnd.ToSafeRegexPatternString()
                + ")|"
                + ")";

            List<string> limiter = Regex.Matches(serializedFilter, pattern)
                .OfType<Match>()
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            foreach (string item in limiter)
            {
                if (item == config.DefinitionContainerMarkerStart
                    || item == config.FilterContainerMarkerStart
                    || item == config.CollectionContainerMarkerStart)
                {
                    stack.Push(item);
                }
                else if (item == config.DefinitionContainerMarkerEnd
                         || item == config.FilterContainerMarkerEnd
                         || item == config.CollectionContainerMarkerEnd)
                {
                    if (stack.Peek() != config.GetComplementLimiter(item))
                    {
                        throw new SerializationException("Unable to deserialize limiter.");
                    }

                    stack.Pop();
                }
            }
        }
    }
}
