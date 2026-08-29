using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public delegate void OnSavingMonde();
public class UI_SavingMode : UI_Base
{
    [SerializeField] private TextMeshProUGUI BatteryText, TimerText, FightText, StageText;
    [SerializeField] private Image BatteryFill, LandImage;
    [SerializeField] private Transform Content;
    [SerializeField] private UI_Inventory_Part item_Part;

    public Dictionary<string, Item_Holder> m_SaveItem = new Dictionary<string, Item_Holder>();
    public Dictionary<string, UI_Inventory_Part> m_Parts = new Dictionary<string, UI_Inventory_Part>();

    public static OnSavingMonde m_OnSaving;

    Vector2 StartPos, EndPos;
    Camera Savingcamera;

    public override bool Init()
    {
        Savingcamera = Camera.main;
        Savingcamera.enabled = false;

        m_OnSaving?.Invoke();

        return base.Init();
    }

    public override void DisableOBJ()
    {
        Savingcamera.enabled = true;
        Base_Canvas.isSave = false;
        base.DisableOBJ();
    }

    private void Update()
    {
        // batteryLevel = 폰에서만 동작, 0.0f ~ 1.0f
        BatteryText.text = (SystemInfo.batteryLevel * 100).ToString() + "%";
        BatteryFill.fillAmount = SystemInfo.batteryLevel;

        // DateTime
        TimerText.text = System.DateTime.Now.ToString("tt hh:mm", CultureInfo.InvariantCulture);

        List<string> difficultyOrder = new List<string> { "Easy", "Normal", "Hard", "Hell" };
        int stagesPerDifficulty = 10; // 스테이지별 난이도 구간 변경점 -> 10-> 100

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
        StageText.text = $"{stageText} {stageForward} - {stageBackward}";

        FightText.text = Stage_Manager.isDead ? Local_Manager.local_Data["Repeating"].Get_Data() : Local_Manager.local_Data["InProgress"].Get_Data();

        if(Input.GetMouseButtonDown(0)) //  마우스가 눌렸을때
        {
            StartPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0)) // 마우스를 누르는 동안
        {
            EndPos = Input.mousePosition;
            float distance = Vector2.Distance(EndPos, StartPos);

            LandImage.color = new Color(1, 1, 1, Mathf.Clamp(distance / (Screen.width / 2),0.3f, 1.0f));

            if (distance >= Screen.width /2)
            {
                DisableOBJ();
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            StartPos = Vector2.zero;
            EndPos = Vector2.zero;
            LandImage.color = new Color(1, 1, 1, 0.3f);
        }
    }

    public void GetItem(Item_Scriptable item)
    {
        if(m_SaveItem.ContainsKey(item.name))
        {
            m_SaveItem[item.name].holder.Count++;
            foreach (var RealItem in m_SaveItem)
            {
                m_Parts[item.name].Init(item.name, RealItem.Value.holder);
            }

            return;
        }
        Item_Holder items = new Item_Holder { Data = item, holder = new Holder() };
        items.holder.Count = 1;
        m_SaveItem.Add(item.name, items);
        var go = Instantiate(item_Part, Content);
        m_Parts.Add(item.name, go);

        foreach (var RealItem in m_SaveItem)
        {
            go.Init(item.name, RealItem.Value.holder);
        }



    }
}
