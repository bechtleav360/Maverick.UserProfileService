using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Sprache;
using UserProfileService.Adapter.Marten.Abstractions;
using UserProfileService.Adapter.Marten.Options;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Models;
using UserProfileService.Queries.Language.Models;
using UserProfileService.Queries.Language.TreeDefinition;
using UserProfileService.Queries.Language.ValidationException;
using UserProfileService.Queries.Language.Validators;

namespace UserProfileService.Adapter.Marten.Implementations;

/// <inheritdoc />
public class OptionsValidateParser : IOptionsValidateParser
{
    private readonly IQueryConverter _converter;
    private readonly ILogger<OptionsValidateParser> _logger;

    /// <summary>
    ///     Creates a <see cref="QueryConverter" /> object.
    /// </summary>
    /// <param name="converter">
    ///     Is used to check if the <see cref="QueryOptions.Filter" /> and
    ///     <see cref="QueryOptions.OrderBy" /> queries can be parsed.
    /// </param>
    /// <param name="logger">The logger that is used to create messages with different severities</param>
    public OptionsValidateParser(IQueryConverter converter, ILogger<OptionsValidateParser> logger)
    {
        _logger = logger;
        _converter = converter;
    }

    private void ValidateSortedList<TResult>(IEnumerable<SortedProperty>? sortedList, string orderByQuery)
    {
        _logger.EnterMethod();

        var titleMessage = $"The $oderBy query '{orderByQuery}' is not valid";

        if (sortedList != null)
        {
            _logger.LogDebugMessage(
                "Validating the sorted list {sortedList} for the order by query.",
                LogHelpers.Arguments(sortedList.ToLogString()));

            _logger.LogInfoMessage("Validating the sorted list for the order by query", LogHelpers.Arguments());

            foreach (SortedProperty sortedProperty in sortedList)
            {
                PropertyValidationResult result = ValidatorHelper.ValidatePropertyInResult<TResult>(
                    sortedProperty.PropertyName,
                    new List<Type>
                    {
                        typeof(int),
                        typeof(long),
                        typeof(string),
                        typeof(DateTime),
                        typeof(DateTimeOffset)
                    });

                switch (result)
                {
                    case PropertyValidationResult.PropertyNotInResultObject:
                        throw new QueryValidationException(
                            titleMessage,
                            $"The property '{sortedProperty.PropertyName}' is not part of the result object '{typeof(TResult).Name}'.");
                    case PropertyValidationResult.PropertyNotOfGivenType:
                        throw new QueryValidationException(
                            titleMessage,
                            $"For the property order by property only following type are only valid {nameof(Int32)}, {nameof(Int64)}, {nameof(String)}, {nameof(DateTime)}, {nameof(DateTimeOffset)}."
                            + $"The property '{sortedProperty.PropertyName}' is neither of this type.");
                    case PropertyValidationResult.None:
                        break;
                    case PropertyValidationResult.PropertyValidationSuccess:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(result), result, "Value of propertyValidationResult is not valid.");
                }
            }

            _logger.LogInfoMessage("The validation of the sorted list was successful", LogHelpers.Arguments());
        }

        _logger.ExitMethod();
    }

    /// <summary>
    ///     Validates an parses a <see cref="QueryOptions" /> to an internal query options mode
    ///     <see cref="QueryOptionsVolatileModel" /> that can be used to query the database.
    /// </summary>
    /// <param name="options">The query options that should be parser and validated.</param>
    /// <typeparam name="TResult">The result object that is used to validate if a property is part of the object.</typeparam>
    /// <returns>Returns a <see cref="QueryOptionsVolatileModel" /> that is used to filter the result set of volatile date.</returns>
    /// <exception cref="ArgumentNullException">If the <see cref="options" /> is null.</exception>
    /// <exception cref="QueryValidationException">
    ///     This exception will be thrown when the <see cref="QueryOptions.Filter" />
    ///     or the <see cref="QueryOptions.OrderBy" /> query are not valid and can not be parsed.
    /// </exception>
    public QueryOptionsVolatileModel ParseAndValidateQueryOptions<TResult>(QueryOptions options)
    {
        _logger.EnterMethod();

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (_logger.IsEnabledForTrace())
        {
            _logger.LogTraceMessage("The query options: {options}", LogHelpers.Arguments(options.Filter.ToLogString()));
        }

        TreeNode? treeNode = null;
        IEnumerable<SortedProperty>? sortedProperty = null;

        var queryOptions = new QueryOptionsVolatileModel
        {
            Filter = options.Filter,
            OrderBy = options.OrderBy,
            Limit = options.Limit,
            // The default-value for the offset is 0.
            // But marten accept only 1 as the page size.
            // To be consistent in our api, we still use 0 as default value, but settings
            // it here to 0 to avoid marten exception.
            Offset = options.Offset == 0 ? 1 : options.Offset
        };

        if (!string.IsNullOrWhiteSpace(options.Filter))
        {
            try
            {
                _logger.LogInfoMessage("Trying to parse the filter query:.", LogHelpers.Arguments());
                treeNode = _converter.CreateFilterQueryTree(options.Filter);
                _logger.LogInfoMessage("Parsing and creating a tree was successfully.", LogHelpers.Arguments());
            }
            catch (ParseException parseException)
            {
                var queryValidationError = new QueryValidationException(
                    $"The $filter query '{options.Filter}' is not valid!",
                    parseException.Message,
                    parseException);

                _logger.LogErrorMessage(parseException, parseException.Message, LogHelpers.Arguments());

                throw queryValidationError;
            }
            catch (Exception e)
            {
                _logger.LogErrorMessage(
                    e,
                    "An error while parsing the filter option. The {options.Filter} is not a valid query string.",
                    LogHelpers.Arguments(options.Filter.ToLogString()));

                throw;
            }
        }

        if (!string.IsNullOrWhiteSpace(options.OrderBy))
        {
            try
            {
                _logger.LogInfoMessage(
                    "Trying to parse the order by query '{orderQuery}'",
                    LogHelpers.Arguments(options.OrderBy));

                sortedProperty = _converter.CreateOrderByQuery(options.OrderBy);

                _logger.LogInfoMessage("The parsing of the order by query was successful", LogHelpers.Arguments());

                _logger.LogInfoMessage("Trying to  validate the order by query.", LogHelpers.Arguments());

                ValidateSortedList<TResult>(sortedProperty, options.OrderBy);

                _logger.LogInfoMessage("Validation of the order by query was successful.", LogHelpers.Arguments());
            }
            catch (ParseException parseException)
            {
                var queryValidationError = new QueryValidationException(
                    $"The $orderBy query '{options.OrderBy}' is not valid!",
                    parseException.Message,
                    parseException);

                _logger.LogErrorMessage(
                    parseException,
                    "An error occurred while parsing the oder by option. The oder by claus {oderBy} is not valid!",
                    LogHelpers.Arguments(options.OrderBy.ToLogString()));

                throw queryValidationError;
            }
            catch (Exception ex)
            {
                _logger.LogErrorMessage(
                    ex,
                    "An error occurred while parsing the oder by option.",
                    LogHelpers.Arguments());

                throw;
            }
        }

        queryOptions.FilterTree = treeNode;
        queryOptions.OrderByList = sortedProperty;

        return _logger.ExitMethod(queryOptions);
    }
}
