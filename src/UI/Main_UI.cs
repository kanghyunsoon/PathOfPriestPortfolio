using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class Main_UI : MonoBehaviour
{
    public static Main_UI instance = null;

    #region Parameter
    [Header("##Default")]
    [SerializeField] private TextMeshProUGUI m_Level_Text;
    [SerializeField] private TextMeshProUGUI m_ALLATK_Text;
    [SerializeField] private TextMeshProUGUI m_LevelUp_Money_Text;
    [SerializeField] private TextMeshProUGUI m_Money_Text;
    [SerializeField] private TextMeshProUGUI m_Stage_Count_Text;
    [SerializeField] private TextMeshProUGUI m_Stage_Text;
    [SerializeField] private TextMeshProUGUI m_Dia_Text;
    Color m_Stage_Color = new Color(0, 0.7295136f, 1, 1);

    [Space(20.0f)]
    [Header("##Fade")]
    [SerializeField] private Image m_Fade;
    [SerializeField] private float m_FadeDuration;

    [Space(20.0f)]
    [Header("##Monster_Slider")]
    [SerializeField] private GameObject m_Monster_Slider_OBJ;
    [SerializeField] private Image m_Monster_Slider;
    [SerializeField] private TextMeshProUGUI m_Monster_Value_Text;

    [Space(20.0f)]
    [Header("##Boss_Slider")]
    [SerializeField] private GameObject m_Boss_Slider_OBJ;
    [SerializeField] private Image m_Boss_Slider;
    [SerializeField] private TextMeshProUGUI m_Boss_Value_Text, m_Boss_Stage_Text;

    [Space(20.0f)]
    [Header("##Dungeon_Slider")]
    [SerializeField] private GameObject m_Dungeon_Slider_OBJ;
    [SerializeField] private Image m_Dungeon_Slider;
    [SerializeField] private TextMeshProUGUI m_Dungeon_Value_Text, m_Dungeon_Stage_Text;
    [SerializeField] private GameObject[] Dungeon_Value_OBJS;
    [SerializeField] private TextMeshProUGUI MonsterCountText;
    [SerializeField] private Image m_MonsterSlider_Fill;
    [SerializeField] private TextMeshProUGUI m_MonsterSlider_Text;
    Coroutine DungeonCoroutine = null;

    [Space(20.0f)]
    [Header("##Dead_Frame")]
    [SerializeField] private GameObject Dead_Frame;

    [Space(20.0f)]
    [Header("##Legendary_PopUp")]
    [SerializeField] private Animator m_Legendary_Popup;
    [SerializeField] private Image m_Item_Frame;
    [SerializeField] private Image m_PopUp_Image;
    [SerializeField] private TextMeshProUGUI m_PopUp_Text;
    Coroutine Legendary_Coroutine;
    bool isPopUp = false;

    [Space(20.0f)]
    [Header("## Item_PopUp")]
    [SerializeField] private Transform m_ItemContent;
    private List<TextMeshProUGUI> m_Item_Texts = new List<TextMeshProUGUI>();
    private List<Coroutine> m_Item_Coroutines = new List<Coroutine>();

    [Space(20f)]
    [Header("## Hero_Frame")]
    [SerializeField] private UI_Main_Part[] m_Main_Parts;
    public Image Main_Character_Fill;
    Dictionary<Player, UI_Main_Part> m_Part = new Dictionary<Player, UI_Main_Part>();

    [Header("## ADS")]
    [SerializeField] private Image Fast_Lock;
    [SerializeField] private GameObject Fast_Fade;
    [SerializeField] private GameObject[] Buffs_Lock;
    [SerializeField] private Image x2Fill;
    [SerializeField] private TextMeshProUGUI x2Text;

    [Header("## LayerButtons")]
    [SerializeField] private Transform[] Layer_Images;
    #endregion
    bool isDead = false;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        TextCheck();
        Monster_Slider_Count();


        Base_Manager.isFast = PlayerPrefs.GetInt("FAST") == 1 ? true : false;

        TimeCheck();
        BuffCheck();

        for (int i = 0; i < m_ItemContent.childCount; i++)
        {
            m_Item_Texts.Add(m_ItemContent.GetChild(i).GetComponent<TextMeshProUGUI>());
            m_Item_Coroutines.Add(null);
        }

        Stage_Manager.m_ReadyEvent += OnReady;
        Stage_Manager.m_BossEvent += OnBoss;
        Stage_Manager.m_ClearEvent += OnClear;
        Stage_Manager.m_DeadEvent += OnDead;
        Stage_Manager.m_DungeonEvent += OnDungeon;
        Stage_Manager.m_DungeonClearEvent += OnDungeonClear;
        Stage_Manager.m_DungeonFailEvent += OnDungeonFail;

        Stage_Manager.State_Change(Stage_State.Ready);
    }

    private void Update()
    {
        if(Data_Manager.m_Data.Buff_x2 > 0.0f)
        {
            x2Fill.fillAmount = Data_Manager.m_Data.Buff_x2 / 1800.0f;
            x2Text.text = Utils.GetTimer(Data_Manager.m_Data.Buff_x2);
        }
    }

    public void LayerCheck(int value)
    {

        if (value != -1)
        {
            StartCoroutine(ImageMoveCoroutine(value));
            Layer_Images[value].transform.GetChild(0).gameObject.SetActive(true);
            Layer_Images[value].transform.GetChild(0).gameObject.transform.localPosition = new Vector2(22.5f, -55.8f);
        }


        for (int i = 0; i < Layer_Images.Length; i++)
        {
            if (value != i  && Layer_Images[i].localScale.x >= 1.3f)
            {
                StartCoroutine(ImageMoveCoroutine(i,true));
                Layer_Images[i].transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    IEnumerator ImageMoveCoroutine(int value, bool ScaleDown = false)
    {
        float current = 0.0f;
        float percent = 0.0f;
        float start = ScaleDown ? 66.0f : 33.0f; // yPosition
        float end = ScaleDown ? 66.0f : 66.0f;   // yPosition
        float startScale = ScaleDown ? 1.5f : 1.0f;
        float endScale = ScaleDown ? 1.0f : 1.5f;
        while (percent < 1.0f)
        {
            current += Time.unscaledDeltaTime;
            percent = current / 0.2f;
            float yPos = Mathf.Lerp(start, end, percent);
            float ScalePos = Mathf.Lerp(startScale, endScale, percent);
            Layer_Images[value].localPosition = new Vector2(0.0f, yPos);
            Layer_Images[value].localScale = new Vector3(ScalePos, ScalePos, 1.0f);
            yield return null;

        }
    }

    // PlayerPrefs, json
    public void BuffCheck()
    {
        for(int i =0; i< Data_Manager.m_Data.Buff_Timers.Length; i++)
        {
            if(Data_Manager.m_Data.Buff_Timers[i] > 0.0f)
            {
                Buffs_Lock[i].SetActive(false);
            }
            else
            {
                Buffs_Lock[i].SetActive(true);
            }
        }
        if(Data_Manager.m_Data.Buff_x2 > 0.0f)
        {
            x2Fill.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            x2Fill.transform.parent.gameObject.SetActive(false);
        }
    }

    private void TimeCheck()
    {
        Time.timeScale = Base_Manager.isFast ? 2.0f : 1.5f;
        Fast_Lock.gameObject.SetActive(!Base_Manager.isFast);
        Fast_Fade.gameObject.SetActive(Base_Manager.isFast);
    }

    public void GetFast()
    {
        bool fast = !Base_Manager.isFast;
        if (fast == true)
        {
            if (Data_Manager.m_Data.Buff_x2 <= 0.0f)
            {
                Base_Manager.ADS.ShowRewardedAds(() =>
                {
                    Data_Manager.m_Data.Buff_x2 = 1800.0f;
                    BuffCheck();
                    TimeCheck();
                });
            }
        }
        Base_Manager.isFast = fast;
        PlayerPrefs.SetInt("FAST", fast == true ? 1 : 0);

        BuffCheck();
        TimeCheck();

    }

    public void GetItem(Item_Scriptable item)
    {
        bool AllActive = true;

        for(int i =0; i< m_Item_Texts.Count; i++)
        {
            if(m_Item_Texts[i].gameObject.activeSelf == false)
            {
                m_Item_Texts[i].gameObject.SetActive(true);
                m_Item_Texts[i].text = string.Format(Local_Manager.local_Data["PopUp/GetItem"].Get_Data(),
                    Utils.String_Color_Rarity(item.rarity) + "[" + Local_Manager.local_Data[item.Item_Name].Get_Data() + "]</color>");

                for (int j =0; j<i; j++)
                {
                    RectTransform rect = m_Item_Texts[j].GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + 50.0f);
                }

                if (m_Item_Coroutines[i] != null)
                {
                    StopCoroutine(m_Item_Coroutines[i]);
                }
                m_Item_Coroutines[i] = StartCoroutine(Item_Text_FadeOut(m_Item_Texts[i].GetComponent<RectTransform>()));

                AllActive = false;
                break;
            }
        }

        if(AllActive)
        {
            GameObject baseRect = null;
            float yCount = 0.0f;
            for (int i = 0; i < m_Item_Texts.Count; i++)
            {
                RectTransform rect = m_Item_Texts[i].GetComponent<RectTransform>();
                if(rect.anchoredPosition.y > yCount)
                {
                    baseRect = rect.gameObject;
                    yCount = rect.anchoredPosition.y;
                }
            }

            for (int i = 0; i < m_Item_Texts.Count; i++)
            {
                if (baseRect == m_Item_Texts[i].gameObject)
                {
                    m_Item_Texts[i].gameObject.SetActive(false);
                    m_Item_Texts[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);

                    m_Item_Texts[i].gameObject.SetActive(true);
                    m_Item_Texts[i].text = string.Format(Local_Manager.local_Data["PopUp/GetItem"].Get_Data(),
                        Utils.String_Color_Rarity(item.rarity) + "[" + item.Item_Name + "]</color>");

                    if (m_Item_Coroutines[i] != null)
                    {
                        StopCoroutine(m_Item_Coroutines[i]);
                    }
                    m_Item_Coroutines[i] = StartCoroutine(Item_Text_FadeOut(m_Item_Texts[i].GetComponent<RectTransform>()));
                }
                else
                {
                    RectTransform rect = m_Item_Texts[i].GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(0.0f, rect.anchoredPosition.y + 50.0f);
                }

            }

        }

        if ((int)item.rarity >= (int)Rarity.Hero)
        {
           GetLegendaryPopUp(item);
        }

    }

    IEnumerator Item_Text_FadeOut(RectTransform rect)
    {
        yield return new WaitForSeconds(2.0f);
        rect.gameObject.SetActive(false);
        rect.anchoredPosition = new Vector2(0.0f, 0.0f);
    }

    IEnumerator Legendary_PopUp_Coroutine()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        isPopUp = false;
        m_Legendary_Popup.SetTrigger("Close");
    }

    public void Set_Boss_State()
    {
        Stage_Manager.isDead = false;
        Stage_Manager.State_Change(Stage_State.Boss);
    }

    private void SliderOBJCheck(SliderType type)
    {
        m_Monster_Slider_OBJ.SetActive(false);
        m_Boss_Slider_OBJ.SetActive(false);
        m_Dungeon_Slider_OBJ.SetActive(false);
        if (Stage_Manager.isDead && !Stage_Manager.isDungeon)
        {
            Dead_Frame.SetActive(true);
            return;
        }
        switch(type)
        {
            case SliderType.Defalut:
                m_Monster_Slider_OBJ.SetActive(true);
                Monster_Slider_Count();
                break;
            case SliderType.Dungeon:
                m_Dungeon_Slider_OBJ.SetActive(true);
                DungeonCoroutine = StartCoroutine(Dungeon_Slider_Coroutine());
                break;
            case SliderType.Boss:
                m_Boss_Slider_OBJ.SetActive(true);
                break;
        }
        Dead_Frame.SetActive(false);

        float value = type == SliderType.Defalut ? 0.0f : 1.0f;
        Boss_Slider_Count(value, 1.0f);
    }

    private void OnReady()
    {
        FadeInOut(true);
        PartInitalize();
    }

    private void PartInitalize()
    {
        m_Part.Clear();
        for (int i = 0; i < 6; i++) m_Main_Parts[i].Initialize();

        int indexValue = 0;

        for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
        {
            var data = Base_Manager.Data.m_Set_Character[i];
            if (data != null)
            {
                indexValue++;
                m_Main_Parts[i].InitData(data.Data, false);
                m_Main_Parts[i].transform.SetSiblingIndex(indexValue);
                m_Part.Add(Character_Spawner.players[i], m_Main_Parts[i]);
            }
        }
    }

    public void Set_Character_Data()
    {
        int indexValue = 0;
        for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
        {
            var data = Base_Manager.Data.m_Set_Character[i];
            if (data != null)
            {
                indexValue++;
                m_Main_Parts[i].InitData(data.Data, true);
                m_Main_Parts[i].transform.SetSiblingIndex(indexValue);
            }
        }
    }

    public void Character_StateCheck(Player player)
    {
        m_Part[player].StateCheck(player);
    }

    private void OnBoss()
    {
        TextCheck();
        SliderOBJCheck(SliderType.Boss);
    }

    private void OnClear()
    {
        SliderOBJCheck(SliderType.Defalut);
        StartCoroutine(Clear_Delay());
    }

    private void OnDungeonClear(int value)
    {
        if (DungeonCoroutine != null)
        {
            StopCoroutine(DungeonCoroutine);
            DungeonCoroutine = null;
        }

        int level = Stage_Manager.DungeonLevel;

        if(Stage_Manager.DungeonLevel == Data_Manager.m_Data.Dungeon_Clear_Level[value])
        {
            Data_Manager.m_Data.Dungeon_Clear_Level[value]++;
            if (value == 0)
            {
                Data_Manager.m_Data.DiaDungeon_Count++;
            }
            else if (value == 1)
            {
                Data_Manager.m_Data.GoldDungeon_Count++;
            }
        }



        if (Data_Manager.m_Data.Key[value] > 0) Data_Manager.m_Data.Key[value]--;
        else Data_Manager.m_Data.KeyAssets[value]--;

        switch (value)
        {
            case 0:
                Data_Manager.m_Data.Dia += (level + 1) * 50;
                break;
            case 1:
                Data_Manager.m_Data.Money += Utils.Data.stageData.MONEY((level + 1) * 50);
                break;
        }
        TextCheck();

        Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["DongeonSuccess"].Get_Data(), Color.white);
        OnClear();
    }


    private void OnDungeon(int value)
    {
        for (int i = 0; i < Dungeon_Value_OBJS.Length; i++) Dungeon_Value_OBJS[i].SetActive(false);

        FadeInOut(true, true);

        PartInitalize();
        Dungeon_Value_OBJS[value].gameObject.SetActive(true);
        SliderOBJCheck(SliderType.Dungeon);

        string dungeonName = Stage_Manager.DungeonType == 0
         ? Local_Manager.local_Data["DiaDungeon"].Get_Data() + "Lv. " + (Stage_Manager.DungeonLevel + 1).ToString()
         : Local_Manager.local_Data["GoldDungeon"].Get_Data()+ "Lv. " + (Stage_Manager.DungeonLevel + 1).ToString();

        m_Dungeon_Stage_Text.text = dungeonName;

        isDead = Stage_Manager.isDead;
        Stage_Manager.isDead = false;
    }

    private void OnDead()
    {
        TextCheck();
        StartCoroutine(Dead_Delay());
    }

    private void OnDungeonFail()
    {
        if (DungeonCoroutine != null)
        {
            StopCoroutine(DungeonCoroutine);
            DungeonCoroutine = null;
        }

        if(isDead) Stage_Manager.isDead = true;
        TextCheck();
        StartCoroutine(Dead_Delay());

        StartCoroutine(ShowMessageAfterDelay());

    }

    private IEnumerator ShowMessageAfterDelay()
    {
        yield return new WaitForSeconds(1.35f); // 1초 대기
        Base_Canvas.instance.Get_Dungeon_Toast().Initialize(Local_Manager.local_Data["DongeonFailed"].Get_Data(), Color.red);
    }

    IEnumerator Dead_Delay()
    {
        yield return StartCoroutine(Clear_Delay());

        SliderOBJCheck(SliderType.Defalut);

        for (int i = 0; i < Spawner.m_Monsters.Count; i++)
        {
            if (Spawner.m_Monsters[i].isBoss == true)
            {
                Destroy(Spawner.m_Monsters[i].gameObject);
            }
            else
            {
                if (Spawner.m_Monsters[i].isDead == false)
                {
                    Spawner.m_Monsters[i].isDead = true;
                    Base_Manager.Pool.m_pool_Dictionary["Monster"].Return(Spawner.m_Monsters[i].gameObject);
                }
            }
        }
        Spawner.m_Monsters.Clear();
    }

    IEnumerator Clear_Delay()
    {
        yield return new WaitForSeconds(1.2f);
        FadeInOut(false);

        yield return new WaitForSeconds(1.0f);
        Stage_Manager.State_Change(Stage_State.Ready);
    }

    public void Monster_Slider_Count()
    {
        float value = (float)Stage_Manager.Count / (float)Stage_Manager.MaxCount;

        if (value > 1.0f)
        {

            value = 1.0f;

            if (Stage_Manager.m_State != Stage_State.Boss)
                Stage_Manager.State_Change(Stage_State.Boss);
        }

        m_Monster_Slider.fillAmount = value;
        m_Monster_Value_Text.text = String.Format("{0:0.0}", value * 100.0f) + "%";
    }

    public void Dungeon_Monster_Slider_Count()
    {
        MonsterCountText.text = "(" + Stage_Manager.Dungeon_Count.ToString() + ")";

        if(Stage_Manager.Dungeon_Count <= 0)
        {
            Stage_Manager.State_Change(Stage_State.DungeonClear, Stage_Manager.DungeonType);
        }
    }

    public void Boss_Slider_Count(double hp, double maxHp)
    {
        float value = (float)hp / (float)maxHp;

        if (value <= 0.0f)
        {
            value = 0.0f;
        }

        if(!Stage_Manager.isDungeon)
        {
            m_Boss_Slider.fillAmount = value;
            m_Boss_Value_Text.text = String.Format("{0:0.0}", value * 100.0f) + "%";
        }
        else
        {
            m_MonsterSlider_Fill.fillAmount = value;
            m_MonsterSlider_Text.text = String.Format("{0:0.0}", value * 100.0f) + "%";
        }
    }

    IEnumerator Dungeon_Slider_Coroutine()
    {
        float t = 60.0f;
        while(t >= 0.0f)
        {


            t -= Time.deltaTime;
            m_Dungeon_Slider.fillAmount = t / 60.0f;
            m_Dungeon_Value_Text.text = string.Format("{0:0.00}/s", t);

            yield return null;
        }
        Stage_Manager.State_Change(Stage_State.DungeonFail);
    }

    public void FadeInOut(bool FadeInOut,bool Sibling = false, Action action = null)
    {
        if(!Sibling)
        {
            m_Fade.transform.SetParent(this.transform);
            m_Fade.transform.SetSiblingIndex(0);
        }
        else
        {
            m_Fade.transform.SetParent(Base_Canvas.instance.transform);
            m_Fade.transform.SetAsLastSibling();
        }
        StartCoroutine(FadeInOut_Coroutine(FadeInOut, action));
    }
    IEnumerator FadeInOut_Coroutine(bool FadeInOut, Action action = null)
    {
        if(FadeInOut == false)
        {
            m_Fade.raycastTarget = true;
        }


        float current = 0.0f;
        float percent = 0.0f;
        float start = FadeInOut ? 1.0f : 0.0f;
        float end = FadeInOut ? 0.0f : 1.0f;

        while(percent < 1.0f)
        {
            current += Time.deltaTime;
            percent = current / m_FadeDuration;
            float LerpPos = Mathf.Lerp(start, end, percent);
            m_Fade.color = new Color(0, 0, 0, LerpPos);

            yield return null;
        }

        if (action != null) action?.Invoke();

        m_Fade.raycastTarget = false;


    }
    public void TextMoney() => m_Money_Text.text = StringMethod.ToCurrencyString(Data_Manager.m_Data.Money);


    public void TextCheck()
    {
        m_Level_Text.text = "LV." + (Data_Manager.m_Data.Level + 1).ToString();
        m_ALLATK_Text.text = StringMethod.ToCurrencyString(Base_Manager.Player.Average_ATK());

        double LevelUpMoneyValue = Utils.CalculatedValue(
            Utils.Data.levelData.B_MONEY,
            Data_Manager.m_Data.Level,
            Utils.Data.levelData.C_MONEY);

        m_LevelUp_Money_Text.text = StringMethod.ToCurrencyString(LevelUpMoneyValue);
        m_LevelUp_Money_Text.color = Utils.Coin_Check(LevelUpMoneyValue) ? Color.green : Color.red;

        m_Money_Text.text = StringMethod.ToCurrencyString(Data_Manager.m_Data.Money);

        m_Stage_Text.text  = Stage_Manager.isDead ? Local_Manager.local_Data["Repeating"].Get_Data() : Local_Manager.local_Data["InProgress"].Get_Data();
        m_Stage_Text.color = Stage_Manager.isDead ? Color.yellow : m_Stage_Color;


        List<string> difficultyOrder = new List<string> { "Easy", "Normal", "Hard", "Hell" };
        int stagesPerDifficulty = 100; // 스테이지별 난이도 구간 변경점 -> 10-> 100

        int stage = Data_Manager.m_Data.Stage;
        int stageValue = stage + 1;

        // 난이도 인덱스 계산
        int difficultyIndex = stage / stagesPerDifficulty;
        string stageText = difficultyIndex < difficultyOrder.Count
            ? Local_Manager.local_Data[difficultyOrder[difficultyIndex]].Get_Data()
            : $"Unknown({difficultyIndex})"; // 미정의 난이도일 경우 대비

        // 난이도 내 회차 및 상세 스테이지 계산
        int stageWithinDifficulty = stage % stagesPerDifficulty;
        int stageForward = (stageWithinDifficulty / 10) + 1;     // 회차 (10개 단위)
        int stageBackward = (stageWithinDifficulty % 10) + 1;     // 1~10

        // UI 적용
        m_Stage_Count_Text.text = $"{stageText} {stageForward} - {stageBackward}";
        m_Dia_Text.text = Data_Manager.m_Data.Dia.ToString();
    }


    private void GetLegendaryPopUp(Item_Scriptable item)
    {
        if (isPopUp)
        {
            m_Legendary_Popup.gameObject.SetActive(false);
        }
        isPopUp = true;
        m_Legendary_Popup.gameObject.SetActive(true);

        m_Item_Frame.sprite = Utils.Get_Atlas(item.rarity.ToString());

        m_PopUp_Image.sprite = Utils.Get_Atlas(item.name);
        m_PopUp_Image.SetNativeSize();

        m_PopUp_Text.text = string.Format(Local_Manager.local_Data["PopUp/GetItem02"].Get_Data(), Utils.String_Color_Rarity(item.rarity) + item.Item_Name);

        if (Legendary_Coroutine != null)
        {
            StopCoroutine(Legendary_Coroutine);
        }
        Legendary_Coroutine = StartCoroutine(Legendary_PopUp_Coroutine());
    }

}
