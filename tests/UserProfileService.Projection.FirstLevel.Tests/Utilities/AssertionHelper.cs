using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using UserProfileService.Common;

namespace UserProfileService.Projection.FirstLevel.Tests.Utilities
{
    public class AssertionHelper
    {
        public static void EventTupleEquivalent(
            IList<EventTuple> tuple,
            IList<EventTuple> expected)
        {
            tuple.Should()
                .BeEquivalentTo(
                    expected,
                    opt => opt
                        .Excluding(t => t.Event.EventId)
                        .Excluding(t => t.Event.MetaData.Batch)
                        .Using<DateTime>(
                            context => context.Subject.Should().BeCloseTo(context.Expectation, 1.Seconds()))
                        .WhenTypeIs<DateTime>()
                        .RespectingRuntimeTypes());
        }
    }
}
