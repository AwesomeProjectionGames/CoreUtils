using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Get a random element of the provided Enumerable
        /// </summary>
        /// <returns>A random element using Random from Unity.</returns>
        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(Random.Range(0, enumerable.Count()));
        }
    
        /// <summary>
        /// Reorder randomly the enumerable
        /// </summary>
        /// <returns>A new IEnumerable with element in random order</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.OrderBy(x => Random.value);
        }
        
        /// <summary>
        /// Filter the enumerable to get only the elements that are not null
        /// </summary>
        /// <returns>A new IEnumerable with only the elements that are not null</returns>
        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Where(x => x != null);
        }
    }
}