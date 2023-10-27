using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace UserProfileService.Hosting.Tracing
{
    /// <summary>
    ///     configure OpenTelemetry tracing
    /// </summary>
    public static class TracingExtensions
    {
        private const int VersionPrefixIdLength = 3;
        private const int TraceIdLength = 32;
        private const int VersionAndTraceIdLength = 36;
        private const int SpanIdLength = 16;
        private const int VersionAndTraceIdAndSpanIdLength = 53;
        private const int OptionsLength = 2;
        private const int TraceParentLengthV0 = 55;

        /// <summary>
        ///     setup OpenTelemetry tracing including AspNetCore + HttpClient instrumentation
        /// </summary>
        /// <param name="services">instance of <see cref="IServiceCollection" /> to configure</param>
        /// <param name="setupTracingOptions">Configure the tracing option</param>
        /// <param name="tracerProviderBuilder">A builder to further configure the tracer provider</param>
        /// <exception cref="ArgumentNullException">thrown when a required argument is null</exception>
        /// <exception cref="ArgumentException">thrown when the service name is not set</exception>
        /// <returns>instance of <paramref name="services" /> after configuration</returns>
        public static IServiceCollection AddUserProfileServiceTracing(
            this IServiceCollection services,
            Action<TracingOptions> setupTracingOptions,
            Action<TracerProviderBuilder>? tracerProviderBuilder = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupTracingOptions is null)
            {
                throw new ArgumentNullException(nameof(setupTracingOptions));
            }

            var tracingOptions = new TracingOptions();
            setupTracingOptions(tracingOptions);

            if (string.IsNullOrWhiteSpace(tracingOptions.ServiceName))
            {
                throw new ArgumentException("You need to provide a service name for tracing");
            }

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            services.AddOpenTelemetryTracing(
                builder =>
                {
                    if (!string.IsNullOrWhiteSpace(tracingOptions.ServiceName))
                    {
                        var attributes = new[]
                        {
                            new KeyValuePair<string, object>("service", tracingOptions.ServiceName)
                        };

                        // decorate our service name so we can find it when we look inside tracing tools
                        builder.SetResourceBuilder(
                            ResourceBuilder.CreateDefault()
                                .AddService(tracingOptions.ServiceName)
                                .AddAttributes(attributes));
                    }

                    builder.AddAspNetCoreInstrumentation(
                        options => options.Enrich
                            = (activity, eventName, rawObject) =>
                            {
                                if (eventName.Equals("OnStartActivity"))
                                {
                                    if (rawObject is HttpRequest httpRequest)
                                    {
                                        HttpContext context = httpRequest.HttpContext;
                                        activity.AddTag("http.scheme", httpRequest.Scheme);
                                        activity.AddTag("http.client_ip", context.Connection.RemoteIpAddress);
                                        activity.AddTag("http.request_content_length", httpRequest.ContentLength);
                                        activity.AddTag("http.request_content_type", httpRequest.ContentType);
                                    }
                                }
                                else if (rawObject is HttpResponse response)
                                {
                                    activity.AddTag("http.response_content_length", response.ContentLength);
                                    activity.AddTag("http.response_content_type", response.ContentType);
                                }
                            });

                    builder.AddHttpClientInstrumentation();

                    if (tracingOptions.OtlpEndpoint != null)
                    {
                        // This switch must be set before creating the GrpcChannel/HttpClient.
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

                        builder.AddOtlpExporter(
                            options =>
                            {
                                options.Endpoint = tracingOptions.OtlpEndpoint;
                                options.Protocol = OtlpExportProtocol.Grpc;
                            });
                    }

                    tracerProviderBuilder?.Invoke(builder);
                });

            return services;
        }

        /// <summary>
        /// Parses an trace parent string and extracts <see cref="ActivityTraceId"/>, <see cref="ActivitySpanId"/>
        /// and <see cref="ActivityTraceFlags"/>.
        /// See: https://github.com/w3c/distributed-tracing/blob/master/trace_context/HTTP_HEADER_FORMAT.md
        /// Example of a trace parent: 00-0af7651916cd43dd8448eb211c80319c-00f067aa0ba902b7-01
        /// Format: {version}-{traceId}-{spanId}-{flags}
        /// </summary>
        /// <param name="traceParent">The trace parent string to parse</param>
        /// <param name="traceId">The parsed trace id</param>
        /// <param name="spanId">The parsed span id</param>
        /// <param name="traceFlags">The parsed trace options</param>
        /// <returns>true when a valid trace parent could be parsed, false otherwise</returns>
        public static bool TryParseTraceParent(
            this string traceParent,
            out ActivityTraceId traceId,
            out ActivitySpanId spanId,
            out ActivityTraceFlags traceFlags)
        {
            traceId = default;
            spanId = default;
            traceFlags = default;
            bool bestAttempt = false;

            if (string.IsNullOrWhiteSpace(traceParent) || traceParent.Length < TraceParentLengthV0)
            {
                return false;
            }

            // if version does not end with delimiter
            if (traceParent[VersionPrefixIdLength - 1] != '-')
            {
                return false;
            }

            byte version0;
            byte version1;

            try
            {
                version0 = HexCharToByte(traceParent[0]);
                version1 = HexCharToByte(traceParent[1]);
            }
            catch (ArgumentOutOfRangeException)
            {
                // it's ok to still parse trace state
                return false;
            }

            switch (version0)
            {
                case 0xf when version1 == 0xf:
                    return false;
                case > 0:
                    // expected version is 00
                    // for higher versions - best attempt parsing of trace id, span id, etc.
                    bestAttempt = true;

                    break;
            }

            if (traceParent[VersionAndTraceIdLength - 1] != '-')
            {
                return false;
            }

            try
            {
                traceId = ActivityTraceId.CreateFromString(
                    traceParent.AsSpan().Slice(VersionPrefixIdLength, TraceIdLength));
            }
            catch (ArgumentOutOfRangeException)
            {
                // it's ok to still parse trace state
                return false;
            }

            if (traceParent[VersionAndTraceIdAndSpanIdLength - 1] != '-')
            {
                return false;
            }

            byte options1;

            try
            {
                spanId = ActivitySpanId.CreateFromString(
                    traceParent.AsSpan().Slice(VersionAndTraceIdLength, SpanIdLength));

                options1 = HexCharToByte(traceParent[VersionAndTraceIdAndSpanIdLength + 1]);
            }
            catch (ArgumentOutOfRangeException)
            {
                // it's ok to still parse trace state
                return false;
            }

            if ((options1 & 1) == 1)
            {
                traceFlags |= ActivityTraceFlags.Recorded;
            }

            if ((!bestAttempt) && (traceParent.Length != VersionAndTraceIdAndSpanIdLength + OptionsLength))
            {
                return false;
            }

            if (bestAttempt)
            {
                if ((traceParent.Length > TraceParentLengthV0) && (traceParent[TraceParentLengthV0] != '-'))
                {
                    return false;
                }
            }

            return true;
        }

        private static byte HexCharToByte(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }

            if (c >= 'a' && c <= 'f')
            {
                return (byte)(c - 'a' + 10);
            }

            throw new ArgumentOutOfRangeException(nameof(c), c, "Must be within: [0-9] or [a-f]");
        }
    }
}
