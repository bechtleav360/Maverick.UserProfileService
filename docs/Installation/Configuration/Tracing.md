# Tracing

Basic configuration requires you to set at least a `ServiceName`. The service name will be displayed in the trace graph and used to correlate the logs.
If you also provide an `OtlpEndpoint` URI the OtlpExporter will be setup to send traces via GRPC in the OTLP format to the provided endpoint.


An example of the trace configuration can resemble this:

```json
{
    "Tracing": {
        "OtlpEndpoint": "http://localhost:4317",
        "ServiceName": "userprofile-service"
    }
}
```