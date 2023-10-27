using AutoFixture;
using AutoFixture.Xunit2;
using UserProfileService.Commands;

namespace UserProfileService.EventCollector.UnitTests
{
    public class AutoDataWithoutExceptionInformation : AutoDataAttribute
    {
        public AutoDataWithoutExceptionInformation() : base(GetWithoutExceptionFixture)
        {
            
        }

        private static Fixture GetWithoutExceptionFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<SubmitCommandFailure>(c => c.Without(o => o.Exception));

            return fixture;
        }
    }
}
