using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;
using System.Text;
using TMPro;

namespace GradedCardExpander
{
    public static class DebugUtils
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("DebugUtils");

        /// <summary>
        /// Recursively dumps the hierarchy of a GameObject and its components
        /// </summary>
        public static void DumpObjectHierarchy(GameObject obj, int depth = 0)
        {
            if (obj == null) return;

            string indent = new string(' ', depth * 2);
            StringBuilder info = new StringBuilder();
            info.Append($"{indent}[{obj.name}]");

            // Add basic info
            info.Append($" Active: {obj.activeInHierarchy}");

            // Check for Image component and its sprite
            Image image = obj.GetComponent<Image>();
            if (image != null)
            {
                info.Append($" [IMAGE]");
                if (image.sprite != null)
                {
                    info.Append($" Sprite: {image.sprite.name} ({image.sprite.texture.width}x{image.sprite.texture.height})");
                    if (image.sprite.texture != null)
                    {
                        info.Append($" Texture: {image.sprite.texture.name}");
                    }
                }
                else
                {
                    info.Append($" Sprite: null");
                }
                info.Append($" Color: {image.color}");
            }

            // Check for RawImage component
            RawImage rawImage = obj.GetComponent<RawImage>();
            if (rawImage != null)
            {
                info.Append($" [RAWIMAGE]");
                if (rawImage.texture != null)
                {
                    info.Append($" Texture: {rawImage.texture.name} ({rawImage.texture.width}x{rawImage.texture.height})");
                }
                else
                {
                    info.Append($" Texture: null");
                }
                info.Append($" Color: {rawImage.color}");
            }

            // Check for Renderer component (for 3D meshes)
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                info.Append($" [RENDERER]");
                info.Append($" Enabled: {renderer.enabled}");
                if (renderer.material != null)
                {
                    info.Append($" Material: {renderer.material.name}");
                    if (renderer.material.mainTexture != null)
                    {
                        info.Append($" MainTexture: {renderer.material.mainTexture.name} ({renderer.material.mainTexture.width}x{renderer.material.mainTexture.height})");
                    }
                }
            }

            // Check for TextMeshProUGUI component
            TextMeshProUGUI tmpText = obj.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                info.Append($" [TEXT] \"{tmpText.text}\" Color: {tmpText.color} Size: {tmpText.fontSize}");
            }

            // Check for Canvas component
            Canvas canvas = obj.GetComponent<Canvas>();
            if (canvas != null)
            {
                info.Append($" [CANVAS] RenderMode: {canvas.renderMode} SortingOrder: {canvas.sortingOrder}");
            }

            // Check for RectTransform (UI elements)
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                info.Append($" [UI] Size: {rectTransform.sizeDelta} Pos: {rectTransform.anchoredPosition}");
            }

            Logger.LogInfo(info.ToString());

            // Recursively dump children
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                DumpObjectHierarchy(obj.transform.GetChild(i).gameObject, depth + 1);
            }
        }

        /// <summary>
        /// Dumps hierarchy with header and footer
        /// </summary>
        public static void DumpObjectHierarchyWithHeader(GameObject obj, string header = null)
        {
            if (obj == null) return;

            string actualHeader = header ?? $"{obj.name} HIERARCHY";
            Logger.LogInfo($"=== DUMPING {actualHeader} ===");
            DumpObjectHierarchy(obj, 0);
            Logger.LogInfo("=== END HIERARCHY DUMP ===");
        }
    }
}