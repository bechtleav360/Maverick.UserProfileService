using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.FilterUtility.Abstraction;
using Maverick.UserProfileService.FilterUtility.Configuration;
using Maverick.UserProfileService.FilterUtility.Extensions;
using Maverick.UserProfileService.FilterUtility.Models;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.FilterUtility.Implementations
{
    /// <inheritdoc />
    public class FilterUtility : IFilterUtility<Filter>
    {
        private readonly FilterUtilityConfiguration _configuration;

        /// <summary>
        ///     Initializes a <see cref="FilterUtility" /> with the default configuration
        /// </summary>
        public FilterUtility()
        {
            _configuration = FilterUtilityConfiguration.DefaultConfiguration;
        }

        /// <summary>
        ///     Initializes a <see cref="FilterUtility" /> with the given configuration
        /// </summary>
        /// <param name="configuration">Custom <see cref="FilterUtilityConfiguration" /></param>
        public FilterUtility(FilterUtilityConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <inheritdoc />
        public Filter Deserialize(string serializedFilter)
        {
            if (string.IsNullOrWhiteSpace(serializedFilter))
            {
                return null;
            }

            var result = new Filter();

            _configuration.ValidateLimiter(serializedFilter);

            EnumFilterDefinition binaryOperatorsDefinition = typeof(BinaryOperator).GetEnumFilterDefinition();
            EnumFilterDefinition filterOperatorsDefinition = typeof(FilterOperator).GetEnumFilterDefinition();

            Match generalFilterMatch = Regex.Match(
                serializedFilter,
                RegexExtensions.GenerateFilterBinaryOperatorRegexPattern(binaryOperatorsDefinition, _configuration),
                RegexOptions.IgnoreCase);

            if (!generalFilterMatch.Success)
            {
                throw new SerializationException("Unable to deserialize filter \"BinaryOperator\"");
            }

            result.CombinedBy =
                EnumExtensions.ParseFilterSerializeAttribute<BinaryOperator>(generalFilterMatch.Groups[1].Value);

            var definitionContainerPattern =
                $"^{_configuration.CollectionContainerMarkerStart.ToSafeRegexPatternString()}(.*){_configuration.CollectionContainerMarkerEnd.ToSafeRegexPatternString()}{_configuration.FilterContainerMarkerEnd.ToSafeRegexPatternString()}";

            Match definitionStringMatch = Regex.Match(
                serializedFilter.Substring(generalFilterMatch.Length),
                definitionContainerPattern);

            if (definitionStringMatch.Success)
            {
                // TODO pattern only works if DefinitionContainerMarkers are not more than 1 character
                string definitionPattern = string.Format(
                    "{0}[^{0}{1}]*{1}",
                    _configuration.DefinitionContainerMarkerStart,
                    _configuration.DefinitionContainerMarkerEnd);

                MatchCollection definitionMatches =
                    Regex.Matches(definitionStringMatch.Groups[1].Value, definitionPattern);

                if (definitionMatches.OfType<Match>().Any())
                {
                    result.Definition = new List<Definitions>();

                    foreach (Match definition in definitionMatches.OfType<Match>().ToList())
                    {
                        result.Definition.Add(
                            definition.ParseDefinitionMatch(filterOperatorsDefinition, _configuration));
                    }
                }
            }

            // if no definition was found, the filter string was not valid and useless
            if (result.Definition == null || result.Definition.Count == 0)
            {
                throw new SerializationException(
                    "Error in syntax of query filter. Could not determine any filter definition.");
            }

            return result;
        }

        /// <inheritdoc />
        public string Serialize(Filter filter)
        {
            if (filter == null)
            {
                return null;
            }

            EnumFilterDefinition binaryOperatorsDefinition = typeof(BinaryOperator).GetEnumFilterDefinition();
            EnumFilterDefinition filterOperatorsDefinition = typeof(FilterOperator).GetEnumFilterDefinition();

            var sb = new StringBuilder(string.Empty);
            sb.Append(_configuration.FilterContainerMarkerStart);

            if (binaryOperatorsDefinition.EncapsulateValues)
            {
                sb.Append(_configuration.StringMarker);
            }

            sb.Append(
                filter.CombinedBy.GetAttribute<FilterSerializeAttribute>()?.SerializationValue
                ?? filter.CombinedBy.ToString());

            if (binaryOperatorsDefinition.EncapsulateValues)
            {
                sb.Append(_configuration.StringMarker);
            }

            sb.Append(_configuration.Separator);
            sb.Append(_configuration.CollectionContainerMarkerStart);

            foreach (Definitions definition in filter.Definition)
            {
                sb.Append(_configuration.DefinitionContainerMarkerStart);
                sb.Append(_configuration.StringMarker);
                sb.Append(definition.FieldName);
                sb.Append(_configuration.StringMarker);
                sb.Append(_configuration.Separator);

                if (binaryOperatorsDefinition.EncapsulateValues)
                {
                    sb.Append(_configuration.StringMarker);
                }

                sb.Append(
                    definition.Operator.GetAttribute<FilterSerializeAttribute>()?.SerializationValue
                    ?? definition.Operator.ToString());

                if (binaryOperatorsDefinition.EncapsulateValues)
                {
                    sb.Append(_configuration.StringMarker);
                }

                sb.Append(_configuration.Separator);

                sb.Append(_configuration.CollectionContainerMarkerStart);

                for (var i = 0; i < definition.Values.Length; i++)
                {
                    sb.Append(
                        string.Format(
                            "{0}{1}{0}{2}",
                            _configuration.StringMarker,
                            definition.Values[i],
                            i != definition.Values.Length - 1 ? _configuration.Separator : string.Empty));
                }

                sb.Append(_configuration.CollectionContainerMarkerEnd);
                sb.Append(_configuration.Separator);

                if (filterOperatorsDefinition.EncapsulateValues)
                {
                    sb.Append(_configuration.StringMarker);
                }

                sb.Append(
                    definition.BinaryOperator.GetAttribute<FilterSerializeAttribute>()?.SerializationValue
                    ?? definition.BinaryOperator.ToString());

                if (filterOperatorsDefinition.EncapsulateValues)
                {
                    sb.Append(_configuration.StringMarker);
                }

                sb.Append(_configuration.DefinitionContainerMarkerEnd);
            }

            sb.Append(_configuration.CollectionContainerMarkerEnd);
            sb.Append(_configuration.FilterContainerMarkerEnd);

            return sb.ToString();
        }
    }
}
