namespace Maverick.UserProfileService.FilterUtility.Configuration
{
    /// <summary>
    ///     Configures the serialization of the wrapper for collection and objects
    /// </summary>
    public class FilterUtilityConfiguration
    {
        /// <summary>
        ///     Which characters are used at the end of an collection to wrap it
        /// </summary>
        public string CollectionContainerMarkerEnd { get; set; }

        /// <summary>
        ///     Which characters are used at the start of an collection to wrap it
        /// </summary>
        public string CollectionContainerMarkerStart { get; set; }

        /// <summary>
        ///     Returns a default configuration
        /// </summary>
        public static FilterUtilityConfiguration DefaultConfiguration =>
            new FilterUtilityConfiguration
            {
                FilterContainerMarkerStart = "(",
                FilterContainerMarkerEnd = ")",
                DefinitionContainerMarkerStart = "{",
                DefinitionContainerMarkerEnd = "}",
                CollectionContainerMarkerStart = "[",
                CollectionContainerMarkerEnd = "]",
                Separator = ",",
                StringMarker = "\""
            };

        /// <summary>
        ///     Which characters are used at the end of the definition to wrap it
        /// </summary>
        public string DefinitionContainerMarkerEnd { get; set; }

        /// <summary>
        ///     Which characters are used at the start of the definition to wrap it
        /// </summary>
        public string DefinitionContainerMarkerStart { get; set; }

        /// <summary>
        ///     Which characters are used at the end of the filter to wrap it
        /// </summary>
        public string FilterContainerMarkerEnd { get; set; }

        /// <summary>
        ///     Which characters are used at the start of the filter to wrap it
        /// </summary>
        public string FilterContainerMarkerStart { get; set; }

        /// <summary>
        ///     Which characters are used as a separator
        /// </summary>
        public string Separator { get; set; }

        /// <summary>
        ///     Which characters are used to mark string values
        /// </summary>
        public string StringMarker { get; set; }
    }
}
