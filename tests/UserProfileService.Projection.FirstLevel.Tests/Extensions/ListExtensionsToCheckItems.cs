using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.FirstLevel.Tests.Extensions
{
    internal static class ListExtensionsToCheckItems
    {
        public static bool CheckAndRemoveItem(this List<string> ids, string id)
        {
            if (ids == null || id == null)
            {
                return false;
            }

            lock (ids)
            {
                int position = ids.IndexOf(id);

                if (position == -1)
                {
                    return false;
                }

                ids.RemoveAt(position);

                return true;
            }
        }

        public static bool CheckAndRemoveObjectIdent(this List<ObjectIdent> ids, ObjectIdent id)
        {
            if (ids == null || id == null)
            {
                return false;
            }

            lock (ids)
            {
                ObjectIdent toRemove = ids.FirstOrDefault(objId => objId.Id == id.Id && objId.Type == id.Type);

                if (toRemove == null)
                {
                    return false;
                }

                ids.Remove(toRemove);

                return true;
            }
        }

        public static bool CheckAndRemoveObjectIdentList(
            this List<ObjectIdent> exceptedList,
            List<ObjectIdent> givenList)
        {
            if (exceptedList == null || givenList == null)
            {
                return false;
            }

            lock (exceptedList)
            {
                int deleted = exceptedList.RemoveAll(
                    objId =>
                        givenList.Any(elementItem => objId.Id == elementItem.Id && objId.Type == elementItem.Type));

                return deleted > 0;
            }
        }
    }
}
