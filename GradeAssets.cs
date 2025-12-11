using UnityEngine;
using System.IO;
using System.Collections.Generic;
using TMPro;

namespace GradedCardExpander
{
    /// <summary>
    /// Static font management for all GradeAssets instances.
    /// </summary>
    public static class FontLoader
    {
        public static Dictionary<string, TMP_FontAsset> LoadedFonts { get; } = new Dictionary<string, TMP_FontAsset>();

        /// <summary>
        /// Loads all .ttf fonts from the specified directory.
        /// </summary>
        public static void LoadFonts(string directory)
        {
            string[] fontFiles = Directory.GetFiles(directory, "*.ttf", SearchOption.TopDirectoryOnly);

            foreach (string fontPath in fontFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(fontPath);
                string fontKey = fileName.ToLower();

                Font unityFont = new Font(fontPath);
                if (unityFont != null)
                {
                    TMP_FontAsset tmpFontAsset = TMP_FontAsset.CreateFontAsset(unityFont);
                    if (tmpFontAsset != null)
                    {
                        tmpFontAsset.name = $"{fileName}_Font_FromFile";
                        if (tmpFontAsset.material != null)
                        {
                            tmpFontAsset.material = new Material(tmpFontAsset.material);
                            tmpFontAsset.material.name = $"{fileName}_Material";
                        }
                        LoadedFonts[fontKey] = tmpFontAsset;
                    }
                }
            }
        }
    }

    public class GradedCardTextConfig
    {
        public Color? Color { get; set; } = null;
        public float? FontSize { get; set; } = null;
        public Vector2? Position { get; set; } = null;
        public Vector2? Position2D { get; set; } = null;
        public TMP_FontAsset Font { get; set; } = null;
        public string Text { get; set; } = null;
        public Color? OutlineColor { get; set; } = null;
        public float? OutlineWidth { get; set; } = null;
        public float? MaxTextWidthPixels { get; set; } = null;
    }

    public class GradedCardGradeConfig
    {
        public GradedCardTextConfig GradeNumberText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeDescriptionText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeNameText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeExpansionRarityText { get; set; } = new GradedCardTextConfig();
        public GradedCardTextConfig GradeSerialText { get; set; } = new GradedCardTextConfig();
    }

    /// <summary>
    /// Holds all assets for a single folder (all grades within it).
    /// Keyed by grade: 0=default (DefaultLabel), 1-10=specific grades.
    /// </summary>
    public class GradeAssets
    {
        public string FolderPath { get; private set; }
        public GradeAssets Parent { get; set; } = null;

        public Dictionary<int, Texture2D> Textures { get; } = new Dictionary<int, Texture2D>();
        public Dictionary<int, Sprite> Sprites { get; } = new Dictionary<int, Sprite>();
        public Dictionary<int, Sprite> CroppedSprites { get; } = new Dictionary<int, Sprite>();
        public Dictionary<int, GradedCardGradeConfig> Configs { get; } = new Dictionary<int, GradedCardGradeConfig>();

        /// <summary>
        /// Loads all assets from a folder. Returns a new GradeAssets instance.
        /// </summary>
        public static GradeAssets LoadFromFolder(string folderPath)
        {
            var assets = new GradeAssets { FolderPath = folderPath };

            // Load DefaultLabel (grade 0)
            assets.LoadGrade(folderPath, 0, "DefaultLabel");

            // Load grade-specific assets (1-10)
            for (int grade = 1; grade <= 10; grade++)
            {
                assets.LoadGrade(folderPath, grade, grade.ToString());
            }

            return assets;
        }

        /// <summary>
        /// Gets the texture for a grade, falling back to grade 0 (DefaultLabel) if not found, then to Parent.
        /// </summary>
        public Texture2D GetTexture(int grade)
        {
            if (Textures.TryGetValue(grade, out var texture))
                return texture;
            if (Textures.TryGetValue(0, out var defaultTexture))
                return defaultTexture;
            return Parent?.GetTexture(grade);
        }

        /// <summary>
        /// Gets the sprite for a grade, falling back to grade 0 (DefaultLabel) if not found, then to Parent.
        /// </summary>
        public Sprite GetSprite(int grade)
        {
            if (Sprites.TryGetValue(grade, out var sprite))
                return sprite;
            if (Sprites.TryGetValue(0, out var defaultSprite))
                return defaultSprite;
            return Parent?.GetSprite(grade);
        }

