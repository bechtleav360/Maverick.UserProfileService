using AutoMapper;
using UserProfileService.Sync.Abstraction.Models.Entities;

namespace UserProfileService.Sync.Projection.UnitTests.Utilities;

public static class CloningHelpers
{
    private static readonly IMapper _defaultMapper =
        new Mapper(new MapperConfiguration(c => c.AddProfile(typeof(CloningProfiles))));

    public static OrganizationSync CloneOrganizationSync(
        OrganizationSync original)
    {
        return _defaultMapper.Map<OrganizationSync>(original);
    }

    private class CloningProfiles : Profile
    {
        public CloningProfiles()
        {
            CreateMap<OrganizationSync, OrganizationSync>();
        }
    }
}
