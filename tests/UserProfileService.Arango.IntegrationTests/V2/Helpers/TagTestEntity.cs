using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Arango.IntegrationTests.V2.Helpers
{
    public class TagTestEntity : Tag
    {
        public string RelatedProfileId { get; set; }
    }
}
