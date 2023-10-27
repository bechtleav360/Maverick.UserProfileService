namespace UserProfileService.Events.Payloads;

/// <summary>
///     Class to parse <see cref="IPayload" /> with json sub types.
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class PayloadBase<TPayload> : IPayload where TPayload : class
{
    /// <summary>
    ///     If the based entity was synchronized from an external source.
    /// </summary>
    public bool IsSynchronized { set; get; }

    /// <summary>
    ///     Type of <see cref="IPayload" />
    /// </summary>
    public string PayloadType { get; } = typeof(TPayload).Name;
}
