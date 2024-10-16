using UnityEngine.AI;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class NavMeshExtensions
    {
        /// <summary>
        /// Check if the path is not invalid (can be imcomplete) and has at least 2 corners
        /// </summary>
        /// <param name="pathToTest"></param>
        /// <returns>True if the path is valid and has at least 2 corners</returns>
        public static bool IsValidAndFilled(this NavMeshPath pathToTest)
        {
            return pathToTest.status != NavMeshPathStatus.PathInvalid && pathToTest.corners.Length > 1;
        }
    }
}