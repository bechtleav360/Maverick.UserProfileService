using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Xunit.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Comparers
{
    public class TestingEqualityComparerForProfiles : IEqualityComparer<IProfile>
    {
        private readonly TestingEqualityComparerForGroups _groupComparer;
        private readonly ITestOutputHelper _outputHelper;
        private readonly TestingEqualityComparerForUsers _userComparer;

        public TestingEqualityComparerForProfiles(
            ITestOutputHelper outputHelper,
            TestingEqualityComparerForUsers userComparer,
            TestingEqualityComparerForGroups groupComparer)
        {
            _outputHelper = outputHelper;
            _userComparer = userComparer;
            _groupComparer = groupComparer;
        }

        public TestingEqualityComparerForProfiles(ITestOutputHelper outputHelper)
            : this(
                outputHelper,
                new TestingEqualityComparerForUsers(outputHelper),
                new TestingEqualityComparerForGroups(outputHelper))
        {
        }

        public bool Equals(
            IProfile x,
            IProfile y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is User userX && y is User userY)
            {
                return userX.Equals(userY, _userComparer);
            }

            if (x is Group groupX && y is Group groupY)
            {
                return groupX.Equals(groupY, _groupComparer);
            }

            if (x is UserBasic userBasicX && y is UserBasic userBasicY)
            {
                var comparer = new TestingEqualityComparerForUserBasic(_outputHelper);

                return comparer.Equals(userBasicX, userBasicY);
            }

            if (x is GroupBasic groupBasicX && y is GroupBasic groupBasicY)
            {
                var comparer = new TestingEqualityComparerForGroupBasic(_outputHelper);

                return comparer.Equals(groupBasicX, groupBasicY);
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Id == y.Id
                && x.Name == y.Name
                && x.DisplayName == y.DisplayName
                && x.ExternalIds.CompareExternalIds(y.ExternalIds, _outputHelper)
                && x.Kind == y.Kind
                && x.CreatedAt.Equals(y.CreatedAt)
                && x.UpdatedAt.Equals(y.UpdatedAt)
                && x.TagUrl == y.TagUrl;
        }

        public int GetHashCode(IProfile obj)
        {
            return HashCode.Combine(
                obj.Id,
                obj.Name,
                obj.DisplayName,
                obj.ExternalIds,
                (int)obj.Kind,
                obj.CreatedAt,
                obj.UpdatedAt,
                obj.TagUrl);
        }
    }

    public class TestingEqualityComparerForAssignmentObjects : IEqualityComparer<IAssignmentObject>
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestingEqualityComparerForAssignmentObjects(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public bool Equals(
            IAssignmentObject x,
            IAssignmentObject y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is RoleView roleViewX && y is RoleView roleViewY)
            {
                var rComparer = new TestingEqualityComparerForRoleView(_outputHelper);

                return rComparer.Equals(roleViewX, roleViewY);
            }

            if (x is FunctionView functionViewX && y is FunctionView functionViewY)
            {
                var fComparer = new TestingEqualityComparerForFunctionView(_outputHelper);

                return fComparer.Equals(functionViewX, functionViewY);
            }

            if (x is FunctionBasic functionBasicX && y is FunctionBasic functionBasicY)
            {
                var fComparerBasic = new TestingEqualityComparerForFunctionBasic(_outputHelper);

                return fComparerBasic.Equals(functionBasicX, functionBasicY);
            }

            if (x is RoleBasic roleX && y is RoleBasic roleY)
            {
                var rComparerBasic = new TestingEqualityComparerForRoleBasic(_outputHelper);

                return rComparerBasic.Equals(roleX, roleY);
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return (x.Id == y.Id
                    && x.Name == y.Name
                    && x.Type == y.Type)
                || y.LinkedProfiles == null
                || x.LinkedProfiles.SequenceEqual(y.LinkedProfiles, new TestingEqualityComparerForMembers());
        }

        public int GetHashCode(IAssignmentObject obj)
        {
            return HashCode.Combine(obj.Id, obj.Name, obj.Type, obj.LinkedProfiles);
        }
    }
}
