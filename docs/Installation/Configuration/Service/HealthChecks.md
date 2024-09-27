# Configure Health-Checks
Here is a sample section on how to configure the health checks:

??? abstract "Health-Checks example configuration"
    ```json
    {
      "Delays": {
        "HealthCheck": "00:05:00",
        "HealthPush": "00:05:00"
      }
    }
    ```

`HealthCheck`- Specifies the interval at which health checks are performed. The value is represented in hours, minutes, and seconds format (HH:MM:SS). For example, **00:05:00** indicates a health check interval of 5 minutes.

`HealthPush`- Specifies the interval at which health push operations are performed. The value is represented in hours, minutes, and seconds format (HH:MM:SS). For example, **00:05:00** indicates a health push interval of 5 minutes.
