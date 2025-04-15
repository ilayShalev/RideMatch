using System;
using System.Collections.Generic;
using System.Linq;

namespace RideMatch.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        // Finds the first matching item or returns default
        public static T FindOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null)
                return default;

            return collection.FirstOrDefault(predicate);
        }

        // Converts a collection to a string with separators
        public static string Join<T>(this IEnumerable<T> collection, string separator)
        {
            if (collection == null)
                return string.Empty;

            return string.Join(separator, collection);
        }

        // Gets a random element from a collection
        public static T RandomElement<T>(this IEnumerable<T> collection, Random random = null)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (!collection.Any())
                throw new InvalidOperationException("Cannot select a random element from an empty collection");

            random = random ?? new Random();
            int index = random.Next(0, collection.Count());

            return collection.ElementAt(index);
        }

        // Creates a deep copy of a collection if the items implement ICloneable
        public static List<T> DeepCopy<T>(this IEnumerable<T> collection) where T : ICloneable
        {
            if (collection == null)
                return new List<T>();

            return collection.Select(item => (T)item.Clone()).ToList();
        }
    }
}