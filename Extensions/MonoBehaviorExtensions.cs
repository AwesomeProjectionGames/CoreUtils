using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class MonoBehaviorExtensions
    {
        public static PhysicsScene PhysicsScene(this MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.gameObject.scene.GetPhysicsScene();
        }
    }
}