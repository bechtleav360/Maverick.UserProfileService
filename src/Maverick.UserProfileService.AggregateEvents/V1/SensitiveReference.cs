using Maverick.UserProfileService.AggregateEvents.Common;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Contains properties to identify the reference to a specified version of sensitive data.
    /// </summary>
    public class SensitiveReference : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Identifies a resource.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     Identifies the type of the related resource.
        /// </summary>
        public string RelatedType { get; set; }

        ///<inheridoc />
        public string Type => nameof(SensitiveReference);

        /// <summary>
        ///     Version number of the estimated state.
        /// </summary>
        public long Version { get; set; }
    }
}
