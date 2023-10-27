namespace UserProfileService.Common.Tests.Utilities.TestModels
{
    // just for tests
    public interface IPayloadObject<TPayload>
    {
        public TPayload Payload { get; set; }
    }
}
