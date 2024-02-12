using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UserProfileService.Messaging.ArangoDb.Extensions;
using Xunit;

namespace UserProfileService.Messaging.ArangoDb.UnitTests.Extensions
{
    public class ObjectExtensionTests
    {
        [Theory]
        [ClassData(typeof(ExtensionTestData))]
        public void GetName_Success(Expression<Func<TestObject, object>> exp, string baseParameter, string expected)
        {
            // Act && Assert
            Assert.Equal(expected, exp.GetName(baseParameter));
        }

        [Fact]
        public void GetName_on_predicate_expression_should_success()
        {
            Expression<Predicate<TestObject3>> exp = p => p.FinishedAt != null;
            string baseParameter = "x";
            string expected = "x.FinishedAt != null";

            Assert.Equal(expected, exp.GetName(baseParameter));
        }
    }

    public class ExtensionTestData : IEnumerable<object[]>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Expression<Func<TestObject, object>> GetExpression(Expression<Func<TestObject, object>> exp)
        {
            return exp;
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
                         {
                             GetExpression(o => o.Object.Name),
                             null,
                             "o.Object.Name"
                         };

            yield return new object[]
                         {
                             GetExpression(o => o.Object.Name),
                             "",
                             "o.Object.Name"
                         };

            yield return new object[]
                         {
                             GetExpression(o => o.Object.Name),
                             " ",
                             "o.Object.Name"
                         };

            yield return new object[]
                         {
                             GetExpression(t => t.Object.Name),
                             null,
                             "t.Object.Name"
                         };

            yield return new object[]
                         {
                             GetExpression(t => t.Object.Name),
                             "x",
                             "x.Object.Name"
                         };

            yield return new object[]
                         {
                             GetExpression(o => o.Object),
                             "x",
                             "x.Object"
                         };

            yield return new object[]
                         {
                             GetExpression(o => o.Object.Name),
                             "s.t",
                             "s.t.Object.Name"
                         };
        }
    }

    public class TestObject3
    {
        public DateTime? FinishedAt { get; set; }
    }

    public class TestObject
    {
        public TestObject2 Object { get; set; }
    }

    public class TestObject2
    {
        public string Name { get; set; }
    }
}