        /// <summary>
        /// Gets the cropped sprite for a grade, falling back to grade 0 (DefaultLabel) if not found, then to Parent.
        /// </summary>
        public Sprite GetCroppedSprite(int grade)
        {
            if (CroppedSprites.TryGetValue(grade, out var sprite))
                return sprite;
            if (CroppedSprites.TryGetValue(0, out var defaultSprite))
                return defaultSprite;
            return Parent?.GetCroppedSprite(grade);
        }

        /// <summary>
        /// Gets the config for a grade, falling back to grade 0 (DefaultLabel) if not found, then to Parent.
        /// </summary>
        public GradedCardGradeConfig GetConfig(int grade)
        {
            if (Configs.TryGetValue(grade, out var config))
                return config;
            if (Configs.TryGetValue(0, out var defaultConfig))
                return defaultConfig;
            return Parent?.GetConfig(grade);
        }

        /// <summary>
        /// Returns true if this GradeAssets has any loaded assets (textures or configs) or has a parent with assets.
        /// </summary>
        public bool HasAssets => Textures.Count > 0 || Configs.Count > 0 || (Parent?.HasAssets ?? false);

        /// <summary>
        /// Logs all loaded assets to a file for debugging.
        /// </summary>
        public void LogToFile(string outputPath, string folderLabel)
        {
            using (var writer = new StreamWriter(outputPath, append: true))
            {
                writer.WriteLine($"=== GradeAssets: {folderLabel} ===");
                writer.WriteLine($"FolderPath: {FolderPath}");
                writer.WriteLine($"HasAssets: {HasAssets}");
                writer.WriteLine();

                writer.WriteLine($"Textures ({Textures.Count}):");
                foreach (var kvp in Textures)
                    writer.WriteLine($"  Grade {kvp.Key}: {kvp.Value?.name ?? "null"} ({kvp.Value?.width}x{kvp.Value?.height})");

                writer.WriteLine($"Sprites ({Sprites.Count}):");
                foreach (var kvp in Sprites)
                    writer.WriteLine($"  Grade {kvp.Key}: {kvp.Value?.name ?? "null"}");

                writer.WriteLine($"CroppedSprites ({CroppedSprites.Count}):");
                foreach (var kvp in CroppedSprites)
                    writer.WriteLine($"  Grade {kvp.Key}: {kvp.Value?.name ?? "null"}");

                writer.WriteLine($"Configs ({Configs.Count}):");
                foreach (var kvp in Configs)
                {
                    writer.WriteLine($"  Grade {kvp.Key}:");
                    LogConfigSection(writer, "    GradeNumberText", kvp.Value.GradeNumberText);
                    LogConfigSection(writer, "    GradeDescriptionText", kvp.Value.GradeDescriptionText);
                    LogConfigSection(writer, "    GradeNameText", kvp.Value.GradeNameText);
                    LogConfigSection(writer, "    GradeExpansionRarityText", kvp.Value.GradeExpansionRarityText);
                    LogConfigSection(writer, "    GradeSerialText", kvp.Value.GradeSerialText);
                }

                writer.WriteLine();
                writer.WriteLine(new string('-', 60));
                writer.WriteLine();
            }
        }

        private static void LogConfigSection(StreamWriter writer, string label, GradedCardTextConfig cfg)
        {
            if (cfg == null) { writer.WriteLine($"{label}: null"); return; }

            var parts = new List<string>();
            if (cfg.Color.HasValue) parts.Add($"Color={cfg.Color.Value}");
            if (cfg.FontSize.HasValue) parts.Add($"FontSize={cfg.FontSize.Value}");
            if (cfg.Position.HasValue) parts.Add($"Pos3D={cfg.Position.Value}");
            if (cfg.Position2D.HasValue) parts.Add($"Pos2D={cfg.Position2D.Value}");
            if (cfg.Font != null) parts.Add($"Font={cfg.Font.name}");
            if (cfg.Text != null) parts.Add($"Text=\"{cfg.Text}\"");
            if (cfg.OutlineColor.HasValue) parts.Add($"OutlineColor={cfg.OutlineColor.Value}");
            if (cfg.OutlineWidth.HasValue) parts.Add($"OutlineWidth={cfg.OutlineWidth.Value}");
            if (cfg.MaxTextWidthPixels.HasValue) parts.Add($"MaxWidth={cfg.MaxTextWidthPixels.Value}");

            writer.WriteLine(parts.Count > 0 ? $"{label}: {string.Join(", ", parts)}" : $"{label}: (defaults)");
        }

