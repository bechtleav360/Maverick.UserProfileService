namespace UserProfileService.Hosting.Tracing
{
    /// <summary>
    /// Generic options used to configure the tracing
    /// </summary>
    public class TracingOptions
    {
        /// <summary>
        /// A service name used to decorate every span for easier lookup in tracing tools like Jaeger or Zipkin etc.
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;
        
        /// <summary>
        /// The OTLP endpoint where the trace spans are sent to
        /// </summary>
        public Uri? OtlpEndpoint { get; set; }
    }
}
