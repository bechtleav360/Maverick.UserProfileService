using JsonSubTypes;
using Newtonsoft.Json;

namespace UserProfileService.Events.Payloads;

/// <summary>
///     Defines the payload of a saga message.
/// </summary>
[JsonConverter(typeof(JsonSubtypes), "PayloadType")]
public interface IPayload
{
    /// <summary>
    ///     Type of <see cref="IPayload" />
    /// </summary>
    string PayloadType { get; }
}
