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
    public class ConditionObjectIdent : IObjectIdent
    {
        /// <summary>
        ///     Condition when the assignment to the object is valid.
        /// </summary>
        public RangeCondition[] Conditions { get; set; } = Array.Empty<RangeCondition>();

        /// <summary>
        ///     Unique identifier of object.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Id { get; set; }

        /// <summary>
        ///     Type of object.
        /// </summary>
        public ObjectType Type { get; set; }

        /// <summary>
        ///     Create an instance of <see cref="ConditionObjectIdent" />.
        /// </summary>
        public ConditionObjectIdent()
        {
        }

        /// <summary>
        ///     Create an instance of <see cref="ConditionObjectIdent" />.
        /// </summary>
        /// <param name="id">Identifier of the object.</param>
        /// <param name="type">Type of the object.</param>
        public ConditionObjectIdent(string id, ObjectType type)
        {
            Id = id;
            Type = type;
        }

        /// <summary>
        ///     Create an instance of <see cref="ConditionObjectIdent" />.
        /// </summary>
        /// <param name="id">Identifier of the object.</param>
        /// <param name="type">Type of the object.</param>
        /// <param name="conditions">Condition when the assignment to the object is valid</param>
        public ConditionObjectIdent(string id, ObjectType type, params RangeCondition[] conditions)
        {
            Id = id;
            Type = type;
            Conditions = conditions;
        }
    }
}
