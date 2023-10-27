using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using UserProfileService.Redis.Configuration;

namespace UserProfileService.Redis.Validation;

/// <summary>
///     A Class used to validate the configuration to connect to Redis
/// </summary>
public class RedisConfigurationValidation : IValidateOptions<RedisConfiguration>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, RedisConfiguration options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error occurred: Options object not provided!");
        }

        var validationErrors = new List<string>();

        if (options.EndpointUrls == null)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.EndpointUrls)}': It should not be null");
        }

        if (options.ConnectRetry < 0)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.ConnectRetry)}': It should not be lower than 0");
        }

        if (options.ConnectTimeout < 0)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.ConnectTimeout)}': It should not be lower than 0");
        }

        if (options.ExpirationTime < 0)
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.ExpirationTime)}': It should not be lower than 0");
        }

        if (options.EndpointUrls != null && !options.EndpointUrls.Any())
        {
            validationErrors.Add(
                $"Configuration error concerning '{nameof(options.EndpointUrls)}': The collection should not be empty");
        }

        if (options.EndpointUrls != null && options.EndpointUrls.Any())
        {
            if (options.EndpointUrls.All(string.IsNullOrWhiteSpace))
            {
                validationErrors.Add(
                    $"Configuration error concerning '{nameof(options.EndpointUrls)}': All endpoints are null or white space characters");
            }
        }

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(options)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        return ValidateOptionsResult.Success;
    }
}
