using System.Collections.Generic;
using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Sum all the vectors in the provided enumerable
        /// </summary>
        /// <returns>The resultant vector of the sum of all the vectors</returns>
        public static Vector3 Resultant(this IEnumerable<Vector3> vectors)
        {
            Vector3 result = Vector3.zero;
            foreach (var vector in vectors)
            {
                result += vector;
            }
            return result;
        }
    
        /// <summary>
        /// Convert a Vector3 to a Vector2 by removing the Y component.
        /// </summary>
        /// <returns>A new Vector2 with the X and Z components of the Vector3</returns>
        public static Vector2 ZisY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }
    }
}