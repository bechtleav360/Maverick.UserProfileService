namespace UserProfileService.Sync.Models.State;

/// <summary>
///     Temporary overview of current step process
/// </summary>
public class StepOperationsTemporary
{
    /// <summary>
    ///     Number of analyzed entities during sync process of current system.
    /// </summary>
    public int Analyzed { get; set; }

    /// <summary>
    ///     Number of total entities found in source system, like LDAP.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Entities { get; set; }

    /// <summary>
    ///     The temporary number of operations performed without differentiating between success/failure or
    ///     create/update/delete.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => Get modifier is needed.
    public int Handled { get; set; }
}
