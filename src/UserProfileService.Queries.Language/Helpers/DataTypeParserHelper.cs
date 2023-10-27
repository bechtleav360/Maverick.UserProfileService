using UserProfileService.Queries.Language.ValidationException;

namespace UserProfileService.Queries.Language.Helpers;

internal static class DataTypeParserHelper
{
    internal static object ParseDataType(Type expectedType, string valueToParse, string propertyName)
    {
        const string errorMessage =
            "The property '{0}' with the value '{1}'could not be parsed. Expected {2}, but parsing this type failed";

        const string titleMessage = "The $filter query is not valid!";

        if (expectedType == typeof(int))
        {
            if (!int.TryParse(valueToParse, out int intResult))
            {
                throw new QueryValidationException(
                    titleMessage,
                    string.Format(errorMessage, propertyName, valueToParse, "an integer"));
            }

            return intResult;
        }

        if (expectedType == typeof(long))
        {
            if (!long.TryParse(valueToParse, out long longResult))
            {
                throw new QueryValidationException(
                    titleMessage,
                    string.Format(errorMessage, propertyName, valueToParse, "a long"));
            }

            return longResult;
        }

        if (expectedType == typeof(double))
        {
            if (!double.TryParse(valueToParse, out double doubleResult))
            {
                throw new QueryValidationException(
                    titleMessage,
                    string.Format(errorMessage, propertyName, valueToParse, "a double"));
            }

            return doubleResult;
        }

        if (expectedType == typeof(float))
        {
            if (!float.TryParse(valueToParse, out float floatTimeResult))
            {
                throw new QueryValidationException(
                    titleMessage,
                    string.Format(errorMessage, propertyName, valueToParse, "a float."));
            }

            return floatTimeResult;
        }

        if (expectedType == typeof(DateTime))
        {
            if (!DateTime.TryParse(valueToParse, out DateTime dateTimeResult))
            {
                throw new QueryValidationException(
                    titleMessage,
                    string.Format(errorMessage, propertyName, valueToParse, "a datetime"));
            }

            return dateTimeResult;
        }

        if (expectedType == typeof(string))
        {
            return valueToParse;
        }

        throw new NotSupportedException(
            $"{titleMessage}. The  property '{propertyName}' with the value '{valueToParse}' could not be parsed. The datatype is not supported.");
    }
}
