using System;
using FluentAssertions.Equivalency;

namespace UserProfileService.Common.Tests.Utilities.Utilities
{
    public class ItShouldOptionsBuilder<T>
    {
        public Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> Options { get; private set; }

        public void Configure(Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> options)
        {
            Options = options;
        }
    }
}
