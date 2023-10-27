using Marten.Events;

namespace UserProfileService.MartenEventStore.UnitTests.Models;

internal class TestStreamAction : StreamAction
{
    /// <inheritdoc />
    public TestStreamAction(Guid id, string key, StreamActionType actionType, long version) : base(
        id,
        key,
        actionType)
    {
        GetType().BaseType!.GetProperty(nameof(Version))!.SetValue(this, version);
    }
}
