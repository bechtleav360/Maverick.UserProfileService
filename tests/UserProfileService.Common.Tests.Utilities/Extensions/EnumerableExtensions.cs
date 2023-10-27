using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.Tests.Utilities.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TElement> DoFunctionForEachAndReturn<TElement>(
            this IEnumerable<TElement> sequence,
            Action<TElement> function)
        {
            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            List<TElement> list = sequence as List<TElement> ?? sequence?.ToList();

            if (list == null)
            {
                return null;
            }

            list.ForEach(function);

            return list;
        }

        /// <summary>
        ///     Returns a random element of a sequence.
        /// </summary>
        /// <typeparam name="TElement">The type of the element</typeparam>
        /// <param name="sequence">The sequence where an element should be randomly picked.</param>
        /// <returns>A random element of the sequence.</returns>
        public static TElement PickRandom<TElement>(this IEnumerable<TElement> sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            List<TElement> list = sequence as List<TElement> ?? sequence?.ToList();

            var random = new Random();
            int index = random.Next(list.Count);

            return list[index];
        }

        /// <summary>
        ///     Shuffle any (I)List with an extension method based on the Fisher-Yates shuffle.
        /// </summary>
        public static void Shuffle<TElement>(
            this IList<TElement> collection,
            Random randomNumberGenerator = null)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.Count == 0)
            {
                return;
            }

            Random random = randomNumberGenerator ?? new Random();

            int n = collection.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                // swap elements
                (collection[k], collection[n]) = (collection[n], collection[k]);
            }
        }

        public static IEnumerable<KeyValuePair<TKey, TVal>> AsKeyValuePairs<TElem, TKey, TVal>(
            this IEnumerable<TElem> elements,
            Func<TElem, TKey> keySelector,
            Func<TElem, TVal> valueSelector)
        {
            return elements.Select(
                item => new KeyValuePair<TKey, TVal>(
                    keySelector.Invoke(item),
                    valueSelector.Invoke(item)));
        }

        public static List<TElem> ReverseAndReturn<TElem>(this IEnumerable<TElem> collection)
        {
            List<TElem> temp = collection as List<TElem>
                ?? collection?.ToList()
                ?? new List<TElem>();

            temp.Reverse();

            return temp;
        }
    }
}
