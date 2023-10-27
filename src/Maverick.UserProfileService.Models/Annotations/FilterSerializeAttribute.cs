using System;

namespace Maverick.UserProfileService.Models.Annotations
{
    /// <summary>
    ///     Defines the value of the regarding enum field that will be used when de-serializing the related query string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FilterSerializeAttribute : Attribute
    {
        /// <summary>
        ///     The value of the enum field that will be used during (de-)serialization.
        /// </summary>
        public string SerializationValue { get; set; }

        /// <summary>
        ///     Initializes a new instance of <see cref="FilterSerializeAttribute" /> with a specified
        ///     <paramref name="serializationValue" />.
        /// </summary>
        /// <param name="serializationValue">The value of the enum field that will be used during (de-)serialization.</param>
        public FilterSerializeAttribute(string serializationValue)
        {
            SerializationValue = serializationValue;
        }
    }
}
