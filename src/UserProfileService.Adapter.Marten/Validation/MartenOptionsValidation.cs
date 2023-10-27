using Microsoft.Extensions.Options;
using Npgsql;
using UserProfileService.Adapter.Marten.Options;

namespace UserProfileService.Adapter.Marten.Validation;

/// <summary>
///     A Class used to validate the configuration to connect to PostgreSQL using the Marten library.
/// </summary>
public class MartenOptionsValidation : IValidateOptions<MartenConnectionOptions>
{
    private static bool ValidatePostgreSqlConnectionString(string? connectionString)
    {
        try
        {
            // If the connection string is not valid, an exception will be throw. (only syntax validation)
            _ = new NpgsqlConnectionStringBuilder(connectionString);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc cref="IValidateOptions{TOptions}" />
    public ValidateOptionsResult Validate(string name, MartenConnectionOptions? options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error occurred: Options object not provided!");
        }

        List<string> validationErrors = GetValidationErrorMessages(options);

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        return ValidateOptionsResult.Success;
    }

    /// <summary>
    ///     Gets a list of possible error messages regarding validation of an <see cref="MartenConnectionOptions" /> instance.
    /// </summary>
    /// <param name="options">The options object to check.</param>
    /// <returns>A list of possible error messages due to validation issues. Will be empty, if validation succeeded.</returns>
    public static List<string> GetValidationErrorMessages(MartenConnectionOptions options)
    {
        var validationErrors = new List<string>();

        if (!ValidatePostgreSqlConnectionString(options.ConnectionString))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.ConnectionString)}': Malformed connection string, not supported by postgreSql");
        }

        if (string.IsNullOrWhiteSpace(options.DatabaseSchema))
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.DatabaseSchema)}': It should not be null, empty or consist only of white-space characters");
        }

        return validationErrors;
    }
}
