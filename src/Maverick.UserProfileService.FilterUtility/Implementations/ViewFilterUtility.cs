using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Maverick.UserProfileService.FilterUtility.Abstraction;
using Maverick.UserProfileService.FilterUtility.Extensions;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.RequestModels;

namespace Maverick.UserProfileService.FilterUtility.Implementations
{
    /// <inheritdoc />
    public class ViewFilterUtility : IFilterUtility<List<ViewFilterModel>>
    {
        /// <inheritdoc />
        public List<ViewFilterModel> Deserialize(string serializedFilter)
        {
            var result = new List<ViewFilterModel>();
            serializedFilter = serializedFilter.Substring(9);

            MatchCollection viewFilterModelMatches = Regex.Matches(
                serializedFilter,
                "(([A-Za-z0-9.\"]|[(](?:\"[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*\"|[0-9]+,[0-9]+)[)])+)[,]?");

            foreach (Match viewFilterMatch in viewFilterModelMatches.OfType<Match>())
            {
                var viewFilterModel = new ViewFilterModel();

                List<string> viewFilterPartialMatches = Regex.Matches(
                        viewFilterMatch.Groups[1].Value,
                        "(([a-zA-Z]|[(](?:\"[^\"\\\\]*(?:\\\\.[^\"\\\\]*)*\"|[0-9]+,[0-9]+)[)])+)")
                    .OfType<Match>()
                    .Select(x => x.Groups[1].Value)
                    .ToList();

                viewFilterModel.DataStoreContext =
                    EnumExtensions.ParseFilterSerializeAttribute<ViewFilterDataStoreContext>(
                        viewFilterPartialMatches[0]);

                viewFilterModel.FieldName = viewFilterPartialMatches[1];

                viewFilterModel.Type =
                    EnumExtensions.ParseFilterSerializeAttribute<ViewFilterTypes>(viewFilterPartialMatches[2]);

                if (viewFilterPartialMatches.Any(x => x.StartsWith("paginated")))
                {
                    List<int> paginationValues = viewFilterPartialMatches.First(x => x.StartsWith("paginated"))
                        .Replace("paginated(", string.Empty)
                        .Replace(")", string.Empty)
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

                    viewFilterModel.Pagination.Offset = paginationValues[0];
                    viewFilterModel.Pagination.Limit = paginationValues[1];
                }

                if (viewFilterPartialMatches.Any(x => x.StartsWith("filter")))
                {
                    viewFilterModel.Filter = viewFilterPartialMatches.First(x => x.StartsWith("filter"))
                        .Substring(7)
                        .TrimEnd(')')
                        .Split(',')
                        .Select(x => x.Trim('"'))
                        .ToArray();
                }

                result.Add(viewFilterModel);
            }

            return result;
        }

        /// <inheritdoc />
        public string Serialize(List<ViewFilterModel> filter)
        {
            if (filter == null || filter.Count == 0)
            {
                throw new ArgumentException(
                    $"Unable to serialize {(filter == null ? "null" : "empty")} ViewFilterList",
                    nameof(filter));
            }

            var stringBuilder = new StringBuilder("contains(");

            for (var i = 0; i < filter.Count; i++)
            {
                stringBuilder.Append(
                    filter[i].DataStoreContext.GetAttribute<FilterSerializeAttribute>()?.SerializationValue
                    ?? filter[i].DataStoreContext.ToString());

                stringBuilder.Append(".");
                stringBuilder.Append(filter[i].Type);
                stringBuilder.Append(".");

                if (filter[i].Filter != null && filter[i].Filter.Length > 0)
                {
                    stringBuilder.Append("filter(");

                    for (var j = 0; j < filter[i].Filter.Length; j++)
                    {
                        stringBuilder.Append("\"" + filter[i].Filter[j] + "\"");

                        if (j != filter[i].Filter.Length - 1)
                        {
                            stringBuilder.Append(",");
                        }
                    }

                    stringBuilder.Append(")");
                }

                stringBuilder.Append(".");
                stringBuilder.Append($"paginated({filter[i].Pagination.Offset},{filter[i].Pagination.Limit})");

                if (i != filter.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }

            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }
}
