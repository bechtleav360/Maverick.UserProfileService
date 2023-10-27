using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.FilterUtility.Configuration;
using Maverick.UserProfileService.FilterUtility.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.FilterUtility.Extensions
{
    internal static class RegexExtensions
    {
        internal static string GenerateFilterBinaryOperatorRegexPattern(
            EnumFilterDefinition definition,
            FilterUtilityConfiguration configuration)
        {
            string stringMarker = definition.EncapsulateValues ? configuration.StringMarker : string.Empty;

            return
                $"\\(({stringMarker}{string.Join("|", definition.ValidValues.Select(x => x.ToSafeRegexPatternString()))}{stringMarker}),";
        }

        internal static string GenerateDefinitionsOperatorRegexPattern(
            EnumFilterDefinition definition,
            FilterUtilityConfiguration configuration)
        {
            string stringMarker = definition.EncapsulateValues ? configuration.StringMarker : string.Empty;

            return
                $"{configuration.DefinitionContainerMarkerStart}\"[^{configuration.StringMarker}]*\"{configuration.Separator}({stringMarker}{string.Join("|", definition.ValidValues.Select(x => x.ToSafeRegexPatternString()))}{stringMarker})";
        }

        internal static string ToSafeRegexPatternString(this string stringValue)
        {
            var result = new StringBuilder();

            foreach (char character in stringValue)
            {
                result.Append("[" + character + "]");
            }

            return result.ToString();
        }

        internal static Definitions ParseDefinitionMatch(
            this Match match,
            EnumFilterDefinition filterOperatorsDefinition,
            FilterUtilityConfiguration config)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match), "Internal error occurred. No value to match set!");
            }

            if (filterOperatorsDefinition?.ValidValues == null)
            {
                throw new ArgumentNullException(
                    nameof(filterOperatorsDefinition),
                    "Internal error occurred. No filter operator definition set!");
            }

            if (config == null)
            {
                throw new ArgumentNullException(
                    nameof(config),
                    "Internal error occurred. No filter utility configuration set!");
            }

            // to collect all errors and pass them to requester
            var exceptionList = new List<Exception>();

            string operatorString = Regex.Match(
                    match.Value,
                    GenerateDefinitionsOperatorRegexPattern(filterOperatorsDefinition, config))
                .Groups[1]
                .Value;

            var @operator = FilterOperator.Equals; // just to "calm" VS/R# down

            if (string.IsNullOrWhiteSpace(operatorString))
            {
                exceptionList.Add(
                    new ValidationException(
                        $"Could not parse operator string. Possible values are: \"{string.Join("\", \"", filterOperatorsDefinition.ValidValues)}\""));
            }
            // otherwise the message would be twice in the exception collection
            else
            {
                try
                {
                    @operator = EnumExtensions.ParseFilterSerializeAttribute<FilterOperator>(operatorString);
                }
                catch (Exception e)
                {
                    exceptionList.Add(e);
                }
            }

            string binaryOperatorString = Regex
                .Match(match.Value, $"]{config.Separator}(.*){config.DefinitionContainerMarkerEnd}")
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(binaryOperatorString))
            {
                exceptionList.Add(new ValidationException("Could not parse binary operator string."));
            }

            string fieldName = Regex.Match(
                    match.Value,
                    string.Format("{0}{1}([^{1}]*){1}", config.DefinitionContainerMarkerStart, config.StringMarker))
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(fieldName))
            {
                exceptionList.Add(new ValidationException("Could not parse field name. Invalid value."));
            }

            string[] values = Regex.Matches(
                    Regex.Match(match.Value, @"\[(.*)\]").Groups[1].Value,
                    string.Format("{0}([^{0}]*){0}", config.StringMarker))
                .OfType<Match>()
                .Select(x => x.Groups[1].Value)
                .Where(term => !string.IsNullOrWhiteSpace(term))
                .ToArray();

            if (values.Length == 0)
            {
                exceptionList.Add(
                    new ValidationException(
                        "Could not parse filter values. Values array is null or contains just invalid strings (i.e. null or whitespaces)."));
            }

            BinaryOperator binaryOperator;

            try
            {
                binaryOperator = EnumExtensions.ParseFilterSerializeAttribute<BinaryOperator>(binaryOperatorString);
            }
            catch (Exception e)
            {
                exceptionList.Add(e);
                binaryOperator = BinaryOperator.And; // just to "calm" VS/R# down
            }

            if (exceptionList.Count > 0)
            {
                throw new AggregateException(exceptionList);
            }

            var result = new Definitions
            {
                BinaryOperator = binaryOperator,
                Operator = @operator,
                FieldName = fieldName,
                Values = values
            };

            return result;
        }
    }
}
