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
        /// <param name="cameraPrefab">Prefab of the camera to use for rendering.</param>
        /// <param name="fov">Field of view for the rendering camera.</param>
        /// <param name="resolution">Resolution (width and height) of the output texture.</param>
        /// <param name="backgroundColor">Background color (alpha supported). Defaults to transparent.</param>
        /// <param name="cameraOffset">Optional offset from the object's bounds center. Defaults to auto distance based on FOV.</param>
        public static Texture2D RenderToTexture(
            this GameObject obj,
            GameObject cameraPrefab,
            Vector3 cameraOffset,
            float fov = 30f,
            int resolution = 512,
            Color? backgroundColor = null)
        {
            Vector3 initialPosition = obj.transform.position;
            Vector3 renderOrigin = new Vector3(9999, 9999, 9999);

            // Create isolated container
            GameObject container = new GameObject("RenderContainer") { hideFlags = HideFlags.HideAndDontSave };
            container.transform.position = renderOrigin;
            obj.transform.position = renderOrigin;

            Bounds bounds = CalculateBounds(obj);
            float distance = CalculateCameraDistance(bounds, fov);

            // Setup camera
            GameObject camObj = Object.Instantiate(cameraPrefab, bounds.center + cameraOffset, Quaternion.identity);
            camObj.hideFlags = HideFlags.HideAndDontSave;
            Camera cam = camObj.GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("Camera prefab does not have a Camera component. Adding one.");
                cam = camObj.AddComponent<Camera>();
            }
            cam.backgroundColor = backgroundColor ?? new Color(0, 0, 0, 0);
            cam.orthographic = false;
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = distance + bounds.extents.magnitude + 0.5f;
            cam.transform.position = bounds.center + cameraOffset;
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
            obj.transform.position = initialPosition;

            return output;
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

        /// <summary>
        /// Clones the MeshRenderers (and optionally Colliders) from this GameObject, 
        /// returning a new GameObject representing the cloned visual hierarchy.
        /// </summary>
        public static GameObject CloneVisual(
            this GameObject sourceObj,
            bool withCollider = false,
            Vector3? localPosition = null,
            Quaternion? localRotation = null,
            Transform? parent = null)
        {
            return CloneVisual(sourceObj, out _, withCollider, localPosition, localRotation, parent);
        }

        /// <summary>
        /// Clones the MeshRenderers (and optionally Colliders) from this GameObject, 
        /// returning a new GameObject representing the cloned visual hierarchy.
        /// </summary>
        public static GameObject CloneVisual(
            this GameObject sourceObj,
            out System.Collections.Generic.List<MeshRenderer> targetRenderers,
            bool withCollider = false,
            Vector3? localPosition = null,
            Quaternion? localRotation = null,
            Transform? parent = null)
        {
            Transform sourceTransform = sourceObj.transform;
            GameObject clonedObj = new GameObject("Ghost_" + sourceTransform.name);
            
            if (parent != null)
            {
                clonedObj.transform.SetParent(parent, false);
            }
            
            clonedObj.transform.localPosition = localPosition ?? Vector3.zero;
            clonedObj.transform.localRotation = localRotation ?? Quaternion.identity;
            clonedObj.transform.localScale = sourceTransform.localScale;

            targetRenderers = new System.Collections.Generic.List<MeshRenderer>();
            var sourceRenderers = sourceTransform.GetComponentsInChildren<MeshRenderer>();
            foreach (var sourceRend in sourceRenderers)
            {
                var sourceFilter = sourceRend.GetComponent<MeshFilter>();
                if (sourceFilter == null) continue;

                var dummy = new GameObject(sourceRend.name);
                dummy.transform.SetParent(clonedObj.transform, false);
                
                // Position and rotation into relative space
                dummy.transform.localPosition = sourceTransform.InverseTransformPoint(sourceRend.transform.position);
                dummy.transform.localRotation = Quaternion.Inverse(sourceTransform.rotation) * sourceRend.transform.rotation;
                
                // Approximate relative scale
                Vector3 relativeScale = new Vector3(
                    sourceRend.transform.lossyScale.x / Mathf.Max(Mathf.Abs(sourceTransform.lossyScale.x), 0.0001f),
                    sourceRend.transform.lossyScale.y / Mathf.Max(Mathf.Abs(sourceTransform.lossyScale.y), 0.0001f),
                    sourceRend.transform.lossyScale.z / Mathf.Max(Mathf.Abs(sourceTransform.lossyScale.z), 0.0001f)
                );
                dummy.transform.localScale = relativeScale;

                dummy.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
                var targetRenderer = dummy.AddComponent<MeshRenderer>();
                targetRenderer.sharedMaterials = sourceRend.sharedMaterials;
                targetRenderers.Add(targetRenderer);

                if (withCollider)
                {
                    var sourceCollider = sourceRend.GetComponent<Collider>();
                    if (sourceCollider != null)
                    {
                        if (sourceCollider is BoxCollider box)
                        {
                            var c = dummy.AddComponent<BoxCollider>();
                            c.center = box.center;
                            c.size = box.size;
                            c.isTrigger = box.isTrigger;
                        }
                        else if (sourceCollider is SphereCollider sphere)
                        {
                            var c = dummy.AddComponent<SphereCollider>();
                            c.center = sphere.center;
                            c.radius = sphere.radius;
                            c.isTrigger = sphere.isTrigger;
                        }
                        else if (sourceCollider is CapsuleCollider capsule)
                        {
                            var c = dummy.AddComponent<CapsuleCollider>();
                            c.center = capsule.center;
                            c.radius = capsule.radius;
                            c.height = capsule.height;
                            c.direction = capsule.direction;
                            c.isTrigger = capsule.isTrigger;
                        }
                        else if (sourceCollider is MeshCollider meshCollider)
                        {
                            var c = dummy.AddComponent<MeshCollider>();
                            c.sharedMesh = meshCollider.sharedMesh;
                            c.convex = meshCollider.convex;
                            c.isTrigger = meshCollider.isTrigger;
                        }
                    }
                }
            }

            return clonedObj;
        }
    }
}
