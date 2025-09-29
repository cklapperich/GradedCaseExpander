using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

/* 
    // this is hwo texturereplacer replaces the gradedcardcase texture. how should WE then replace it? 
    if ((Object)(object)CardCropGrading == (Object)null)
    {
        Texture2D cachedTexture = GetCachedTexture("GradedCardCase");
        if ((Object)(object)cachedTexture != (Object)null)
        {
            int num3 = 271;
            int num4 = 484;
            int num5 = 128;
            int num6 = ((Texture)cachedTexture).height - 208;
            Texture2D val2 = new Texture2D(num4, num5, (TextureFormat)4, false);
            Graphics.CopyTexture((Texture)(object)cachedTexture, 0, 0, num3, num6, num4, num5, (Texture)(object)val2, 0, 0, 0, 0);
            Sprite val3 = TextureToSprite(val2);
            if ((Object)(object)val3 != (Object)null)
            {
                CardCropGrading = val3;
            }
        }
    }
*/
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

                // Apply grade-specific sprite or fallback to DefaultLabel
                ApplyGradeSprite(__instance, grade);

                // Clear CardBackMesh to prevent black rectangle
                // ClearCardBackMesh(__instance);

                // Apply text configuration
                if (Plugin.GradeConfigs.ContainsKey(grade))
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[grade]);
                }
                else if (Plugin.GradeConfigs.ContainsKey(0)) // Fallback to DefaultLabel.txt
                {
                    ApplyTextConfiguration(__instance, Plugin.GradeConfigs[0]);
                }
                else
                {
                    Logger.LogWarning($"No text configuration found for grade {grade} or fallback");
                }
            }
        }

        private static void ApplyGradeSprite(Card3dUIGroup instance, int grade)
        {
            // Mimic TextureReplacer's exact approach but with grade-specific sprites
            Transform transform = instance.m_GradedCardGrp.transform;
            if (transform != null)
            {
                Transform labelImageTransform = transform.Find("LabelImage");
                if (labelImageTransform != null)
                {
                    Image labelImageComponent = labelImageTransform.GetComponent<Image>();
                    if (labelImageComponent != null && labelImageComponent.sprite != null)
                    {
                        // Try grade-specific cropped sprite first, then fallback to DefaultLabel cropped sprite
                        Sprite spriteToApply = null;
                        if (Plugin.GradeCroppedSprites.ContainsKey(grade))
                        {
                            spriteToApply = Plugin.GradeCroppedSprites[grade];
                        }
                        else if (Plugin.DefaultLabelCroppedSprite != null)
                        {
                            spriteToApply = Plugin.DefaultLabelCroppedSprite;
                        }

                        if (spriteToApply != null)
                        {
                            // Copy the original sprite name like TextureReplacer does
                            // spriteToApply.name = labelImageComponent.sprite.name;

                            // Replace the sprite
                            labelImageComponent.sprite = spriteToApply;
                            labelImageComponent.color = Color.white;

                            // Hide company elements like TextureReplacer does
                            HideCompanyElements(transform);
                        }
                        else
                        {
                            Logger.LogWarning($"No sprite available for grade {grade} and no DefaultLabel fallback");
                        }
                    }
                }
            }
        }

        private static void HideCompanyElements(Transform transform)
        {
            // TextureReplacer already disables this and we can't re-enable it without maybe doing a post-postfix to texturereplacers setcardui patch??
            // but we can leave it disabled for now
            // Hide company elements exactly like TextureReplacer does
            Transform labelImageCompany = transform.Find("LabelImageCompany");
            if (labelImageCompany != null)
            {
                labelImageCompany.gameObject.SetActive(false);
            }
            // TextureReplacer already disables this and we can't re-enable it without maybe doing a post-postfix to texturereplacers setcardui patch??
            // but for some reason it still shows up? so we will uncomment...?
            Transform gradingCompanyText = transform.Find("GradingCompanyText");
            if (gradingCompanyText != null)
            {
                gradingCompanyText.gameObject.SetActive(false);
            }
        }

        private static void ClearCardBackMesh(Card3dUIGroup instance)
        {
            // Find and modify CardBackMesh material color
            Transform cardBackMesh = instance.m_GradedCardGrp?.transform.Find("CardBackMeshBlocker");
            Renderer cardBackRenderer = cardBackMesh?.GetComponent<Renderer>();

            if (cardBackRenderer != null && cardBackRenderer.material != null)
            {   // important! T_CardBackMesh.png is replaced with a transparent PNG, for testing, outside of this mode.
                // disabling this causes REALLY weird issues with transparency. Setting this to false does cause the right parts of the label to be transparent, but if you have multiple overlapping cards in your hand, the labels render through each other!
                cardBackRenderer.enabled=true; 
                cardBackRenderer.material.color = Color.clear; // BUT this doesnt seem to do anything? You still get a black rectangle.
            }
        }

        private static void ApplyTextConfiguration(Card3dUIGroup instance, GradedCardGradeConfig config)
        {
            GradedCardTextUtils.ApplyTextConfiguration(instance, config, is3D: true);
        }
    }
}