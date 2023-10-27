using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Annotations;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Data;

[TestData(true, nameof(PathTreeTestsData))]
public class PathTreeTestsData
{
    public static class OneNodeWithGroupAndRangeConditionsFirstCase
    {
        [ProfileVertexRootNode]
        public const string RelatedObjectId = "ursRoot-D684BC7C-A9EF-457F-8E0D-0A7DF5911390";
        
        [ProfileVertexRootNode]
        public const string ObjectIdFirstGroup = "grp-37E75AF7-114A-403A-ADA3-26C978CA98B8";
        
        [ProfileVertexNode(RelatedObjectId)]
        public const string ObjectIdFirstGroupClone = ObjectIdFirstGroup;
        
        [ProfileVertexEdge(RelatedObjectId, ObjectIdFirstGroup)]
        public const int RangeConditionsForEdge = 5;
    }
    
    public static class OneNodeWithGroupAndRangeConditionsSecondCase
    {
        [ProfileVertexRootNode]
        public const string RelatedObjectId = "urs-044098D6-C3F4-4AF3-9893-D28023548330";
        
        [ProfileVertexRootNode]
        public const string ObjectIdFirstGroup = "group-1BF204CF-8BA4-4BAA-BA38-8BF074B04FDA";
        
        [ProfileVertexNode(RelatedObjectId)]
        public const string ObjectIdFirstGroupClone = ObjectIdFirstGroup;
        
        [ProfileVertexEdge(RelatedObjectId, ObjectIdFirstGroup)]
        public const int RangeConditionsForEdge = 5;
    }
    
    public static class OneUserWithGroupAndRangeConditionThirdCase
    {
        [Group("FanClubAndreas")]
        public const string GroupProfile = "group-FanClubAndreas-1BF204CF-8BA4-4BAA-BA38-8BF074B04FDA";
     
        [User("Andreas")]
        [AssignedTo(GroupProfile, ContainerType.Group, 0, 12)]
        public const string UserProfile = "urs-Andreas-5B88CBF5-C492-4C93-AE2B-F91A06C8CFDF";
    }
    
    public static class OneUserWithGroupAndRangeConditionFouthCase
    {
        [Group("FanClubNone")]
        public const string GroupProfile = "group-FanClubNone-878A5DEA-2CD7-40ED-943E-B67F2B47E35E";
     
        [User("Stefan")]
        [AssignedTo(GroupProfile, ContainerType.Group, 0, 12)]
        public const string UserProfile = "user-Stefan-C384E626-AF48-4BFA-940E-A93AB591E90E";
    }
    
    public static class OneUserWithGroupAndRangeConditionFifthCase
    {
        [Group("Robotik")]
        public const string GroupProfile ="group-Robotik-BDB6E9B2-2054-4E97-80B1-83E4B6C2E318";

        [User("Abdul")]
        [AssignedTo(GroupProfile, ContainerType.Group, 0, 12)]
        public const string UserProfile = "user-Abdul-FF3385A9-2680-4593-8AB9-64774980C50D";
    }
    
}