        private void LoadGrade(string folderPath, int grade, string baseName)
        {
            // Create unique prefix from folder name for texture/sprite naming
            string folderName = Path.GetFileName(folderPath);
            string namePrefix = string.IsNullOrEmpty(folderName) ? "root" : folderName;

            // Load texture and sprites
            string imagePath = Path.Combine(folderPath, $"{baseName}.png");
            Texture2D texture = LoadTexture(imagePath);
            if (texture != null)
            {
                texture.name = $"{namePrefix}_{baseName}_Texture";
                Textures[grade] = texture;

                Sprites[grade] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Sprites[grade].name = $"{namePrefix}_{baseName}_Sprite";

                Texture2D croppedTexture = CropTexture(texture);
                croppedTexture.name = $"{namePrefix}_{baseName}_CroppedTexture";
                CroppedSprites[grade] = Sprite.Create(croppedTexture, new Rect(0, 0, croppedTexture.width, croppedTexture.height), new Vector2(0.5f, 0.5f));
                CroppedSprites[grade].name = $"{namePrefix}_{baseName}_CroppedSprite";
            }

            // Load config
            string configPath = Path.Combine(folderPath, $"{baseName}.txt");
            if (File.Exists(configPath))
            {
                Configs[grade] = LoadTextConfig(configPath);
            }
        }

        private static Texture2D LoadTexture(string imagePath)
        {
            if (!File.Exists(imagePath))
                return null;

            byte[] imageData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply();
            return texture;
        }

        private static Texture2D CropTexture(Texture2D sourceTexture)
        {
            int cropX = 271;
            int cropWidth = 484;
            int cropHeight = 128;
            int cropY = sourceTexture.height - 208;

            Texture2D croppedTexture = new Texture2D(cropWidth, cropHeight, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(sourceTexture, 0, 0, cropX, cropY, cropWidth, cropHeight, croppedTexture, 0, 0, 0, 0);
            croppedTexture.filterMode = FilterMode.Bilinear;
            croppedTexture.wrapMode = TextureWrapMode.Clamp;
            croppedTexture.Apply();
            return croppedTexture;
        }

        private static GradedCardGradeConfig LoadTextConfig(string configFile)
        {
            var gradeConfig = new GradedCardGradeConfig();
            string[] lines = File.ReadAllLines(configFile);
            GradedCardTextConfig currentTextConfig = null;

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentTextConfig = sectionName switch
                    {
                        "GradeNumberText" => gradeConfig.GradeNumberText,
                        "GradeDescriptionText" => gradeConfig.GradeDescriptionText,
                        "GradeNameText" => gradeConfig.GradeNameText,
                        "GradeExpansionRarityText" => gradeConfig.GradeExpansionRarityText,
                        "GradeSerialText" => gradeConfig.GradeSerialText,
                        _ => null
                    };
                    continue;
                }

                if (currentTextConfig != null && trimmedLine.Contains("="))
                {
                    string[] parts = trimmedLine.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        ParseConfigValue(currentTextConfig, key, value);
                    }
                }
            }

            return gradeConfig;
        }

        private static void ParseConfigValue(GradedCardTextConfig config, string key, string value)
        {
            switch (key)
            {
                case "Color":
                    if (ColorUtility.TryParseHtmlString(value, out Color color))
                        config.Color = color;
                    break;
                case "FontSize":
                    if (float.TryParse(value, out float fontSize))
                        config.FontSize = fontSize;
                    break;
                case "Font":
                    if (FontLoader.LoadedFonts.ContainsKey(value.ToLower()))
                        config.Font = FontLoader.LoadedFonts[value.ToLower()];
                    break;
                case "OffsetX":
                    if (float.TryParse(value, out float offsetX))
                        config.Position = new Vector2(offsetX, config.Position?.y ?? 0);
                    break;
                case "OffsetY":
                    if (float.TryParse(value, out float offsetY))
                        config.Position = new Vector2(config.Position?.x ?? 0, offsetY);
                    break;
                case "Offset2DX":
                    if (float.TryParse(value, out float offset2DX))
                        config.Position2D = new Vector2(offset2DX, config.Position2D?.y ?? 0);
                    break;
                case "Offset2DY":
                    if (float.TryParse(value, out float offset2DY))
                        config.Position2D = new Vector2(config.Position2D?.x ?? 0, offset2DY);
                    break;
                case "Text":
                    config.Text = value;
                    break;
                case "OutlineColor":
                    if (ColorUtility.TryParseHtmlString(value, out Color outlineColor))
                        config.OutlineColor = outlineColor;
                    break;
                case "OutlineWidth":
                    if (float.TryParse(value, out float outlineWidth))
                        config.OutlineWidth = outlineWidth;
                    break;
                case "MaxTextWidthPixels":
                    if (float.TryParse(value, out float maxWidth))
                        config.MaxTextWidthPixels = maxWidth;
                    break;
            }
        }
    }

}
