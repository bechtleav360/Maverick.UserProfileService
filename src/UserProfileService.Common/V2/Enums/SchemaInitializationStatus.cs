namespace UserProfileService.Common.V2.Enums;

/// <summary>
///     Represents the initialization status of a schema.
/// </summary>
public enum SchemaInitializationStatus
{
    /// <summary>
    ///     When the schema was last checked, it last check was still valid
    ///     and the check was skipped.
    /// </summary>
    WaitingForNextCheck,
    /// <summary>
    ///     The schema has been checked and exists.
    /// </summary>
    Checked,
    /// <summary>
    ///     The schema has been created.
    /// </summary>
    SchemaCreated,
    /// <summary>
    ///     An error occured during initialization.
    /// </summary>
    ErrorOccurred
}
