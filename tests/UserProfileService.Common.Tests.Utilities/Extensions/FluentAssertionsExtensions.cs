using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Bogus;
using FluentAssertions;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class FluentAssertionsExtensions
    {
        private static void CheckList(IAssertionContext<IEnumerable> context)
        {
            if (context.Expectation == null)
            {
                context.Subject.Should().NotBeNull();
            }
            else
            {
                context.Subject.Should().BeEquivalentTo(context.Expectation);
            }
        }
        
         /// <summary>
        ///     Asserts that the <see cref="HttpResponseMessage" /> response headers contain values that matches <paramref name="ex" /> in
        ///     <paramref name="headerName" /> header.
        /// </summary>
        /// <param name="assertions"></param>
        /// <param name="headerName">The name of the header to find.</param>
        /// <param name="expectedValuePattern">Expected <see cref="Regex"/> pattern to use to validate header values.</param>
        /// <param name="because">
        ///     A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        ///     is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        ///     Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        public static AndConstraint<HttpResponseMessageAssertions> HaveResponseHeaderValue(
            this HttpResponseMessageAssertions assertions,
            string headerName,
            string expectedValuePattern,
            string because = "",
            params object[] becauseArgs)
        {
            Continuation success = Execute.Assertion
                                          .ForCondition(assertions.Subject != null)
                                          .BecauseOf(because, becauseArgs)
                                          .FailWith(
                                              "Expected value pattern {0} to exist in header {1}{reason}, but HttpResponseMessage was <null>.",
                                              expectedValuePattern,
                                              headerName);

            if (success)
            {
                Execute.Assertion
                       .ForCondition(IsInResponseHeader(assertions.Subject, headerName, expectedValuePattern))
                       .BecauseOf(because, becauseArgs)
                       .FailWith(
                           "Expected value pattern {0} to exist in header {1}{reason}, but found {2}.",
                           expectedValuePattern,
                           headerName,
                           GetResponseHeaderValuesOrDefault(assertions.Subject, headerName));
            }

            return new AndConstraint<HttpResponseMessageAssertions>(assertions);
        }
         
        private static IEnumerable<string> GetResponseHeaderValuesOrDefault(
            HttpResponseMessage subject,
            string headerName)
        {
            return subject.Headers.TryGetValues(headerName, out IEnumerable<string> values)
                ? values
                : Enumerable.Empty<string>();
        }
        
        private static bool IsInResponseHeader(
            HttpResponseMessage subject,
            string headerName,
            string headerValuePattern)
        {
            return subject.Headers.TryGetValues(headerName, out IEnumerable<string> values)
                && values.All(v => Regex.IsMatch(v, headerValuePattern, RegexOptions.CultureInvariant));
        }
        
        /// <summary>
        ///     Asserts that the <see cref="HttpResponseMessage" /> response headers contain values that matches <paramref name="expectedValuePattern" /> in
        ///     <paramref name="header" /> header.
        /// </summary>
        /// <param name="assertions"></param>
        /// <param name="header">The header to find.</param>
        /// <param name="expectedValuePattern">Expected <see cref="Regex"/> pattern to use to validate header values.</param>
        /// <param name="because">
        ///     A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        ///     is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        ///     Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        public static AndConstraint<HttpResponseMessageAssertions> HaveResponseHeaderValue(
            this HttpResponseMessageAssertions assertions,
            HttpResponseHeader header,
            string expectedValuePattern,
            string because = "",
            params object[] becauseArgs)
        {
            return assertions.HaveResponseHeaderValue(header.ToString("G"), expectedValuePattern, because, becauseArgs);
        }

        private static void CheckList<TListElement>(IAssertionContext<IList<TListElement>> context)
        {
            if (context.Expectation == null
                || context.Expectation.Count == 0)
            {
                context.Subject.Should().BeNullOrEmpty();
            }
            else
            {
                context.Subject.Should().BeEquivalentTo(context.Expectation);
            }
        }

        public static EquivalencyAssertionOptions<TCompareObject> TreatEmptyListsAndNullTheSame<TCompareObject>(
            this EquivalencyAssertionOptions<TCompareObject> options)
        {
            options.Using<IEnumerable>(CheckList)
                .When(oInfo => typeof(IEnumerable).IsAssignableFrom(oInfo.RuntimeType));

            return options;
        }

        public static EquivalencyAssertionOptions<TCompareObject> TreatEmptyListsAndNullTheSame<TCompareObject, TListElement>(
            this EquivalencyAssertionOptions<TCompareObject> options,
            Expression<Func<TCompareObject, IList<TListElement>>> _)
        {
            options.Using<IList<TListElement>>(CheckList)
                .When(oInfo => typeof(IList<>).MakeGenericType(typeof(TListElement)).IsAssignableFrom(oInfo.RuntimeType));

            return options;
        }
    }
}
