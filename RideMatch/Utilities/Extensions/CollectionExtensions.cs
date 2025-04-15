using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideMatch.Utilities.Extensions
{
    public static class CollectionExtensions
    {
        // Finds the first matching item or returns default
        public static T FindOrDefault<T>(this IEnumerable<T> collection, Func<T, bool> predicate);

        // Converts a collection to a string with separators
        public static string Join<T>(this IEnumerable<T> collection, string separator);

        // Gets a random element from a collection
        public static T RandomElement<T>(this IEnumerable<T> collection, Random random = null);
    
        // Creates a deep copy of a collection if the items implement ICloneable
        public static List<T> DeepCopy<T>(this IEnumerable<T> collection) where T : ICloneable
    }
}
