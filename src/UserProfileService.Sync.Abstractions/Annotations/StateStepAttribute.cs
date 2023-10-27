using System;

namespace UserProfileService.Sync.Abstraction.Annotations;

/// <summary>
///     This attribute tags the saga message with the
///     defined saga step.
/// </summary>
public class StateStepAttribute : Attribute
{
    /// <summary>
    ///     The saga step that tags the saga messages.
    /// </summary>
    public string SagaStepName { set; get; }

    /// <summary>
    ///     Creates the instance of <inheritdoc cref="StateStepAttribute" />
    /// </summary>
    /// <param name="sagaStepName">The name of the saga step.</param>
    public StateStepAttribute(string sagaStepName)
    {
        SagaStepName = sagaStepName;
    }
}
