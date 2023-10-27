using System;
using System.Linq;
using UserProfileService.Sync.Models.State;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="Process" />s and the corresponding implementations.
/// </summary>
public static class ProcessExtension
{
    /// <summary>
    ///     Sums the number of all operations for the given step.
    /// </summary>
    /// <param name="process">Process of step to get number from.</param>
    /// <param name="stepId">Id of step to get number from.</param>
    /// <returns>Total number of operations for given step.</returns>
    public static int GetFinalNumberOfStep(this Process process, string stepId)
    {
        return process.Systems.Select(
                s => s.Value.Steps.TryGetValue(stepId, out Step step)
                    ? step.Final.Total
                    : 0)
            .Sum();
    }

    /// <summary>
    ///     Sums the number of all handled operations for the given step.
    /// </summary>
    /// <param name="process">Process of step to get number from.</param>
    /// <param name="stepId">Id of step to get number from.</param>
    /// <returns>Total number of operations for given step.</returns>
    public static int GetHandledNumberOfStep(this Process process, string stepId)
    {
        return process?.Systems?.Select(
                    s => s.Value.Steps.TryGetValue(stepId, out Step step)
                        ? step.Handled.Total
                        : 0)
                .Sum()
            ?? 0;
    }

    /// <summary>
    ///     Updates the<see cref="Step.UpdatedAt" /> time of the current step and also adjusts the time of the current system.
    /// </summary>
    /// <param name="process">Process where the current step is to be updated.</param>
    /// <returns>The updated process.</returns>
    public static Process UpdateStepTime(this Process process)
    {
        if (string.IsNullOrWhiteSpace(process?.System))
        {
            return process;
        }

        if (string.IsNullOrWhiteSpace(process.Step))
        {
            return process;
        }

        process.UpdateSystemTime();

        process.CurrentStep.UpdatedAt = DateTime.UtcNow;

        return process;
    }

    /// <summary>
    ///     Updates the <see cref="Models.State.System.UpdatedAt" /> time of the current system.
    /// </summary>
    /// <param name="process">Process where the current system is to be updated.</param>
    /// <returns>The updated process.</returns>
    public static Process UpdateSystemTime(this Process process)
    {
        if (string.IsNullOrWhiteSpace(process?.System))
        {
            return process;
        }

        process.UpdateProcessTime();
        process.CurrentSystem.UpdatedAt = DateTime.UtcNow;

        return process;
    }

    /// <summary>
    ///     Updates the <see cref="Process.UpdatedAt" /> time of the current process.
    /// </summary>
    /// <param name="process">Process to be updated.</param>
    /// <returns>The updated process.</returns>
    public static Process UpdateProcessTime(this Process process)
    {
        if (process == null)
        {
            return null;
        }

        process.UpdatedAt = DateTime.UtcNow;

        return process;
    }

    /// <summary>
    ///     Set the status to the current step and update the time of step.
    /// </summary>
    /// <param name="process">Process of step to update.</param>
    /// <param name="status">Future status of step.</param>
    /// <returns>The updated process.</returns>
    public static Process SetStepStatus(this Process process, StepStatus status)
    {
        if (string.IsNullOrWhiteSpace(process?.System))
        {
            return process;
        }

        if (string.IsNullOrWhiteSpace(process.Step))
        {
            return process;
        }

        process.UpdateStepTime();
        process.CurrentStep.Status = status;

        return process;
    }

    /// <summary>
    ///     Set the status to the process and update the time of process.
    /// </summary>
    /// <param name="process">Process to update.</param>
    /// <param name="status">Future status of process.</param>
    /// <returns>The updated process.</returns>
    public static Process SetProcessStatus(this Process process, ProcessStatus status)
    {
        if (process == null)
        {
            return null;
        }

        process.UpdateProcessTime();
        process.Status = status;

        return process;
    }

    /// <summary>
    ///     Update process properties after it has been aborted
    /// </summary>
    /// <param name="process"> Process to abort</param>
    /// <param name="processHasFailed"> True if the aborted process already failed (not started)</param>
    /// <returns>The updated process.</returns>
    public static Process AbortProcess(this Process process, bool processHasFailed = false)
    {
        if (process == null)
        {
            return null;
        }

        DateTime updateTime = DateTime.UtcNow;

        process.Status = processHasFailed ? ProcessStatus.Failed : ProcessStatus.Aborted;
        process.FinishedAt = updateTime;
        process.UpdatedAt = updateTime;

        return process;
    }
}
