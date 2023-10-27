namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <summary>
///     The synchronization operation describe which operation are
///     possible on an item.
/// </summary>
public class SynchronizationOperations
{
    /// <summary>
    ///     The Configuration of the converter (to save a copy of an external id with the specified format if necessary).
    /// </summary>
    public ConverterConfiguration Converter { get; set; }

    /// <summary>
    ///     If an item can be deleted by force.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get property is needed.
    public bool ForceDelete { get; set; }

    /// <summary>
    ///     Operation that can be apply on an item (Nothing, Add, Remove, Update or All).
    /// </summary>
    public SynchronizationOperation Operations { get; set; } = SynchronizationOperation.Nothing;
}
