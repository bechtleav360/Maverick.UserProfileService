using System;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Defines a object by id and type.
    /// </summary>
    public class ObjectIdent : IObjectIdent, IComparable<ObjectIdent>
    {
        /// <summary>
        ///     Unique identifier of object.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Id { get; set; }

        /// <summary>
        ///     Type of object.
        /// </summary>
        public ObjectType Type { get; set; } = ObjectType.Unknown;

        /// <summary>
        ///     Create an instance of <see cref="ObjectIdent" />.
        /// </summary>
        public ObjectIdent()
        {
        }

        /// <summary>
        ///     Create an instance of <see cref="ObjectIdent" />.
        /// </summary>
        /// <param name="id">Identifier of the object.</param>
        /// <param name="type">Type of the object.</param>
        public ObjectIdent(string id, ObjectType type)
        {
            Id = id;
            Type = type;
        }

        /// <inheritdoc />
        public int CompareTo(ObjectIdent other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            int idComparison = string.Compare(Id, other.Id, StringComparison.InvariantCulture);

            if (idComparison != 0)
            {
                return idComparison;
            }

            return Type.CompareTo(other.Type);
        }
    }
}
