using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.V1.Models
{
    /// <summary>
    ///     Specifies a Reference to a UPS-Object.
    /// </summary>
    public class Reference
    {
        /// <summary>
        ///     The id pointing of the referenced object.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Specifies the type of the referenced object.
        /// </summary>
        public ObjectType Type { get; set; }
    }
}