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
                string expansionName = cardData.expansionType.ToString();

                // Get assets for this card - if none found, don't modify anything
                GradeAssets assets = Plugin.GetAssetsForCard(cardData, expansionName);
                if (assets == null)
                {
                    // No assets for this expansion/company - leave card unmodified
                    return;
                }

                Sprite croppedSprite = assets.GetCroppedSprite(grade);
                Texture2D fullTexture = assets.GetTexture(grade);
                GradedCardGradeConfig config = assets.GetConfig(grade);

                // Apply cropped sprites to both LabelImage and LabelImageBack
                Transform transform = __instance.m_GradedCardGrp.transform;
                Transform cullGrp = transform.Find("GradingSlabCullGrp");
                if (cullGrp == null) return;

                // Apply to LabelImage (current implementation)
                ApplySpriteToComponent(cullGrp, "LabelImage", croppedSprite);

                // Apply cropped sprite to LabelImageBack
                Transform componentTransform = cullGrp.Find("LabelImageBack");
                Image imageComponent = componentTransform.GetComponent<Image>();
                if (croppedSprite != null)
                {
                    imageComponent.sprite = croppedSprite;
                    imageComponent.color = Color.white;
                }
                else
                {
                    imageComponent.color = Color.clear;
                }

                // Hide company elements like TextureReplacer does
                HideCompanyElements(cullGrp);

                // Apply text configuration
                if (config != null)
                {
                    ApplyTextConfiguration(__instance, config);
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

            if (slabRenderer == null) return;
            if (fullTexture == null) return;
            slabRenderer.material.mainTexture = (Texture)fullTexture;
            slabRenderer.material.color = Color.white;
            // Logger.LogInfo($"After changes - texture: {slabRenderer.material.mainTexture?.name}");
        }

        private static void ApplyCardBackTexture(Card3dUIGroup instance, Texture2D fullTexture)
        {
            // Find CardBackMeshBlocker and assign FULL image texture
            Transform cardBackMesh = instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            Renderer cardBackRenderer = cardBackMesh?.GetComponent<Renderer>();

            if (cardBackRenderer==null) return;
            // Try to replace the texture with our custom one
            if (fullTexture != null)
            {
                cardBackRenderer.material.mainTexture = (Texture)fullTexture;
            }  
        }

        private static void ApplyTextConfiguration(Card3dUIGroup instance, GradedCardGradeConfig config)
        {
            GradedCardTextUtils.ApplyTextConfiguration(instance, config, is3D: true);
        }
    }
}