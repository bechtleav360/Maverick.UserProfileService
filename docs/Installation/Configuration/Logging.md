# Logging

The configuration uses the default .NET Core "Logging" configuration of the MEL stack and extends it in an easy way.  
We are using `NLog` internally to write logs.

## Configuration LogLevel

> **Note:** If no configuration is provided the logframework will only log `Information` or higher (higher meaning greater loglevel value)

| LogLevel    | Value | Method         | Description                                                                                                                                                        |
|-------------|-------|----------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Trace       | 0     | LogTrace       | Contain the most detailed messages. These messages may contain sensitive app data. These messages are disabled by default and should not be enabled in production. |
| Debug       | 1     | LogDebug       | For debugging and development. Use with caution in production due to the high volume.                                                                              |
| Information | 2     | LogInformation | Tracks the general flow of the app. May have long-term value.                                                                                                      |
| Warning     | 3     | LogWarning     | For abnormal or unexpected events. Typically includes errors or conditions that don't cause the app to fail.                                                       |
| Error       | 4     | LogError       | For errors and exceptions that cannot be handled. These messages indicate a failure in the current operation or request, not an app-wide failure.                  |
| Critical    | 5     | LogCritical    | For failures that require immediate attention. Examples: data loss scenarios, out of disk space.                                                                   |
| None        | 6     |                | Specifies that a logging category should not write any messages.                                                                                                   |

> **Note:** In the `LogLevel` sub-section you can configure the category specific log level and the global log level.
> The most specific category configuration is used meaning if `SomeCategory` is set to Error but `SomeCategory.SubSomeCategory`
> is set to trace, than everything in `SomeCategory` only logs error or higher, but everything staring from `SomeCategory.SubSomeCategory` will log trace or higher

To specify the global loglevel use the `Default` category name

??? abstract "Logging example configuration"
    ``` json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Trace",
          "<CategoryName>": "<LogLevel as string>"
        }
      }
    }
    ```

##### Default configuration (console only)

??? abstract "Logging example console configuration"
    ``` json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information"
        }
      }
    }
    ```

## File Logging

To include file logging you need to add the following config-keys inside the "Logging" section

??? abstract "Logging example file configuration"
    ``` json
    {
      "Logging": {
        "EnableLogFile": true, // (Optional) default: false
        "LogFilePath": "logs" // (Optional) default: "logs"
        "LogFileMaxHistory": 3 // (Optional) default: 3
      }
    }
    ```

## Log format (Text or JSON)

To change the log format from JSON (default) to Plaintext change the `LogFormat` key to the value of `text` or `json`

??? abstract "Logging example log format configuration"
    ``` json
    {
      "Logging": {
        "LogFormat": "text"
      }
    }
    ```