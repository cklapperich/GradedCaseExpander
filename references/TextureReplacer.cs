using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using Dummiesman;
using HarmonyLib;
using I2.Loc;
using TextureReplacer.Properties;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TextureReplacer;

[BepInPlugin("shaklin.TextureReplacer", "TextureReplacer", "1.3.6")]
public class BepInExPlugin : BaseUnityPlugin
{
    [HarmonyPatch(typeof(CardExpansionSelectScreen), "OpenScreen")]
    [HarmonyPriority(0)]
    internal static class CardExpansionSelectScreen_Patch_My
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            CardExpansionSelectScreen instance = CSingleton<CardExpansionSelectScreen>.Instance;
            if ((Object)(object)instance != (Object)null)
            {
                ((MonoBehaviour)instance).StartCoroutine(ApplyNextFrame());
            }
        }

        private static IEnumerator ApplyNextFrame()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return null;
                CardExpansionSelectScreen inst = CSingleton<CardExpansionSelectScreen>.Instance;
                if ((Object)(object)inst != (Object)null && (Object)(object)inst.m_ScreenGrp != (Object)null)
                {
                    ReplaceTexts(inst.m_ScreenGrp);
                    break;
                }
            }
        }

        private static void ReplaceTexts(GameObject screenGrp)
        {
            TMP_Text[] componentsInChildren = screenGrp.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text tMP_Text in componentsInChildren)
            {
                if ((Object)(object)tMP_Text == (Object)null)
                {
                    continue;
                }
                string text = tMP_Text.text;
                if (!string.IsNullOrEmpty(text) && (text == "Megabot" || text == "FantasyRPG" || text == "CatJob"))
                {
                    string translation = LocalizationManager.GetTranslation(text);
                    if (!string.IsNullOrEmpty(translation))
                    {
                        tMP_Text.text = translation;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CollectionBinderUI), "OpenSortAlbumScreen")]
    [HarmonyPriority(0)]
    internal static class CollectionBinderUI_Patch1
    {
        [HarmonyPostfix]
        private static void Postfix(CollectionBinderUI __instance)
        {
            ((MonoBehaviour)__instance).StartCoroutine(ApplyNextFrame(__instance));
        }

        private static IEnumerator ApplyNextFrame(CollectionBinderUI ui)
        {
            yield return null;
            ReplaceIfMatch((Component)(object)ui.m_ExpansionBtnList[0], "Megabot");
            ReplaceIfMatch((Component)(object)ui.m_ExpansionBtnList[1], "FantasyRPG");
            ReplaceIfMatch((Component)(object)ui.m_ExpansionBtnList[2], "CatJob");
        }

        private static void ReplaceIfMatch(Component btn, string key)
        {
            TMP_Text componentInChildren = btn.GetComponentInChildren<TMP_Text>();
            if ((Object)(object)componentInChildren != (Object)null && componentInChildren.text == key)
            {
                string translation = LocalizationManager.GetTranslation(key);
                if (!string.IsNullOrEmpty(translation))
                {
                    componentInChildren.text = translation;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PhoneManager), "EnterPhoneMode")]
    private static class PhoneManager_Patch
    {
        [HarmonyPostfix]
        private static void Postfix_EnterPhoneMode(PhoneManager __instance)
        {
            if (Instance.doPhone)
            {
                return;
            }
            Instance.doPhone = true;
            List<List<ItemData>> list = new List<List<ItemData>> { CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemDataList };
            foreach (List<ItemData> item in list)
            {
                Instance.ReplaceItemDataInList(item);
            }
        }
    }

    [HarmonyPatch]
    public static class MonsterDataPatch
    {
        private class Holder
        {
            public bool IsGhost;
        }

        private static readonly ConditionalWeakTable<MonsterData, Holder> Table = new ConditionalWeakTable<MonsterData, Holder>();

        private static Holder H(MonsterData md)
        {
            if (!Table.TryGetValue(md, out var value))
            {
                value = new Holder();
                Table.Add(md, value);
            }
            return value;
        }

        [HarmonyPatch(typeof(MonsterData), "GetIcon")]
        [HarmonyPrefix]
        private static void GetIcon_Prefix(MonsterData __instance, ECardExpansionType cardExpansionType)
        {
            H(__instance).IsGhost = cardExpansionType == ECardExpansionType.Ghost;
        }

        [HarmonyPatch(typeof(MonsterData), "GetName")]
        [HarmonyPostfix]
        private static void GetName_Postfix(MonsterData __instance, ref string __result)
        {
            string text = __instance.Name ?? "";
            int num = text.IndexOf('#');
            if (num >= 0)
            {
                string text2 = text.Substring(0, num).Trim();
                string text3 = text.Substring(num + 1).Trim();
                string text4 = (H(__instance).IsGhost ? text3 : text2);
                __result = text4;
            }
        }
    }

    [HarmonyPatch(typeof(CardUI), "SetCardUI")]
    private static class CardUI_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix_SetCardUI(CardUI __instance, ref CardData cardData)
        {
            foreach (List<Sprite> item in new List<List<Sprite>> { __instance.m_CardBorderSpriteList, __instance.m_CardBGSpriteList, __instance.m_CardRaritySpriteList, __instance.m_CardElementBGSpriteList })
            {
                Instance.ReplaceSpritesCardsInList(item);
            }
            foreach (List<Image> item2 in new List<List<Image>> { __instance.m_FoilShowList, __instance.m_FoilBlendedShowList, __instance.m_FoilDarkenImageList })
            {
                Instance.ReplaceSpritesFoilsInList(item2);
            }
            return true;
        }

        [HarmonyPostfix]
        private static void Postfix_SetCardUI(CardUI __instance, ref CardData cardData)
        {
            //IL_01be: Unknown result type (might be due to invalid IL or missing references)
            //IL_00c7: Unknown result type (might be due to invalid IL or missing references)
            //IL_1686: Unknown result type (might be due to invalid IL or missing references)
            //IL_169d: Unknown result type (might be due to invalid IL or missing references)
            //IL_16cd: Unknown result type (might be due to invalid IL or missing references)
            //IL_16e4: Unknown result type (might be due to invalid IL or missing references)
            //IL_1757: Unknown result type (might be due to invalid IL or missing references)
            //IL_176e: Unknown result type (might be due to invalid IL or missing references)
            //IL_1725: Unknown result type (might be due to invalid IL or missing references)
            //IL_173c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ec4: Unknown result type (might be due to invalid IL or missing references)
            //IL_0edb: Unknown result type (might be due to invalid IL or missing references)
            //IL_0ae9: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b00: Unknown result type (might be due to invalid IL or missing references)
            //IL_18d7: Unknown result type (might be due to invalid IL or missing references)
            //IL_18ee: Unknown result type (might be due to invalid IL or missing references)
            //IL_17e2: Unknown result type (might be due to invalid IL or missing references)
            //IL_17f9: Unknown result type (might be due to invalid IL or missing references)
            //IL_17b0: Unknown result type (might be due to invalid IL or missing references)
            //IL_17c7: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bc6: Unknown result type (might be due to invalid IL or missing references)
            //IL_0bdd: Unknown result type (might be due to invalid IL or missing references)
            //IL_191e: Unknown result type (might be due to invalid IL or missing references)
            //IL_1935: Unknown result type (might be due to invalid IL or missing references)
            //IL_1056: Unknown result type (might be due to invalid IL or missing references)
            //IL_106d: Unknown result type (might be due to invalid IL or missing references)
            //IL_09b1: Unknown result type (might be due to invalid IL or missing references)
            //IL_09c8: Unknown result type (might be due to invalid IL or missing references)
            //IL_0f45: Unknown result type (might be due to invalid IL or missing references)
            //IL_0f5c: Unknown result type (might be due to invalid IL or missing references)
            //IL_0f13: Unknown result type (might be due to invalid IL or missing references)
            //IL_0f2a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0e41: Unknown result type (might be due to invalid IL or missing references)
            //IL_0e58: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b6a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b81: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b38: Unknown result type (might be due to invalid IL or missing references)
            //IL_0b4f: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c47: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c5e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c15: Unknown result type (might be due to invalid IL or missing references)
            //IL_0c2c: Unknown result type (might be due to invalid IL or missing references)
            //IL_10d7: Unknown result type (might be due to invalid IL or missing references)
            //IL_10ee: Unknown result type (might be due to invalid IL or missing references)
            //IL_10a5: Unknown result type (might be due to invalid IL or missing references)
            //IL_10bc: Unknown result type (might be due to invalid IL or missing references)
            Card3dUIGroup value = Traverse.Create((object)__instance).Field("m_Card3dUIGroup").GetValue<Card3dUIGroup>();
            if ((Object)(object)value != (Object)null)
            {
                GameObject gradedCardGrp = value.m_GradedCardGrp;
                Transform transform = gradedCardGrp.transform;
                if ((Object)(object)transform != (Object)null)
                {
                    Transform val = transform.Find("LabelImage");
                    if ((Object)(object)CardCropGrading != (Object)null && (Object)(object)val != (Object)null)
                    {
                        Image component = ((Component)val).GetComponent<Image>();
                        if ((Object)(object)component != (Object)null && (Object)(object)component.sprite != (Object)null)
                        {
                            ((Object)CardCropGrading).name = ((Object)component.sprite).name;
                            component.sprite = CardCropGrading;
                            component.color = Color.white;
                            Transform val2 = transform.Find("LabelImageCompany");
                            if ((Object)(object)val2 != (Object)null)
                            {
                                ((Component)val2).gameObject.SetActive(false);
                            }
                            Transform val3 = transform.Find("GradingCompanyText");
                            if ((Object)(object)val3 != (Object)null)
                            {
                                ((Component)val3).gameObject.SetActive(false);
                            }
                        }
                    }
                    Transform val4 = transform.Find("NameText");
                    if ((Object)(object)val4 != (Object)null)
                    {
                        TextMeshProUGUI component2 = ((Component)val4).GetComponent<TextMeshProUGUI>();
                        if (Object.op_Implicit((Object)(object)component2))
                        {
                            component2.enableAutoSizing = true;
                            component2.fontSizeMin = 8f;
                            component2.fontSizeMax = 37.15f;
                            component2.alignment = TextAlignmentOptions.Left;
                            component2.enableWordWrapping = false;
                            component2.maxVisibleLines = 1;
                            component2.isRightToLeftText = false;
                            component2.overflowMode = TextOverflowModes.Ellipsis;
                            RectTransform rectTransform = component2.rectTransform;
                            float x = rectTransform.offsetMin.x;
                            rectTransform.SetInsetAndSizeFromParentEdge((Edge)0, x, 300f);
                            component2.ForceMeshUpdate();
                        }
                    }
                }
            }
            if (ChangeCards.Value)
            {
                GameObject normalGrp = __instance.m_NormalGrp;
                if ((Object)(object)normalGrp != (Object)null)
                {
                    Transform[] componentsInChildren = normalGrp.GetComponentsInChildren<Transform>(true);
                    if (componentsInChildren != null)
                    {
                        Transform[] array = componentsInChildren;
                        foreach (Transform val5 in array)
                        {
                            if (!((Object)(object)val5 != (Object)null))
                            {
                                continue;
                            }
                            if (!ShowEvolvesBox.Value && ((Object)val5).name == "EvoBG")
                            {
                                ((Component)val5).gameObject.SetActive(false);
                            }
                            if (!ShowEvolves.Value)
                            {
                                if (((Object)val5).name == "EvoBorder")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                else if (((Object)val5).name == "EvoBasicIcon")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                else if (((Object)val5).name == "EvoBasicText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                else if (((Object)val5).name == "EvoNameText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                else if (((Object)val5).name == "EvoPreviousStageIcon")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                else if (((Object)val5).name == "EvoText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            if (!ShowPlayEffect.Value)
                            {
                                if (((Object)val5).name == "TitleText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "TitleText")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowPlayEffectBox.Value)
                            {
                                if (((Object)val5).name == "TitleBG")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "TitleBG")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowDescriptionText.Value && ((Object)val5).name == "DescriptionText")
                            {
                                ((Component)val5).gameObject.SetActive(false);
                            }
                            if (!ShowArtist.Value)
                            {
                                if (((Object)val5).name == "ArtistText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                if (((Object)val5).name == "CompanyText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (((Object)val5).name == "ArtistText")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                                if (((Object)val5).name == "CompanyText")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                            }
                            if (!ShowStats.Value)
                            {
                                if (((Object)val5).name == "StatText_1")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                if (((Object)val5).name == "StatText_2")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                if (((Object)val5).name == "StatText_3")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                                if (((Object)val5).name == "StatText_4")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (((Object)val5).name == "StatText_1")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                                if (((Object)val5).name == "StatText_2")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                                if (((Object)val5).name == "StatText_3")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                                if (((Object)val5).name == "StatText_4")
                                {
                                    ((Component)val5).gameObject.SetActive(true);
                                }
                            }
                            if (!ShowName.Value)
                            {
                                if (((Object)val5).name == "NameText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "NameText")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowNumber.Value)
                            {
                                if (((Object)val5).name == "NumberText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "NumberText")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowEdition.Value)
                            {
                                if (((Object)val5).name == "FirstEditionText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "FirstEditionText")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowRarity.Value)
                            {
                                if (((Object)val5).name == "RarityText")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "RarityText")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                            if (!ShowRarityIcon.Value)
                            {
                                if (((Object)val5).name == "RarityImage")
                                {
                                    ((Component)val5).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val5).name == "RarityImage")
                            {
                                ((Component)val5).gameObject.SetActive(true);
                            }
                        }
                    }
                    if (ShowFullFrame.Value)
                    {
                        RectTransform[] componentsInChildren2 = normalGrp.GetComponentsInChildren<RectTransform>(true);
                        if (componentsInChildren2 != null)
                        {
                            RectTransform[] array2 = componentsInChildren2;
                            foreach (RectTransform val6 in array2)
                            {
                                if (!((Object)(object)val6 != (Object)null))
                                {
                                    continue;
                                }
                                if (ShowPixelGlow.Value)
                                {
                                    if (((Object)val6).name == "CameraFoilShine" && ((Object)((Transform)val6).parent).name == "CardBorder")
                                    {
                                        Transform parent = ((Transform)val6).parent;
                                        Transform parent2 = parent.parent;
                                        Transform val7 = parent2.Find("FoilGrp");
                                        Transform val8 = val7.Find("GlowMask");
                                        if ((Object)(object)val8 != (Object)null)
                                        {
                                            ((Transform)val6).SetParent(val8, true);
                                        }
                                    }
                                    if (((Object)val6).name == "CameraFoilShine" && ((Object)((Transform)val6).parent).name == "GlowMask")
                                    {
                                        val6.offsetMax = new Vector2(-165f, -45f);
                                        val6.offsetMin = new Vector2(158f, 45f);
                                    }
                                }
                                if (((Object)val6).name == "MonsterMask")
                                {
                                    Image component3 = ((Component)val6).GetComponent<Image>();
                                    if ((Object)(object)component3 != (Object)null && (Object)(object)component3.sprite != (Object)null && (Object)(object)component3.sprite.texture != (Object)null)
                                    {
                                        string name = ((Object)component3.sprite.texture).name;
                                        if (name == "WhiteTile" && (Object)(object)CardFoilMaskC != (Object)null)
                                        {
                                            Sprite val9 = TextureToSprite(CardFoilMaskC);
                                            ((Object)val9).name = name;
                                            component3.sprite = val9;
                                        }
                                    }
                                }
                                if (((Object)val6).name == "GlowMaskMonsterGrp")
                                {
                                    ((Component)val6).gameObject.SetActive(false);
                                }
                                if (((Object)val6).name == "MonsterMaskGrp")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val6.offsetMax = new Vector2(-41f, -68f);
                                        val6.offsetMin = new Vector2(21f, 8f);
                                    }
                                    else if (cardData.expansionType == ECardExpansionType.FantasyRPG || cardData.expansionType == ECardExpansionType.CatJob)
                                    {
                                        val6.offsetMax = new Vector2(-28f, 142f);
                                        val6.offsetMin = new Vector2(33f, -102f);
                                    }
                                    else
                                    {
                                        val6.offsetMax = new Vector2(-38f, -38f);
                                        val6.offsetMin = new Vector2(38f, 38f);
                                    }
                                }
                                if (((Object)val6).name == "Image")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val6.offsetMax = new Vector2(-166f, -52f);
                                        val6.offsetMin = new Vector2(160f, 52f);
                                    }
                                    else if (cardData.expansionType == ECardExpansionType.FantasyRPG || cardData.expansionType == ECardExpansionType.CatJob)
                                    {
                                        val6.offsetMax = new Vector2(-166f, -185f);
                                        val6.offsetMin = new Vector2(160f, 183f);
                                    }
                                    else
                                    {
                                        val6.offsetMax = new Vector2(-158f, -52f);
                                        val6.offsetMin = new Vector2(152f, 52f);
                                    }
                                }
                                if (((Object)val6).name == "NameTextGrp" || ((Object)val6).name == "NumberTextGrp" || ((Object)val6).name == "FirstEditionTextGrp" || ((Object)val6).name == "StatText_1" || ((Object)val6).name == "StatText_2" || ((Object)val6).name == "StatText_3" || ((Object)val6).name == "StatText_4")
                                {
                                    ((Transform)val6).SetSiblingIndex(15);
                                }
                            }
                        }
                    }
                    else if (ShowFull.Value)
                    {
                        RectTransform[] componentsInChildren3 = normalGrp.GetComponentsInChildren<RectTransform>(true);
                        if (componentsInChildren3 != null)
                        {
                            RectTransform[] array3 = componentsInChildren3;
                            foreach (RectTransform val10 in array3)
                            {
                                if (!((Object)(object)val10 != (Object)null))
                                {
                                    continue;
                                }
                                if (ShowPixelGlow.Value)
                                {
                                    if (((Object)val10).name == "CameraFoilShine" && ((Object)((Transform)val10).parent).name == "CardBorder")
                                    {
                                        Transform parent3 = ((Transform)val10).parent;
                                        Transform parent4 = parent3.parent;
                                        Transform val11 = parent4.Find("FoilGrp");
                                        Transform val12 = val11.Find("GlowMask");
                                        if ((Object)(object)val12 != (Object)null)
                                        {
                                            ((Transform)val10).SetParent(val12, true);
                                        }
                                    }
                                    if (((Object)val10).name == "CameraFoilShine" && ((Object)((Transform)val10).parent).name == "GlowMask")
                                    {
                                        val10.offsetMax = new Vector2(-165f, -45f);
                                        val10.offsetMin = new Vector2(158f, 45f);
                                    }
                                }
                                if (((Object)val10).name == "GlowMaskMonsterGrp")
                                {
                                    ((Component)val10).gameObject.SetActive(false);
                                }
                                if (((Object)val10).name == "MonsterMaskGrp")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val10.offsetMax = new Vector2(-2f, -30f);
                                        val10.offsetMin = new Vector2(-17f, -31f);
                                    }
                                    else if (cardData.expansionType == ECardExpansionType.FantasyRPG || cardData.expansionType == ECardExpansionType.CatJob)
                                    {
                                        val10.offsetMax = new Vector2(11f, 184f);
                                        val10.offsetMin = new Vector2(-7f, -143f);
                                    }
                                    else
                                    {
                                        val10.offsetMax = new Vector2(0f, 0f);
                                        val10.offsetMin = new Vector2(0f, 0f);
                                    }
                                }
                                if (((Object)val10).name == "MonsterMask")
                                {
                                    Image component4 = ((Component)val10).GetComponent<Image>();
                                    if ((Object)(object)component4 != (Object)null && (Object)(object)component4.sprite != (Object)null && (Object)(object)component4.sprite.texture != (Object)null)
                                    {
                                        string name2 = ((Object)component4.sprite.texture).name;
                                        if (name2 == "WhiteTile" && (Object)(object)CardFoilMaskC != (Object)null)
                                        {
                                            Sprite val13 = TextureToSprite(CardFoilMaskC);
                                            ((Object)val13).name = name2;
                                            component4.sprite = val13;
                                        }
                                    }
                                }
                                if (((Object)val10).name == "Image")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val10.offsetMax = new Vector2(-180f, -57f);
                                        val10.offsetMin = new Vector2(172f, 57f);
                                    }
                                    else if (cardData.expansionType == ECardExpansionType.FantasyRPG || cardData.expansionType == ECardExpansionType.CatJob)
                                    {
                                        val10.offsetMax = new Vector2(-180f, -190f);
                                        val10.offsetMin = new Vector2(172f, 188f);
                                    }
                                    else
                                    {
                                        val10.offsetMax = new Vector2(-172f, -57f);
                                        val10.offsetMin = new Vector2(165f, 57f);
                                    }
                                }
                                if (((Object)val10).name == "NameTextGrp" || ((Object)val10).name == "NumberTextGrp" || ((Object)val10).name == "FirstEditionTextGrp" || ((Object)val10).name == "StatText_1" || ((Object)val10).name == "StatText_2" || ((Object)val10).name == "StatText_3" || ((Object)val10).name == "StatText_4")
                                {
                                    ((Transform)val10).SetSiblingIndex(15);
                                }
                            }
                        }
                    }
                }
            }
            if (ChangeCardsFull.Value)
            {
                GameObject fullArtGrp = __instance.m_FullArtGrp;
                if ((Object)(object)fullArtGrp != (Object)null)
                {
                    Transform[] componentsInChildren4 = fullArtGrp.GetComponentsInChildren<Transform>(true);
                    if (componentsInChildren4 != null)
                    {
                        Transform[] array4 = componentsInChildren4;
                        foreach (Transform val14 in array4)
                        {
                            if (!((Object)(object)val14 != (Object)null))
                            {
                                continue;
                            }
                            if (!ShowEvolvesBoxFull.Value && ((Object)val14).name == "EvoBG")
                            {
                                ((Component)val14).gameObject.SetActive(false);
                            }
                            if (!ShowStatsFull.Value)
                            {
                                if (((Object)val14).name == "StatText_1")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                if (((Object)val14).name == "StatText_2")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                if (((Object)val14).name == "StatText_3")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                if (((Object)val14).name == "StatText_4")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (((Object)val14).name == "StatText_1")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                                if (((Object)val14).name == "StatText_2")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                                if (((Object)val14).name == "StatText_3")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                                if (((Object)val14).name == "StatText_4")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                            }
                            if (!ShowArtistFull.Value)
                            {
                                if (((Object)val14).name == "ArtistText")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                if (((Object)val14).name == "CompanyText")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (((Object)val14).name == "ArtistText")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                                if (((Object)val14).name == "CompanyText")
                                {
                                    ((Component)val14).gameObject.SetActive(true);
                                }
                            }
                            if (!ShowEvolvesFull.Value)
                            {
                                if (((Object)val14).name == "EvoBorder")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                else if (((Object)val14).name == "EvoBasicIcon")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                else if (((Object)val14).name == "EvoBasicText")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                else if (((Object)val14).name == "EvoNameText")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                else if (((Object)val14).name == "EvoPreviousStageIcon")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                                else if (((Object)val14).name == "EvoText")
                                {
                                    ((Component)val14).gameObject.SetActive(false);
                                }
                            }
                            if (!ShowPlayEffectFull.Value && ((Object)val14).name == "TitleText")
                            {
                                ((Component)val14).gameObject.SetActive(false);
                            }
                            if (!ShowPlayEffectBoxFull.Value && ((Object)val14).name == "TitleBG")
                            {
                                ((Component)val14).gameObject.SetActive(false);
                            }
                            if (!ShowDescriptionTextFull.Value && ((Object)val14).name == "DescriptionText")
                            {
                                ((Component)val14).gameObject.SetActive(false);
                            }
                        }
                    }
                    if (ShowCardFull.Value)
                    {
                        RectTransform[] componentsInChildren5 = fullArtGrp.GetComponentsInChildren<RectTransform>(true);
                        if (componentsInChildren5 != null)
                        {
                            RectTransform[] array5 = componentsInChildren5;
                            foreach (RectTransform val15 in array5)
                            {
                                if (!((Object)(object)val15 != (Object)null))
                                {
                                    continue;
                                }
                                if (((Object)val15).name == "ElementBGMask")
                                {
                                    val15.offsetMax = new Vector2(36f, 40f);
                                    val15.offsetMin = new Vector2(-36f, -40f);
                                }
                                if (((Object)val15).name == "ElementBGImage")
                                {
                                    val15.offsetMax = new Vector2(0f, 0f);
                                    val15.offsetMin = new Vector2(0f, 0f);
                                }
                                if (((Object)val15).name == "MonsterMask")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val15.offsetMax = new Vector2(43f, 4f);
                                        val15.offsetMin = new Vector2(-43f, -4f);
                                    }
                                    else
                                    {
                                        val15.offsetMax = new Vector2(40f, 4f);
                                        val15.offsetMin = new Vector2(-40f, -4f);
                                    }
                                }
                                if (((Object)val15).name == "Image")
                                {
                                    if (cardData.expansionType == ECardExpansionType.Megabot)
                                    {
                                        val15.offsetMax = new Vector2(-54f, 160f);
                                        val15.offsetMin = new Vector2(47f, -160f);
                                    }
                                    else
                                    {
                                        val15.offsetMax = new Vector2(-67f, 142f);
                                        val15.offsetMin = new Vector2(60f, -142.5f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (ChangeCardsGhost.Value)
            {
                CardUI ghostCard = __instance.m_GhostCard;
                GameObject val16 = ((ghostCard != null) ? ((Component)ghostCard).gameObject : null);
                if ((Object)(object)val16 != (Object)null)
                {
                    RectTransform[] componentsInChildren6 = val16.GetComponentsInChildren<RectTransform>(true);
                    if (ShowCardFullGhost.Value && componentsInChildren6 != null && componentsInChildren6.Length != 0)
                    {
                        RectTransform[] array6 = componentsInChildren6;
                        foreach (RectTransform val17 in array6)
                        {
                            if ((Object)(object)val17 != (Object)null)
                            {
                                if (((Object)val17).name == "MonsterMask")
                                {
                                    val17.offsetMax = new Vector2(3f, 4f);
                                    val17.offsetMin = new Vector2(-5.5f, -2f);
                                }
                                if (((Object)val17).name == "Image")
                                {
                                    val17.offsetMax = new Vector2(-83f, 110f);
                                    val17.offsetMin = new Vector2(78f, -111f);
                                }
                            }
                        }
                    }
                    Transform[] componentsInChildren7 = val16.GetComponentsInChildren<Transform>(true);
                    if (componentsInChildren7 != null)
                    {
                        Transform[] array7 = componentsInChildren7;
                        foreach (Transform val18 in array7)
                        {
                            if (!((Object)(object)val18 != (Object)null))
                            {
                                continue;
                            }
                            if (!ShowStatsGhost.Value)
                            {
                                if (((Object)val18).name == "StatText_1")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                                if (((Object)val18).name == "StatText_2")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                                if (((Object)val18).name == "StatText_3")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                                if (((Object)val18).name == "StatText_4")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                if (((Object)val18).name == "StatText_1")
                                {
                                    ((Component)val18).gameObject.SetActive(true);
                                }
                                if (((Object)val18).name == "StatText_2")
                                {
                                    ((Component)val18).gameObject.SetActive(true);
                                }
                                if (((Object)val18).name == "StatText_3")
                                {
                                    ((Component)val18).gameObject.SetActive(true);
                                }
                                if (((Object)val18).name == "StatText_4")
                                {
                                    ((Component)val18).gameObject.SetActive(true);
                                }
                            }
                            if (!ShowBoxesGhost.Value)
                            {
                                if (((Object)val18).name == "CardStat")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val18).name == "CardStat")
                            {
                                ((Component)val18).gameObject.SetActive(true);
                            }
                            if (!ShowNameGhost.Value)
                            {
                                if (((Object)val18).name == "NameText")
                                {
                                    ((Component)val18).gameObject.SetActive(false);
                                }
                            }
                            else if (((Object)val18).name == "NameText")
                            {
                                ((Component)val18).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
            Instance.ApplyCustomFont(__instance.m_CardFront);
            if ((Object)(object)value != (Object)null)
            {
                Instance.ApplyCustomFont(value.m_GradedCardGrp);
            }
        }
    }

    private static BepInExPlugin Instance;

    private static ConfigEntry<bool> modEnabled;

    private static ConfigEntry<KeyCode> reloadKey;

    private static ConfigEntry<bool> ChangeCards;

    private static ConfigEntry<bool> ChangeCardsFull;

    private static ConfigEntry<bool> ChangeCardsGhost;

    private static ConfigEntry<bool> ShowEvolves;

    private static ConfigEntry<bool> ShowEvolvesBox;

    private static ConfigEntry<bool> ShowPlayEffect;

    private static ConfigEntry<bool> ShowPlayEffectBox;

    private static ConfigEntry<bool> ShowDescriptionText;

    private static ConfigEntry<bool> ShowArtist;

    private static ConfigEntry<bool> ShowStats;

    private static ConfigEntry<bool> ShowName;

    private static ConfigEntry<bool> ShowNumber;

    private static ConfigEntry<bool> ShowEdition;

    private static ConfigEntry<bool> ShowFull;

    private static ConfigEntry<bool> ShowFullFrame;

    private static ConfigEntry<bool> ShowRarity;

    private static ConfigEntry<bool> ShowRarityIcon;

    private static ConfigEntry<bool> ShowPixelGlow;

    private static ConfigEntry<bool> ShowCardFull;

    private static ConfigEntry<bool> ShowEvolvesBoxFull;

    private static ConfigEntry<bool> ShowStatsFull;

    private static ConfigEntry<bool> ShowArtistFull;

    private static ConfigEntry<bool> ShowEvolvesFull;

    private static ConfigEntry<bool> ShowPlayEffectFull;

    private static ConfigEntry<bool> ShowPlayEffectBoxFull;

    private static ConfigEntry<bool> ShowDescriptionTextFull;

    private static ConfigEntry<bool> ShowNameGhost;

    private static ConfigEntry<bool> ShowBoxesGhost;

    private static ConfigEntry<bool> ShowStatsGhost;

    private static ConfigEntry<bool> ShowCardFullGhost;

    private static ConfigEntry<float> ShopSignX;

    private static ConfigEntry<float> ShopSignY;

    private static ConfigEntry<float> ShopSignZ;

    private static ConfigEntry<float> ShopSignW;

    private static ConfigEntry<float> ShopSignH;

    private static ConfigEntry<Color> ShopSignColor;

    private static ConfigEntry<Color> ShopSignColor2;

    private static ConfigEntry<float> ShopSignFontSize;

    private static ConfigEntry<bool> UseCustomShopSignFont;

    private static ConfigEntry<float> ShopSignOBJX;

    private static ConfigEntry<float> ShopSignOBJY;

    private static ConfigEntry<float> ShopSignOBJZ;

    public static string path_tex = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_textures/");

    public static string path_mes = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_meshes/");

    public static string path_nam = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "objects_data/");

    private static MeshFilter[] mesh_list = (MeshFilter[])(object)new MeshFilter[0];

    private static Material[] mat_list = (Material[])(object)new Material[0];

    private static Image[] image_list = new Image[0];

    private static bool init_inTitle = false;

    private static bool init_inGame = false;

    private static bool init_filePaths = true;

    private static bool init_filePathGos = false;

    private static bool shouldReload = false;

    public static Dictionary<string, string> filePaths_tex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, string> filePaths_obj = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, string> filePaths_ttf = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public static Dictionary<string, string> filePaths_nam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

    private static Dictionary<string, Mesh> cachedMeshes = new Dictionary<string, Mesh>();

    private static Dictionary<string, Cubemap> cachedCubemap = new Dictionary<string, Cubemap>();

    private static Dictionary<string, TMP_FontAsset> cachedFonts = new Dictionary<string, TMP_FontAsset>();

    private static Dictionary<string, string> cachedData = new Dictionary<string, string>();

    private static Texture2D CardFoilMaskC;

    private static Sprite CardCropGrading;

    private static Color card_color;

    private static Color card_color_outline;

    private static float card_outline = -1f;

    private static TMP_FontAsset billboardTextFont;

    public static GameObject tempmesh = new GameObject((string)null);

    private Cubemap cubemap_day;

    private Cubemap cubemap_night;

    private Cubemap cubemap_sun;

    private static CubemapFace c1 = (CubemapFace)5;

    private static CubemapFace c2 = (CubemapFace)0;

    private static CubemapFace c3 = (CubemapFace)3;

    private static CubemapFace c4 = (CubemapFace)2;

    private static CubemapFace c5 = (CubemapFace)1;

    private static CubemapFace c6 = (CubemapFace)4;

    private string bagname = "";

    private string playtablename = "";

    private string cardname = "";

    private Texture2D blackBackground;

    private bool showMessage = false;

    private bool doPhone = false;

    private static Dictionary<string, Mesh> vanilla_podium_meshes = new Dictionary<string, Mesh>();

    private static Dictionary<string, Texture> vanilla_podium_textures = new Dictionary<string, Texture>();

    private static readonly string[] renderer_names = new string[35]
    {
        "SmallMetalShelf", "SmallPersonalShelf", "PersonalShelfGlass", "HugePersonalShelf", "TallCardDisplayCase", "SleekCardDisplayCase", "Weapons closet", "AdapterMesh", "CardDisplayTable", "ShelvingModel",
        "CounterModel", "Monitor", "DrawerModel", "LongShelf", "Model", "Workbench", "Table 1_Half", "Table 1", "Chair 2 (12)", "Chair 2 (13)",
        "Chair 2 (11)", "Chair 2 (10)", "Chair 2 (14)", "Chair 2 (15)", "door B", "windows door", "GlassDoor", "House01_BtmCut", "House_01_BtmCut1", "Door",
        "crank", "door frame", "House01_Cut", "House19_Cut", "House19_BtmCut"
    };

    private static readonly string[] podium_meshes = new string[29]
    {
        "BatD_Statue_Mesh", "Bear_AtkPose", "Beetle_AtkPose", "Beetle_AtkPose2", "BugA_AtkPose", "BugB_IdlePose", "BugC_IdlePose", "BugD_AtkPose", "EarthDragon_IdlePose", "Figurine_PigB_Mesh",
        "FireDragon_AtkPose", "FireWolfA_IdlePose", "FireWolfB_IdlePose", "FireWolfC_IdlePose", "FireWolfD_IdlePose", "FoxD_IdlePose", "GolemA_Mesh", "PiggyA_IdlePose", "PiggyC_IdlePose", "PiggyD_AtkPose",
        "PiggyD_IdlePose", "ShellfishD_IdlePose", "StarfishA_Mesh", "StarfishD_Mesh", "ThunderDragon_IdlePose", "TreeD_IdlePose", "WaterDragon_AtkPPose", "Wisp_AtkPose", "Wisp_IdlePose"
    };

    private static readonly string[] podium_textures = new string[27]
    {
        "T_BatD", "T_Bear", "T_Bear", "T_Beetle", "T_BugA", "T_BugB", "T_BugC", "T_BugD", "T_DragonEarth", "T_PiggyB",
        "T_DragonFire", "T_FireWolfA", "T_FireWolfB", "T_FireWolfC", "T_FireWolfD", "T_FoxD", "T_GolemA", "T_PiggyA", "T_PiggyC", "T_PiggyD",
        "T_ShellyD", "T_StarfishA", "T_StarfishD", "T_DragonThunder", "T_TreeD", "T_DragonWater", "T_Wisp"
    };

    private static readonly string[] figures_textures = new string[19]
    {
        "T_PiggyD", "T_FoxB", "T_PiggyB", "T_GolemB", "T_BatD", "T_Beetle", "T_GolemD", "T_PiggyC", "T_StarfishD", "T_BatB",
        "T_GolemC", "T_StarfishC", "T_ToonZPlushie", "T_StarfishB", "T_BatC", "T_StarfishA", "T_GolemA", "T_BatA", "T_PiggyA"
    };

    private void Awake()
    {
        //IL_0439: Unknown result type (might be due to invalid IL or missing references)
        //IL_0443: Expected O, but got Unknown
        //IL_0476: Unknown result type (might be due to invalid IL or missing references)
        //IL_0480: Expected O, but got Unknown
        //IL_04b3: Unknown result type (might be due to invalid IL or missing references)
        //IL_04bd: Expected O, but got Unknown
        //IL_052e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0566: Unknown result type (might be due to invalid IL or missing references)
        //IL_05ec: Unknown result type (might be due to invalid IL or missing references)
        //IL_05f6: Expected O, but got Unknown
        //IL_0629: Unknown result type (might be due to invalid IL or missing references)
        //IL_0633: Expected O, but got Unknown
        //IL_0666: Unknown result type (might be due to invalid IL or missing references)
        //IL_0670: Expected O, but got Unknown
        Instance = this;
        modEnabled = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "ENABLED", true, "Enable this mod");
        reloadKey = ((BaseUnityPlugin)this).Config.Bind<KeyCode>("General", "RELOAD KEY (FOR MODDER)", (KeyCode)286, "Key to Reload Textures & Meshes");
        ChangeCards = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "(NORMAL) CHANGE CARDS", true, "If True all below Options can be activate/deactivate.\nIf False it is completely turned off\nYou have to reload your savegame!");
        ChangeCardsFull = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "(FULL) CHANGE CARDS", true, "If True all below Options can be activate/deactivate.\nIf False it is completely turned off\nYou have to reload your savegame!");
        ChangeCardsGhost = ((BaseUnityPlugin)this).Config.Bind<bool>("General", "(GHOST) CHANGE CARDS", true, "If True all below Options can be activate/deactivate.\nIf False it is completely turned off\nYou have to reload your savegame!");
        ShowStats = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW STATS TEXT", true, "Show 'Stats' text on normal cards.\nYou have to reload your savegame!");
        ShowEvolves = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW EVOLVES", true, "Show 'Evolves from' on normal cards.\nYou have to reload your savegame!");
        ShowEvolvesBox = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW EVOLVES BOX", true, "Show 'Evolves from' box on normal cards.\nYou have to reload your savegame!");
        ShowPlayEffect = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW PLAYEFFECT TEXT", true, "Show 'Play Effect' text on normal cards.\nYou have to reload your savegame!");
        ShowPlayEffectBox = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW PLAYEFFECT BOX", true, "Show 'Play Effect' box on normal cards.\nYou have to reload your savegame!");
        ShowDescriptionText = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW DESCRIPTION TEXT", true, "Show 'Description' text on normal cards.\nYou have to reload your savegame!");
        ShowArtist = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW ARTIST TEXT", true, "Show 'Artist and Copyright' box on normal cards.\nYou have to reload your savegame!");
        ShowName = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW NAME TEXT", true, "Show 'Name' text on normal cards.\nYou have to reload your savegame!");
        ShowNumber = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW NUMBER TEXT", true, "Show 'Number' text on normal cards.\nYou have to reload your savegame!");
        ShowEdition = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW EDITION TEXT", true, "Show 'Edition' text on normal cards.\nYou have to reload your savegame!");
        ShowFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW FULL IMAGE", false, "Show full Image on normal cards.\nYou have to reload your savegame!");
        ShowFullFrame = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW FULL IMAGE WITH FRAME", false, "Show full Image with frame on normal cards.\nYou have to reload your savegame!");
        ShowRarity = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW RARITY TEXT", true, "Show Rarity text on normal cards.\nYou have to reload your savegame!");
        ShowRarityIcon = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW RARITY ICON", true, "Show Rarity icon on normal cards.\nYou have to reload your savegame!");
        ShowPixelGlow = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(NORMAL) SHOW PIXEL GLOW", false, "Show Pixel Glow Effect on normal full image cards.\nYou have to reload your savegame!");
        ShowStatsFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW STATS TEXT", true, "Show 'Stats' text on full cards.\nYou have to reload your savegame!");
        ShowEvolvesBoxFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW EVOLVES BOX", true, "Show 'Evolves from' box on full cards.\nYou have to reload your savegame!");
        ShowArtistFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW ARTIST TEXT", true, "Show 'Artist and Copyright' box on full cards.\nYou have to reload your savegame!");
        ShowCardFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW FULL IMAGE", false, "Show full Image on full cards.\nYou have to reload your savegame!");
        ShowEvolvesFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW EVOLVES", true, "Show 'Evolves from' on cards.\nYou have to reload your savegame!");
        ShowPlayEffectFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW PLAYEFFECT TEXT", true, "Show 'Play Effect' text on cards.\nYou have to reload your savegame!");
        ShowPlayEffectBoxFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW PLAYEFFECT BOX", true, "Show 'Play Effect' box on cards.\nYou have to reload your savegame!");
        ShowDescriptionTextFull = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(FULL) SHOW DESCRIPTION TEXT", true, "Show 'Description' text on full cards.\nYou have to reload your savegame!");
        ShowNameGhost = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(GHOST) SHOW NAME TEXT", true, "Show 'Name' text on ghost cards.\nYou have to reload your savegame!");
        ShowStatsGhost = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(GHOST) SHOW STATS TEXT", true, "Show 'Stats' text on ghost cards.\nYou have to reload your savegame!");
        ShowBoxesGhost = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(GHOST) SHOW BOXES", true, "Show boxes on ghost cards.\nYou have to reload your savegame!");
        ShowCardFullGhost = ((BaseUnityPlugin)this).Config.Bind<bool>("Options", "(GHOST) SHOW FULL IMAGE", false, "Show full Image on ghost cards.\nYou have to reload your savegame!");
        ShopSignX = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAME X (FORWARD/BACKWARD)", 4.26f, new ConfigDescription("Move the shop name forward and backward", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-20f, 20f), Array.Empty<object>()));
        ShopSignY = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAME Y (UP/DOWN)", 3.97f, new ConfigDescription("Move the shop name up and down", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-20f, 20f), Array.Empty<object>()));
        ShopSignZ = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAME Z (RIGHT/LEFT)", -9.65f, new ConfigDescription("Move the shop name right and left", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-30f, 20f), Array.Empty<object>()));
        ShopSignW = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAME WIDTH", 671.82f, "The width of the shop name");
        ShopSignH = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAME HEIGHT", 130f, "The height of the shop name");
        ShopSignColor = ((BaseUnityPlugin)this).Config.Bind<Color>("ShopName", "SHOPNAMEFONT COLOR", new Color(0.706f, 0.521f, 0f, 1f), "The color of the shop name font");
        ShopSignColor2 = ((BaseUnityPlugin)this).Config.Bind<Color>("ShopName", "SHOPNAMEFONT COLOR 2", new Color(0.706f, 0.521f, 0f, 1f), "The color of the shop name font");
        ShopSignFontSize = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPNAMEFONT SIZE", 75f, "The size of the shop name font");
        UseCustomShopSignFont = ((BaseUnityPlugin)this).Config.Bind<bool>("ShopName", "USE CUSTOM SHOPNAMEFONT", true, "Use custom font 'ShopName_Font' if available?");
        ShopSignOBJX = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPSIGN X (FORWARD/BACKWARD)", 4.809998f, new ConfigDescription("Move the shop sign forward and backward", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-20f, 20f), Array.Empty<object>()));
        ShopSignOBJY = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPSIGN Y (UP/DOWN)", 3.940002f, new ConfigDescription("Move the shop sign up and down", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-20f, 20f), Array.Empty<object>()));
        ShopSignOBJZ = ((BaseUnityPlugin)this).Config.Bind<float>("ShopName", "SHOPSIGN Z (RIGHT/LEFT)", -9.816345f, new ConfigDescription("Move the shop sign right and left", (AcceptableValueBase)(object)new AcceptableValueRange<float>(-30f, 20f), Array.Empty<object>()));
        ShopSignX.SettingChanged += OnShopSignChanged;
        ShopSignY.SettingChanged += OnShopSignChanged;
        ShopSignZ.SettingChanged += OnShopSignChanged;
        ShopSignW.SettingChanged += OnShopSignChanged;
        ShopSignH.SettingChanged += OnShopSignChanged;
        ShopSignColor.SettingChanged += OnShopSignChanged;
        ShopSignColor2.SettingChanged += OnShopSignChanged;
        ShopSignFontSize.SettingChanged += OnShopSignChanged;
        ShopSignOBJX.SettingChanged += OnShopSignOBJChanged;
        ShopSignOBJY.SettingChanged += OnShopSignOBJChanged;
        ShopSignOBJZ.SettingChanged += OnShopSignOBJChanged;
        UseCustomShopSignFont.SettingChanged += OnShopSignFontChanged;
        if (modEnabled.Value)
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);
        }
    }

    private void checkFolders()
    {
        if (!Directory.Exists(path_tex))
        {
            Directory.CreateDirectory(path_tex);
        }
        if (!Directory.Exists(path_mes))
        {
            Directory.CreateDirectory(path_mes);
        }
        if (!Directory.Exists(path_nam))
        {
            Directory.CreateDirectory(path_nam);
        }
        if (!Directory.Exists(path_nam + "accessories"))
        {
            Directory.CreateDirectory(path_nam + "accessories");
        }
        if (!Directory.Exists(path_nam + "boosterpacks"))
        {
            Directory.CreateDirectory(path_nam + "boosterpacks");
        }
        if (!Directory.Exists(path_nam + "cards"))
        {
            Directory.CreateDirectory(path_nam + "cards");
        }
        if (!Directory.Exists(path_nam + "figurines"))
        {
            Directory.CreateDirectory(path_nam + "figurines");
        }
    }

    private void OnGUI()
    {
        //IL_0052: Unknown result type (might be due to invalid IL or missing references)
        //IL_0058: Expected O, but got Unknown
        //IL_0080: Unknown result type (might be due to invalid IL or missing references)
        //IL_0091: Unknown result type (might be due to invalid IL or missing references)
        //IL_0096: Unknown result type (might be due to invalid IL or missing references)
        //IL_009f: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a6: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b6: Expected O, but got Unknown
        //IL_00b8: Expected O, but got Unknown
        //IL_00b8: Unknown result type (might be due to invalid IL or missing references)
        //IL_00bd: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c6: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c7: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cc: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cd: Unknown result type (might be due to invalid IL or missing references)
        //IL_00dd: Expected O, but got Unknown
        //IL_00df: Expected O, but got Unknown
        //IL_0109: Unknown result type (might be due to invalid IL or missing references)
        //IL_0140: Unknown result type (might be due to invalid IL or missing references)
        //IL_0028: Unknown result type (might be due to invalid IL or missing references)
        //IL_0032: Expected O, but got Unknown
        //IL_003a: Unknown result type (might be due to invalid IL or missing references)
        if (!showMessage)
        {
            if ((Object)(object)blackBackground == (Object)null)
            {
                blackBackground = new Texture2D(1, 1);
                blackBackground.SetPixel(0, 0, Color.black);
                blackBackground.Apply();
            }
            GUIStyle val = new GUIStyle();
            val.normal.background = blackBackground;
            GUI.Box(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), GUIContent.none, val);
            GUIStyle val2 = new GUIStyle
            {
                fontSize = 30,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            };
            GUIStyle val3 = new GUIStyle
            {
                fontSize = 25,
                normal = new GUIStyleState
                {
                    textColor = Color.white
                }
            };
            Rect val4 = default(Rect);
            ((Rect)(ref val4))..ctor((float)(Screen.width / 2 - 200), (float)(Screen.height / 2 - 75), 400f, 50f);
            GUI.Label(val4, "[TextureReplacer]", val2);
            Rect val5 = default(Rect);
            ((Rect)(ref val5))..ctor((float)(Screen.width / 2 - 200), (float)(Screen.height / 2 - 25), 400f, 100f);
            GUI.Label(val5, "Caching Textures & Meshes\nPlease wait!", val3);
            init_filePaths = false;
        }
    }

    private void Update()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_0511: Unknown result type (might be due to invalid IL or missing references)
        //IL_0516: Unknown result type (might be due to invalid IL or missing references)
        //IL_04ac: Unknown result type (might be due to invalid IL or missing references)
        //IL_04b1: Unknown result type (might be due to invalid IL or missing references)
        //IL_0447: Unknown result type (might be due to invalid IL or missing references)
        //IL_044e: Expected O, but got Unknown
        //IL_03a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_03ac: Expected O, but got Unknown
        //IL_056d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0572: Unknown result type (might be due to invalid IL or missing references)
        //IL_05b1: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if ((((Scene)(ref activeScene)).name == "Title" && !init_filePaths) || shouldReload)
        {
            init_filePaths = true;
            showMessage = true;
            checkFolders();
            filePaths_tex.Clear();
            filePaths_obj.Clear();
            filePaths_ttf.Clear();
            filePaths_nam.Clear();
            try
            {
                string[] array = new string[2] { "*.png", "*.txt" };
                string[] array2 = array;
                foreach (string searchPattern in array2)
                {
                    string[] files = Directory.GetFiles(path_tex, searchPattern, SearchOption.AllDirectories);
                    foreach (string path in files)
                    {
                        string fileName = Path.GetFileName(path);
                        string directoryName = Path.GetDirectoryName(path);
                        if (!filePaths_tex.ContainsKey(fileName))
                        {
                            filePaths_tex.Add(fileName, directoryName + "/");
                        }
                    }
                }
            }
            catch
            {
            }
            try
            {
                string[] array3 = new string[1] { "*.obj" };
                string[] array4 = array3;
                foreach (string searchPattern2 in array4)
                {
                    string[] files2 = Directory.GetFiles(path_mes, searchPattern2, SearchOption.AllDirectories);
                    foreach (string path2 in files2)
                    {
                        string fileName2 = Path.GetFileName(path2);
                        string directoryName2 = Path.GetDirectoryName(path2);
                        if (!filePaths_obj.ContainsKey(fileName2))
                        {
                            filePaths_obj.Add(fileName2, directoryName2 + "/");
                        }
                    }
                }
            }
            catch
            {
            }
            try
            {
                string[] array5 = new string[2] { "*.ttf", "*.otf" };
                string[] array6 = array5;
                foreach (string searchPattern3 in array6)
                {
                    string[] files3 = Directory.GetFiles(path_tex, searchPattern3, SearchOption.AllDirectories);
                    foreach (string path3 in files3)
                    {
                        string fileName3 = Path.GetFileName(path3);
                        string directoryName3 = Path.GetDirectoryName(path3);
                        if (!filePaths_ttf.ContainsKey(fileName3))
                        {
                            filePaths_ttf.Add(fileName3, directoryName3 + "/");
                        }
                    }
                }
            }
            catch
            {
            }
            try
            {
                string[] array7 = new string[1] { "*.txt" };
                string[] array8 = array7;
                foreach (string searchPattern4 in array8)
                {
                    string[] files4 = Directory.GetFiles(path_nam, searchPattern4, SearchOption.AllDirectories);
                    foreach (string path4 in files4)
                    {
                        string fileName4 = Path.GetFileName(path4);
                        string directoryName4 = Path.GetDirectoryName(path4);
                        if (!filePaths_nam.ContainsKey(fileName4))
                        {
                            filePaths_nam.Add(fileName4, directoryName4 + "/");
                        }
                    }
                }
            }
            catch
            {
            }
            CacheMeshesAtStart();
            CacheTexturesAtStart();
            CacheCubemapsAtStart();
            CacheFontsAtStart();
            CacheCardDataAtStart();
            if ((Object)(object)CardFoilMaskC == (Object)null)
            {
                Bitmap cardFoilMaskC = Resources.CardFoilMaskC;
                try
                {
                    if (cardFoilMaskC != null)
                    {
                        using MemoryStream memoryStream = new MemoryStream();
                        ((Image)cardFoilMaskC).Save((Stream)memoryStream, ImageFormat.Png);
                        byte[] array9 = memoryStream.ToArray();
                        Texture2D val = new Texture2D(((Image)cardFoilMaskC).Width, ((Image)cardFoilMaskC).Height);
                        ImageConversion.LoadImage(val, array9);
                        ((Object)val).name = "CardFoilMask";
                        CardFoilMaskC = val;
                    }
                }
                finally
                {
                    ((IDisposable)cardFoilMaskC)?.Dispose();
                }
            }
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
            if (shouldReload)
            {
                shouldReload = false;
                doPhone = false;
                DoReplace();
                Resources.UnloadUnusedAssets();
                activeScene = SceneManager.GetActiveScene();
                if (((Scene)(ref activeScene)).name == "Start")
                {
                    CSingleton<SkyboxBlender>.Instance.UpdateLightingAndReflections(forceUpdate: true);
                    DynamicGI.UpdateEnvironment();
                    CSingleton<LightManager>.Instance.ToggleShopLight();
                    CSingleton<LightManager>.Instance.ToggleShopLight();
                    RefreshAllCards();
                }
            }
            init_filePathGos = true;
        }
        if (init_filePathGos)
        {
            activeScene = SceneManager.GetActiveScene();
            if (((Scene)(ref activeScene)).name == "Title" && !init_inTitle)
            {
                init_inTitle = true;
                bagname = "";
                playtablename = "";
                init_inGame = false;
                doPhone = false;
                DoReplace();
            }
            activeScene = SceneManager.GetActiveScene();
            if (((Scene)(ref activeScene)).name == "Start" && !init_inGame)
            {
                init_inGame = true;
                init_inTitle = false;
                DoReplace();
            }
            if (Input.GetKeyDown(reloadKey.Value))
            {
                shouldReload = true;
            }
        }
    }

    public void RefreshAllCards_NoReflection()
    {
        CardUI[] array = Object.FindObjectsOfType<CardUI>(false);
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        CardUI[] array2 = array;
        foreach (CardUI cardUI in array2)
        {
            CardUI cardUI2 = (Object.op_Implicit((Object)(object)((Component)cardUI).transform.parent) ? ((Component)((Component)cardUI).transform.parent).GetComponentInParent<CardUI>() : null);
            if ((Object)(object)cardUI2 != (Object)null)
            {
                num3++;
                continue;
            }
            CardData cardData = cardUI.GetCardData();
            if (cardData == null)
            {
                num2++;
                continue;
            }
            try
            {
                cardUI.SetCardUI(cardData);
                num++;
            }
            catch
            {
            }
        }
    }

    public void RefreshAllCards()
    {
        CardUI[] array = Object.FindObjectsOfType<CardUI>(true);
        HashSet<CardUI> hashSet = new HashSet<CardUI>();
        Type typeFromHandle = typeof(CardUI);
        FieldInfo field = typeFromHandle.GetField("m_CardData", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo field2 = typeFromHandle.GetField("m_FullArtCard", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo field3 = typeFromHandle.GetField("m_GhostCard", BindingFlags.Instance | BindingFlags.NonPublic);
        CardUI[] array2 = array;
        foreach (CardUI obj in array2)
        {
            CardUI cardUI = field2?.GetValue(obj) as CardUI;
            CardUI cardUI2 = field3?.GetValue(obj) as CardUI;
            if ((Object)(object)cardUI != (Object)null)
            {
                hashSet.Add(cardUI);
            }
            if ((Object)(object)cardUI2 != (Object)null)
            {
                hashSet.Add(cardUI2);
            }
        }
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        CardUI[] array3 = array;
        foreach (CardUI cardUI3 in array3)
        {
            if (hashSet.Contains(cardUI3))
            {
                num3++;
                continue;
            }
            if (!(field?.GetValue(cardUI3) is CardData cardUI4))
            {
                num2++;
                continue;
            }
            try
            {
                cardUI3.SetCardUI(cardUI4);
                num++;
            }
            catch
            {
            }
        }
    }

    private void CacheTexturesAtStart()
    {
        Debug.Log((object)"[TextureReplacer] Caching Textures...");
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            string text = item.Key.Replace(".png", "");
            Texture2D val = null;
            val = ((!text.ToLower().EndsWith("_n") && !text.ToLower().EndsWith("_normal") && !text.ToLower().EndsWith(" n")) ? LoadPNG(item.Value + text + ".png") : LoadPNG_Bump(item.Value + text + ".png"));
            if ((Object)(object)val != (Object)null)
            {
                val.Apply(true, true);
                cachedTextures[text] = val;
            }
        }
        Debug.Log((object)"[TextureReplacer] Textures chached!");
    }

    private Texture2D GetCachedTexture(string name)
    {
        if (cachedTextures.TryGetValue(name, out var value))
        {
            return value;
        }
        return null;
    }

    private void CacheMeshesAtStart()
    {
        //IL_00a3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00aa: Expected O, but got Unknown
        //IL_01c7: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ea: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f1: Expected O, but got Unknown
        Debug.Log((object)"[TextureReplacer] Caching Meshes...");
        foreach (KeyValuePair<string, string> item in filePaths_obj)
        {
            string text = item.Key.Replace(".obj", "");
            tempmesh = new OBJLoader().Load(item.Value + text + ".obj");
            if ((Object)(object)tempmesh != (Object)null)
            {
                try
                {
                    ((Object)tempmesh).name = text;
                    List<Mesh> list = new List<Mesh>();
                    foreach (Transform item2 in tempmesh.transform)
                    {
                        Transform val = item2;
                        Mesh mesh = ((Component)val).gameObject.GetComponent<MeshFilter>().mesh;
                        Vector3[] vertices = mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            vertices[i].x = 0f - vertices[i].x;
                        }
                        mesh.vertices = vertices;
                        int[] triangles = mesh.triangles;
                        for (int j = 0; j < triangles.Length; j += 3)
                        {
                            int num = triangles[j];
                            triangles[j] = triangles[j + 2];
                            triangles[j + 2] = num;
                        }
                        mesh.triangles = triangles;
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                        list.Add(mesh);
                    }
                    CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[list.Count];
                    for (int k = 0; k < list.Count; k++)
                    {
                        ((CombineInstance)(ref array[k])).mesh = list[k];
                        ((CombineInstance)(ref array[k])).transform = Matrix4x4.identity;
                    }
                    Mesh val2 = new Mesh();
                    val2.CombineMeshes(array, false);
                    ((Object)val2).name = text;
                    cachedMeshes[text] = val2;
                    cachedMeshes[text].UploadMeshData(true);
                }
                catch
                {
                }
            }
            Object.Destroy((Object)(object)tempmesh);
        }
        Debug.Log((object)"[TextureReplacer] Meshes cached!");
    }

    private Mesh GetCachedMesh(string name)
    {
        if (cachedMeshes.TryGetValue(name, out var value))
        {
            return value;
        }
        return null;
    }

    private void ApplyFace(Cubemap cubemap, int faceIndex, Texture2D faceTexture, CubemapFace[] faceMapping)
    {
        cubemap.SetPixels(faceTexture.GetPixels(), faceMapping[faceIndex]);
    }

    private void CacheCubemapsAtStart()
    {
        //IL_00eb: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f2: Expected O, but got Unknown
        //IL_0107: Unknown result type (might be due to invalid IL or missing references)
        //IL_010e: Expected O, but got Unknown
        //IL_0147: Unknown result type (might be due to invalid IL or missing references)
        //IL_014d: Expected I4, but got Unknown
        //IL_014f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0155: Expected I4, but got Unknown
        //IL_0157: Unknown result type (might be due to invalid IL or missing references)
        //IL_015d: Expected I4, but got Unknown
        //IL_015f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0165: Expected I4, but got Unknown
        //IL_0167: Unknown result type (might be due to invalid IL or missing references)
        //IL_016d: Expected I4, but got Unknown
        //IL_016f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0175: Expected I4, but got Unknown
        Debug.Log((object)"[TextureReplacer] Caching Cubemaps...");
        Dictionary<string, Cubemap> dictionary = new Dictionary<string, Cubemap>
        {
            { "FS003_Day", null },
            { "FS003_Night", null },
            { "FS003_Sun", null }
        };
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            string text = item.Key.Replace(".png", "");
            foreach (string key in dictionary.Keys)
            {
                if (!text.StartsWith(key))
                {
                    continue;
                }
                string filePath = item.Value + text + ".png";
                Texture2D val = LoadPNGHDR(filePath);
                if (((Texture)val).width == 1024 && ((Texture)val).height == 6144)
                {
                    Cubemap val2 = new Cubemap(1024, (TextureFormat)4, true);
                    for (int i = 0; i < 6; i++)
                    {
                        Texture2D val3 = new Texture2D(1024, 1024, (TextureFormat)4, false);
                        val3.SetPixels(val.GetPixels(0, i * 1024, 1024, 1024));
                        val3.Apply();
                        ApplyFace(val2, i, val3, (CubemapFace[])(object)new CubemapFace[6]
                        {
                            (CubemapFace)(int)c1,
                            (CubemapFace)(int)c2,
                            (CubemapFace)(int)c3,
                            (CubemapFace)(int)c4,
                            (CubemapFace)(int)c5,
                            (CubemapFace)(int)c6
                        });
                    }
                    val2.Apply();
                    UpdateEnvironment(val2);
                    switch (key)
                    {
                        case "FS003_Day":
                            cubemap_day = val2;
                            cachedCubemap[text] = cubemap_day;
                            break;
                        case "FS003_Night":
                            cubemap_night = val2;
                            cachedCubemap[text] = cubemap_night;
                            break;
                        case "FS003_Sun":
                            cubemap_sun = val2;
                            cachedCubemap[text] = cubemap_sun;
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning((object)$"[TextureReplacer] Skipping {text}: Texture dimensions are {((Texture)val).width}x{((Texture)val).height}, not suitable for cubemap (expected 1024x6144).");
                }
                break;
            }
        }
        Debug.Log((object)"[TextureReplacer] Cubemaps cached!");
    }

    private void UpdateEnvironment(Cubemap cubemap)
    {
        CSingleton<SkyboxBlender>.Instance.UpdateLightingAndReflections(forceUpdate: true);
    }

    private Cubemap GetCachedCubemap(string name)
    {
        if (cachedCubemap.TryGetValue(name, out var value))
        {
            return value;
        }
        return null;
    }

    private void CacheFontsAtStart()
    {
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0061: Expected O, but got Unknown
        //IL_00b7: Unknown result type (might be due to invalid IL or missing references)
        //IL_00be: Expected O, but got Unknown
        //IL_0186: Unknown result type (might be due to invalid IL or missing references)
        //IL_0188: Unknown result type (might be due to invalid IL or missing references)
        //IL_01aa: Unknown result type (might be due to invalid IL or missing references)
        //IL_01ac: Unknown result type (might be due to invalid IL or missing references)
        foreach (KeyValuePair<string, string> item in filePaths_ttf)
        {
            string text = item.Key.Substring(0, item.Key.LastIndexOf('.'));
            if (text == "Card_Font")
            {
                Font font = new Font(item.Value + item.Key);
                TMP_FontAsset tMP_FontAsset = TMP_FontAsset.CreateFontAsset(font);
                tMP_FontAsset.material.shader = Shader.Find("TextMeshPro/Distance Field");
                cachedFonts[text] = tMP_FontAsset;
            }
            else if (text == "ShopName_Font")
            {
                Font font2 = new Font(item.Value + item.Key);
                TMP_FontAsset tMP_FontAsset2 = TMP_FontAsset.CreateFontAsset(font2);
                tMP_FontAsset2.material.shader = Shader.Find("TextMeshPro/Distance Field");
                cachedFonts[text] = tMP_FontAsset2;
            }
        }
        Color val = default(Color);
        foreach (KeyValuePair<string, string> item2 in filePaths_tex)
        {
            if (!(item2.Key == "Card_Font.txt"))
            {
                continue;
            }
            string[] array = File.ReadAllLines(item2.Value + item2.Key);
            if (array.Length == 3)
            {
                if (ColorUtility.TryParseHtmlString("#" + array[0], ref val))
                {
                    card_color = val;
                }
                if (ColorUtility.TryParseHtmlString("#" + array[1], ref val))
                {
                    card_color_outline = val;
                }
                try
                {
                    card_outline = float.Parse(array[2], CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }
        }
        Debug.Log((object)"[TextureReplacer] Card Font cached at start");
    }

    private TMP_FontAsset GetCachedFont(string name)
    {
        if (cachedFonts.TryGetValue(name, out var value))
        {
            return value;
        }
        return null;
    }

    private void CacheCardDataAtStart()
    {
        foreach (KeyValuePair<string, string> item in filePaths_nam)
        {
            if (item.Key.Contains("Expansions_Type"))
            {
                string value = File.ReadAllText(item.Value + item.Key);
                cachedData[item.Key] = value;
                continue;
            }
            string[] array = File.ReadAllLines(item.Value + item.Key);
            if (array.Length != 0 && !string.IsNullOrEmpty(array[0]))
            {
                cachedData[item.Key] = array[0];
            }
        }
        Debug.Log((object)"[TextureReplacer] Card Data cached at start");
    }

    private string GetCachedCardData(string name)
    {
        if (cachedData.TryGetValue(name, out var value))
        {
            return value;
        }
        return null;
    }

    private void OnShopSignChanged(object sender, EventArgs e)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            UpdateShopSign();
        }
    }

    private void OnShopSignFontChanged(object sender, EventArgs e)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            UpdateShopSignFont();
        }
    }

    private void OnShopSignOBJChanged(object sender, EventArgs e)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            UpdateShopOBJSign();
        }
    }

    private void UpdateShopSign()
    {
        //IL_0056: Unknown result type (might be due to invalid IL or missing references)
        //IL_0076: Unknown result type (might be due to invalid IL or missing references)
        //IL_013d: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f8: Unknown result type (might be due to invalid IL or missing references)
        //IL_0104: Unknown result type (might be due to invalid IL or missing references)
        //IL_0109: Unknown result type (might be due to invalid IL or missing references)
        //IL_0115: Unknown result type (might be due to invalid IL or missing references)
        //IL_011a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0126: Unknown result type (might be due to invalid IL or missing references)
        //IL_012b: Unknown result type (might be due to invalid IL or missing references)
        RectTransform val = ((IEnumerable<RectTransform>)Resources.FindObjectsOfTypeAll<RectTransform>()).FirstOrDefault((Func<RectTransform, bool>)((RectTransform rt) => ((Object)((Component)rt).gameObject).name == "Billboard_ShopName_Text"));
        if ((Object)(object)val != (Object)null)
        {
            ((Transform)val).position = new Vector3(ShopSignX.Value, ShopSignY.Value, ShopSignZ.Value);
            val.sizeDelta = new Vector2(ShopSignW.Value, ShopSignH.Value);
        }
        CanvasRenderer val2 = ((IEnumerable<CanvasRenderer>)Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault((Func<CanvasRenderer, bool>)((CanvasRenderer rt) => ((Object)((Component)rt).gameObject).name == "Billboard_ShopName_Text"));
        if (!((Object)(object)val2 != (Object)null))
        {
            return;
        }
        TMP_Text component = ((Component)val2).GetComponent<TMP_Text>();
        if ((Object)(object)component != (Object)null)
        {
            if (component.enableVertexGradient)
            {
                VertexGradient colorGradient = component.colorGradient;
                colorGradient.topLeft = ShopSignColor.Value;
                colorGradient.topRight = ShopSignColor.Value;
                colorGradient.bottomLeft = ShopSignColor2.Value;
                colorGradient.bottomRight = ShopSignColor2.Value;
                component.colorGradient = colorGradient;
            }
            component.color = Color.white;
            component.fontSize = ShopSignFontSize.Value;
            component.fontSizeMax = ShopSignFontSize.Value;
            CSingleton<LightManager>.Instance.ToggleShopLight();
            CSingleton<LightManager>.Instance.ToggleShopLight();
        }
    }

    private void UpdateShopSignFont()
    {
        CanvasRenderer val = ((IEnumerable<CanvasRenderer>)Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault((Func<CanvasRenderer, bool>)((CanvasRenderer rt) => ((Object)((Component)rt).gameObject).name == "Billboard_ShopName_Text"));
        if (!((Object)(object)val != (Object)null))
        {
            return;
        }
        TMP_Text component = ((Component)val).GetComponent<TMP_Text>();
        if (!((Object)(object)component != (Object)null))
        {
            return;
        }
        if (UseCustomShopSignFont.Value)
        {
            TMP_FontAsset cachedFont = Instance.GetCachedFont("ShopName_Font");
            if ((Object)(object)cachedFont != (Object)null)
            {
                component.font = cachedFont;
            }
        }
        else
        {
            if ((Object)(object)billboardTextFont == (Object)null)
            {
                billboardTextFont = component.font;
            }
            component.font = billboardTextFont;
        }
    }

    private void UpdateShopOBJSign()
    {
        //IL_0065: Unknown result type (might be due to invalid IL or missing references)
        //IL_0079: Unknown result type (might be due to invalid IL or missing references)
        //IL_007e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0083: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b6: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b8: Unknown result type (might be due to invalid IL or missing references)
        MeshFilter val = ((IEnumerable<MeshFilter>)Resources.FindObjectsOfTypeAll<MeshFilter>()).FirstOrDefault((Func<MeshFilter, bool>)((MeshFilter rt) => ((Object)((Component)rt).gameObject).name == "Billboard"));
        if ((Object)(object)val != (Object)null)
        {
            Transform transform = ((Component)val).gameObject.transform;
            Transform transform2 = CSingleton<LightManager>.Instance.m_ShoplightGrp.transform;
            Vector3 val2 = new Vector3(4.941549f, 1.716644f, -5.770413f) - new Vector3(4.809998f, 3.940002f, -9.816345f);
            transform.position = new Vector3(ShopSignOBJX.Value, ShopSignOBJY.Value, ShopSignOBJZ.Value);
            transform2.position = transform.position + val2;
        }
    }

    private IEnumerator UpdateShopSignDelay()
    {
        CanvasRenderer billboardRectTransform = null;
        while ((Object)(object)billboardRectTransform == (Object)null)
        {
            billboardRectTransform = ((IEnumerable<CanvasRenderer>)Resources.FindObjectsOfTypeAll<CanvasRenderer>()).FirstOrDefault((Func<CanvasRenderer, bool>)((CanvasRenderer rt) => ((Object)((Component)rt).gameObject).name == "Billboard_ShopName_Text"));
            if ((Object)(object)billboardRectTransform == (Object)null)
            {
                yield return (object)new WaitForSeconds(1f);
            }
        }
        TMP_Text billboardText = ((Component)billboardRectTransform).GetComponent<TMP_Text>();
        while ((Object)(object)billboardText == (Object)null)
        {
            yield return (object)new WaitForSeconds(1f);
            billboardText = ((Component)billboardRectTransform).GetComponent<TMP_Text>();
        }
        float timeout = 10f;
        while (billboardText.text == "MY CARD EMPIRE" && timeout > 0f)
        {
            yield return (object)new WaitForSeconds(1f);
            timeout -= 1f;
        }
        CSingleton<LightManager>.Instance.m_BillboardText.color = Color.white;
        _ = (Color)Traverse.Create((object)CSingleton<LightManager>.Instance).Field("m_BillboardTextOriginalColor").GetValue();
        Color bcolor = Color.white;
        Traverse.Create((object)CSingleton<LightManager>.Instance).Field("m_BillboardTextOriginalColor").SetValue((object)bcolor);
        if ((Object)(object)billboardTextFont == (Object)null)
        {
            billboardTextFont = billboardText.font;
        }
        if (UseCustomShopSignFont.Value)
        {
            TMP_FontAsset customFont = Instance.GetCachedFont("ShopName_Font");
            if ((Object)(object)customFont != (Object)null)
            {
                billboardText.font = customFont;
            }
        }
        cardname = "";
        UpdateShopSign();
    }

    private IEnumerator UpdateShopSignOBJDelay()
    {
        MeshFilter billboardRectTransform = null;
        while ((Object)(object)billboardRectTransform == (Object)null)
        {
            billboardRectTransform = ((IEnumerable<MeshFilter>)Resources.FindObjectsOfTypeAll<MeshFilter>()).FirstOrDefault((Func<MeshFilter, bool>)((MeshFilter rt) => ((Object)((Component)rt).gameObject).name == "Billboard"));
            if ((Object)(object)billboardRectTransform == (Object)null)
            {
                yield return (object)new WaitForSeconds(1f);
            }
        }
        UpdateShopOBJSign();
    }

    private void DoReplace()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_0b44: Unknown result type (might be due to invalid IL or missing references)
        //IL_0b49: Unknown result type (might be due to invalid IL or missing references)
        //IL_047f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0484: Unknown result type (might be due to invalid IL or missing references)
        //IL_049c: Unknown result type (might be due to invalid IL or missing references)
        //IL_04a3: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            mesh_list = Resources.FindObjectsOfTypeAll<MeshFilter>();
            if (mesh_list.Length != 0)
            {
                Debug.Log((object)"[TextureReplacer] Custom 3D Objects replacing...");
                MeshFilter[] array = mesh_list;
                foreach (MeshFilter val in array)
                {
                    if (!((Object)(object)val != (Object)null) || !((Object)(object)val.sharedMesh != (Object)null))
                    {
                        continue;
                    }
                    if (podium_meshes.Contains(((Object)val.sharedMesh).name) && !vanilla_podium_meshes.ContainsKey(((Object)val.sharedMesh).name))
                    {
                        vanilla_podium_meshes.Add(((Object)val.sharedMesh).name, val.sharedMesh);
                    }
                    if (!((Object)val).name.Contains("IdlePose") && !((Object)val).name.Contains("AtkPose"))
                    {
                        string name = ((Object)val.sharedMesh).name.Replace("'s", "");
                        Mesh cachedMesh = GetCachedMesh(name);
                        if ((Object)(object)cachedMesh != (Object)null)
                        {
                            val.mesh = cachedMesh;
                            val.mesh.UploadMeshData(true);
                        }
                    }
                }
                Debug.Log((object)"[TextureReplacer] Custom 3D Objects replaced!");
            }
        }
        mat_list = Resources.FindObjectsOfTypeAll<Material>();
        Material val2 = null;
        if (mat_list.Length != 0)
        {
            Debug.Log((object)"[TextureReplacer] Custom Textures replacing...");
            Material[] array2 = mat_list;
            foreach (Material val3 in array2)
            {
                if ((Object)(object)val3 != (Object)null && ((Object)val3).name == "vegetation tex9 albedo")
                {
                    val2 = val3;
                    break;
                }
            }
        }
        if (mat_list.Length != 0)
        {
            Debug.Log((object)"[TextureReplacer] Custom Textures replacing...");
            Material[] array3 = mat_list;
            foreach (Material val4 in array3)
            {
                bool flag = false;
                if (!((Object)(object)val4 != (Object)null))
                {
                    continue;
                }
                Material val5 = val4;
                string[] texturePropertyNames = val5.GetTexturePropertyNames();
                string[] array4 = texturePropertyNames;
                foreach (string text in array4)
                {
                    try
                    {
                        string text2 = "";
                        if ((Object)(object)val5.GetTexture(text) != (Object)null)
                        {
                            text2 = ((Object)val5.GetTexture(text)).name;
                        }
                        Texture2D val6 = null;
                        if (text2 == "T_PaperBagAlbedoClosed" || text2 == "T_PaperBagAlbedoOpen")
                        {
                            if (bagname == "")
                            {
                                val6 = GetRandomTexture(text2);
                            }
                            else
                            {
                                val6 = GetCachedTexture(bagname.Replace("T_PaperBagAlbedoClosed", text2).Replace("T_PaperBagAlbedoOpen", text2));
                                if ((Object)(object)val6 == (Object)null)
                                {
                                    val6 = GetRandomTexture(text2);
                                }
                            }
                        }
                        else
                        {
                            val6 = ((!(text2 == "wood1") && !(text2 == "credit_card_D") && !(text2 == "mcp_building_32_billboards_d")) ? GetCachedTexture(text2) : ((!(text2 == "wood1")) ? GetRandomTexture(text2) : GetRandomTexture("wood")));
                        }
                        if (!((Object)(object)val6 != (Object)null))
                        {
                            continue;
                        }
                        ((Object)val6).name = text2;
                        if (!flag && (text.Contains("_BaseMap") || text.Contains("_MainTex")))
                        {
                            flag = true;
                            if ((Object)(object)val5.mainTexture != (Object)null && podium_textures.Contains(text2) && !vanilla_podium_textures.ContainsKey(((Object)val5.mainTexture).name))
                            {
                                vanilla_podium_textures.Add(((Object)val5.mainTexture).name, val5.mainTexture);
                            }
                            if (text.Contains("_MainTex"))
                            {
                                val5.mainTexture = (Texture)(object)val6;
                            }
                            else
                            {
                                val5.SetTexture("_BaseMap", (Texture)(object)val6);
                            }
                            if (val5.HasColor("_BaseColor"))
                            {
                                Vector4 vector = val5.GetVector("_BaseColor");
                                val5.SetVector("_BaseColor", new Vector4(1f, 1f, 1f, vector.w));
                            }
                            switch (text2)
                            {
                                default:
                                    if (!(text2 == "Ceiling"))
                                    {
                                        if (val5.HasProperty("_MetallicGlossMap") && (Object)(object)val5.GetTexture("_MetallicGlossMap") == (Object)null)
                                        {
                                            Texture2D cachedTexture = GetCachedTexture(text2 + "_m");
                                            if ((Object)(object)cachedTexture != (Object)null)
                                            {
                                                val5.EnableKeyword("_METALLICGLOSSMAP");
                                                val5.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture);
                                            }
                                        }
                                        if (val5.HasProperty("_BumpMap") && (Object)(object)val5.GetTexture("_BumpMap") == (Object)null)
                                        {
                                            Texture2D cachedTexture2 = GetCachedTexture(text2 + "_n");
                                            if ((Object)(object)cachedTexture2 != (Object)null && val5.HasProperty("_BumpMap"))
                                            {
                                                val5.EnableKeyword("_NORMALMAP");
                                                val5.SetTexture("_BumpMap", (Texture)(object)cachedTexture2);
                                                val5.SetInt("_UseNormalMap", 1);
                                            }
                                        }
                                        if (val5.HasProperty("_OcclusionMap") && (Object)(object)val5.GetTexture("_OcclusionMap") == (Object)null)
                                        {
                                            Texture2D cachedTexture3 = GetCachedTexture(text2 + "_o");
                                            if ((Object)(object)cachedTexture3 != (Object)null && val5.HasProperty("_OcclusionMap"))
                                            {
                                                val5.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture3);
                                            }
                                        }
                                        break;
                                    }
                                    goto case "floor";
                                case "floor":
                                case "wood 1":
                                case "wood 2 wall":
                                {
                                    string name2 = "";
                                    string name3 = "";
                                    switch (text2)
                                    {
                                        case "floor":
                                            name2 = "floor_m";
                                            name3 = "floor_n";
                                            break;
                                        case "wood 1":
                                            name2 = "wood 1_m";
                                            name3 = "wood 1_n";
                                            break;
                                        case "wood 2 wall":
                                            name2 = "wood 2 wall_m";
                                            name3 = "wood 2 wall_n";
                                            break;
                                        case "Ceiling":
                                            name2 = "Ceiling M";
                                            val5.SetFloat("_Mode", 2f);
                                            val5.SetOverrideTag("RenderType", "Transparent");
                                            val5.SetInt("_SrcBlend", 5);
                                            val5.SetInt("_DstBlend", 10);
                                            val5.SetInt("_ZWrite", 1);
                                            val5.DisableKeyword("_ALPHATEST_ON");
                                            val5.EnableKeyword("_ALPHABLEND_ON");
                                            val5.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                                            val5.renderQueue = 3000;
                                            break;
                                    }
                                    Texture2D cachedTexture4 = GetCachedTexture(name2);
                                    if ((Object)(object)cachedTexture4 != (Object)null && val5.HasProperty("_MetallicGlossMap"))
                                    {
                                        val5.EnableKeyword("_METALLICGLOSSMAP");
                                        val5.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture4);
                                    }
                                    Texture2D cachedTexture5 = GetCachedTexture(name3);
                                    if ((Object)(object)cachedTexture5 != (Object)null && val5.HasProperty("_BumpMap"))
                                    {
                                        val5.EnableKeyword("_NORMALMAP");
                                        val5.SetTexture("_BumpMap", (Texture)(object)cachedTexture5);
                                        val5.SetInt("_UseNormalMap", 1);
                                    }
                                    break;
                                }
                            }
                            goto IL_08e9;
                        }
                        if (flag && (text.Contains("_BaseMap") || text.Contains("_MainTex")))
                        {
                            continue;
                        }
                        if (text.Contains("_BumpMap"))
                        {
                            val5.EnableKeyword("_NORMALMAP");
                            val5.SetTexture("_BumpMap", (Texture)(object)val6);
                            val5.SetInt("_UseNormalMap", 1);
                        }
                        else if (text.Contains("_MetallicGlossMap"))
                        {
                            val5.EnableKeyword("_METALLICGLOSSMAP");
                            val5.SetTexture("_MetallicGlossMap", (Texture)(object)val6);
                        }
                        else if (text.Contains("_OcclusionMap"))
                        {
                            val5.SetTexture("_OcclusionMap", (Texture)(object)val6);
                        }
                        goto IL_08e9;
                        IL_08e9:
                        if (text2.StartsWith("FS003_"))
                        {
                            Cubemap val7 = GetCachedCubemap(text2);
                            if ((Object)(object)val7 != (Object)null)
                            {
                                ((Object)val7).name = text2;
                                val5.SetTexture(text, (Texture)(object)val7);
                            }
                        }
                        else if (!text.Contains("_BaseMap") && !text.Contains("_MainTex"))
                        {
                            val5.SetTexture(text, (Texture)(object)val6);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        image_list = Resources.FindObjectsOfTypeAll<Image>();
        if (image_list.Length != 0)
        {
            Image[] array5 = image_list;
            foreach (Image image in array5)
            {
                if (!((Object)(object)image != (Object)null) || !((Object)(object)image.sprite != (Object)null))
                {
                    continue;
                }
                Texture2D val8 = null;
                if (((Object)image.sprite).name == "GameStartBG_Blur")
                {
                    val8 = GetRandomTexture("GameStartBG_Blur");
                }
                else if (((Object)image.sprite).name == "PhoneBG2")
                {
                    val8 = GetRandomTexture("PhoneBG");
                }
                else if (((Object)image.sprite).name == "CardBack")
                {
                    if (cardname == "")
                    {
                        val8 = GetRandomTexture("CardBack");
                    }
                    else
                    {
                        val8 = GetCachedTexture(cardname);
                        if ((Object)(object)val8 == (Object)null)
                        {
                            val8 = GetRandomTexture("CardBack");
                        }
                    }
                }
                else
                {
                    val8 = GetCachedTexture(((Object)image.sprite).name);
                }
                if ((Object)(object)val8 != (Object)null)
                {
                    ((Object)val8).name = ((Object)image.sprite).name;
                    Sprite sprite = TextureToSprite(val8);
                    image.sprite = sprite;
                    ((Object)image.sprite).name = ((Object)val8).name;
                }
            }
        }
        changeMenu();
        FixControllerSprites();
        activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            FixSomeObjects();
            FixPhone();
            ReplaceSpriteLists();
            FixNewStatues();
            ((MonoBehaviour)this).StartCoroutine(UpdateShopSignDelay());
            ((MonoBehaviour)this).StartCoroutine(UpdateShopSignOBJDelay());
        }
        Debug.Log((object)"[TextureReplacer] Custom Textures replaced!");
    }

    private Texture2D GetRandomTexture(string baseName)
    {
        List<string> list = new List<string>();
        foreach (string key in cachedTextures.Keys)
        {
            if (key == baseName)
            {
                list.Add(key);
            }
            else if (key.StartsWith(baseName) && !key.StartsWith("wood "))
            {
                string s = key.Substring(baseName.Length);
                if (int.TryParse(s, out var _))
                {
                    list.Add(key);
                }
            }
        }
        if (list.Count == 0)
        {
            return null;
        }
        list = list.OrderBy((string x) => Random.value).ToList();
        string text = list[Random.Range(0, list.Count)];
        if (text.Contains("T_PaperBagAlbedoClosed") || text.Contains("T_PaperBagAlbedoOpen"))
        {
            bagname = text;
        }
        if (text.Contains("PlayTable_chair") || text.Contains("PlayTable_chair_metal") || text.Contains("PlayTable_metal") || text.Contains("PlayTable_wood"))
        {
            playtablename = text;
        }
        if (text.Contains("CardBack"))
        {
            cardname = text;
        }
        return cachedTextures[text];
    }

    private void FixSomeObjects()
    {
        Renderer[] array = Resources.FindObjectsOfTypeAll<Renderer>();
        Renderer[] array2 = array;
        foreach (Renderer val in array2)
        {
            if (!((Object)(object)val != (Object)null) || !renderer_names.Contains(((Object)val).name))
            {
                continue;
            }
            Material[] materials = val.materials;
            if (((Object)val).name == "SmallMetalShelf")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "SmallCabinet_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "MediumMetal", "SmallCabinet_metal_medium");
            }
            else if (((Object)val).name == "SmallPersonalShelf")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "PersonalShelfSmall_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "MediumLightMetal", "PersonalShelfSmall_metal_medium");
                UpdateMaterialGlass(materials, "Glass_03(Clear)", "PersonalShelfSmall_glass.txt");
            }
            else if (((Object)val).name == "PersonalShelfGlass")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "PersonalShelfBig_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "MediumLightMetal", "PersonalShelfBig_metal_medium");
                UpdateMaterial(((Object)val).name, val, materials, "LightMetal", "PersonalShelfBig_metal_light");
                UpdateMaterialGlass(materials, "Glass_02(Clear)", "PersonalShelfBig_glass.txt");
            }
            else if (((Object)val).name == "HugePersonalShelf")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "PersonalShelfHuge_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "metal_bronze", "PersonalShelfHuge_bronze");
                UpdateMaterial(((Object)val).name, val, materials, "dark_bronze", "PersonalShelfHuge_bronze_dark");
                UpdateMaterialGlass(materials, "Glass_02(Clear)", "PersonalShelfHuge_glass.txt");
            }
            else if (((Object)val).name == "TallCardDisplayCase")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "CardDisplaySmall_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "MediumMetal", "CardDisplaySmall_metal_medium");
                UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplaySmall_glass.txt");
            }
            else if (((Object)val).name == "AdapterMesh" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "DisplayCardShelfA")
            {
                UpdateMaterial(((Object)val).name, val, materials, "adapter plastic", "CardDisplaySmall_plastic");
            }
            else if (((Object)val).name == "SleekCardDisplayCase")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "CardDisplayBig_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "LightMetal", "CardDisplayBig_metal_light");
                UpdateMaterial(((Object)val).name, val, materials, "LightBright", "CardDisplayBig_metal_bright");
                UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplayBig_glass.txt");
            }
            else if (((Object)val).name == "AdapterMesh" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "DisplayCardShelfB")
            {
                UpdateMaterial(((Object)val).name, val, materials, "adapter plastic", "CardDisplayBig_plastic");
            }
            else if (((Object)val).name == "Weapons closet")
            {
                UpdateMaterial(((Object)val).name, val, materials, "Weapons closet", "CardTable");
                UpdateMaterial(((Object)val).name, val, materials, "Weapons closet metal", "CardTable_metal");
            }
            else if (((Object)val).name == "AdapterMesh" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "CardShelfGrp")
            {
                UpdateMaterial(((Object)val).name, val, materials, "adapter plastic", "CardTable_plastic");
            }
            else if (((Object)val).name == "CardDisplayTable")
            {
                UpdateMaterial(((Object)val).name, val, materials, "black material", "CardDisplayTable");
                UpdateMaterialGlass(materials, "Glass_03(Clear)", "CardDisplayTable_glass.txt");
                UpdateMaterialGlass(materials, "GlassEdge", "CardDisplayTable_glass_edge.txt");
            }
            else if (((Object)val).name == "AdapterMesh" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "DisplayCardTableA")
            {
                UpdateMaterial(((Object)val).name, val, materials, "adapter plastic", "CardDisplayTable_plastic");
            }
            else if (((Object)val).name == "AdapterMesh" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "VintageCardTableLong")
            {
                UpdateMaterial(((Object)val).name, val, materials, "adapter plastic", "CardVintageTable_plastic");
            }
            else if (((Object)val).name == "ShelvingModel" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "SmallShelfA")
            {
                UpdateMaterial(((Object)val).name, val, materials, "MAT_ShelfA", "SidedShelfSingle");
            }
            else if (((Object)val).name == "ShelvingModel" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "ShelfGrp")
            {
                UpdateMaterial(((Object)val).name, val, materials, "shelving", "SidedShelfDouble");
            }
            else if (((Object)val).name == "CounterModel")
            {
                UpdateMaterial(((Object)val).name, val, materials, "Weapons closet", "CheckoutCounter");
                UpdateMaterial(((Object)val).name, val, materials, "Weapons closet metal", "CheckoutCounter_metal");
            }
            else if (((Object)val).name == "Monitor" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "Interactable_CashierCounter")
            {
                UpdateMaterial(((Object)val).name, val, materials, "Cash_R_Monitor_Button", "CashRegister_button");
                UpdateMaterial(((Object)val).name, val, materials, "Cash_R_Monitor_Silver", "CashRegister_silver");
                UpdateMaterial(((Object)val).name, val, materials, "Cash_R_Monitor_Screen", "CashRegister_Screen");
                UpdateMaterial(((Object)val).name, val, materials, "Cash_R_Monitor_Black_Plastic", "CashRegister_plastic");
                UpdateMaterial(((Object)val).name, val, materials, "lambert3", "CashRegister_lambert1");
                UpdateMaterial(((Object)val).name, val, materials, "lambert4", "CashRegister_lambert2");
                UpdateMaterial(((Object)val).name, val, materials, "lambert5", "CashRegister_lambert3");
                UpdateMaterial(((Object)val).name, val, materials, "lambert6", "CashRegister_lambert4");
            }
            else if (((Object)val).name == "DrawerModel" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "Interactable_CashierCounter")
            {
                UpdateMaterial(((Object)val).name, val, materials, "lambert3", "CashRegister_lambert5");
                UpdateMaterial(((Object)val).name, val, materials, "lambert4", "CashRegister_lambert6");
            }
            else if (((Object)val).name == "LongShelf")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "WideShelf");
            }
            else if (((Object)val).name == "Model")
            {
                if (((Object)GetFirstParent(((Component)val).gameObject)).name == "Interactable_TrashBin" || ((Object)GetFirstParent(((Component)val).gameObject)).name == "ShelfManager")
                {
                    UpdateMaterial(((Object)val).name, val, materials, "black plastic", "TrashBin_plastic");
                    UpdateMaterial(((Object)val).name, val, materials, "bin metallic", "TrashBin_metal");
                }
            }
            else if (((Object)val).name == "Workbench" && ((Object)GetFirstParent(((Component)val).gameObject)).name == "InteractableWorkbench")
            {
                UpdateMaterial(((Object)val).name, val, materials, "DarkMetal", "Workbench_metal_dark");
                UpdateMaterial(((Object)val).name, val, materials, "MediumMetal", "Workbench_metal_medium");
            }
            else if (((Object)GetFirstParent(((Component)val).gameObject)).name == "Interactable_PlayTable")
            {
                if (((Object)val).name == "Table 1_Half")
                {
                    UpdateMaterial(((Object)val).name, val, materials, "wood1", "PlayTable_wood");
                    UpdateMaterial(((Object)val).name, val, materials, "table metallic", "PlayTable_metal");
                }
                else if (((Object)val).name == "Table 1")
                {
                    UpdateMaterial(((Object)val).name, val, materials, "wood1", "PlayTable_wood");
                    UpdateMaterial(((Object)val).name, val, materials, "table metallic", "PlayTable_metal");
                }
                else if (((Object)val).name == "Chair 2 (10)" || ((Object)val).name == "Chair 2 (11)" || ((Object)val).name == "Chair 2 (12)" || ((Object)val).name == "Chair 2 (13)" || ((Object)val).name == "Chair 2 (14)" || ((Object)val).name == "Chair 2 (15)")
                {
                    UpdateMaterial(((Object)val).name, val, materials, "white wood", "PlayTable_chair");
                    UpdateMaterial(((Object)val).name, val, materials, "chair metallic", "PlayTable_chair_metal");
                }
            }
            else if (((Object)val).name == "door B")
            {
                if (((Object)GetParent(((Component)val).gameObject)).name == "Windows door")
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "metal material", "StoreFront_metal_dark");
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "black metal", "StoreFront_metal_bright");
                    UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
                }
            }
            else if (((Object)val).name == "windows door")
            {
                if (((Object)GetParent(((Component)val).gameObject)).name == "Windows door")
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "metal material", "StoreFront_metal_dark");
                    UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
                }
            }
            else if (((Object)val).name == "GlassDoor")
            {
                if (((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("GlassDoorGrp"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "metal material", "StoreFront_metal_dark");
                    UpdateMaterialGlass(materials, "window glass", "StoreFront_glass.txt");
                }
            }
            else if (((Object)val).name == "House01_BtmCut")
            {
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Wall_01", "StoreBFront_wall_stone");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Wall_02", "stone shopBwall");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "door wood", "StoreBFront_doorframe2_wood");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Window", "shopBwindow_frame");
                UpdateMaterialGlass(materials, "Glass_02(Fade)", "StoreBFront_glass.txt");
            }
            else if (((Object)val).name == "House_01_BtmCut1")
            {
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Window", "shopBwindow");
            }
            else if (((Object)val).name == "Door")
            {
                if (((Object)GetParent(((Component)val).gameObject)).name.Contains("OpenedDoor1") || ((Object)GetParent(((Component)val).gameObject)).name.Contains("LockedDoor1"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door wood", "StoreBFront_door_wood");
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door metal", "StoreBFront_metal_bright");
                }
                if (((Object)GetParent(((Component)val).gameObject)).name.Contains("OpenedDoor2") || ((Object)GetParent(((Component)val).gameObject)).name.Contains("LockedDoor2"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door wood", "StoreFront_door_wood");
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door metal", "StoreFront_metal_bright");
                }
            }
            else if (((Object)val).name == "crank")
            {
                if (((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("OpenedDoor1") || ((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("LockedDoor1"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door metal", "StoreBFront_crank_metal_bright");
                }
                if (((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("OpenedDoor2") || ((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("LockedDoor2"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door metal", "StoreFront_crank_metal_bright");
                }
            }
            else if (((Object)val).name == "door frame")
            {
                if (((Object)GetParent(((Component)val).gameObject)).name.Contains("OpenedDoor1") || ((Object)GetParent(((Component)val).gameObject)).name.Contains("LockedDoor1"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door wood", "StoreBFront_doorframe_wood");
                }
                if (((Object)GetParent(((Component)val).gameObject)).name.Contains("OpenedDoor2") || ((Object)GetParent(((Component)val).gameObject)).name.Contains("LockedDoor2"))
                {
                    UpdateMaterialBuildings(((Object)val).name, val, materials, "door wood", "StoreFront_doorframe_wood");
                }
            }
            else if (((Object)val).name == "House19_Cut")
            {
                if (((Object)GetParent(((Component)val).gameObject)).name.Contains("ShopGrp"))
                {
                    UpdateMaterialOnce(((Object)val).name, val, materials[0], "House_02_Wall_01", "StoreFront_brickwall_outside");
                    UpdateMaterialOnce(((Object)val).name, val, materials[2], "Roof_02", "StoreFront_roof");
                    UpdateMaterialOnce(((Object)val).name, val, materials[3], "House_03_Roof", "StoreFront_roof_windows");
                    UpdateMaterialOnce(((Object)val).name, val, materials[4], "Bay_Window", "StoreFront_bay_windows");
                    UpdateMaterialOnce(((Object)val).name, val, materials[5], "Glass_01", "StoreFront_windowsGlass_outside");
                    UpdateMaterialOnce(((Object)val).name, val, materials[6], "House_02_Door", "StoreFront_windows_frame");
                    UpdateMaterialOnce(((Object)val).name, val, materials[7], "House_07_Windows", "StoreFront_windows_walledup");
                    UpdateMaterialOnce(((Object)val).name, val, materials[1], "Porch_02", "Store_AltCeiling");
                }
            }
            else if (((Object)val).name == "House19_BtmCut")
            {
                if (((Object)GetGrandParent(((Component)val).gameObject)).name.Contains("LockedRoomBlocker_Grp"))
                {
                    UpdateMaterialOnce(((Object)val).name, val, materials[0], "House_02_Wall_01", "StoreFront_brickwallBottom_outside");
                    UpdateMaterialOnce(((Object)val).name, val, materials[1], "House_02_Door", "StoreFront_windowsBottom_frame");
                    UpdateMaterialOnce(((Object)val).name, val, materials[2], "Glass_01", "StoreFront_windowsGlassBottom_outside");
                }
            }
            else if (((Object)val).name == "House01_Cut")
            {
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Wall_01", "StoreBFront_stonewall_outside");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "Roof_02", "StoreBFront_roof");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "House_01_Window", "StoreBFront_windows_outside");
                UpdateMaterialBuildings(((Object)val).name, val, materials, "Glass_01", "StoreBFront_windowsGlass_outside");
                UpdateMaterialOnce(((Object)val).name, val, materials[2], "House_01_Wall_02", "StoreB_HiddenObject");
                UpdateMaterialOnce(((Object)val).name, val, materials[3], "House_01_Wall_02", "StoreB_HiddenWall");
                UpdateMaterialOnce(((Object)val).name, val, materials[4], "House_01_Wall_02", "StoreB_AltCeiling");
            }
        }
    }

    private void FixPhone()
    {
        //IL_0a4e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0a50: Unknown result type (might be due to invalid IL or missing references)
        //IL_0a79: Unknown result type (might be due to invalid IL or missing references)
        //IL_0a86: Unknown result type (might be due to invalid IL or missing references)
        //IL_0a88: Unknown result type (might be due to invalid IL or missing references)
        string text = "!!!";
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            if (item.Key == "MobileProviderText.txt")
            {
                string[] array = File.ReadAllLines(item.Value + item.Key);
                if (array.Length != 0)
                {
                    text = array[0];
                }
            }
        }
        string[] array2 = new string[0];
        foreach (KeyValuePair<string, string> item2 in filePaths_tex)
        {
            if (item2.Key == "MobileButtonsText.txt")
            {
                string[] array3 = File.ReadAllLines(item2.Value + item2.Key);
                if (array3.Length != 0)
                {
                    array2 = array3;
                }
            }
        }
        string[] array4 = new string[0];
        foreach (KeyValuePair<string, string> item3 in filePaths_tex)
        {
            if (item3.Key == "Phone_icontext.txt")
            {
                string[] array5 = File.ReadAllLines(item3.Value + item3.Key);
                if (array5.Length != 0)
                {
                    array4 = array5;
                }
            }
        }
        GameObject val = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "UI_PhoneScreen_Grp"));
        if ((Object)(object)val != (Object)null)
        {
            GameObject val2 = FindInChildren(val.transform, "PhoneButtonGrp_RestockBoardGame");
            if ((Object)(object)val2 != (Object)null)
            {
                UpdateIconSprite(val2, "Icon", "Phone_RestockBoardGame_icon");
                UpdateIconSprite(val2, "Icon2", "Phone_RestockBoardGame_icon_small");
                UpdateIconSprite(val2, "BG", "Phone_RestockBoardGame_back");
                UpdateIconSprite(val2, "BG2", "Phone_RestockBoardGame_back2");
                UpdateIconSprite(val2, "BtnInteraction", "Phone_RestockBoardGame_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val2, "Text", array2[0]);
                }
            }
            GameObject val3 = FindInChildren(val.transform, "PhoneButtonGrp_CustomerReview");
            if ((Object)(object)val3 != (Object)null)
            {
                GameObject val4 = FindInChildren(val3.transform, "Icon");
                if ((Object)(object)val4 != (Object)null)
                {
                    UpdateIconSprite(val4, "Icon", "Phone_CustomerReview_icon");
                }
                UpdateIconSprite(val3, "Icon (1)", "Phone_CustomerReview_icon_right");
                UpdateIconSprite(val3, "Icon (2)", "Phone_CustomerReview_icon_left");
                UpdateIconSprite(val3, "BG", "Phone_CustomerReview_back");
                UpdateIconSprite(val3, "BG2", "Phone_CustomerReview_back2");
                UpdateIconSprite(val3, "BtnInteraction", "Phone_CustomerReview_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val3, "Text", array2[1]);
                }
            }
            GameObject val5 = FindInChildren(val.transform, "PhoneButtonGrp_Restock");
            if ((Object)(object)val5 != (Object)null)
            {
                UpdateIconSprite(val5, "Icon", "Phone_Restock_icon");
                UpdateIconSprite(val5, "Icon2", "Phone_Restock_icon_small");
                UpdateIconSprite(val5, "BG", "Phone_Restock_back");
                UpdateIconSprite(val5, "BG2", "Phone_Restock_back2");
                UpdateIconSprite(val5, "BtnInteraction", "Phone_Restock_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val5, "Text", array2[2]);
                }
            }
            GameObject val6 = FindInChildren(val.transform, "PhoneButtonGrp_Furniture");
            if ((Object)(object)val6 != (Object)null)
            {
                UpdateIconSprite(val6, "Icon", "Phone_Furniture_icon");
                UpdateIconSprite(val6, "BG", "Phone_Furniture_back");
                UpdateIconSprite(val6, "BG2", "Phone_Furniture_back2");
                UpdateIconSprite(val6, "BtnInteraction", "Phone_Furniture_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val6, "Text", array2[3]);
                }
            }
            GameObject val7 = FindInChildren(val.transform, "PhoneButtonGrp_ExpandShop");
            if ((Object)(object)val7 != (Object)null)
            {
                UpdateIconSprite(val7, "Icon", "Phone_ExpandShop_icon");
                UpdateIconSprite(val7, "BG", "Phone_ExpandShop_back");
                UpdateIconSprite(val7, "BG2", "Phone_ExpandShop_back2");
                UpdateIconSprite(val7, "BtnInteraction", "Phone_ExpandShop_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val7, "Text", array2[4]);
                }
            }
            GameObject val8 = FindInChildren(val.transform, "PhoneButtonGrp_Setting");
            if ((Object)(object)val8 != (Object)null)
            {
                UpdateIconSprite(val8, "Icon", "Phone_Setting_icon");
                UpdateIconSprite(val8, "BG", "Phone_Setting_back");
                UpdateIconSprite(val8, "BG2", "Phone_Setting_back2");
                UpdateIconSprite(val8, "BtnInteraction", "Phone_Setting_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val8, "Text", array2[5]);
                }
            }
            GameObject val9 = FindInChildren(val.transform, "PhoneButtonGrp_GameEvent");
            if ((Object)(object)val9 != (Object)null)
            {
                UpdateIconSprite(val9, "Icon", "Phone_GameEvent_icon");
                UpdateIconSprite(val9, "BG", "Phone_GameEvent_back");
                UpdateIconSprite(val9, "BG2", "Phone_GameEvent_back2");
                UpdateIconSprite(val9, "BtnInteraction", "Phone_GameEvent_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val9, "Text", array2[6]);
                }
            }
            GameObject val10 = FindInChildren(val.transform, "PhoneButtonGrp_PriceCheck");
            if ((Object)(object)val10 != (Object)null)
            {
                GameObject val11 = FindInChildren(val10.transform, "Icon");
                if ((Object)(object)val11 != (Object)null)
                {
                    UpdateIconSprite(val11, "Icon (3)", "Phone_PriceCheck_icon_small");
                }
                UpdateIconSprite(val10, "Icon", "Phone_PriceCheck_icon");
                UpdateIconSprite(val10, "BG", "Phone_PriceCheck_back");
                UpdateIconSprite(val10, "BG2", "Phone_PriceCheck_back2");
                UpdateIconSprite(val10, "BtnInteraction", "Phone_PriceCheck_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val10, "Text", array2[7]);
                }
            }
            GameObject val12 = FindInChildren(val.transform, "PhoneButtonGrp_Hiring");
            if ((Object)(object)val12 != (Object)null)
            {
                UpdateIconSprite(val12, "Icon", "Phone_Hiring_icon");
                UpdateIconSprite(val12, "BG", "Phone_Hiring_back");
                UpdateIconSprite(val12, "BG2", "Phone_Hiring_back2");
                UpdateIconSprite(val12, "BtnInteraction", "Phone_Hiring_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val12, "Text", array2[8]);
                }
            }
            GameObject val13 = FindInChildren(val.transform, "PhoneButtonGrp_RentBill");
            if ((Object)(object)val13 != (Object)null)
            {
                UpdateIconSprite(val13, "Icon", "Phone_RentBill_icon");
                UpdateIconSprite(val13, "BG", "Phone_RentBill_back");
                UpdateIconSprite(val13, "BG2", "Phone_RentBill_back2");
                UpdateIconSprite(val13, "BtnInteraction", "Phone_RentBill_over");
                if (array2.Length >= 10)
                {
                    UpdateButtonText(val13, "Text", array2[9]);
                }
            }
            GameObject val14 = FindInChildren(val.transform, "TopBarGrp");
            if ((Object)(object)val14 != (Object)null)
            {
                UpdateIconSprite(val14, "TopBar", "Phone_TopBar");
                GameObject val15 = FindInChildren(val14.transform, "MobileProviderText");
                if ((Object)(object)val15 != (Object)null)
                {
                    TextMeshProUGUI component = val15.GetComponent<TextMeshProUGUI>();
                    if ((Object)(object)component != (Object)null && text != "!!!")
                    {
                        component.text = text;
                    }
                }
            }
            GameObject val16 = FindInChildren(val.transform, "PhoneButtonGrp_BuyDecoration");
            if ((Object)(object)val16 != (Object)null)
            {
                UpdateIconSprite(val16, "Icon", "Phone_BuyDecoration_icon");
                UpdateIconSprite(val16, "BG", "Phone_BuyDecoration_back");
                UpdateIconSprite(val16, "BG2", "Phone_BuyDecoration_back2");
                UpdateIconSprite(val16, "BtnInteraction", "Phone_BuyDecoration_over");
                if (array2.Length >= 11)
                {
                    UpdateButtonText(val16, "Text", array2[10]);
                }
            }
            GameObject val17 = FindInChildren(val.transform, "PhoneButtonGrp_Grading");
            if ((Object)(object)val17 != (Object)null)
            {
                UpdateIconSprite(val17, "Icon", "Phone_Grading_icon");
                UpdateIconSprite(val17, "BG", "Phone_Grading_back");
                UpdateIconSprite(val17, "BG2", "Phone_Grading_back2");
                UpdateIconSprite(val17, "BtnInteraction", "Phone_Grading_over");
                if (array2.Length >= 12)
                {
                    UpdateButtonText(val17, "Text", array2[11]);
                }
                if (array4.Length >= 3)
                {
                    GameObject val18 = FindInChildren(val17.transform, "Text");
                    if ((Object)(object)val18 != (Object)null)
                    {
                        TextMeshProUGUI component2 = val18.GetComponent<TextMeshProUGUI>();
                        if ((Object)(object)component2 != (Object)null)
                        {
                            component2.text = array4[0];
                            Color val19 = default(Color);
                            if (ColorUtility.TryParseHtmlString("#" + array4[1], ref val19))
                            {
                                component2.outlineColor = Color32.op_Implicit(val19);
                            }
                            Color val20 = default(Color);
                            if (ColorUtility.TryParseHtmlString("#" + array4[2], ref val20))
                            {
                                component2.color = Color.white;
                                component2.faceColor = Color32.op_Implicit(val20);
                            }
                        }
                    }
                }
            }
        }
        string[] array6 = new string[0];
        foreach (KeyValuePair<string, string> item4 in filePaths_tex)
        {
            if (item4.Key == "MobileUIText.txt")
            {
                string[] array7 = File.ReadAllLines(item4.Value + item4.Key);
                if (array7.Length != 0)
                {
                    array6 = array7;
                }
            }
        }
        if (array6.Length < 13)
        {
            return;
        }
        GameObject val21 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "RestockItemBoardGameScreen_Grp"));
        if ((Object)(object)val21 != (Object)null)
        {
            TextMeshProUGUI[] array8 = FindTextMeshProInChildren(val21.transform).ToArray();
            TextMeshProUGUI[] array9 = array8;
            foreach (TextMeshProUGUI textMeshProUGUI in array9)
            {
                if ((Object)(object)textMeshProUGUI != (Object)null)
                {
                    if (textMeshProUGUI.text == "TABLE TOP GEEK")
                    {
                        textMeshProUGUI.text = array6[0];
                    }
                    if (textMeshProUGUI.text == "Speedrobo Games")
                    {
                        textMeshProUGUI.text = array6[1];
                    }
                    if (textMeshProUGUI.text == "https://speedrobogames.com/")
                    {
                        textMeshProUGUI.text = array6[2];
                    }
                    if (textMeshProUGUI.text == "www.tabletopgeek.com/products/list")
                    {
                        textMeshProUGUI.text = array6[3];
                    }
                }
            }
        }
        GameObject val22 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "RestockItemScreen_Grp"));
        if ((Object)(object)val22 != (Object)null)
        {
            TextMeshProUGUI[] array10 = FindTextMeshProInChildren(val22.transform).ToArray();
            TextMeshProUGUI[] array11 = array10;
            foreach (TextMeshProUGUI textMeshProUGUI2 in array11)
            {
                if ((Object)(object)textMeshProUGUI2 != (Object)null && textMeshProUGUI2.text == "www.tetramon-tcg.com/store/products")
                {
                    textMeshProUGUI2.text = array6[4];
                }
            }
        }
        GameObject val23 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "FurnitureShopUIScreen_Grp"));
        if ((Object)(object)val23 != (Object)null)
        {
            TextMeshProUGUI[] array12 = FindTextMeshProInChildren(val23.transform).ToArray();
            TextMeshProUGUI[] array13 = array12;
            foreach (TextMeshProUGUI textMeshProUGUI3 in array13)
            {
                if ((Object)(object)textMeshProUGUI3 != (Object)null)
                {
                    if (textMeshProUGUI3.text == "MY DIY RACKS")
                    {
                        textMeshProUGUI3.text = array6[5];
                    }
                    if (textMeshProUGUI3.text == "We Got Everything")
                    {
                        textMeshProUGUI3.text = array6[6];
                    }
                    if (textMeshProUGUI3.text == "www.mydiyracks.com/shop/product-listing")
                    {
                        textMeshProUGUI3.text = array6[7];
                    }
                }
            }
        }
        GameObject val24 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "ExpansionShopUIScreen_Grp"));
        if ((Object)(object)val24 != (Object)null)
        {
            TextMeshProUGUI[] array14 = FindTextMeshProInChildren(val24.transform).ToArray();
            TextMeshProUGUI[] array15 = array14;
            foreach (TextMeshProUGUI textMeshProUGUI4 in array15)
            {
                if ((Object)(object)textMeshProUGUI4 != (Object)null)
                {
                    if (textMeshProUGUI4.text == "RENO BIGG")
                    {
                        textMeshProUGUI4.text = array6[8];
                        textMeshProUGUI4.alignment = TextAlignmentOptions.Left;
                    }
                    if (textMeshProUGUI4.text == "BIGG")
                    {
                        textMeshProUGUI4.text = array6[9];
                        textMeshProUGUI4.alignment = TextAlignmentOptions.Center;
                    }
                    if (textMeshProUGUI4.text == "www.renobigg.com/index.php?ws=service_id=1302938")
                    {
                        textMeshProUGUI4.text = array6[10];
                    }
                }
            }
        }
        GameObject val25 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "CheckPriceScreen_Grp"));
        if ((Object)(object)val25 != (Object)null)
        {
            TextMeshProUGUI[] array16 = FindTextMeshProInChildren(val25.transform).ToArray();
            TextMeshProUGUI[] array17 = array16;
            foreach (TextMeshProUGUI textMeshProUGUI5 in array17)
            {
                if ((Object)(object)textMeshProUGUI5 != (Object)null)
                {
                    if (textMeshProUGUI5.text == "TCG PRICE")
                    {
                        textMeshProUGUI5.text = array6[11];
                    }
                    if (textMeshProUGUI5.text == "www.tcgprice.com/info/pricelist")
                    {
                        textMeshProUGUI5.text = array6[12];
                    }
                }
            }
        }
        if (array6.Length < 15)
        {
            return;
        }
        GameObject val26 = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject obj) => ((Object)obj).name == "GradedCardWebsiteCanvasGrp"));
        if (!((Object)(object)val26 != (Object)null))
        {
            return;
        }
        TextMeshProUGUI[] array18 = FindTextMeshProInChildren(val26.transform).ToArray();
        TextMeshProUGUI[] array19 = array18;
        foreach (TextMeshProUGUI textMeshProUGUI6 in array19)
        {
            if ((Object)(object)textMeshProUGUI6 != (Object)null)
            {
                if (textMeshProUGUI6.text == "https://www.thecardinalsgrading.com/submission")
                {
                    textMeshProUGUI6.text = array6[13];
                }
                if (textMeshProUGUI6.text == "The Cardinal's Grading")
                {
                    textMeshProUGUI6.text = array6[14];
                }
            }
        }
    }

    private void UpdateIconSprite(GameObject parent, string childName, string spriteName)
    {
        GameObject val = FindInChildren(parent.transform, childName);
        if ((Object)(object)val != (Object)null)
        {
            Image component = val.GetComponent<Image>();
            if ((Object)(object)component != (Object)null)
            {
                UpdateSprite(component, spriteName);
            }
        }
    }

    public static GameObject[] FindChildrenWithNamePrefix(Transform parent, string prefix)
    {
        return (from child in ((Component)parent).GetComponentsInChildren<Transform>(true)
            where ((Object)child).name.StartsWith(prefix)
            select ((Component)child).gameObject).ToArray();
    }

    private void FixNewStatues()
    {
        InteractableObject[] array = Resources.FindObjectsOfTypeAll<InteractableObject>();
        if (array.Length == 0)
        {
            return;
        }
        foreach (InteractableObject interactableObject in array)
        {
            if ((Object)(object)interactableObject == (Object)null)
            {
                continue;
            }
            GameObject gameObject = ((Component)interactableObject).gameObject;
            string name = ((Object)gameObject).name.Replace("Interactable_", "") + "_Mesh";
            string text = ((Object)gameObject).name.Replace("Interactable_", "");
            if (!((Object)(object)gameObject != (Object)null) || !((Object)gameObject).name.StartsWith("Interactable_MonsterStatue"))
            {
                continue;
            }
            MeshFilter[] componentsInChildren = gameObject.GetComponentsInChildren<MeshFilter>();
            if (componentsInChildren == null || componentsInChildren.Length == 0)
            {
                continue;
            }
            foreach (MeshFilter val in componentsInChildren)
            {
                if ((Object)(object)val == (Object)null)
                {
                    continue;
                }
                Renderer component = ((Component)val).GetComponent<Renderer>();
                RemoveOtherTextures(component.material);
                if (((Object)val).name.Contains("IdlePose") || ((Object)val).name.Contains("AtkPose"))
                {
                    Mesh cachedMesh = GetCachedMesh(name);
                    if ((Object)(object)cachedMesh != (Object)null)
                    {
                        ((Object)cachedMesh).name = ((Object)val.mesh).name;
                        val.mesh = cachedMesh;
                        val.mesh.UploadMeshData(true);
                    }
                    else if (vanilla_podium_meshes.ContainsKey(((Object)val.mesh).name))
                    {
                        val.mesh = vanilla_podium_meshes[((Object)val.mesh).name];
                        val.mesh.UploadMeshData(true);
                    }
                    Renderer component2 = ((Component)val).GetComponent<Renderer>();
                    if ((Object)(object)component2 != (Object)null && (Object)(object)component2.material != (Object)null)
                    {
                        Material material = component2.material;
                        Texture2D cachedTexture = GetCachedTexture(text);
                        if ((Object)(object)cachedTexture != (Object)null)
                        {
                            ((Object)cachedTexture).name = text;
                            material.SetTexture("_MainTex", (Texture)(object)cachedTexture);
                            material.SetFloat("_Mode", 1f);
                            material.SetOverrideTag("RenderType", "TransparentCutout");
                            material.EnableKeyword("_ALPHATEST_ON");
                            material.DisableKeyword("_ALPHABLEND_ON");
                            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            material.SetFloat("_Cutoff", 0.5f);
                            material.SetInt("_ZWrite", 1);
                            material.renderQueue = 2450;
                            material.SetInt("_Cull", 0);
                            material.SetInt("_SrcBlend", 1);
                            material.SetInt("_DstBlend", 0);
                        }
                        else if (vanilla_podium_textures.ContainsKey(((Object)material.GetTexture("_MainTex")).name))
                        {
                            material.SetTexture("_MainTex", vanilla_podium_textures[((Object)material.GetTexture("_MainTex")).name]);
                        }
                        Texture2D cachedTexture2 = GetCachedTexture(text + "_m");
                        if ((Object)(object)cachedTexture2 != (Object)null)
                        {
                            ((Object)cachedTexture2).name = text + "_m";
                            material.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture2);
                            material.EnableKeyword("_METALLICGLOSSMAP");
                        }
                        Texture2D cachedTexture3 = GetCachedTexture(text + "_n");
                        if ((Object)(object)cachedTexture3 != (Object)null)
                        {
                            ((Object)cachedTexture3).name = text + "_n";
                            material.EnableKeyword("_NORMALMAP");
                            material.SetTexture("_BumpMap", (Texture)(object)cachedTexture3);
                            material.SetInt("_UseNormalMap", 1);
                            material.SetFloat("_BumpScale", 1f);
                        }
                        Texture2D cachedTexture4 = GetCachedTexture(text + "_o");
                        if ((Object)(object)cachedTexture4 != (Object)null)
                        {
                            ((Object)cachedTexture4).name = text + "_o";
                            material.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture4);
                        }
                    }
                }
                else if (((Object)val).name == "StatuePodium")
                {
                    string name2 = ((Object)gameObject).name.Replace("Interactable_", "") + "_Podium_Mesh";
                    string text2 = ((Object)gameObject).name.Replace("Interactable_", "") + "_Podium";
                    Mesh cachedMesh2 = GetCachedMesh(name2);
                    if ((Object)(object)cachedMesh2 != (Object)null)
                    {
                        ((Object)cachedMesh2).name = ((Object)val.mesh).name;
                        val.mesh = cachedMesh2;
                        val.mesh.UploadMeshData(true);
                    }
                    Renderer component3 = ((Component)val).GetComponent<Renderer>();
                    if ((Object)(object)component3 != (Object)null && (Object)(object)component3.material != (Object)null)
                    {
                        Material material2 = component3.material;
                        Texture2D cachedTexture5 = GetCachedTexture(text2);
                        if ((Object)(object)cachedTexture5 != (Object)null)
                        {
                            ((Object)cachedTexture5).name = text2;
                            material2.SetTexture("_MainTex", (Texture)(object)cachedTexture5);
                        }
                        else if (vanilla_podium_textures.ContainsKey(((Object)material2.GetTexture("_MainTex")).name))
                        {
                            material2.SetTexture("_MainTex", vanilla_podium_textures[((Object)material2.GetTexture("_MainTex")).name]);
                        }
                        Texture2D cachedTexture6 = GetCachedTexture(text2 + "_m");
                        if ((Object)(object)cachedTexture6 != (Object)null)
                        {
                            ((Object)cachedTexture6).name = text2 + "_m";
                            material2.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture6);
                            material2.EnableKeyword("_METALLICGLOSSMAP");
                        }
                        Texture2D cachedTexture7 = GetCachedTexture(text2 + "_n");
                        if ((Object)(object)cachedTexture7 != (Object)null)
                        {
                            ((Object)cachedTexture7).name = text2 + "_n";
                            material2.EnableKeyword("_NORMALMAP");
                            material2.SetTexture("_BumpMap", (Texture)(object)cachedTexture7);
                            material2.SetInt("_UseNormalMap", 1);
                            material2.SetFloat("_BumpScale", 1f);
                        }
                        Texture2D cachedTexture8 = GetCachedTexture(text2 + "_o");
                        if ((Object)(object)cachedTexture8 != (Object)null)
                        {
                            ((Object)cachedTexture8).name = text2 + "_o";
                            material2.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture8);
                        }
                    }
                }
                else
                {
                    if (!((Object)val).name.Contains("StatuePodium ("))
                    {
                        continue;
                    }
                    string name3 = ((Object)gameObject).name.Replace("Interactable_", "") + "_Podium_Mesh2";
                    string text3 = ((Object)gameObject).name.Replace("Interactable_", "") + "_Podium2";
                    Mesh cachedMesh3 = GetCachedMesh(name3);
                    if ((Object)(object)cachedMesh3 != (Object)null)
                    {
                        ((Object)cachedMesh3).name = ((Object)val.mesh).name;
                        val.mesh = cachedMesh3;
                        val.mesh.UploadMeshData(true);
                    }
                    Renderer component4 = ((Component)val).GetComponent<Renderer>();
                    if ((Object)(object)component4 != (Object)null && (Object)(object)component4.material != (Object)null)
                    {
                        Material material3 = component4.material;
                        Texture2D cachedTexture9 = GetCachedTexture(text3);
                        if ((Object)(object)cachedTexture9 != (Object)null)
                        {
                            ((Object)cachedTexture9).name = text3;
                            material3.SetTexture("_MainTex", (Texture)(object)cachedTexture9);
                        }
                        else if (vanilla_podium_textures.ContainsKey(((Object)material3.GetTexture("_MainTex")).name))
                        {
                            material3.SetTexture("_MainTex", vanilla_podium_textures[((Object)material3.GetTexture("_MainTex")).name]);
                        }
                        Texture2D cachedTexture10 = GetCachedTexture(text3 + "_m");
                        if ((Object)(object)cachedTexture10 != (Object)null)
                        {
                            ((Object)cachedTexture10).name = text3 + "_m";
                            material3.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture10);
                            material3.EnableKeyword("_METALLICGLOSSMAP");
                        }
                        Texture2D cachedTexture11 = GetCachedTexture(text3 + "_n");
                        if ((Object)(object)cachedTexture11 != (Object)null)
                        {
                            ((Object)cachedTexture11).name = text3 + "_n";
                            material3.EnableKeyword("_NORMALMAP");
                            material3.SetTexture("_BumpMap", (Texture)(object)cachedTexture11);
                            material3.SetInt("_UseNormalMap", 1);
                            material3.SetFloat("_BumpScale", 1f);
                        }
                        Texture2D cachedTexture12 = GetCachedTexture(text3 + "_o");
                        if ((Object)(object)cachedTexture12 != (Object)null)
                        {
                            ((Object)cachedTexture12).name = text3 + "_o";
                            material3.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture12);
                        }
                    }
                }
            }
        }
    }

    private void RemoveOtherTextures(Material material)
    {
        if (material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", (Texture)null);
            material.DisableKeyword("_NORMALMAP");
        }
        if (material.HasProperty("_MetallicGlossMap"))
        {
            material.SetTexture("_MetallicGlossMap", (Texture)null);
            material.DisableKeyword("_METALLICGLOSSMAP");
        }
        if (material.HasProperty("_OcclusionMap"))
        {
            material.SetTexture("_OcclusionMap", (Texture)null);
        }
    }

    private void changeMenu()
    {
        string[] source = new string[5] { "BGBlack", "BGBorder", "BGHighlight", "BGMidtone", "BtnInteraction" };
        string[] source2 = new string[8] { "BGBlack", "BGBorder", "BGHighlight", "BGMidtone", "BtnInteraction", "BG", "Gradient (1)", "Text" };
        string[] parentNames = new string[10] { "MainMenuBtn", "QuitBtn", "ResumeBtn", "SaveGameBtn", "SettingBtn", "LoadGameBtn", "NewGameBtn", "QuitBtn", "BackBtn", "FeedbackBtn" };
        CanvasRenderer[] array = Resources.FindObjectsOfTypeAll<CanvasRenderer>();
        CanvasRenderer[] array2 = array;
        foreach (CanvasRenderer val in array2)
        {
            if (!((Object)(object)val != (Object)null) || !(((Object)GetFirstParent(((Component)val).gameObject)).name == "Canvas"))
            {
                continue;
            }
            if (source.Contains(((Object)val).name) && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && !HasSpecParent(((Component)val).gameObject, "GameUIScreen"))
            {
                UpdateIconColor_Parent(((Component)val).gameObject, "All_" + ((Object)val).name);
            }
            if (((Object)val).name.Contains("Text") && !HasSpecParent(((Component)val).gameObject, "RoadmapText"))
            {
                UpdateTextColor_Parent(((Component)val).gameObject, "All_Menu_Text_Color");
            }
            if (source.Contains(((Object)val).name) && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && !HasSpecParent(((Component)val).gameObject, "GameUIScreen"))
            {
                if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "AnimGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "All_BG_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "Mask")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "All_BG2_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "UIGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "All_BG3_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && HasSpecParent(((Component)val).gameObject, "SettingScreen") && ((Object)GetParent(((Component)val).gameObject)).name == "ScreenGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "All_BG4_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && HasSpecParent(((Component)val).gameObject, "LanguageScreen"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "All_BG5_Color");
                }
            }
            if (source2.Contains(((Object)val).name) && ((Object)GetFirstParent(((Component)val).gameObject)).name == "Canvas")
            {
                if (HasSpecParent(((Component)val).gameObject, "MainMenuBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "MainMenuBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "QuitBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "QuitBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "ResumeBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "ResumeBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "SaveGameBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "SaveGameBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "SettingBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "SettingBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "LoadGameBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "LoadGameBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "NewGameBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "NewGameBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "QuitBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "QuitBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "BackBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BackBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "FeedbackBtn"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "FeedbackBtn_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "ConfirmOverwriteSaveScreen") && HasSpecParent(((Component)val).gameObject, "BGBarGrp"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "OverwriteSaveBtnYes_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "ConfirmOverwriteSaveScreen") && HasSpecParent(((Component)val).gameObject, "BGBarGrp (1)"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "OverwriteSaveBtnNo_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "LoadGameOverwriteAutoSaveSlotScreen") && HasSpecParent(((Component)val).gameObject, "BGBarGrp"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "OverwriteLoadBtnYes_" + ((Object)val).name);
                }
                else if (HasSpecParent(((Component)val).gameObject, "LoadGameOverwriteAutoSaveSlotScreen") && HasSpecParent(((Component)val).gameObject, "BGBarGrp (1)"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "OverwriteLoadBtnNo_" + ((Object)val).name);
                }
                else if (((Object)val).name == "BG" && HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name == "ScaleGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "Roadmap_Frame");
                }
                else if (((Object)val).name == "BG" && HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name == "Mask")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "Roadmap_BG");
                }
                else if (((Object)val).name == "Gradient (1)" && HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name == "Seperator")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "Roadmap_Text_Seperator_Color");
                }
                else if (HasSpecParent(((Component)val).gameObject, "SettingScreen"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "SettingScreenBtn_" + ((Object)val).name);
                }
                if (((Object)val).name == "Text" && HasAnyParent(((Component)val).gameObject, parentNames))
                {
                    UpdateTextColor_Parent(((Component)val).gameObject, "Menu_Text_Color");
                }
            }
            if (((Object)val).name == "Mask" && HasSpecParent(((Component)val).gameObject, "RoadmapText"))
            {
                GameObject[] array3 = FindChildrenWithNamePrefix(((Component)val).transform, "Text");
                if (array3 != null)
                {
                    GameObject[] array4 = array3;
                    foreach (GameObject val2 in array4)
                    {
                        if ((Object)(object)val2 != (Object)null)
                        {
                            UpdateTextColor_Parent(val2, "Roadmap_Text_Color");
                        }
                    }
                }
                GameObject[] array5 = FindChildrenWithNamePrefix(((Component)val).transform, "Title");
                if (array5 != null)
                {
                    GameObject[] array6 = array5;
                    foreach (GameObject val3 in array6)
                    {
                        if ((Object)(object)val3 != (Object)null)
                        {
                            UpdateTextColor_Parent(val3, "Roadmap_Text_Color");
                        }
                    }
                }
            }
            if (HasSpecParent(((Component)val).gameObject, "PauseScreen") || HasSpecParent(((Component)val).gameObject, "SaveLoadGameSlotSelectScreen") || HasSpecParent(((Component)val).gameObject, "SettingScreen") || HasSpecParent(((Component)val).gameObject, "TitleScreen") || HasSpecParent(((Component)val).gameObject, "ConfirmOverwriteSaveScreen") || HasSpecParent(((Component)val).gameObject, "ControllerSelectorUIGrp"))
            {
                if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "AnimGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BG_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "Mask")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BG2_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && ((Object)GetParent(((Component)val).gameObject)).name != "ScreenGrp" && ((Object)GetParent(((Component)val).gameObject)).name == "UIGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BG3_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && HasSpecParent(((Component)val).gameObject, "SettingScreen") && ((Object)GetParent(((Component)val).gameObject)).name == "ScreenGrp")
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BG4_Color");
                }
                else if (((Object)val).name == "BG" && !HasSpecParent(((Component)val).gameObject, "RoadmapText") && HasSpecParent(((Component)val).gameObject, "LanguageScreen"))
                {
                    UpdateIconColor_Parent(((Component)val).gameObject, "BG5_Color");
                }
            }
        }
    }

    private void UpdateIconColor_Parent(GameObject parent, string fileName)
    {
        //IL_00a2: Unknown result type (might be due to invalid IL or missing references)
        if (!((Object)(object)parent != (Object)null))
        {
            return;
        }
        Color color = default(Color);
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            if (!(item.Key == fileName + ".txt"))
            {
                continue;
            }
            string[] array = File.ReadAllLines(item.Value + item.Key);
            if (array.Length != 0 && ColorUtility.TryParseHtmlString("#" + array[0], ref color))
            {
                Image component = parent.GetComponent<Image>();
                if ((Object)(object)component != (Object)null)
                {
                    component.color = color;
                }
            }
            break;
        }
    }

    private void UpdateTextColor_Parent(GameObject parent, string fileName)
    {
        //IL_0094: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ba: Unknown result type (might be due to invalid IL or missing references)
        //IL_00bc: Unknown result type (might be due to invalid IL or missing references)
        Color color = default(Color);
        Color val = default(Color);
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            if (!(item.Key == fileName + ".txt"))
            {
                continue;
            }
            string[] array = File.ReadAllLines(item.Value + item.Key);
            if (array.Length != 2)
            {
                continue;
            }
            TextMeshProUGUI component = parent.GetComponent<TextMeshProUGUI>();
            if ((Object)(object)component != (Object)null)
            {
                if (ColorUtility.TryParseHtmlString("#" + array[0], ref color))
                {
                    component.color = color;
                }
                if (ColorUtility.TryParseHtmlString("#" + array[1], ref val))
                {
                    component.outlineColor = Color32.op_Implicit(val);
                }
            }
        }
    }

    private void UpdateSprite(Image image, string spritename)
    {
        //IL_0037: Unknown result type (might be due to invalid IL or missing references)
        Texture2D cachedTexture = GetCachedTexture(spritename);
        if ((Object)(object)cachedTexture != (Object)null)
        {
            Sprite val = TextureToSprite(cachedTexture);
            ((Object)val).name = ((Object)image.sprite).name;
            image.sprite = val;
            image.color = Color.white;
        }
    }

    private void UpdateButtonText(GameObject parent, string childName, string text)
    {
        List<GameObject> list = FindAllInChildren(parent.transform, childName);
        bool flag = true;
        if (list == null)
        {
            return;
        }
        foreach (GameObject item in list)
        {
            TextMeshProUGUI component = item.GetComponent<TextMeshProUGUI>();
            if ((Object)(object)component != (Object)null)
            {
                if (component.text == "TCG" && flag)
                {
                    flag = false;
                }
                else
                {
                    component.text = text;
                }
            }
        }
    }

    private GameObject FindInChildren(Transform parent, string childName)
    {
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_0017: Expected O, but got Unknown
        foreach (Transform item in parent)
        {
            Transform val = item;
            if (((Object)val).name == childName)
            {
                return ((Component)val).gameObject;
            }
            GameObject val2 = FindInChildren(val, childName);
            if ((Object)(object)val2 != (Object)null)
            {
                return val2;
            }
        }
        return null;
    }

    public static List<GameObject> FindAllInChildren(Transform parent, string name)
    {
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Expected O, but got Unknown
        List<GameObject> list = new List<GameObject>();
        foreach (Transform item in parent)
        {
            Transform val = item;
            if (((Object)val).name == name)
            {
                list.Add(((Component)val).gameObject);
            }
            list.AddRange(FindAllInChildren(val, name));
        }
        return list;
    }

    public static List<TextMeshProUGUI> FindTextMeshProInChildren(Transform parent)
    {
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Expected O, but got Unknown
        List<TextMeshProUGUI> list = new List<TextMeshProUGUI>();
        foreach (Transform item in parent)
        {
            Transform val = item;
            TextMeshProUGUI component = ((Component)val).GetComponent<TextMeshProUGUI>();
            if ((Object)(object)component != (Object)null)
            {
                list.Add(component);
            }
            list.AddRange(FindTextMeshProInChildren(val));
        }
        return list;
    }

    private GameObject GetFirstParent(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        while ((Object)(object)parent != (Object)null && (Object)(object)parent.parent != (Object)null)
        {
            parent = parent.parent;
        }
        return ((Object)(object)parent != (Object)null) ? ((Component)parent).gameObject : null;
    }

    private GameObject GetParent(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        return ((Object)(object)parent != (Object)null) ? ((Component)parent).gameObject : null;
    }

    private GameObject GetGrandParent(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        Transform val = ((parent != null) ? parent.parent : null);
        return ((Object)(object)val != (Object)null) ? ((Component)val).gameObject : null;
    }

    private GameObject GetSpecParent(GameObject obj, string parentName)
    {
        Transform parent = obj.transform.parent;
        while ((Object)(object)parent != (Object)null)
        {
            if (((Object)parent).name == parentName)
            {
                return ((Component)parent).gameObject;
            }
            parent = parent.parent;
        }
        return null;
    }

    private bool HasSpecParent(GameObject obj, string parentName)
    {
        Transform parent = obj.transform.parent;
        while ((Object)(object)parent != (Object)null)
        {
            if (((Object)parent).name == parentName)
            {
                return true;
            }
            parent = parent.parent;
        }
        return false;
    }

    private bool HasAnyParent(GameObject obj, string[] parentNames)
    {
        Transform currentTransform = obj.transform.parent;
        while ((Object)(object)currentTransform != (Object)null)
        {
            if (Array.Exists(parentNames, (string name) => name == ((Object)currentTransform).name))
            {
                return true;
            }
            currentTransform = currentTransform.parent;
        }
        return false;
    }

    private void UpdateMaterialGlass(Material[] mats, string materialName, string textureKey)
    {
        //IL_00fb: Unknown result type (might be due to invalid IL or missing references)
        if (mats == null)
        {
            return;
        }
        Color val = default(Color);
        foreach (KeyValuePair<string, string> item in filePaths_tex)
        {
            if (!(item.Key == textureKey))
            {
                continue;
            }
            string[] array = File.ReadAllLines(item.Value + item.Key);
            if (array.Length != 2 || !ColorUtility.TryParseHtmlString("#" + array[0], ref val) || !float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                continue;
            }
            val.a = Mathf.Clamp01(result);
            foreach (Material val2 in mats)
            {
                if ((Object)(object)val2 != (Object)null && (((Object)val2).name == materialName || ((Object)val2).name == materialName + " (Instance)"))
                {
                    val2.SetColor("_Color", val);
                }
            }
        }
    }

    private void UpdateMaterial(string objname, Renderer renderer, Material[] mats, string materialName, string textureKey)
    {
        //IL_00c9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d0: Expected O, but got Unknown
        //IL_0240: Unknown result type (might be due to invalid IL or missing references)
        //IL_024d: Unknown result type (might be due to invalid IL or missing references)
        if (mats == null)
        {
            return;
        }
        bool flag = false;
        for (int i = 0; i < mats.Length; i++)
        {
            Material val = mats[i];
            if (!((Object)(object)val != (Object)null) || (!(((Object)val).name == materialName + " (Instance)") && !(((Object)val).name == materialName)))
            {
                continue;
            }
            if (objname == "Weapons closet")
            {
                if (i == 0 && ((Object)val).name == "Weapons closet (Instance)")
                {
                    textureKey = "CardTable_bottom";
                }
                else if (i == 2 && ((Object)val).name == "Weapons closet (Instance)")
                {
                    textureKey = "CardTable_top";
                }
            }
            Shader val2 = Shader.Find("Standard");
            Material val3 = new Material(val2);
            ((Object)val3).name = materialName;
            Texture2D val4 = null;
            string text = "";
            switch (textureKey)
            {
                default:
                    if (!(textureKey == "PlayTable_wood"))
                    {
                        val4 = GetCachedTexture(textureKey);
                        break;
                    }
                    goto case "PlayTable_chair";
                case "PlayTable_chair":
                case "PlayTable_chair_metal":
                case "PlayTable_metal":
                    if (playtablename == "")
                    {
                        val4 = GetRandomTexture(textureKey);
                        text = playtablename;
                        break;
                    }
                    val4 = GetCachedTexture(playtablename.Replace("PlayTable_chair", textureKey).Replace("PlayTable_chair_metal", textureKey).Replace("PlayTable_metal", textureKey)
                        .Replace("PlayTable_wood", textureKey));
                    if ((Object)(object)val4 == (Object)null)
                    {
                        val4 = GetRandomTexture(textureKey);
                    }
                    else
                    {
                        text = playtablename.Replace("PlayTable_chair", textureKey).Replace("PlayTable_chair_metal", textureKey).Replace("PlayTable_metal", textureKey)
                            .Replace("PlayTable_wood", textureKey);
                    }
                    break;
            }
            if (!((Object)(object)val4 != (Object)null))
            {
                continue;
            }
            try
            {
                val3.mainTexture = (Texture)(object)val4;
                ((Object)val3.mainTexture).name = textureKey;
                val3.SetVector("_Color", new Vector4(1f, 1f, 1f, 1f));
                val3.color = Color.white;
                mats[i] = val3;
                Texture2D val5 = null;
                if (text != "")
                {
                    val5 = GetCachedTexture(text + "_m");
                    if ((Object)(object)val5 == (Object)null)
                    {
                        val5 = GetCachedTexture(textureKey + "_m");
                    }
                }
                else
                {
                    val5 = GetCachedTexture(textureKey + "_m");
                }
                if ((Object)(object)val5 != (Object)null && mats[i].HasProperty("_MetallicGlossMap"))
                {
                    mats[i].EnableKeyword("_METALLICGLOSSMAP");
                    mats[i].SetTexture("_MetallicGlossMap", (Texture)(object)val5);
                }
                Texture2D val6 = null;
                if (text != "")
                {
                    val6 = GetCachedTexture(text + "_n");
                    if ((Object)(object)val6 == (Object)null)
                    {
                        val6 = GetCachedTexture(textureKey + "_n");
                    }
                }
                else
                {
                    val6 = GetCachedTexture(textureKey + "_n");
                }
                if ((Object)(object)val6 != (Object)null && mats[i].HasProperty("_BumpMap"))
                {
                    mats[i].EnableKeyword("_NORMALMAP");
                    mats[i].SetTexture("_BumpMap", (Texture)(object)val6);
                    mats[i].SetInt("_UseNormalMap", 1);
                    mats[i].SetFloat("_BumpScale", 1f);
                }
                Texture2D val7 = null;
                if (text != "")
                {
                    val7 = GetCachedTexture(text + "_o");
                    if ((Object)(object)val7 == (Object)null)
                    {
                        val7 = GetCachedTexture(textureKey + "_o");
                    }
                }
                else
                {
                    val7 = GetCachedTexture(textureKey + "_o");
                }
                if ((Object)(object)val7 != (Object)null && mats[i].HasProperty("_OcclusionMap"))
                {
                    mats[i].SetTexture("_OcclusionMap", (Texture)(object)val7);
                }
                flag = true;
            }
            catch
            {
            }
        }
        if (flag)
        {
            renderer.materials = mats;
        }
    }

    private void UpdateMaterialBuildings(string objname, Renderer renderer, Material[] mats, string materialName, string textureKey)
    {
        //IL_00c0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c7: Expected O, but got Unknown
        //IL_00fe: Unknown result type (might be due to invalid IL or missing references)
        if (mats == null)
        {
            return;
        }
        bool flag = false;
        for (int i = 0; i < mats.Length; i++)
        {
            Material val = mats[i];
            if (!((Object)(object)val != (Object)null) || (!(((Object)val).name == materialName + " (Instance)") && !(((Object)val).name == materialName)))
            {
                continue;
            }
            if (objname == "Weapons closet")
            {
                if (i == 0 && ((Object)val).name == "Weapons closet (Instance)")
                {
                    textureKey = "CardTable_bottom";
                }
                else if (i == 2 && ((Object)val).name == "Weapons closet (Instance)")
                {
                    textureKey = "CardTable_top";
                }
            }
            Material val2 = new Material(Shader.Find("Standard"));
            Texture2D cachedTexture = GetCachedTexture(textureKey);
            if (!((Object)(object)cachedTexture != (Object)null))
            {
                continue;
            }
            try
            {
                ((Object)val2).name = ((Object)val).name;
                val2.mainTexture = (Texture)(object)cachedTexture;
                val2.color = Color.white;
                mats[i] = val2;
                if (materialName == "Glass_01" || materialName.StartsWith("Ceiling"))
                {
                    val2.SetFloat("_Mode", 2f);
                    val2.SetOverrideTag("RenderType", "Transparent");
                    val2.SetInt("_SrcBlend", 5);
                    val2.SetInt("_DstBlend", 10);
                    val2.SetInt("_ZWrite", 1);
                    val2.DisableKeyword("_ALPHATEST_ON");
                    val2.EnableKeyword("_ALPHABLEND_ON");
                    val2.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    val2.renderQueue = 3000;
                }
                Texture2D cachedTexture2 = GetCachedTexture(textureKey + "_m");
                if ((Object)(object)cachedTexture2 != (Object)null && val2.HasProperty("_MetallicGlossMap"))
                {
                    val2.EnableKeyword("_METALLICGLOSSMAP");
                    val2.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture2);
                }
                Texture2D cachedTexture3 = GetCachedTexture(textureKey + "_n");
                if ((Object)(object)cachedTexture3 != (Object)null && val2.HasProperty("_BumpMap"))
                {
                    val2.EnableKeyword("_NORMALMAP");
                    val2.SetTexture("_BumpMap", (Texture)(object)cachedTexture3);
                    val2.SetInt("_UseNormalMap", 1);
                }
                Texture2D cachedTexture4 = GetCachedTexture(textureKey + "_o");
                if ((Object)(object)cachedTexture4 != (Object)null && val2.HasProperty("_OcclusionMap"))
                {
                    val2.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture4);
                }
                flag = true;
            }
            catch
            {
            }
        }
        if (flag)
        {
            renderer.materials = mats;
        }
    }

    private void UpdateMaterialOnce(string objname, Renderer renderer, Material mat, string materialName, string textureKey)
    {
        //IL_0071: Unknown result type (might be due to invalid IL or missing references)
        //IL_0076: Unknown result type (might be due to invalid IL or missing references)
        //IL_0083: Unknown result type (might be due to invalid IL or missing references)
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_008c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0099: Expected O, but got Unknown
        //IL_00af: Unknown result type (might be due to invalid IL or missing references)
        if ((Object)(object)renderer == (Object)null || (Object)(object)mat == (Object)null || (!(((Object)mat).name == materialName) && !(((Object)mat).name == materialName + " (Instance)")))
        {
            return;
        }
        Texture2D cachedTexture = GetCachedTexture(textureKey);
        if (!((Object)(object)cachedTexture != (Object)null))
        {
            return;
        }
        try
        {
            Material val = new Material(Shader.Find("Standard"))
            {
                name = ((Object)mat).name,
                mainTexture = (Texture)(object)cachedTexture,
                color = Color.white
            };
            ((Object)val.mainTexture).name = textureKey;
            val.SetColor("_Color", Color.white);
            Texture2D val2 = GetCachedTexture(textureKey + "_m") ?? GetCachedTexture(textureKey + " M");
            if ((Object)(object)val2 != (Object)null && val.HasProperty("_MetallicGlossMap"))
            {
                val.EnableKeyword("_METALLICGLOSSMAP");
                val.SetTexture("_MetallicGlossMap", (Texture)(object)val2);
            }
            Texture2D val3 = GetCachedTexture(textureKey + "_n") ?? GetCachedTexture(textureKey + " N");
            if ((Object)(object)val3 != (Object)null && val.HasProperty("_BumpMap"))
            {
                val.EnableKeyword("_NORMALMAP");
                val.SetTexture("_BumpMap", (Texture)(object)val3);
                val.SetInt("_UseNormalMap", 1);
            }
            Texture2D cachedTexture2 = GetCachedTexture(textureKey + "_o");
            if ((Object)(object)cachedTexture2 != (Object)null && val.HasProperty("_OcclusionMap"))
            {
                val.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture2);
            }
            if (materialName == "Glass_01" || materialName.StartsWith("Ceiling") || materialName == "House_07_Windows" || materialName == "House_01_Wall_02")
            {
                val.SetFloat("_Mode", 2f);
                val.SetOverrideTag("RenderType", "Transparent");
                val.SetInt("_SrcBlend", 5);
                val.SetInt("_DstBlend", 10);
                val.SetInt("_ZWrite", 1);
                val.DisableKeyword("_ALPHATEST_ON");
                val.EnableKeyword("_ALPHABLEND_ON");
                val.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                val.renderQueue = 3000;
            }
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (((Object)materials[i]).name == materialName || ((Object)materials[i]).name == materialName + " (Instance)")
                {
                    materials[i] = val;
                    renderer.materials = materials;
                    break;
                }
            }
        }
        catch
        {
        }
    }

    private void ReplaceSpriteLists()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_03db: Unknown result type (might be due to invalid IL or missing references)
        //IL_03e0: Unknown result type (might be due to invalid IL or missing references)
        Scene activeScene = SceneManager.GetActiveScene();
        if (!(((Scene)(ref activeScene)).name == "Start"))
        {
            return;
        }
        List<List<Sprite>> list = new List<List<Sprite>>
        {
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBorderList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBGList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardFrontImageList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardBackImageList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CardFoilMaskImageList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_TetramonImageList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_GradedCardScratchTextureList
        };
        foreach (List<Sprite> item in list)
        {
            ReplaceSpritesInList(item);
        }
        List<List<MonsterData>> list2 = new List<List<MonsterData>>
        {
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_DataList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_MegabotDataList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_FantasyRPGDataList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_CatJobDataList,
            CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_SpecialCardImageList
        };
        foreach (List<MonsterData> item2 in list2)
        {
            ReplaceMonsterDataInList(item2);
        }
        List<List<CollectionPackImageSprite>> list3 = new List<List<CollectionPackImageSprite>> { CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_CollectionPackImageSpriteList };
        foreach (List<CollectionPackImageSprite> item3 in list3)
        {
            ReplaceStockDataInList(item3);
        }
        List<List<ItemData>> list4 = new List<List<ItemData>> { CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemDataList };
        foreach (List<ItemData> item4 in list4)
        {
            ReplaceItemDataInList(item4);
        }
        List<List<ItemMeshData>> list5 = new List<List<ItemMeshData>> { CSingleton<InventoryBase>.Instance.m_StockItemData_SO.m_ItemMeshDataList };
        foreach (List<ItemMeshData> item5 in list5)
        {
            ReplaceItemDataMeshInList(item5);
        }
        List<List<EDecoObject>> list6 = new List<List<EDecoObject>> { CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_OtherDecoList };
        foreach (List<EDecoObject> item6 in list6)
        {
            ReplaceDecoDataInList(item6);
        }
        List<List<EDecoObject>> list7 = new List<List<EDecoObject>> { CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_PosterDecoList };
        foreach (List<EDecoObject> item7 in list7)
        {
            ReplaceDecoDataInList(item7);
        }
        ReplaceReStockImagesInList(CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_FurniturePurchaseDataList);
        ReplaceFloorImagesInList(CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_FloorDecoDataList, "floor");
        ReplaceFloorImagesInList(CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_CeilingDecoDataList, "ceiling");
        ReplaceFloorImagesInList(CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_WallDecoDataList, "wall");
        ReplaceFloorImagesInList(CSingleton<InventoryBase>.Instance.m_ObjectData_SO.m_WallBarDecoDataList, "wallbar");
        activeScene = SceneManager.GetActiveScene();
        if (((Scene)(ref activeScene)).name == "Start")
        {
            List<UI_PriceTag> list8 = new List<UI_PriceTag>
            {
                CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagCardPrefab,
                CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagItemBoxPrefab,
                CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagPackageBoxPrefab,
                CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagPrefab,
                CSingleton<PriceTagUISpawner>.Instance.m_UIPriceTagWarehouseRackPrefab
            };
            foreach (UI_PriceTag item8 in list8)
            {
                ReplaceSpritesInPriceList(item8);
            }
            ((MonoBehaviour)this).StartCoroutine(ReplacePriceTagBackWithDelay());
        }
        ReplaceCardExpansionNameList(CSingleton<InventoryBase>.Instance.m_TextSO.m_CardExpansionNameList);
    }

    private IEnumerator ReplacePriceTagBackWithDelay()
    {
        while (CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList.Count == 0)
        {
            yield return (object)new WaitForSeconds(1f);
        }
        Color color = default(Color);
        foreach (KeyValuePair<string, string> filePath in filePaths_tex)
        {
            if (!(filePath.Key == "PriceTag_back_color.txt"))
            {
                continue;
            }
            string[] lines = File.ReadAllLines(filePath.Value + filePath.Key);
            if (lines.Length != 0 && ColorUtility.TryParseHtmlString("#" + lines[0], ref color))
            {
                if (CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList.Count > 4)
                {
                    CSingleton<LightManager>.Instance.m_ItemMatOriginalColorList[4] = color;
                }
                if (CSingleton<LightManager>.Instance.m_ItemMatList.Count > 4 && (Object)(object)CSingleton<LightManager>.Instance.m_ItemMatList[4] != (Object)null)
                {
                    CSingleton<LightManager>.Instance.m_ItemMatList[4].color = color;
                }
            }
            break;
        }
    }

    private void ReplaceSpritesInList(List<Sprite> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            Sprite val = spriteList[i];
            if ((Object)(object)val != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)val).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val2 = TextureToSprite(cachedTexture);
                    ((Object)val2).name = ((Object)val).name;
                    spriteList[i] = val2;
                }
                else if (((Object)val).name == "CardFoilMask" && (ShowFull.Value || ShowFullFrame.Value) && (Object)(object)CardFoilMaskC != (Object)null)
                {
                    Sprite val3 = TextureToSprite(CardFoilMaskC);
                    ((Object)val3).name = ((Object)val).name;
                    spriteList[i] = val3;
                }
            }
        }
    }

    private void ReplaceMonsterDataInList(List<MonsterData> spriteList)
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        for (int i = 0; i < spriteList.Count; i++)
        {
            Sprite icon = spriteList[i].Icon;
            Sprite ghostIcon = spriteList[i].GhostIcon;
            string name = spriteList[i].Name;
            EElementIndex elementIndex = spriteList[i].ElementIndex;
            if (dictionary.ContainsKey(name))
            {
                dictionary[name]++;
            }
            else
            {
                dictionary[name] = 1;
            }
            string text = name;
            if (dictionary[name] > 1)
            {
                text = name + dictionary[name];
            }
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(elementIndex.ToString()))
            {
                string value2;
                if (cachedData.TryGetValue(text + "_" + elementIndex.ToString() + "_NAME.txt", out var value))
                {
                    spriteList[i].Name = value;
                }
                else if (cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_NAME.txt", out value2))
                {
                    spriteList[i].Name = value2;
                }
                string value4;
                if (cachedData.TryGetValue(text + "_" + elementIndex.ToString() + "_DESC.txt", out var value3))
                {
                    spriteList[i].Description = value3;
                }
                else if (cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_DESC.txt", out value4))
                {
                    spriteList[i].Description = value4;
                }
                string value6;
                if (cachedData.TryGetValue(text + "_" + elementIndex.ToString() + "_ARTIST.txt", out var value5))
                {
                    spriteList[i].ArtistName = value5;
                }
                else if (cachedData.TryGetValue(name + "_" + elementIndex.ToString() + "_ARTIST.txt", out value6))
                {
                    spriteList[i].ArtistName = value6;
                }
            }
            if ((Object)(object)icon != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)icon).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)icon).name;
                    spriteList[i].Icon = val;
                }
            }
            if ((Object)(object)ghostIcon != (Object)null)
            {
                Texture2D cachedTexture2 = GetCachedTexture(((Object)ghostIcon).name);
                if ((Object)(object)cachedTexture2 != (Object)null)
                {
                    Sprite val2 = TextureToSprite(cachedTexture2);
                    ((Object)val2).name = ((Object)ghostIcon).name;
                    spriteList[i].GhostIcon = val2;
                }
            }
        }
    }

    private void ReplaceStockDataInList(List<CollectionPackImageSprite> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            CollectionPackImageSprite collectionPackImageSprite = spriteList[i];
            Sprite fullSprite = collectionPackImageSprite.fullSprite;
            Sprite bottomSprite = collectionPackImageSprite.bottomSprite;
            Sprite topSprite = collectionPackImageSprite.topSprite;
            if ((Object)(object)fullSprite != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)fullSprite).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)fullSprite).name;
                    collectionPackImageSprite.fullSprite = val;
                }
            }
            if ((Object)(object)bottomSprite != (Object)null)
            {
                Texture2D cachedTexture2 = GetCachedTexture(((Object)bottomSprite).name);
                if ((Object)(object)cachedTexture2 != (Object)null)
                {
                    Sprite val2 = TextureToSprite(cachedTexture2);
                    ((Object)val2).name = ((Object)bottomSprite).name;
                    collectionPackImageSprite.bottomSprite = val2;
                }
            }
            if ((Object)(object)topSprite != (Object)null)
            {
                Texture2D cachedTexture3 = GetCachedTexture(((Object)topSprite).name);
                if ((Object)(object)cachedTexture3 != (Object)null)
                {
                    Sprite val3 = TextureToSprite(cachedTexture3);
                    ((Object)val3).name = ((Object)topSprite).name;
                    collectionPackImageSprite.topSprite = val3;
                }
            }
        }
    }

    private void ReplaceItemDataInList(List<ItemData> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            Sprite icon = spriteList[i].icon;
            if (!((Object)(object)icon != (Object)null))
            {
                continue;
            }
            if (spriteList[i].name != "")
            {
                string text = SanitizeFileName(spriteList[i].name);
                string text2 = ((Object)spriteList[i].icon).name.Replace("Icon_Playmat", "");
                string text3 = null;
                string text4 = null;
                if (text2 != null && text != null && text.StartsWith("Playmat"))
                {
                    text3 = text.Replace("Playmat ", "Playmat_" + text2 + "_") + "_NAME.txt";
                }
                string text5 = text + "_NAME.txt";
                if (text3 != null)
                {
                    text4 = (from f in Directory.EnumerateFiles(path_nam, text3, SearchOption.AllDirectories)
                        where !f.Contains(Path.Combine(path_nam, "cards"))
                        select f).FirstOrDefault();
                }
                if (text4 != null)
                {
                    try
                    {
                        string[] array = File.ReadAllLines(text4);
                        spriteList[i].name = array[0];
                    }
                    catch
                    {
                    }
                }
                else if (text5 != null)
                {
                    string text6 = (from f in Directory.EnumerateFiles(path_nam, text5, SearchOption.AllDirectories)
                        where !f.Contains(Path.Combine(path_nam, "cards"))
                        select f).FirstOrDefault();
                    if (text6 != null)
                    {
                        try
                        {
                            string[] array2 = File.ReadAllLines(text6);
                            spriteList[i].name = array2[0];
                        }
                        catch
                        {
                        }
                    }
                }
            }
            Texture2D cachedTexture = GetCachedTexture(((Object)icon).name);
            if ((Object)(object)cachedTexture != (Object)null)
            {
                Sprite val = TextureToSprite(cachedTexture);
                ((Object)val).name = ((Object)icon).name;
                spriteList[i].icon = val;
            }
        }
    }

    private void ReplaceDecoDataInList(List<EDecoObject> dataList)
    {
        Dictionary<string, int> dictionary = new Dictionary<string, int>
        {
            { "Statue", 1 },
            { "Plant", 1 },
            { "Sign", 1 },
            { "Poster", 1 }
        };
        for (int i = 0; i < dataList.Count; i++)
        {
            DecoPurchaseData itemDecoPurchaseData = InventoryBase.GetItemDecoPurchaseData(dataList[i]);
            if (itemDecoPurchaseData != null)
            {
                string key = "Other";
                if (itemDecoPurchaseData.name.Contains("Statue"))
                {
                    key = "Statue";
                }
                else if (itemDecoPurchaseData.name.Contains("Plant"))
                {
                    key = "Plant";
                }
                else if (itemDecoPurchaseData.name.Contains("Sign"))
                {
                    key = "Sign";
                }
                else if (itemDecoPurchaseData.name.Contains("Poster"))
                {
                    key = "Poster";
                }
                int num = ((!dictionary.ContainsKey(key)) ? 1 : dictionary[key]);
                string key2 = $"{num}_{itemDecoPurchaseData.name}_NAME.txt";
                if (cachedData.TryGetValue(key2, out var value))
                {
                    itemDecoPurchaseData.mainNameText = value;
                }
                Texture2D cachedTexture = GetCachedTexture(((Object)itemDecoPurchaseData.icon).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)itemDecoPurchaseData.icon).name;
                    itemDecoPurchaseData.icon = val;
                }
                if (dictionary.ContainsKey(key))
                {
                    dictionary[key]++;
                }
            }
        }
    }

    private string SanitizeFileName(string fileName)
    {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidFileNameChars)
        {
            fileName = fileName.Replace(c.ToString(), "_");
        }
        return fileName;
    }

    private void ReplaceItemDataMeshInList(List<ItemMeshData> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            ReplaceMesh(spriteList[i], isPrimary: true);
            ReplaceMesh(spriteList[i], isPrimary: false);
        }
    }

    private void ReplaceMesh(ItemMeshData itemData, bool isPrimary)
    {
        //IL_00af: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b6: Expected O, but got Unknown
        //IL_00d1: Unknown result type (might be due to invalid IL or missing references)
        Mesh val = (isPrimary ? itemData.mesh : itemData.meshSecondary);
        if ((Object)(object)val == (Object)null || (Object)(object)itemData.material == (Object)null)
        {
            return;
        }
        string name = ((Object)itemData.material.mainTexture).name;
        Texture2D cachedTexture = GetCachedTexture(name + "_m");
        Texture2D cachedTexture2 = GetCachedTexture(name + "_n");
        Texture2D cachedTexture3 = GetCachedTexture(name + "_o");
        if ((Object)(object)cachedTexture != (Object)null || (Object)(object)cachedTexture2 != (Object)null || (Object)(object)cachedTexture3 != (Object)null)
        {
            Material val2 = new Material(Shader.Find("Standard"));
            val2.mainTexture = itemData.material.mainTexture;
            val2.color = itemData.material.color;
            if (figures_textures.Contains(name))
            {
                val2.SetFloat("_Mode", 1f);
                val2.SetOverrideTag("RenderType", "TransparentCutout");
                val2.EnableKeyword("_ALPHATEST_ON");
                val2.DisableKeyword("_ALPHABLEND_ON");
                val2.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                val2.SetFloat("_Cutoff", 0.5f);
                val2.SetInt("_ZWrite", 1);
                val2.renderQueue = 2450;
                val2.SetInt("_Cull", 0);
                val2.SetInt("_SrcBlend", 1);
                val2.SetInt("_DstBlend", 0);
            }
            itemData.material = val2;
            if ((Object)(object)cachedTexture != (Object)null && itemData.material.HasProperty("_MetallicGlossMap"))
            {
                itemData.material.EnableKeyword("_METALLICGLOSSMAP");
                itemData.material.SetTexture("_MetallicGlossMap", (Texture)(object)cachedTexture);
            }
            if ((Object)(object)cachedTexture2 != (Object)null && itemData.material.HasProperty("_BumpMap"))
            {
                itemData.material.EnableKeyword("_NORMALMAP");
                itemData.material.SetTexture("_BumpMap", (Texture)(object)cachedTexture2);
                itemData.material.SetInt("_UseNormalMap", 1);
            }
            if ((Object)(object)cachedTexture3 != (Object)null && itemData.material.HasProperty("_OcclusionMap"))
            {
                itemData.material.SetTexture("_OcclusionMap", (Texture)(object)cachedTexture3);
            }
        }
        Mesh val3 = null;
        if (name.StartsWith("T_Manga_"))
        {
            string name2 = name.Replace("T_Manga_", "Manga_Mesh");
            val3 = GetCachedMesh(name2) ?? GetCachedMesh(((Object)val).name);
        }
        else if (name == "T_D20Dice" && ((Object)val).name == "DiceBox")
        {
            string text = name + "1";
            string name3 = text.Replace("T_D20Dice", "DiceBox");
            val3 = GetCachedMesh(name3) ?? GetCachedMesh(((Object)val).name);
        }
        else if (name == "T_D20Dice" && ((Object)val).name == "DiceBoxGlass")
        {
            string text2 = name + "1";
            string name4 = text2.Replace("T_D20Dice", "DiceBoxGlass");
            val3 = GetCachedMesh(name4) ?? GetCachedMesh(((Object)val).name);
        }
        else if (name.StartsWith("T_D20Dice") && ((Object)val).name == "DiceBox")
        {
            string text3 = name.Replace(" ", "");
            string name5 = text3.Replace("T_D20Dice", "DiceBox");
            val3 = GetCachedMesh(name5) ?? GetCachedMesh(((Object)val).name);
        }
        else if (name.StartsWith("T_D20Dice") && ((Object)val).name == "DiceBoxGlass")
        {
            string text4 = name.Replace(" ", "");
            string name6 = text4.Replace("T_D20Dice", "DiceBoxGlass");
            val3 = GetCachedMesh(name6) ?? GetCachedMesh(((Object)val).name);
        }
        else
        {
            val3 = GetCachedMesh(((Object)val).name);
        }
        if ((Object)(object)val3 != (Object)null)
        {
            if (isPrimary)
            {
                itemData.mesh = val3;
            }
            else
            {
                itemData.meshSecondary = val3;
            }
            val3.UploadMeshData(true);
        }
    }

    private void ReplaceReStockImagesInList(List<FurniturePurchaseData> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            Sprite icon = items[i].icon;
            if ((Object)(object)icon != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)icon).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)icon).name;
                    items[i].icon = val;
                }
            }
        }
    }

    private void ReplaceFloorImagesInList(List<ShopDecoData> items, string name = "")
    {
        //IL_01b6: Unknown result type (might be due to invalid IL or missing references)
        //IL_01bb: Unknown result type (might be due to invalid IL or missing references)
        for (int i = 0; i < items.Count; i++)
        {
            Sprite icon = items[i].icon;
            if ((Object)(object)icon != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(i + "_" + name + "_icon");
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)icon).name;
                    items[i].icon = val;
                }
            }
            Texture mainTexture = items[i].mainTexture;
            Texture roughnessMap = items[i].roughnessMap;
            Texture normalMap = items[i].normalMap;
            if ((Object)(object)mainTexture != (Object)null)
            {
                Texture2D cachedTexture2 = GetCachedTexture(i + "_" + name + "_mainTexture");
                if ((Object)(object)cachedTexture2 != (Object)null)
                {
                    ((Object)mainTexture).name = ((Object)mainTexture).name;
                    items[i].mainTexture = (Texture)(object)cachedTexture2;
                }
            }
            if ((Object)(object)roughnessMap != (Object)null)
            {
                Texture2D cachedTexture3 = GetCachedTexture(i + "_" + name + "_roughnessMap");
                if ((Object)(object)cachedTexture3 != (Object)null)
                {
                    ((Object)roughnessMap).name = ((Object)roughnessMap).name;
                    items[i].roughnessMap = (Texture)(object)cachedTexture3;
                }
            }
            if ((Object)(object)normalMap != (Object)null)
            {
                Texture2D cachedTexture4 = GetCachedTexture(i + "_" + name + "_normalMap");
                if ((Object)(object)cachedTexture4 != (Object)null)
                {
                    ((Object)normalMap).name = ((Object)normalMap).name;
                    items[i].normalMap = (Texture)(object)cachedTexture4;
                }
            }
            items[i].color = Color.white;
        }
    }

    private void ExportTexture(Texture2D texture, string fileName)
    {
        //IL_004b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0051: Expected O, but got Unknown
        //IL_006a: Unknown result type (might be due to invalid IL or missing references)
        if (!((Object)(object)texture == (Object)null))
        {
            RenderTexture temporary = RenderTexture.GetTemporary(((Texture)texture).width, ((Texture)texture).height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)2);
            Graphics.Blit((Texture)(object)texture, temporary);
            RenderTexture active = RenderTexture.active;
            RenderTexture.active = temporary;
            Texture2D val = new Texture2D(((Texture)texture).width, ((Texture)texture).height, (TextureFormat)4, false);
            val.ReadPixels(new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), 0, 0);
            val.Apply();
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(temporary);
            byte[] array = ImageConversion.EncodeToPNG(val);
            if (array != null)
            {
                string text = Path.Combine(path_tex + "/export/", fileName + ".png");
                File.WriteAllBytes(text, array);
                Debug.Log((object)("Exported texture to: " + text));
            }
            Object.Destroy((Object)(object)val);
        }
    }

    private void ReplaceCardExpansionNameList(List<string> items)
    {
        string text = Directory.EnumerateFiles(path_nam, "Expansions_Names.txt", SearchOption.AllDirectories).FirstOrDefault();
        if (text == null)
        {
            return;
        }
        try
        {
            string[] array = File.ReadAllLines(text);
            for (int i = 0; i < Math.Min(items.Count, array.Length); i++)
            {
                items[i] = array[i];
            }
            LanguageSourceData languageSourceData = LocalizationManager.Sources[0];
            Dictionary<string, int> dictionary = new Dictionary<string, int>
            {
                { "Tetramon Base", 0 },
                { "Tetramon Destiny", 1 },
                { "Tetramon Ghost", 2 },
                { "Tetramon", 3 },
                { "Ghost", 4 },
                { "Destiny", 5 },
                { "Megabot", 6 },
                { "FantasyRPG", 7 },
                { "CatJob", 8 }
            };
            int num = ((languageSourceData.mLanguages != null) ? languageSourceData.mLanguages.Count : 0);
            foreach (KeyValuePair<string, int> item in dictionary)
            {
                string key = item.Key;
                int value = item.Value;
                if (value < 0 || value >= array.Length)
                {
                    continue;
                }
                string text2 = array[value];
                if (languageSourceData.mDictionary.TryGetValue(key, out var value2) && value2 != null)
                {
                    EnsureLanguagesSize(value2, num);
                    for (int j = 0; j < value2.Languages.Length; j++)
                    {
                        value2.Languages[j] = text2;
                    }
                    continue;
                }
                TermData termData = new TermData();
                termData.Term = key;
                termData.Languages = new string[Math.Max(num, 1)];
                TermData termData2 = termData;
                for (int k = 0; k < termData2.Languages.Length; k++)
                {
                    termData2.Languages[k] = text2;
                }
                languageSourceData.mTerms.Add(termData2);
                languageSourceData.mDictionary[key] = termData2;
            }
        }
        catch
        {
        }
    }

    private static void EnsureLanguagesSize(TermData term, int langCount)
    {
        if (term.Languages == null)
        {
            term.Languages = new string[Math.Max(langCount, 1)];
        }
        else if (langCount > term.Languages.Length)
        {
            Array.Resize(ref term.Languages, langCount);
        }
    }

    private void ReplaceSpritesCardsInList(List<Sprite> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            Sprite val = spriteList[i];
            if ((Object)(object)val != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)val).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val2 = TextureToSprite(cachedTexture);
                    ((Object)val2).name = ((Object)val).name;
                    spriteList[i] = val2;
                }
            }
        }
    }

    private void ReplaceSpritesFoilsInList(List<Image> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            Image image = spriteList[i];
            if ((Object)(object)image != (Object)null && (Object)(object)image.sprite != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)image.sprite).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val = TextureToSprite(cachedTexture);
                    ((Object)val).name = ((Object)image.sprite).name;
                    image.sprite = val;
                }
            }
        }
    }

    private void ReplaceSpritesInPriceList(UI_PriceTag priceTag)
    {
        if (!((Object)(object)priceTag.m_UIGrp == (Object)null))
        {
            ApplyColorToChild("BG", "PriceTag_front_color.txt");
            ApplyColorToChild("IconBG", "PriceTag_backIcon_color.txt");
            ApplyColorToChild("PriceBG", "PriceTag_backPrice_color.txt");
            ApplyColorToChildText("PriceText", "PriceTag_text_color.txt");
            ApplyColorToChildText("AmountText", "PriceTag_textAmount_color.txt");
        }
        void ApplyColorToChild(string childName, string fileNameKey)
        {
            //IL_00a7: Unknown result type (might be due to invalid IL or missing references)
            Transform val = priceTag.m_UIGrp.Find(childName);
            if (!((Object)(object)val == (Object)null))
            {
                Image component = ((Component)val).GetComponent<Image>();
                Color color = default(Color);
                if (!((Object)(object)component == (Object)null))
                {
                    foreach (KeyValuePair<string, string> item in filePaths_tex)
                    {
                        if (!(item.Key != fileNameKey))
                        {
                            string[] array = File.ReadAllLines(item.Value + item.Key);
                            if (array.Length != 0 && ColorUtility.TryParseHtmlString("#" + array[0], ref color))
                            {
                                component.color = color;
                            }
                        }
                    }
                }
            }
        }
        void ApplyColorToChildText(string childName, string fileNameKey)
        {
            //IL_00af: Unknown result type (might be due to invalid IL or missing references)
            //IL_00de: Unknown result type (might be due to invalid IL or missing references)
            //IL_00e0: Unknown result type (might be due to invalid IL or missing references)
            Transform val = priceTag.m_UIGrp.Find(childName);
            if (!((Object)(object)val == (Object)null))
            {
                TextMeshProUGUI component = ((Component)val).GetComponent<TextMeshProUGUI>();
                Color color = default(Color);
                Color val2 = default(Color);
                if (!((Object)(object)component == (Object)null))
                {
                    foreach (KeyValuePair<string, string> item2 in filePaths_tex)
                    {
                        if (!(item2.Key != fileNameKey))
                        {
                            string[] array = File.ReadAllLines(item2.Value + item2.Key);
                            if (array.Length > 1 && ColorUtility.TryParseHtmlString("#" + array[0], ref color))
                            {
                                component.color = color;
                                if (array.Length > 1 && ColorUtility.TryParseHtmlString("#" + array[1], ref val2))
                                {
                                    component.outlineColor = Color32.op_Implicit(val2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private Vector4 HexToRgba(string hexColor)
    {
        //IL_00a0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a5: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        hexColor = hexColor.TrimStart(new char[1] { '#' });
        int num = int.Parse(hexColor.Substring(0, 2), NumberStyles.HexNumber);
        int num2 = int.Parse(hexColor.Substring(2, 2), NumberStyles.HexNumber);
        int num3 = int.Parse(hexColor.Substring(4, 2), NumberStyles.HexNumber);
        int num4 = ((hexColor.Length == 8) ? int.Parse(hexColor.Substring(6, 2), NumberStyles.HexNumber) : 255);
        float num5 = (float)num / 255f;
        float num6 = (float)num2 / 255f;
        float num7 = (float)num3 / 255f;
        float num8 = (float)num4 / 255f;
        return new Vector4(num5, num6, num7, num8);
    }

    public static Texture2D LoadPNG(string filePath)
    {
        //IL_0017: Unknown result type (might be due to invalid IL or missing references)
        //IL_001d: Expected O, but got Unknown
        Texture2D val = null;
        if (File.Exists(filePath))
        {
            byte[] array = File.ReadAllBytes(filePath);
            val = new Texture2D(2, 2);
            ImageConversion.LoadImage(val, array);
        }
        return val;
    }

    public static Texture2D LoadPNG_Bump(string filePath)
    {
        //IL_001b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0021: Expected O, but got Unknown
        Texture2D val = null;
        if (File.Exists(filePath))
        {
            byte[] array = File.ReadAllBytes(filePath);
            val = new Texture2D(2, 2, (TextureFormat)63, true, true);
            ImageConversion.LoadImage(val, array);
        }
        return val;
    }

    public static Texture2D LoadPNGHDR(string filePath, float gamma = 0.4f)
    {
        //IL_001c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0022: Expected O, but got Unknown
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        //IL_005f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0063: Unknown result type (might be due to invalid IL or missing references)
        //IL_0077: Unknown result type (might be due to invalid IL or missing references)
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a3: Unknown result type (might be due to invalid IL or missing references)
        Texture2D val = null;
        if (File.Exists(filePath))
        {
            byte[] array = File.ReadAllBytes(filePath);
            val = new Texture2D(2, 2, (TextureFormat)4, false);
            if (!ImageConversion.LoadImage(val, array))
            {
                Debug.LogError((object)"[TextureReplacer] Failed to load PNG image.");
                return null;
            }
            Color[] pixels = val.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color val2 = pixels[i];
                val2.r = ApplyGamma(val2.r, gamma);
                val2.g = ApplyGamma(val2.g, gamma);
                val2.b = ApplyGamma(val2.b, gamma);
                pixels[i] = val2;
            }
            val.SetPixels(pixels);
            val.Apply();
        }
        return val;
    }

    private static float ApplyGamma(float colorValue, float gamma)
    {
        if (gamma <= 0f)
        {
            gamma = 1f;
        }
        float num = Mathf.Pow(colorValue, 1f / gamma);
        return Mathf.Clamp01(num);
    }

    public static Sprite TextureToSprite(Texture2D texture)
    {
        //IL_0019: Unknown result type (might be due to invalid IL or missing references)
        //IL_0028: Unknown result type (might be due to invalid IL or missing references)
        return Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), new Vector2(0.5f, 0.5f), 50f, 0u, (SpriteMeshType)0);
    }

    private void ApplyCustomFont(GameObject cardFront)
    {
        //IL_009a: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c3: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c8: Unknown result type (might be due to invalid IL or missing references)
        if ((Object)(object)cardFront == (Object)null)
        {
            return;
        }
        TMP_FontAsset cachedFont = Instance.GetCachedFont("Card_Font");
        if ((Object)(object)cachedFont == (Object)null)
        {
            return;
        }
        TextMeshProUGUI[] componentsInChildren = cardFront.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (componentsInChildren == null)
        {
            return;
        }
        TextMeshProUGUI[] array = componentsInChildren;
        foreach (TextMeshProUGUI textMeshProUGUI in array)
        {
            if ((Object)(object)textMeshProUGUI == (Object)null || (Object)(object)textMeshProUGUI.font == (Object)null || string.IsNullOrEmpty(textMeshProUGUI.text))
            {
                continue;
            }
            try
            {
                textMeshProUGUI.font = cachedFont;
                _ = card_color;
                if (true)
                {
                    textMeshProUGUI.color = card_color;
                }
                _ = card_color_outline;
                if (true)
                {
                    textMeshProUGUI.outlineColor = Color32.op_Implicit(card_color_outline);
                }
                try
                {
                    if (card_outline >= 0f)
                    {
                        textMeshProUGUI.outlineWidth = card_outline;
                    }
                }
                catch
                {
                }
                if (((Object)((Component)textMeshProUGUI).gameObject).name == "NumberText")
                {
                    textMeshProUGUI.characterSpacing = -10f;
                }
            }
            catch
            {
            }
        }
    }

    private void FixControllerSprites()
    {
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_KeyboardBtnImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_LeftMouseClickImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_RightMouseClickImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_LeftMouseHoldImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_RightMouseHoldImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_MiddleMouseScrollImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_EnterImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_SpacebarImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_TabImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_ShiftImage);
        ReplaceSprite(ref CSingleton<CGameManager>.Instance.m_TextSO.m_QuestionMarkImage);
        ReplaceSpritesInListController(CSingleton<CGameManager>.Instance.m_TextSO.m_GamepadCtrlBtnSpriteList);
        ReplaceSpritesInListController(CSingleton<CGameManager>.Instance.m_TextSO.m_XBoxCtrlBtnSpriteList);
        ReplaceSpritesInListController(CSingleton<CGameManager>.Instance.m_TextSO.m_PSCtrlBtnSpriteList);
    }

    private void ReplaceSprite(ref Sprite sprite)
    {
        if ((Object)(object)sprite != (Object)null)
        {
            Texture2D cachedTexture = GetCachedTexture(((Object)sprite).name);
            if ((Object)(object)cachedTexture != (Object)null)
            {
                Sprite val = TextureToSprite(cachedTexture);
                ((Object)val).name = ((Object)sprite).name;
                sprite = val;
            }
        }
    }

    private void ReplaceSpritesInListController(List<Sprite> spriteList)
    {
        for (int i = 0; i < spriteList.Count; i++)
        {
            Sprite val = spriteList[i];
            if ((Object)(object)val != (Object)null)
            {
                Texture2D cachedTexture = GetCachedTexture(((Object)val).name);
                if ((Object)(object)cachedTexture != (Object)null)
                {
                    Sprite val2 = TextureToSprite(cachedTexture);
                    ((Object)val2).name = ((Object)val).name;
                    spriteList[i] = val2;
                }
            }
        }
    }
}
