using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Some user or service that initiates the command, event or process.
    /// </summary>
    public class Initiator
    {
        /// <summary>
        ///     Identifier of initiator.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Human friendly name representing the initiator.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Defines the type of the initiator.
        /// </summary>
        public InitiatorType Type { get; set; } = InitiatorType.Unknown;
    }
}
