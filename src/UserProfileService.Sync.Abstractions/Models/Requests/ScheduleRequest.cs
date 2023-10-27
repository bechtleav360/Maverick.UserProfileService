namespace UserProfileService.Sync.Abstraction.Models.Requests;

/// <summary>
///     Request to change status of sync schedule.
/// </summary>
public class ScheduleRequest
{
    /// <summary>
    ///     Defines whether the sync by the task scheduler can run at a certain interval.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="ScheduleRequest" />.
    /// </summary>
    /// <param name="isActive">Defines whether the sync by the task scheduler can run at a certain interval.</param>
    public ScheduleRequest(bool isActive)
    {
        IsActive = isActive;
    }
}
