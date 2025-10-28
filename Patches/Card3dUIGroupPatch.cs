using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;
/* THIS METHOD PATCHES THE 3D GRADED CARDS */

namespace GradedCardExpander.Patches
{
    [HarmonyPatch(typeof(Card3dUIGroup))]
    [HarmonyPatch("EvaluateCardGrade")]
    public class Card3dUIGroupDisableCardBackPatch
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Card3dUIGroupPatch");

        static void Postfix(Card3dUIGroup __instance, CardData cardData)
        {
            // Only apply to graded cards (grade > 0)
            if (__instance.m_GradedCardGrp != null && cardData != null && cardData.cardGrade > 0)
            {
                int grade = cardData.cardGrade;

                // Get both cropped sprite and full texture for this grade
                Sprite croppedSprite = null;
                Texture2D fullTexture = null;

                if (Plugin.GradeCroppedSprites.ContainsKey(grade))
                {
                    croppedSprite = Plugin.GradeCroppedSprites[grade];
                    fullTexture = Plugin.GradeTextures[grade];
                }
                else if (Plugin.DefaultLabelCroppedSprite != null)
                {
                    croppedSprite = Plugin.DefaultLabelCroppedSprite;
                    fullTexture = Plugin.DefaultLabelTexture;
                }

                // Apply cropped sprites to both LabelImage and LabelImageBack
                Transform transform = __instance.m_GradedCardGrp.transform;

                // Apply to LabelImage (current implementation)
                ApplySpriteToComponent(transform, "LabelImage", croppedSprite);

                // Apply cropped sprite to LabelImageBack
                Transform componentTransform = transform.Find("LabelImageBack");
                Image imageComponent = componentTransform.GetComponent<Image>();
                if (croppedSprite != null)
                {
                    imageComponent.sprite = croppedSprite;
                    imageComponent.color = Color.white;
                    // Logger.LogInfo($"Applied cropped sprite to LabelImageBack");
                }
                else
                {
                    imageComponent.color = Color.clear;
                    // Logger.LogInfo($"No cropped sprite, set LabelImageBack to clear");
                }

                // Hide company elements like TextureReplacer does
                HideCompanyElements(transform);

                // Replace textures on both CardBackMeshBlocker and Slab_BaseMesh
                ApplyCardBackTexture(__instance, fullTexture);
                //ApplySlabBaseMeshTexture(__instance, fullTexture);

                // Apply text configuration
                if (Plugin.GradeConfigs.ContainsKey(grade))
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[grade]);
                }
                else if (Plugin.GradeConfigs.ContainsKey(0)) // Fallback to DefaultLabel.txt
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[0]);
                }
            }
        }

        private static void ApplySpriteToComponent(Transform transform, string componentName, Sprite sprite)
        {
            Transform componentTransform = transform.Find(componentName);
            Image imageComponent = componentTransform.GetComponent<Image>();
            imageComponent.sprite = sprite;
            imageComponent.color = Color.white;
        }

        private static void HideCompanyElements(Transform transform)
        {
            // Hide company elements exactly like TextureReplacer does
            Transform labelImageCompany = transform.Find("LabelImageCompany");
            labelImageCompany.gameObject.SetActive(false);

            Transform gradingCompanyText = transform.Find("GradingCompanyText");
            gradingCompanyText.gameObject.SetActive(false);
        }

        private static void ApplySlabBaseMeshTexture(Card3dUIGroup instance, Texture2D fullTexture)
        {
            Transform slabBaseMesh = instance.m_GradedCardGrp?.transform.Find("Slab_BaseMesh");
            Renderer slabRenderer = slabBaseMesh?.GetComponent<Renderer>();

            if (slabRenderer != null)
            {

                if (fullTexture != null)
                {
                    slabRenderer.material.mainTexture = (Texture)fullTexture;
                    // Logger.LogInfo($"Set Slab_BaseMesh texture to: {fullTexture.name}");
                }

                slabRenderer.material.color = Color.white;
                // Logger.LogInfo($"After changes - texture: {slabRenderer.material.mainTexture?.name}");
            }
        }

        private static void ApplyCardBackTexture(Card3dUIGroup instance, Texture2D fullTexture)
        {
            // Find CardBackMeshBlocker and assign FULL image texture
            Transform cardBackMesh = instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            Renderer cardBackRenderer = cardBackMesh?.GetComponent<Renderer>();

            if (cardBackRenderer != null)
            {
             
                // Try to replace the texture with our custom one
                if (fullTexture != null)
                {
                    cardBackRenderer.material.mainTexture = (Texture)fullTexture;
                    // Logger.LogInfo($"Set texture to: {fullTexture.name}");
                }

                // DEBUG: Color CardBackMeshBlocker bright blue (will multiply with texture)
                cardBackRenderer.material.color = Color.blue;

                // Logger.LogInfo($"After changes - texture: {cardBackRenderer.material.mainTexture?.name}, color: {cardBackRenderer.material.color}");
            }       
        }

        private static void ApplyTextConfiguration(Card3dUIGroup instance, GradedCardGradeConfig config)
        {
            GradedCardTextUtils.ApplyTextConfiguration(instance, config, is3D: true);
        }
    }
}