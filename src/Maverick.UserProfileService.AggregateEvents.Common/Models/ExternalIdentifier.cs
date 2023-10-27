namespace Maverick.UserProfileService.AggregateEvents.Common.Models
{
    /// <summary>
    ///     Model that describes an external identifier.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class ExternalIdentifier
    {
        /// <summary>
        ///     Unique identifier to an entity in external system.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     True if the present Id is a converted one.
        /// </summary>
        public bool IsConverted { get; set; }

        /// <summary>
        ///     Unique key of the external system.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Create an instance of <see cref="ExternalIdentifier" />
        /// </summary>
        public ExternalIdentifier()
        {
        }

        /// <summary>
        ///     Create an instance of <see cref="ExternalIdentifier" />
        /// </summary>
        /// <param name="id">Identifier of entity.</param>
        /// <param name="source">Key of source system the identifier is from.</param>
        /// <param name="isConverted">True if the id has been converted before, else false.</param>
        public ExternalIdentifier(string id, string source, bool isConverted = false)
        {
            Id = id;
            Source = source;
            IsConverted = isConverted;
        }
    }
}
