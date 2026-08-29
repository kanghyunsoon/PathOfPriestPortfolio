using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base_Canvas : MonoBehaviour
{
    public static Base_Canvas instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (Base_Manager.isNight)
        {
            Get_UI("#NightWarning");

            StartCoroutine(ShowOfflineAfterNightWarning());
        }
        else
        {
            if (Utils.TimerCheck() >= 10.0d)
            {
                Get_UI("#Offline");
            }
        }

        Base_Manager.Sound.Play(Sound.BGM, "Main");


        Status_Button.onClick.AddListener(() => Get_UI("#Stat", true, true, true,0));

        Hero_Button.onClick.AddListener(() => Get_UI("#Heros", true, true, true,1));

        Relic_Button.onClick.AddListener(() => Get_UI("#Relic", false, true, true,2));

        Dungeon_Button.onClick.AddListener(() => Get_UI("#Dungeon", true, true, true,3));

        Smelt_Button.onClick.AddListener(() => Get_UI("#Smelt", false, true, true, 4));

        Shop_Button.onClick.AddListener(() => Get_UI("#Shop", false, true, true,5));

        Inventory_Button.onClick.AddListener(() => Get_UI("#Inventory"));
        SavingMode_Button.onClick.AddListener(() =>
        {
            Get_UI("#SavingMode");
            isSave = true;
        });
        ADSBuff_Button.onClick.AddListener(() => Get_UI("#ADS_Buff"));

        Setting_Button.onClick.AddListener(() => Get_UI("#Setting"));

        DailyQuest_Button.onClick.AddListener(() => Get_UI("#DailyQuest"));

        Achievement_Button.onClick.AddListener(() => Get_UI("#Achievement"));
        LeaderBoard_Button.onClick.AddListener(() => Base_Manager.GPGS.ShowReaderBoardUI());

        //StartCoroutine(DelayTutorial());
    }

    IEnumerator DelayTutorial()
    {
        yield return new WaitForSeconds(0.5f);
        StartTutorial(Hero_Button);
    }

    public Transform COIN;
    [SerializeField] private Transform LAYER;
    [SerializeField] private Transform Back_Layer;

    [SerializeField] private Button
        Status_Button,
        Hero_Button,
        Relic_Button,
        Dungeon_Button,
        Smelt_Button,
        Shop_Button,
        Inventory_Button,
        SavingMode_Button,
        ADSBuff_Button,
        Setting_Button,
        DailyQuest_Button,
        Achievement_Button,
        LeaderBoard_Button;

    [HideInInspector] public  PopUp_UI popup = null;
    [HideInInspector] public  UI_Base m_UI;
    public static bool isSave = false;
    [SerializeField] private GameObject Tutorial_Panel;

    public void StartTutorial(Button originButton)
    {
        Tutorial_Panel.SetActive(true);
        GameObject copy = Instantiate(originButton.gameObject);
        RectTransform copyRect = copy.GetComponent<RectTransform>();
        RectTransform originRect = originButton.GetComponent<RectTransform>();

        copy.transform.SetParent(Tutorial_Panel.transform);
        copyRect.anchorMin = new Vector2(0.5f, 0.5f);
        copyRect.anchorMax = new Vector2(0.5f, 0.5f);
        copyRect.pivot = new Vector2(0.5f, 0.5f);
        copyRect.sizeDelta = originRect.sizeDelta;

        copyRect.position = originRect.position;
        copyRect.localScale = Vector3.one;

        Button copyButton = copy.GetComponent<Button>();
        copyButton.onClick = originButton.onClick;
        copyButton.onClick.AddListener(() => EndTutorial());

    }

    private void EndTutorial()
    {
        for(int i = 0; i < Tutorial_Panel.transform.childCount; i++)
        {
            Destroy(Tutorial_Panel.transform.GetChild(i).gameObject);
            Tutorial_Panel.SetActive(false);
        }
    }

    private void Update()
    {
        // KeyCode.Escape = ESC, 뒤로가기 버튼
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Utils.UI_Holder.Count > 0)
                Utils.ClosePopupUI();
            else
            {
                Get_UI("#BackButton");
            }
        }
    }

    public UI_ToastPopUp Get_Toast()
    {
        return Instantiate(Resources.Load<UI_ToastPopUp>("UI/#PopUp"), transform);
    }

    public UI_ToastPopUp Get_Dungeon_Toast()
    {
        return Instantiate(Resources.Load<UI_ToastPopUp>("UI/#DungeonPopUp"), transform);
    }


    public Transform HOLDER_LAYER(int value)
    {
        return LAYER.GetChild(value);
    }


    public void Get_UI(string temp, bool Fade = false, bool Back =false, bool Close = false, int value = -1)
    {
        if(Utils.UI_Holder.Count > 0)
        {
            var peekData = Utils.UI_Holder.Peek();
            Main_UI.instance.LayerCheck(-1);
            if (peekData.name == temp)
            {
                Utils.CloseAllPopupUI();
                return;
            }
        }


        if(Close)
        {
            Utils.CloseAllPopupUI();
        }

        if (Fade)
        {
            Main_UI.instance.FadeInOut(false, true, () => GetPopupUI(temp, Back));
            Main_UI.instance.LayerCheck(value);
            return;
        }
        Main_UI.instance.LayerCheck(value);

        GetPopupUI(temp, Back);
    }

    public void GetPopupUI(string temp, bool Back = false)
    {
        if (m_UI != null) m_UI = null;

        var go = Instantiate(Resources.Load<UI_Base>("UI/" + temp), Back == true ? Back_Layer : transform);
        m_UI = go;
        m_UI.name = temp;

        Utils.UI_Holder.Push(go);
    }

    public PopUp_UI PopUpItem()
    {
        if (popup != null) Destroy(popup.gameObject);

        popup = Instantiate(Resources.Load<PopUp_UI>("UI/PopUp_Item"), transform);

        return popup;
    }

    private IEnumerator ShowOfflineAfterNightWarning()
    {
        // #NightWarning이 켜져 있으면 대기
        while (Utils.UI_Holder.Count > 0 && Utils.UI_Holder.Peek().name == "#NightWarning")
        {
            yield return null; // 프레임마다 검사
        }

        // #NightWarning이 꺼졌고, 조건이 맞으면 #Offline 실행
        if (Utils.TimerCheck() >= 10.0d)
        {
            Get_UI("#Offline");
        }
    }
}
