using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UI_Status : UI_Base
{
    [SerializeField] private StatusType m_StatusState;
    [SerializeField] private RectTransform m_Bar;
    [SerializeField] private RectTransform BottomContent;
    [SerializeField] private Button[] m_Bottom_Buttons;

    [SerializeField] private TextMeshProUGUI ALLDPS, ATK, HP;
    [SerializeField] private TextMeshProUGUI GoldDrop, ItemDrop, ATKSpeed, CriticalPercentage, CriticalDamage;
    [SerializeField] private GameObject[] Panel_OBJS;


    public override bool Init()
    {
        Main_UI.instance.FadeInOut(true, true, null);
        TextCheck();

        for (int i = 0; i < m_Bottom_Buttons.Length; i++)
        {
            int index = i;
            m_Bottom_Buttons[index].onClick.RemoveAllListeners();
            m_Bottom_Buttons[index].onClick.AddListener(() => Status_Check((StatusType)index));
        }

        return base.Init();
    }

    private void TextCheck()
    {
        ALLDPS.text = StringMethod.ToCurrencyString(Base_Manager.Player.Average_ATK());
        ATK.text = StringMethod.ToCurrencyString(Base_Manager.Player.Main_ATK());
        HP.text = StringMethod.ToCurrencyString(Base_Manager.Player.Main_HP());
        GoldDrop.text = string.Format("{0:0}%", Base_Manager.Player.GoldDropPercentage());
        ItemDrop.text = string.Format("{0:0}%", Base_Manager.Player.ItemDropPercentage());
        ATKSpeed.text = string.Format("{0:0.0}%", Base_Manager.Player.ATKSpeed());
        CriticalPercentage.text = string.Format("{0:0}%", Base_Manager.Player.CriticalPercentage());
        CriticalDamage.text = string.Format("{0:0}%", Base_Manager.Player.CriticalDamage());
    }

    public void Status_Check(StatusType m_State)
    {
        for(int i = 0; i < Panel_OBJS.Length; i++)
        {
            Panel_OBJS[i].SetActive(false);
        }

        Panel_OBJS[(int)m_State].SetActive(true);

        m_StatusState = m_State;
        StartCoroutine(barMovementCoroutine(
            m_Bottom_Buttons[(int)m_State].GetComponent<RectTransform>().anchoredPosition,
            m_Bottom_Buttons[(int)m_State].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x));
    }


    IEnumerator barMovementCoroutine(Vector2 endPos, float endXPos)
    {
        float current = 0;
        float percent = 0;
        Vector2 start = m_Bar.anchoredPosition;
        Vector2 end = new Vector2(endPos.x, BottomContent.anchoredPosition.y- 367);

        float startX = m_Bar.sizeDelta.x;
        float endX = endXPos + 50.0f;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.1f;
            Vector2 LerpPos = Vector2.Lerp(start, end, percent);
            float LerpPosX = Mathf.Lerp(startX, endX, percent);

            m_Bar.anchoredPosition = LerpPos;
            m_Bar.sizeDelta = new Vector2(Mathf.Clamp(LerpPosX, 200.0f, Mathf.Infinity), m_Bar.sizeDelta.y);
            TextCheck();
            yield return null;
        }
    }

    public override void DisableOBJ()
    {
        Main_UI.instance.LayerCheck(-1);
        base.DisableOBJ();
    }
}
