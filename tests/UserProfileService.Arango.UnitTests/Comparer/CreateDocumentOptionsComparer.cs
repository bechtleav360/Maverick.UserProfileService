using System;
using System.Collections.Generic;
using FluentAssertions;
using Maverick.Client.ArangoDb.Public.Models.Document;

namespace UserProfileService.Arango.UnitTests.Comparer
{
    /// <summary>
    ///     A Comparer for the type <see cref="CreateDocumentOptions" />
    /// </summary>
    public class CreateDocumentOptionsComparer : IEqualityComparer<CreateDocumentOptions>
    {
        /// <inheritdoc />
        public bool Equals(CreateDocumentOptions x, CreateDocumentOptions y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            try
            {
                x.Should().BeEquivalentTo(y);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public int GetHashCode(CreateDocumentOptions obj)
        {
            var hashcode = new HashCode();
            hashcode.Add(obj.OverWriteMode);
            hashcode.Add(obj.Overwrite);
            hashcode.Add(obj.ReturnNew);
            hashcode.Add(obj.ReturnOld);
            hashcode.Add(obj.WaitForSync);

            return hashcode.ToHashCode();
        }
    }
}
