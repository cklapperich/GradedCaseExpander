using I2.Loc;
using TMPro;
using UnityEngine;

public class Card3dUIGroup : MonoBehaviour
{
    public CardUI m_CardUI;

    public GameObject m_CardFrontMeshPos;

    public GameObject m_CardBackMesh;

    public GameObject m_NewCardIndicator;

    public GameObject m_GradedCardGrp;

    public GameObject m_SlabTopLayerMesh;

    public Transform m_ScaleGrp;

    public Transform m_CardUIAnimGrp;

    public TextMeshProUGUI m_CardCountText;

    public TextMeshProUGUI m_GradeNumberText;

    public TextMeshProUGUI m_GradeDescriptionText;

    public TextMeshProUGUI m_GradeNameText;

    public TextMeshProUGUI m_GradeExpansionRarityText;

    public TextMeshProUGUI m_GradeSerialText;

    public bool m_IgnoreCulling;

    private bool m_IsActive;

    private bool m_IsSmoothLerpingToPos;

    private bool m_IsIgnoreUpForce;

    private float m_Timer;

    private float m_UpTimer;

    private float m_LerpSpeed = 3f;

    private float m_UpLerpSpeed = 5f;

    private float m_UpLerpHeight = 0.1f;

    private float m_Accelration;

    private Vector3 m_StartPos;

    private Quaternion m_StartRot;

    private Vector3 m_StartScale;

    private Transform m_TargetTransform;

    private void Awake()
    {
        m_NewCardIndicator.SetActive(false);
        m_CardUI.InitCard3dUIGroup(this);
    }

    private void Start()
    {
        Card3dUISpawner.AddCardToManager(this);
        m_CardUI.SetBrightness(CSingleton<LightManager>.Instance.GetBrightness());
    }

    public void SetVisibility(bool isVisible)
    {
        ((Component)this).gameObject.SetActive(isVisible);
    }

    public void EvaluateCardGrade(CardData cardData)
    {
        if (cardData.cardGrade > 0)
        {
            m_GradedCardGrp.SetActive(true);
            ((TMP_Text)m_GradeNumberText).text = cardData.cardGrade.ToString();
            ((TMP_Text)m_GradeDescriptionText).text = GameInstance.GetCardGradeString(cardData.cardGrade);
            ((TMP_Text)m_GradeNameText).text = ((TMP_Text)m_CardUI.m_MonsterNameText).text;
            ((TMP_Text)m_GradeExpansionRarityText).text = LocalizationManager.GetTranslation(cardData.expansionType.ToString()) + " " + CPlayerData.GetFullCardTypeName(cardData);
        }
        else
        {
            m_GradedCardGrp.SetActive(false);
        }
    }

    public void SetCardCountText(int count, bool showDuplicate)
    {
        if (showDuplicate)
        {
            ((TMP_Text)m_CardCountText).text = "X " + count + " (" + (count + 1) + ")";
        }
        else
        {
            ((TMP_Text)m_CardCountText).text = "X " + count;
        }
    }

    public void SetCardCountTextVisibility(bool isVisible)
    {
        ((Component)m_CardCountText).gameObject.SetActive(isVisible);
    }

    public void ActivateCard()
    {
        m_IsActive = true;
    }

    public void DisableCard()
    {
        m_IsActive = false;
        Card3dUISpawner.DisableCard(this);
    }

    public void SetLocalScale(Vector3 localScale)
    {
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        ((Component)m_ScaleGrp).transform.localScale = localScale;
    }

    public void SmoothLerpToTransform(Transform targetTransform, Transform targetParent, bool ignoreUpForce = false)
    {
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Unknown result type (might be due to invalid IL or missing references)
        //IL_0045: Unknown result type (might be due to invalid IL or missing references)
        //IL_004a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0056: Unknown result type (might be due to invalid IL or missing references)
        //IL_005b: Unknown result type (might be due to invalid IL or missing references)
        m_Timer = 0f;
        m_UpTimer = 0f;
        m_Accelration = 0f;
        ((Component)this).transform.parent = targetParent;
        m_StartPos = ((Component)this).transform.position;
        m_StartRot = ((Component)this).transform.rotation;
        m_StartScale = ((Component)this).transform.localScale;
        m_TargetTransform = targetTransform;
        m_IsSmoothLerpingToPos = true;
        m_IsIgnoreUpForce = ignoreUpForce;
    }

    private void Update()
    {
        //IL_002a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0055: Unknown result type (might be due to invalid IL or missing references)
        //IL_005a: Unknown result type (might be due to invalid IL or missing references)
        //IL_016c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0177: Unknown result type (might be due to invalid IL or missing references)
        //IL_017c: Unknown result type (might be due to invalid IL or missing references)
        //IL_017d: Unknown result type (might be due to invalid IL or missing references)
        //IL_018d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0192: Unknown result type (might be due to invalid IL or missing references)
        //IL_0193: Unknown result type (might be due to invalid IL or missing references)
        //IL_01a9: Unknown result type (might be due to invalid IL or missing references)
        //IL_01b4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01c4: Unknown result type (might be due to invalid IL or missing references)
        //IL_01da: Unknown result type (might be due to invalid IL or missing references)
        //IL_01e5: Unknown result type (might be due to invalid IL or missing references)
        //IL_01f5: Unknown result type (might be due to invalid IL or missing references)
        //IL_00af: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ba: Unknown result type (might be due to invalid IL or missing references)
        //IL_00bf: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00d0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e6: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f1: Unknown result type (might be due to invalid IL or missing references)
        //IL_0101: Unknown result type (might be due to invalid IL or missing references)
        //IL_0117: Unknown result type (might be due to invalid IL or missing references)
        //IL_0122: Unknown result type (might be due to invalid IL or missing references)
        //IL_0132: Unknown result type (might be due to invalid IL or missing references)
        //IL_0207: Unknown result type (might be due to invalid IL or missing references)
        //IL_020c: Unknown result type (might be due to invalid IL or missing references)
        if (m_IsSmoothLerpingToPos)
        {
            m_UpTimer += Time.deltaTime * m_UpLerpSpeed * 0.75f;
            Vector3 val = Vector3.up * (Mathf.PingPong(Mathf.Clamp(m_UpTimer, 0f, 2f), 1f) * m_UpLerpHeight);
            if (m_UpTimer > 0.2f)
            {
                m_Timer += Time.deltaTime * m_LerpSpeed * (1f + m_Accelration);
                m_Accelration += Time.deltaTime;
                ((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, m_TargetTransform.position + val, Time.deltaTime * 10f);
                ((Component)this).transform.rotation = Quaternion.Lerp(((Component)this).transform.rotation, m_TargetTransform.rotation, Time.deltaTime * 10f);
                ((Component)this).transform.localScale = Vector3.Lerp(((Component)this).transform.localScale, m_TargetTransform.localScale, Time.deltaTime * 10f);
            }
            else
            {
                m_Timer += Time.deltaTime * m_LerpSpeed * 0.1f;
                ((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, m_TargetTransform.position + val, Time.deltaTime * 2f) + val;
                ((Component)this).transform.rotation = Quaternion.Lerp(((Component)this).transform.rotation, m_TargetTransform.rotation, Time.deltaTime * 2f);
                ((Component)this).transform.localScale = Vector3.Lerp(((Component)this).transform.localScale, m_TargetTransform.localScale, Time.deltaTime * 2f);
            }
            if (m_IsIgnoreUpForce)
            {
                val = Vector3.zero;
                m_UpTimer = 2f;
            }
        }
    }

    public bool IsActive()
    {
        return m_IsActive;
    }
}
