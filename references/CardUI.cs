using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public GameObject m_NormalGrp;

    public GameObject m_FullArtGrp;

    public GameObject m_SpecialCardGrp;

    public CardUI m_FullArtCard;

    public CardUI m_GhostCard;

    public GameObject m_CardFront;

    public GameObject m_CardBack;

    public GameObject m_FoilGrp;

    public GameObject m_GradedCardCaseGrp;

    public bool m_Show2DGradedCase;

    public Transform m_GradedCardFrontScaling;

    public List<Image> m_FoilShowList;

    public List<Image> m_FoilBlendedShowList;

    public List<Image> m_FoilDarkenImageList;

    public GameObject m_ExtraFoil;

    public Image m_CardBackImage;

    public Image m_MonsterImage;

    public Image m_MonsterMaskImage;

    public Image m_FullArtBGImage;

    public Image m_FullArtBGImageMask;

    public Image m_CardBorderImage;

    public Image m_CardBGImage;

    public Image m_CardFoilMaskImage;

    public Image m_MonsterMask;

    public Image m_MonsterGlowMask;

    public Image m_RarityImage;

    public Image m_AncientArtifactImage;

    public Image m_SpecialCardImage;

    public Image m_SpecialCardGlowImage;

    public Image m_BrightnessControl;

    public Image m_GradedCardTextureImage;

    public TextMeshProUGUI m_FirstEditionText;

    public TextMeshProUGUI m_MonsterNameText;

    public TextMeshProUGUI m_NumberText;

    public TextMeshProUGUI m_FameText;

    public TextMeshProUGUI m_DescriptionText;

    public TextMeshProUGUI m_ChampionText;

    public TextMeshProUGUI m_RarityText;

    public TextMeshProUGUI m_Stat1Text;

    public TextMeshProUGUI m_Stat2Text;

    public TextMeshProUGUI m_Stat3Text;

    public TextMeshProUGUI m_Stat4Text;

    public TextMeshProUGUI m_ArtistText;

    public TextMeshProUGUI m_GradeNumberText;

    public TextMeshProUGUI m_GradeDescriptionText;

    public TextMeshProUGUI m_GradeNameText;

    public TextMeshProUGUI m_GradeExpansionRarityText;

    public TextMeshProUGUI m_GradeSerialText;

    public List<Sprite> m_CardBorderSpriteList;

    public List<Sprite> m_CardBGSpriteList;

    public List<Sprite> m_CardRaritySpriteList;

    public List<Sprite> m_CardElementBGSpriteList;

    private CardData m_CardData;

    private MonsterData m_MonsterData;

    private ECardBorderType m_CardBorderType;

    private bool m_IsNestedFullArt;

    private bool m_IsFoil;

    private bool m_IsDimensionCard;

    private bool m_IsChampionCard;

    public List<GameObject> m_ChampionCardEnableObjectList;

    private CardUISetting m_CardUISetting;

    private Vector3 m_ArtworkImageLocalPos;

    private Card3dUIGroup m_Card3dUIGroup;

    public List<GameObject> m_FarDistanceCullObjList;

    private List<bool> m_FarDistanceCullObjVisibilityList = new List<bool>();

    private bool m_IsFarDistanceCulled;

    public GameObject m_EvoGrp;

    public GameObject m_EvoBasicGrp;

    public Image m_EvoPreviousStageIcon;

    public TextMeshProUGUI m_EvoPreviousStageNameText;

    public void InitCard3dUIGroup(Card3dUIGroup card3dUIGroup)
    {
        m_Card3dUIGroup = card3dUIGroup;
    }

    public void SetAncientArtifactCardUI(EMonsterType ancientCardType)
    {
        m_NormalGrp.SetActive(false);
        m_FullArtGrp.SetActive(false);
        ((Component)m_AncientArtifactImage).gameObject.SetActive(true);
        m_AncientArtifactImage.sprite = InventoryBase.GetAncientArtifactSprite(ancientCardType);
    }

    public void ShowFoilList(bool isActive)
    {
        //IL_0003: Unknown result type (might be due to invalid IL or missing references)
        //IL_000d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0012: Unknown result type (might be due to invalid IL or missing references)
        //IL_0072: Unknown result type (might be due to invalid IL or missing references)
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a1: Unknown result type (might be due to invalid IL or missing references)
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        if (isActive)
        {
            Color color = Color.white * 0.95f;
            color.a = 1f;
            for (int i = 0; i < m_FoilDarkenImageList.Count; i++)
            {
                ((Graphic)m_FoilDarkenImageList[i]).color = color;
            }
            if (Object.op_Implicit((Object)(object)m_FullArtBGImage))
            {
                ((Graphic)m_FullArtBGImage).color = color;
            }
        }
        else
        {
            for (int j = 0; j < m_FoilDarkenImageList.Count; j++)
            {
                ((Graphic)m_FoilDarkenImageList[j]).color = Color.white;
            }
            if (Object.op_Implicit((Object)(object)m_FullArtBGImage))
            {
                ((Graphic)m_FullArtBGImage).color = Color.white;
            }
        }
        for (int k = 0; k < m_FoilShowList.Count; k++)
        {
            ((Behaviour)m_FoilShowList[k]).enabled = isActive;
        }
    }

    public void ShowFoilBlendedList(bool isActive)
    {
        for (int i = 0; i < m_FoilBlendedShowList.Count; i++)
        {
            ((Behaviour)m_FoilBlendedShowList[i]).enabled = isActive;
        }
    }

    public void SetFoilCullListVisibility(bool isActive)
    {
        if (!(!m_IsFarDistanceCulled && isActive) && (!m_IsFarDistanceCulled || isActive))
        {
            for (int i = 0; i < m_FoilShowList.Count; i++)
            {
                ((Component)m_FoilShowList[i]).gameObject.SetActive(isActive);
            }
            for (int j = 0; j < m_FoilBlendedShowList.Count; j++)
            {
                ((Component)m_FoilBlendedShowList[j]).gameObject.SetActive(isActive);
            }
        }
    }

    public void SetFoilMaterialList(List<Material> mat)
    {
        for (int i = 0; i < m_FoilShowList.Count; i++)
        {
            ((Graphic)m_FoilShowList[i]).material = mat[i];
        }
        if (Object.op_Implicit((Object)(object)m_FullArtCard))
        {
            m_FullArtCard.SetFoilMaterialList(mat);
        }
        if (Object.op_Implicit((Object)(object)m_GhostCard))
        {
            m_GhostCard.SetFoilMaterialList(mat);
        }
    }

    public void SetFoilBlendedMaterialList(List<Material> mat)
    {
        for (int i = 0; i < m_FoilBlendedShowList.Count; i++)
        {
            ((Graphic)m_FoilBlendedShowList[i]).material = mat[i];
        }
        if (Object.op_Implicit((Object)(object)m_FullArtCard))
        {
            m_FullArtCard.SetFoilBlendedMaterialList(mat);
        }
        if (Object.op_Implicit((Object)(object)m_GhostCard))
        {
            m_GhostCard.SetFoilBlendedMaterialList(mat);
        }
    }

     {
        //IL_00fe: Unknown result type (might be due to invalid IL or missing references)
        //IL_0113: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c8: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e8: Unknown result type (might be due to invalid IL or missing references)
        if (m_CardData.cardGrade <= 0)
        {
            isShow = false;
        }
        m_GradedCardCaseGrp.SetActive(isShow);
        if (isShow)
        {
            ((TMP_Text)m_GradeNumberText).text = m_CardData.cardGrade.ToString();
            ((TMP_Text)m_GradeDescriptionText).text = GameInstance.GetCardGradeString(m_CardData.cardGrade);
            ((TMP_Text)m_GradeNameText).text = ((TMP_Text)m_MonsterNameText).text;
            ((TMP_Text)m_GradeExpansionRarityText).text = LocalizationManager.GetTranslation(m_CardData.expansionType.ToString()) + " " + CPlayerData.GetFullCardTypeName(m_CardData);
            m_CardFront.transform.localPosition = ((Component)m_GradedCardFrontScaling).transform.localPosition;
            m_CardFront.transform.localScale = ((Component)m_GradedCardFrontScaling).transform.localScale;
        }
        else
        {
            m_CardFront.transform.localPosition = Vector3.zero;
            m_CardFront.transform.localScale = Vector3.one;
        }
    }

    public void SetGhostCardUI(MonsterData data, bool isBlackGhost)
    {
        ((TMP_Text)m_GhostCard.m_MonsterNameText).text = data.GetName();
        ((TMP_Text)m_GhostCard.m_Stat1Text).text = data.BaseStats.Strength.ToString();
        ((TMP_Text)m_GhostCard.m_Stat2Text).text = data.BaseStats.Vitality.ToString();
        ((TMP_Text)m_GhostCard.m_Stat3Text).text = data.BaseStats.Spirit.ToString();
        ((TMP_Text)m_GhostCard.m_Stat4Text).text = data.BaseStats.Magic.ToString();
        ((TMP_Text)m_GhostCard.m_FameText).text = CPlayerData.GetCardFameAmount(m_CardData).ToString();
        m_GhostCard.m_MonsterImage.sprite = data.GetIcon(ECardExpansionType.Ghost);
        m_GhostCard.m_MonsterMaskImage.sprite = m_GhostCard.m_MonsterImage.sprite;
        m_GhostCard.m_FoilGrp.SetActive(m_IsFoil);
        m_GhostCard.ShowFoilList(m_IsFoil);
        m_GhostCard.ShowFoilBlendedList(m_IsFoil);
        m_NormalGrp.SetActive(false);
        m_FullArtGrp.SetActive(false);
        m_SpecialCardGrp.SetActive(false);
        ((Component)m_GhostCard).gameObject.SetActive(true);
        ((Component)m_AncientArtifactImage).gameObject.SetActive(false);
        if (isBlackGhost)
        {
            m_GhostCard.m_NormalGrp.SetActive(false);
            m_GhostCard.m_FullArtGrp.SetActive(true);
        }
        else
        {
            m_GhostCard.m_NormalGrp.SetActive(true);
            m_GhostCard.m_FullArtGrp.SetActive(false);
        }
    }

    private void LoadStreamTextureCompleted(CEventPlayer_LoadStreamTextureCompleted evt)
    {
        if (evt.m_FileName == m_CardData.expansionType.ToString() + "_" + m_CardData.monsterType)
        {
            CEventManager.RemoveListener<CEventPlayer_LoadStreamTextureCompleted>(LoadStreamTextureCompleted);
            if (evt.m_IsSuccess)
            {
                m_MonsterImage.sprite = m_MonsterData.GetIcon(m_CardData.expansionType);
            }
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying || Application.isMobilePlatform)
        {
            CEventManager.RemoveListener<CEventPlayer_LoadStreamTextureCompleted>(LoadStreamTextureCompleted);
        }
    }

    public void SetCardUI(CardData cardData)
    {
        m_CardData = cardData;
        if (m_CardData.monsterType <= EMonsterType.EarlyPlayer)
        {
            m_SpecialCardImage.sprite = InventoryBase.GetSpecialCardImage(m_CardData.monsterType);
            m_SpecialCardGlowImage.sprite = m_SpecialCardImage.sprite;
            m_SpecialCardGrp.SetActive(true);
            m_NormalGrp.SetActive(false);
            m_FullArtGrp.SetActive(false);
            ((Component)m_GhostCard).gameObject.SetActive(false);
            ((Component)m_AncientArtifactImage).gameObject.SetActive(false);
            m_FoilGrp.SetActive(true);
            ShowFoilList(isActive: true);
            ShowFoilBlendedList(isActive: true);
            return;
        }
        m_MonsterData = InventoryBase.GetMonsterData(cardData.monsterType);
        m_CardUISetting = InventoryBase.GetCardUISetting(m_CardData.expansionType);
        m_MonsterImage.sprite = m_MonsterData.GetIcon(m_CardData.expansionType);
        if ((Object)(object)m_MonsterImage.sprite == (Object)null)
        {
            m_MonsterImage.sprite = CSingleton<LoadStreamTexture>.Instance.m_LoadingSprite;
            CEventManager.AddListener<CEventPlayer_LoadStreamTextureCompleted>(LoadStreamTextureCompleted);
        }
        m_CardBackImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBackSprite(m_CardData.expansionType);
        if (!m_IsNestedFullArt)
        {
            m_CardFoilMaskImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFoilMaskSprite(m_CardData.expansionType);
        }
        m_CardBorderType = m_CardData.GetCardBorderType();
        m_IsDimensionCard = m_CardData.isDestiny;
        m_IsFoil = m_CardData.isFoil;
        m_FoilGrp.SetActive(m_IsFoil);
        ShowFoilList(m_IsFoil);
        ShowFoilBlendedList(m_IsFoil);
        if (Object.op_Implicit((Object)(object)m_ExtraFoil))
        {
            if (m_CardBorderType == ECardBorderType.Base || m_CardBorderType == ECardBorderType.FirstEdition)
            {
                m_ExtraFoil.gameObject.SetActive(false);
            }
            else
            {
                m_ExtraFoil.gameObject.SetActive(true);
            }
        }
        m_IsChampionCard = m_CardData.isChampionCard;
        if (m_CardData.expansionType == ECardExpansionType.Destiny)
        {
            m_CardBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFrontSprite(EElementIndex.Destiny);
        }
        else if (m_IsChampionCard)
        {
            m_CardBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFrontSprite(EElementIndex.Champion);
            m_CardBorderType = ECardBorderType.FullArt;
        }
        else if (m_CardData.expansionType == ECardExpansionType.Ghost)
        {
            if (m_CardData.isDestiny)
            {
                m_CardBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFrontSprite(EElementIndex.GhostBlack);
            }
            else
            {
                m_CardBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFrontSprite(EElementIndex.GhostWhite);
            }
        }
        else
        {
            m_CardBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardFrontSprite(m_MonsterData.ElementIndex);
        }
        m_CardBorderImage.sprite = m_CardBorderSpriteList[(int)m_CardBorderType];
        m_RarityImage.sprite = m_CardRaritySpriteList[(int)m_MonsterData.Rarity];
        ((TMP_Text)m_MonsterNameText).text = m_MonsterData.GetName();
        int num = (int)((int)(m_MonsterData.MonsterType - 1) * CPlayerData.GetCardAmountPerMonsterType(m_CardData.expansionType) + m_CardBorderType);
        num++;
        if (m_IsFoil)
        {
            num += 6;
        }
        string text = "";
        text = ((num < 10) ? ("00" + num) : ((num >= 100) ? num.ToString() : ("0" + num)));
        ((TMP_Text)m_NumberText).text = text;
        if (m_IsChampionCard)
        {
            ((Component)m_FameText).gameObject.SetActive(false);
            ((Component)m_DescriptionText).gameObject.SetActive(false);
            ((Component)m_ArtistText).gameObject.SetActive(false);
            ((Behaviour)m_ChampionText).enabled = true;
        }
        else
        {
            ((TMP_Text)m_DescriptionText).text = m_MonsterData.GetDescription();
            ((Component)m_DescriptionText).gameObject.SetActive(true);
            ((TMP_Text)m_ArtistText).text = m_MonsterData.GetArtistName();
            ((Component)m_ArtistText).gameObject.SetActive(true);
            if (m_MonsterData.PreviousEvolution == EMonsterType.None)
            {
                m_EvoBasicGrp.SetActive(true);
                ((Component)m_EvoPreviousStageIcon).gameObject.SetActive(false);
                ((Component)m_EvoPreviousStageNameText).gameObject.SetActive(false);
            }
            else
            {
                m_EvoBasicGrp.SetActive(false);
                MonsterData monsterData = InventoryBase.GetMonsterData(m_MonsterData.PreviousEvolution);
                m_EvoPreviousStageIcon.sprite = monsterData.GetIcon(m_CardData.expansionType);
                ((TMP_Text)m_EvoPreviousStageNameText).text = monsterData.GetName();
                ((Component)m_EvoPreviousStageNameText).gameObject.SetActive(true);
                ((Component)m_EvoPreviousStageIcon).gameObject.SetActive(true);
            }
            ((Behaviour)m_ChampionText).enabled = false;
        }
        for (int i = 0; i < m_ChampionCardEnableObjectList.Count; i++)
        {
            m_ChampionCardEnableObjectList[i].SetActive(m_IsChampionCard);
        }
        ((TMP_Text)m_RarityText).text = m_MonsterData.GetRarityName();
        ((TMP_Text)m_Stat1Text).text = m_MonsterData.BaseStats.Strength.ToString();
        ((TMP_Text)m_Stat2Text).text = m_MonsterData.BaseStats.Vitality.ToString();
        ((TMP_Text)m_Stat3Text).text = m_MonsterData.BaseStats.Spirit.ToString();
        ((TMP_Text)m_Stat4Text).text = m_MonsterData.BaseStats.Magic.ToString();
        EvaluateCardUISetting();
        if (m_IsNestedFullArt)
        {
            return;
        }
        if (m_CardBorderType == ECardBorderType.Base || m_CardBorderType == ECardBorderType.FullArt)
        {
            ((Behaviour)m_FirstEditionText).enabled = false;
        }
        else
        {
            if (m_CardBorderType == ECardBorderType.FirstEdition)
            {
                ((TMP_Text)m_FirstEditionText).text = LocalizationManager.GetTranslation("1st Edition");
            }
            else if (m_CardBorderType == ECardBorderType.Silver)
            {
                ((TMP_Text)m_FirstEditionText).text = LocalizationManager.GetTranslation("Silver Edition");
            }
            else if (m_CardBorderType == ECardBorderType.Gold)
            {
                ((TMP_Text)m_FirstEditionText).text = LocalizationManager.GetTranslation("Gold Edition");
            }
            else if (m_CardBorderType == ECardBorderType.EX)
            {
                ((TMP_Text)m_FirstEditionText).text = "EX";
            }
            ((Behaviour)m_FirstEditionText).enabled = true;
        }
        if (m_CardData.expansionType == ECardExpansionType.Ghost && m_CardBorderType == ECardBorderType.FullArt)
        {
            SetGhostCardUI(m_MonsterData, m_CardData.isDestiny);
        }
        else if (m_CardBorderType == ECardBorderType.FullArt && Object.op_Implicit((Object)(object)m_FullArtCard))
        {
            m_FullArtCard.MarkAsNestedFullArt(isNested: true);
            m_FullArtCard.SetCardUI(m_CardData);
            m_FullArtCard.m_MonsterMaskImage.sprite = m_FullArtCard.m_MonsterImage.sprite;
            if (m_CardData.expansionType == ECardExpansionType.Destiny)
            {
                m_FullArtBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBGSprite(EElementIndex.Destiny);
            }
            else if (m_IsChampionCard)
            {
                m_FullArtBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBGSprite(EElementIndex.Champion);
            }
            else if (m_CardData.expansionType == ECardExpansionType.Ghost)
            {
                if (m_CardData.isDestiny)
                {
                    m_FullArtBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBGSprite(EElementIndex.GhostBlack);
                }
                else
                {
                    m_FullArtBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBGSprite(EElementIndex.GhostWhite);
                }
            }
            else
            {
                m_FullArtBGImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetCardBGSprite(m_MonsterData.ElementIndex);
            }
            m_FullArtCard.m_FoilGrp.SetActive(m_IsFoil);
            m_FullArtCard.ShowFoilList(m_IsFoil);
            m_FullArtCard.ShowFoilBlendedList(m_IsFoil);
            ((TMP_Text)m_FullArtCard.m_FameText).text = ((TMP_Text)m_FameText).text;
            ((TMP_Text)m_FullArtCard.m_DescriptionText).text = ((TMP_Text)m_DescriptionText).text;
            ((TMP_Text)m_FullArtCard.m_ArtistText).text = ((TMP_Text)m_ArtistText).text;
            ((Component)m_FullArtCard.m_ArtistText).gameObject.SetActive(((Component)m_ArtistText).gameObject.activeSelf);
            if (m_MonsterData.PreviousEvolution == EMonsterType.None)
            {
                m_FullArtCard.m_EvoBasicGrp.SetActive(true);
                ((Component)m_FullArtCard.m_EvoPreviousStageNameText).gameObject.SetActive(false);
                ((Component)m_FullArtCard.m_EvoPreviousStageIcon).gameObject.SetActive(false);
            }
            else
            {
                m_FullArtCard.m_EvoBasicGrp.SetActive(false);
                MonsterData monsterData2 = InventoryBase.GetMonsterData(m_MonsterData.PreviousEvolution);
                m_FullArtCard.m_EvoPreviousStageIcon.sprite = monsterData2.GetIcon(m_CardData.expansionType);
                ((TMP_Text)m_FullArtCard.m_EvoPreviousStageNameText).text = monsterData2.GetName();
                ((Component)m_FullArtCard.m_EvoPreviousStageNameText).gameObject.SetActive(true);
                ((Component)m_FullArtCard.m_EvoPreviousStageIcon).gameObject.SetActive(true);
            }
            m_NormalGrp.SetActive(false);
            m_FullArtGrp.SetActive(true);
            m_SpecialCardGrp.SetActive(false);
            ((Component)m_GhostCard).gameObject.SetActive(false);
            ((Component)m_AncientArtifactImage).gameObject.SetActive(false);
        }
        else
        {
            m_NormalGrp.SetActive(true);
            m_FullArtGrp.SetActive(false);
            m_SpecialCardGrp.SetActive(false);
            ((Component)m_GhostCard).gameObject.SetActive(false);
            ((Component)m_AncientArtifactImage).gameObject.SetActive(false);
        }
        if (Object.op_Implicit((Object)(object)m_Card3dUIGroup))
        {
            m_Card3dUIGroup.EvaluateCardGrade(m_CardData);
        }
        m_GradedCardTextureImage.sprite = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.GetGradedCardScratchTexture(m_CardData.cardGrade);
        ShowGradedCardCase(m_Show2DGradedCase);
    }

    private void EvaluateCardUISetting()
    {
        //IL_00ac: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cc: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d1: Unknown result type (might be due to invalid IL or missing references)
        //IL_00dc: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e1: Unknown result type (might be due to invalid IL or missing references)
        //IL_010f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0114: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_0350: Unknown result type (might be due to invalid IL or missing references)
        //IL_0365: Unknown result type (might be due to invalid IL or missing references)
        //IL_0370: Unknown result type (might be due to invalid IL or missing references)
        //IL_0375: Unknown result type (might be due to invalid IL or missing references)
        //IL_0390: Unknown result type (might be due to invalid IL or missing references)
        //IL_03b0: Unknown result type (might be due to invalid IL or missing references)
        //IL_03d0: Unknown result type (might be due to invalid IL or missing references)
        //IL_03f0: Unknown result type (might be due to invalid IL or missing references)
        //IL_03fb: Unknown result type (might be due to invalid IL or missing references)
        //IL_0400: Unknown result type (might be due to invalid IL or missing references)
        //IL_042c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0437: Unknown result type (might be due to invalid IL or missing references)
        //IL_043c: Unknown result type (might be due to invalid IL or missing references)
        //IL_045c: Unknown result type (might be due to invalid IL or missing references)
        //IL_047c: Unknown result type (might be due to invalid IL or missing references)
        //IL_04d9: Unknown result type (might be due to invalid IL or missing references)
        //IL_04ee: Unknown result type (might be due to invalid IL or missing references)
        //IL_04f9: Unknown result type (might be due to invalid IL or missing references)
        //IL_04fe: Unknown result type (might be due to invalid IL or missing references)
        //IL_0519: Unknown result type (might be due to invalid IL or missing references)
        //IL_019d: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b8: Unknown result type (might be due to invalid IL or missing references)
        //IL_01d3: Unknown result type (might be due to invalid IL or missing references)
        //IL_01e8: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f3: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f8: Unknown result type (might be due to invalid IL or missing references)
        //IL_020e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0219: Unknown result type (might be due to invalid IL or missing references)
        //IL_021e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0233: Unknown result type (might be due to invalid IL or missing references)
        //IL_023e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0243: Unknown result type (might be due to invalid IL or missing references)
        //IL_0248: Unknown result type (might be due to invalid IL or missing references)
        //IL_0268: Unknown result type (might be due to invalid IL or missing references)
        //IL_0288: Unknown result type (might be due to invalid IL or missing references)
        //IL_02a3: Unknown result type (might be due to invalid IL or missing references)
        //IL_02c3: Unknown result type (might be due to invalid IL or missing references)
        //IL_02de: Unknown result type (might be due to invalid IL or missing references)
        //IL_02f3: Unknown result type (might be due to invalid IL or missing references)
        //IL_02fe: Unknown result type (might be due to invalid IL or missing references)
        //IL_0303: Unknown result type (might be due to invalid IL or missing references)
        //IL_031e: Unknown result type (might be due to invalid IL or missing references)
        if (Object.op_Implicit((Object)(object)m_MonsterNameText))
        {
            if (m_IsNestedFullArt)
            {
                ((Behaviour)m_MonsterNameText).enabled = m_CardUISetting.showNameFullArt;
            }
            else
            {
                ((Behaviour)m_MonsterNameText).enabled = m_CardUISetting.showName;
            }
        }
        ((Behaviour)m_Stat1Text).enabled = m_CardUISetting.showStat1;
        ((Behaviour)m_Stat2Text).enabled = m_CardUISetting.showStat2;
        ((Behaviour)m_Stat3Text).enabled = m_CardUISetting.showStat3;
        ((Behaviour)m_Stat4Text).enabled = m_CardUISetting.showStat4;
        ((TMP_Text)m_Stat1Text).transform.localPosition = m_CardUISetting.stat1PosOffset;
        ((TMP_Text)m_Stat1Text).transform.localScale = Vector3.one + m_CardUISetting.stat1ScaleOffset;
        if (m_ArtworkImageLocalPos != Vector3.zero)
        {
            ((Component)m_MonsterImage).transform.localPosition = m_ArtworkImageLocalPos;
        }
        m_ArtworkImageLocalPos = ((Component)m_MonsterImage).transform.localPosition;
        if (!m_IsNestedFullArt)
        {
            if (!m_CardUISetting.showEdition && ((Behaviour)m_FirstEditionText).enabled)
            {
                ((Behaviour)m_FirstEditionText).enabled = false;
            }
            ((Behaviour)m_RarityImage).enabled = m_CardUISetting.showRarity;
            ((Behaviour)m_RarityText).enabled = m_CardUISetting.showRarity;
            ((Behaviour)m_NumberText).enabled = m_CardUISetting.showNumber;
            ((TMP_Text)m_NumberText).transform.localPosition = m_CardUISetting.numberPosOffset;
            ((TMP_Text)m_FirstEditionText).transform.localPosition = m_CardUISetting.editionPosOffset;
            ((Component)m_MonsterMask).transform.localPosition = m_CardUISetting.monsterImagePosOffset;
            ((Component)m_MonsterMask).transform.localScale = Vector3.one + m_CardUISetting.monsterImageScaleOffset;
            ((Component)m_MonsterImage).transform.localPosition = m_ArtworkImageLocalPos + m_CardUISetting.artworkImagePosOffset;
            ((Component)m_MonsterImage).transform.localScale = Vector3.one + -m_CardUISetting.monsterImageScaleOffset;
            ((Component)m_MonsterMaskImage).transform.localPosition = ((Component)m_MonsterImage).transform.localPosition;
            ((Component)m_MonsterMaskImage).transform.localScale = ((Component)m_MonsterImage).transform.localScale;
            ((Component)m_MonsterGlowMask).transform.localPosition = m_CardUISetting.monsterImagePosOffset;
            ((Component)m_MonsterGlowMask).transform.localScale = ((Component)m_MonsterMask).transform.localScale;
            ((TMP_Text)m_FameText).transform.localPosition = m_CardUISetting.famePosOffset;
            ((TMP_Text)m_FameText).transform.localScale = Vector3.one + m_CardUISetting.fameScaleOffset;
            ((TMP_Text)m_MonsterNameText).transform.localPosition = m_CardUISetting.namePosOffset;
        }
        else
        {
            ((Behaviour)m_Stat1Text).enabled = m_CardUISetting.fullArtShowStat1;
            ((Component)m_MonsterMask).transform.localPosition = m_CardUISetting.fullArtMonsterImagePosOffset;
            ((Component)m_MonsterMask).transform.localScale = Vector3.one + m_CardUISetting.fullArtMonsterImageScaleOffset;
            ((Component)m_MonsterGlowMask).transform.localPosition = m_CardUISetting.fullArtMonsterImagePosOffset;
            ((Component)m_MonsterGlowMask).transform.localScale = ((Component)m_MonsterMask).transform.localScale;
            ((Component)m_FullArtBGImageMask).transform.localScale = ((Component)m_MonsterMask).transform.localScale;
            ((Component)m_FullArtBGImageMask).transform.localScale = ((Component)m_MonsterMask).transform.localScale + m_CardUISetting.fullArtBGMaskScaleOffset;
            m_MonsterMask.sprite = m_CardBGImage.sprite;
            ((Component)m_MonsterImage).transform.localPosition = m_ArtworkImageLocalPos + m_CardUISetting.fullArtArtworkImagePosOffset;
            ((Component)m_MonsterMaskImage).transform.localPosition = ((Component)m_MonsterImage).transform.localPosition;
            ((Component)m_MonsterMaskImage).transform.localScale = ((Component)m_MonsterImage).transform.localScale;
            m_CardFoilMaskImage.sprite = m_CardBGImage.sprite;
            m_FullArtBGImageMask.sprite = m_CardBGImage.sprite;
            m_MonsterGlowMask.sprite = m_CardBGImage.sprite;
            ((TMP_Text)m_FameText).transform.localPosition = m_CardUISetting.fullArtFamePosOffset;
            ((TMP_Text)m_FameText).transform.localScale = Vector3.one + m_CardUISetting.fullArtFameScaleOffset;
            ((TMP_Text)m_MonsterNameText).transform.localPosition = m_CardUISetting.fullArtNamePosOffset;
        }
    }

    private void MarkAsNestedFullArt(bool isNested)
    {
        m_IsNestedFullArt = isNested;
    }

    public void SetIsUnlocked(bool isUnlocked)
    {
        m_CardBack.SetActive(!isUnlocked);
        if (Object.op_Implicit((Object)(object)m_CardFront))
        {
            m_CardFront.SetActive(isUnlocked);
        }
    }

    public CardData GetCardData()
    {
        return m_CardData;
    }

    public void SetBrightness(float brightness)
    {
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0026: Unknown result type (might be due to invalid IL or missing references)
        Color color = ((Graphic)m_BrightnessControl).color;
        color.a = (1f - brightness) * 0.95f;
        ((Graphic)m_BrightnessControl).color = color;
    }

    public void SetFarDistanceCull()
    {
        if (!m_IsFarDistanceCulled)
        {
            m_IsFarDistanceCulled = true;
            m_FarDistanceCullObjVisibilityList.Clear();
            for (int i = 0; i < m_FarDistanceCullObjList.Count; i++)
            {
                m_FarDistanceCullObjVisibilityList.Add(m_FarDistanceCullObjList[i].activeSelf);
                m_FarDistanceCullObjList[i].SetActive(false);
            }
        }
    }

    public void ResetFarDistanceCull()
    {
        if (m_IsFarDistanceCulled)
        {
            m_IsFarDistanceCulled = false;
            for (int i = 0; i < m_FarDistanceCullObjVisibilityList.Count; i++)
            {
                m_FarDistanceCullObjList[i].SetActive(m_FarDistanceCullObjVisibilityList[i]);
            }
            m_FarDistanceCullObjVisibilityList.Clear();
        }
    }

    public bool IsCard3dUIGroupSet()
    {
        return (Object)(object)m_Card3dUIGroup != (Object)null;
    }
}
