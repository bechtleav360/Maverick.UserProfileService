using UserProfileService.Projection.Abstractions.EnumModels;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     Contains the relation to an entity an its profile.
/// </summary>
public class FirstLevelRelationProfile
{
    /// <summary>
    ///     The profile that has an relation to a given entity.
    /// </summary>
    public IFirstLevelProjectionProfile Profile { set; get; }

    /// <summary>
    ///     The relation that the profile has with an entity.
    /// </summary>
    public FirstLevelMemberRelation Relation { get; set; }

    /// <summary>
    ///     Creates the object <see cref="FirstLevelRelationProfile" />.
    /// </summary>
    /// <param name="profile">The profile that is needed to create the <see cref="FirstLevelRelationProfile" />.</param>
    /// <param name="relation">States if the profile is a direct or indirect member. </param>
    public FirstLevelRelationProfile(IFirstLevelProjectionProfile profile, FirstLevelMemberRelation relation)
    {
        Profile = profile;
        Relation = relation;
    }

    /// <summary>
    ///     Creates the object <see cref="FirstLevelRelationProfile" />.
    /// </summary>
    public FirstLevelRelationProfile()
    {
    }
}
