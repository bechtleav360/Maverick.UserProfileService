using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using FluentValidation;
using FluentValidation.Validators;

namespace UserProfileService.Saga.Validation.Utilities;

/// <summary>
///     Validates a JSON document.
/// </summary>
/// <typeparam name="T">The type of the property</typeparam>
public class JsonDocumentValidator<T> : PropertyValidator<T, string>
{
    private readonly Type _jsonNodeType;
    private JsonException _lastOccurredException;

    /// <inheritdoc />
    public override string Name => "StringJsonDocumentValidator";

    /// <summary>
    ///     Initializes a new instance of <see cref="JsonDocumentValidator{T}" />.
    /// </summary>
    /// <param name="jsonNodeType">The type of the expected JSON object (i.e. <see cref="JsonArray" />)</param>
    /// <exception cref="ArgumentException"><paramref name="jsonNodeType" /> is not inheritable from <see cref="JsonNode" />.</exception>
    public JsonDocumentValidator(Type jsonNodeType)
    {
        if (!typeof(JsonNode).IsAssignableFrom(jsonNodeType))
        {
            throw new ArgumentException(
                $"The provided type is not supported by this method. It must be of type {nameof(JsonNode)} or a derived type of it.",
                nameof(jsonNodeType));
        }

        _jsonNodeType = jsonNodeType;
    }

    private bool IsValidJsonAndOfExpectedType(string term)
    {
        _lastOccurredException = null;

        return TrayParseJsonString(term, out JsonNode parsed)
            && (parsed.GetType() == _jsonNodeType
                || _jsonNodeType.IsInstanceOfType(parsed));
    }

    private bool TrayParseJsonString(
        string term,
        out JsonNode parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(term))
        {
            return false;
        }

        try
        {
            parsed = JsonNode.Parse(term);
        }
        catch (JsonException jsonReaderException)
        {
            _lastOccurredException = jsonReaderException;

            return false;
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        // if JSON reader threw an exception ...
        if (!string.IsNullOrWhiteSpace(_lastOccurredException?.Message))
        {
            return $"The provided string is not a valid JSON string - error details: {_lastOccurredException.Message}";
        }

        // Otherwise the resulting JSON type was not valid ...
        string specifiedErrorMessage;

        if (_jsonNodeType == typeof(JsonArray))
        {
            specifiedErrorMessage = "- it should be a JSON array (wrapped by '[' ']')";
        }
        else if (_jsonNodeType == typeof(JsonObject))
        {
            specifiedErrorMessage = "- it should be a JSON object/document (wrapped by curly braces '{', '}')";
        }
        else if (_jsonNodeType == typeof(JsonValue))
        {
            specifiedErrorMessage =
                "- it should be a JSON value - either a quoted string (like \"foo\"), a boolean value or a number";
        }
        else
        {
            specifiedErrorMessage = "at all";
        }

        return $"The provided string is not a valid JSON string {specifiedErrorMessage}";
    }

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return IsValidJsonAndOfExpectedType(value);
    }
}
