#nullable enable

using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Renders a visual gameobject into a Texture2D using a temporary camera and isolated setup.
        /// </summary>
        /// <param name="obj">The object to render.</param>
        /// <param name="layer">The layer index to isolate the rendering (should be a unique unused layer).</param>
        /// <param name="fov">Field of view for the rendering camera.</param>
        /// <param name="resolution">Resolution (width and height) of the output texture.</param>
        /// <param name="backgroundColor">Background color (alpha supported). Defaults to transparent.</param>
        /// <param name="cameraOffset">Optional offset from the object's bounds center. Defaults to auto distance based on FOV.</param>
        public static Texture2D RenderToTexture(
            this GameObject obj,
            int layer,
            float fov = 30f,
            int resolution = 512,
            Color? backgroundColor = null,
            Vector3? cameraOffset = null)
        {
            Vector3 renderOrigin = new Vector3(9999, 9999, 9999);

            // Create isolated container
            GameObject container = new GameObject("RenderContainer") { hideFlags = HideFlags.HideAndDontSave };
            container.transform.position = renderOrigin;

            GameObject instance = Object.Instantiate(obj, container.transform);
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.transform.localPosition = Vector3.zero;

            SetLayerRecursively(instance, layer);

            Bounds bounds = CalculateBounds(instance);
            float distance = CalculateCameraDistance(bounds, fov);
            Vector3 camOffset = cameraOffset ?? new Vector3(0, 0, -distance);

            // Setup camera
            GameObject camObj = new GameObject("RenderCamera") { hideFlags = HideFlags.HideAndDontSave };
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor ?? new Color(0, 0, 0, 0);
            cam.cullingMask = 1 << layer;
            cam.orthographic = false;
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = distance + bounds.extents.magnitude + 0.5f;
            cam.transform.position = bounds.center + camOffset;
            cam.transform.LookAt(bounds.center);

            // Render to texture
            RenderTexture rt = new RenderTexture(resolution, resolution, 24)
            {
                antiAliasing = 4,
                format = RenderTextureFormat.ARGB32,
                enableRandomWrite = false
            };
            cam.targetTexture = rt;

            RenderTexture.active = rt;
            cam.Render();

            Texture2D output = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
            output.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            output.Apply();

            // Cleanup
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(camObj);
            Object.DestroyImmediate(container);

            return output;
        }

        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private static Bounds CalculateBounds(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one * 0.5f);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static float CalculateCameraDistance(Bounds bounds, float fov)
        {
            float radius = bounds.extents.magnitude;
            float distance = radius / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            return distance;
        }
    }
}
