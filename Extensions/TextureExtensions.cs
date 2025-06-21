using UnityEngine;

namespace AwesomeProjectionCoreUtils.Extensions
{
    public static class TextureExtensions
    {
        private static ComputeShader _hueShiftShader;
        /// <summary>
        /// Applies a hue shift to this Texture2D using a compute shader and returns a new modified Texture2D.
        /// </summary>
        /// <param name="texture">The source texture.</param>
        /// <param name="hueDegrees">Hue shift amount in degrees [0-360].</param>
        /// <returns>A new Texture2D with hue shifted.</returns>
        public static Texture2D WithHueShift(this Texture2D texture, float hueDegrees)
        {
            // Redirect to the full HSL method with neutral saturation and luminance scaling
            return texture.WithHSLAdjust(hueDegrees, 1f, 1f);
        }

        /// <summary>
        /// Applies HSL (Hue, Saturation, Lightness) adjustments to a Texture2D using a compute shader and returns a new modified texture.
        /// </summary>
        /// <param name="texture">The source <see cref="Texture2D"/> to be modified.</param>
        /// <param name="hueDegrees">Hue shift in degrees. The value is wrapped within [0, 360].</param>
        /// <param name="saturationScale">
        /// A multiplier for saturation. 
        /// Values &gt; 1 increase saturation, values between 0 and 1 decrease it, and 1 leaves it unchanged.
        /// </param>
        /// <param name="luminanceScale">
        /// A multiplier for lightness. 
        /// Values &gt; 1 increase brightness, values between 0 and 1 darken it, and 1 leaves it unchanged.
        /// </param>
        /// <returns>A new <see cref="Texture2D"/> with the HSL adjustments applied.</returns>
        /// <remarks>
        /// The operation is GPU-accelerated using a compute shader. The returned texture is newly created and does not modify the original.
        /// </remarks>
        public static Texture2D WithHSLAdjust(this Texture2D texture, float hueDegrees, float saturationScale = 1f, float luminanceScale = 1f)
        {
            if (_hueShiftShader == null)
                _hueShiftShader = Resources.Load<ComputeShader>("HueShiftMain");

            float hueShift = (hueDegrees % 360f) / 360f;

            int kernel = _hueShiftShader.FindKernel("HueShiftMain");

            int width = texture.width;
            int height = texture.height;

            RenderTexture rt = new RenderTexture(width, height, 0)
            {
                enableRandomWrite = true,
                format = RenderTextureFormat.ARGB32
            };
            rt.Create();

            _hueShiftShader.SetTexture(kernel, "Source", texture);
            _hueShiftShader.SetTexture(kernel, "Result", rt);
            _hueShiftShader.SetFloat("HueShift", hueShift);
            _hueShiftShader.SetFloat("SaturationScale", saturationScale);
            _hueShiftShader.SetFloat("LuminanceScale", luminanceScale); // <- still called ValueScale in shader for compatibility

            int threadGroupsX = Mathf.CeilToInt(width / 8f);
            int threadGroupsY = Mathf.CeilToInt(height / 8f);
            _hueShiftShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            RenderTexture.active = rt;
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = null;

            rt.Release();
            return result;
        }


        /// <summary>
        /// Converts a Sprite to a standalone readable Texture2D.
        /// </summary>
        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            if (sprite.rect.width != sprite.texture.width || sprite.rect.height != sprite.texture.height)
            {
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.textureRect.x,
                    (int)sprite.textureRect.y,
                    (int)sprite.textureRect.width,
                    (int)sprite.textureRect.height
                );
                newTex.SetPixels(pixels);
                newTex.Apply();
                return newTex;
            }
            else
            {
                return sprite.texture;
            }
        }
        
        /// <summary>
        /// Converts a Texture2D to a Sprite with the specified pivot and pixels per unit.
        /// </summary>
        /// <param name="texture">The Texture2D to convert.</param>
        /// <param name="pivot">The pivot point for the sprite. 0,0 is bottom-left, 0.5,0.5 is center, 1,1 is top-right. By default, it is set to the center (0.5, 0.5).</param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        public static Sprite ToSprite(this Texture2D texture, Vector2? pivot = null, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}