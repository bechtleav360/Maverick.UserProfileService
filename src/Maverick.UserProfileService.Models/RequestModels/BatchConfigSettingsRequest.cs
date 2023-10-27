using System;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     Request to set config for multiple
    /// </summary>
    public class BatchConfigSettingsRequest
    {
        /// <summary>
        ///     Config to be set for the given profiles.
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        ///     Profiles to be set the config for.
        /// </summary>
        public string[] ProfileIds { get; set; } = Array.Empty<string>();
    }
}
