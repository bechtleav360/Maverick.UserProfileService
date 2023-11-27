using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Maverick.Client.ArangoDb.Public.Configuration;
using Microsoft.Extensions.Options;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

public class ArangoConfigurationValidation : IValidateOptions<ArangoConfiguration>
{
    private static IEnumerable<string> ValidateConnectionString(string value)
    {
        var msg = new List<string>();

        MatchCollection regEx = Regex.Matches(value, @"(?<key>[a-zA-Z]+)\s*=\s*(?<value>[^;]+)\s*;?");

        Dictionary<string, string> builder = regEx
            .Select(
                o => new
                {
                    Key = o?.Groups["key"].Value.Trim(),
                    Value = o?.Groups["value"].Value.Trim()
                })
            .Where(o => !string.IsNullOrWhiteSpace(o.Key) && !string.IsNullOrWhiteSpace(o.Value))
            .GroupBy(x => x.Key, x => x, (_, e) => e.First())
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        if (!builder.TryGetValue("endpoints", out string ep)
            || string.IsNullOrWhiteSpace(ep))
        {
            msg.Add("Connection string validation error: Keyword 'endpoints' is missing!");
        }

        if (!builder.TryGetValue("database", out string db)
            || string.IsNullOrWhiteSpace(db))
        {
            msg.Add("Connection string validation error: Keyword 'database' is missing!");
        }

        if (!builder.TryGetValue("username", out string un)
            || string.IsNullOrWhiteSpace(un))
        {
            msg.Add("Connection string validation error: Keyword 'username' is missing!");
        }

        if (!builder.TryGetValue("password", out string pwd)
            || string.IsNullOrWhiteSpace(pwd))
        {
            msg.Add("Connection string validation error: Keyword 'password' is missing!");
        }

        return msg;
    }

    private static List<string> ValidateClusterConfigItem(
        string type,
        KeyValuePair<string, ArangoCollectionClusterConfiguration> configItem)
    {
        var msg = new List<string>();

        if (string.IsNullOrWhiteSpace(configItem.Key))
        {
            msg.Add(
                $"Cluster config validation error regarding {type}: One config item has an empty key or it's key contains only whitespaces. Possible values are collection names or '*'!");
        }

        if (!string.IsNullOrWhiteSpace(configItem.Key) && configItem.Key.Length > 256)
        {
            msg.Add(
                $"Cluster config validation error regarding {type}: The key '{configItem.Key}' of the cluster config represents a collection name OR asterisk character '*' for all collections. But it exceeds the maximum allowed length of 256 characters.");
        }

        if (!string.IsNullOrWhiteSpace(configItem.Key)
            && !Regex.IsMatch(configItem.Key, @"^([a-zA-Z][a-zA-Z0-9\-_]*|\*)$"))
        {
            msg.Add(
                $"Cluster config validation error regarding {type}: The key '{configItem.Key}' of the cluster config represents a collection name OR asterisk character '*' for all collections. But it contains invalid characters or does not start with a letter.");
        }

        if (configItem.Value == null)
        {
            msg.Add(
                $"Cluster config validation error regarding {type}: One config item has an empty cluster configuration item (key. '{configItem.Key}').");
        }

        return msg;
    }

    private static IEnumerable<string> ValidateExceptionConfigItem(
        ArangoExceptionConfiguration exceptionConfiguration)
    {
        const string type = nameof(ArangoExceptionConfiguration);

        var msg = new List<string>();

        if (exceptionConfiguration == null)
        {
            msg.Add($"Exception config validation error regarding '{type}': Configuration must not be null.");

            return msg;
        }

        if (exceptionConfiguration.DurationOfBreak >= TimeSpan.Zero
            && exceptionConfiguration.DurationOfBreak <= TimeSpan.FromMinutes(5))
        {
            msg.Add(
                $"Exception config validation error regarding {type}: The property of '{exceptionConfiguration.DurationOfBreak}' must be between greater than zero and less than 5 minutes..");
        }

        if (exceptionConfiguration.SleepDuration == null)
        {
            msg.Add(
                $"Exception config validation error regarding {type}: The property of '{exceptionConfiguration.SleepDuration}' must be not be null.");
        }

        if (exceptionConfiguration.RetryCount == 0 && exceptionConfiguration.RetryEnabled)
        {
            msg.Add(
                $"Exception config validation error regarding {type}: The property of '{exceptionConfiguration.DurationOfBreak}' must be not be zero, if the retry mechanism is enabled.");
        }

        // exceptionConfiguration.ExceptionHandler can be null. Will be ignored in Connection.cs

        return msg;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string name, ArangoConfiguration options)
    {
        if (options == null)
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(ArangoConfiguration)} validation error occurred: Options object not provided!");
        }

        var validationErrors = new List<string>();

        validationErrors.AddRange(ValidateConnectionString(options.ConnectionString));

        if (options.ClusterConfiguration?.DocumentCollections is
            {
                Count: > 0
            })
        {
            validationErrors.AddRange(
                options.ClusterConfiguration.DocumentCollections.SelectMany(
                    kv =>
                        ValidateClusterConfigItem(
                            "document collections",
                            kv)));
        }

        if (options.ClusterConfiguration?.EdgeCollections is
            {
                Count: > 0
            })
        {
            validationErrors.AddRange(
                options.ClusterConfiguration.EdgeCollections.SelectMany(
                    kv =>
                        ValidateClusterConfigItem(
                            "edge collections",
                            kv)));
        }

        if (options.MinutesBetweenChecks <= 0)
        {
            validationErrors.Add(
                $"Configuration validation error regarding '{nameof(options.MinutesBetweenChecks)}': It should be greater than 0, but is {options.MinutesBetweenChecks}!");
        }

        if (validationErrors.Any())
        {
            return ValidateOptionsResult.Fail(
                $"{nameof(ArangoConfiguration)} validation error(s) occurred:{Environment.NewLine}{string.Join(Environment.NewLine, validationErrors.Select((v, i) => $"{i + 1}. {v}"))}");
        }

        validationErrors.AddRange(ValidateExceptionConfigItem(options.ExceptionConfiguration));

        return ValidateOptionsResult.Success;
    }
}
