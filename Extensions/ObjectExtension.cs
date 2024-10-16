using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Return true if the object is a unity object and is not null.
        /// </summary>
        /// <remarks>Usefully to test interfaces inheriting from unity objects</remarks>
        /// <param name="unityObject">The potential unity object to test</param>
        /// <returns>True if the object is a unity object and is not null</returns>
        public static bool IsAlive(this object unityObject)
        {
            var o = unityObject as Object;
            return o != null;
        }
    }
}