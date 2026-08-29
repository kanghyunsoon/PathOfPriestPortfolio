using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Dungeon : UI_Base
{
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI[] KeyTexts;
    [SerializeField] private TextMeshProUGUI[] KeyHolders;
    [SerializeField] private TextMeshProUGUI[] ClearAssets;
    [SerializeField] private TextMeshProUGUI[] Levels;

    [SerializeField] private Button[] Key01ArrowButton, Key02ArrowButton;
    int[] Level = new int[2];
    public override bool Init()
    {
        Main_UI.instance.FadeInOut(true, true, null);
        Render_Manager.instance.DUNGEON.init();
        for(int i =0; i<KeyTexts.Length; i++)
        {
            KeyTexts[i].text = "(" + Data_Manager.m_Data.Key[i].ToString() + "/2)";
            KeyHolders[i].color = (Data_Manager.m_Data.Key[i] + Data_Manager.m_Data.KeyAssets[i]) <= 0 ? Color.red : Color.green;
            Levels[i].text = (Data_Manager.m_Data.Dungeon_Clear_Level[i] + 1).ToString();
            Level[i] = Data_Manager.m_Data.Dungeon_Clear_Level[i];
        }
        int levelCount = ((Data_Manager.m_Data.Dungeon_Clear_Level[1] + 1) * 5);
        var value = Utils.CalculatedValue(Utils.Data.stageData.B_MONEY, levelCount, Utils.Data.stageData.M_MONEY);

        ClearAssets[0].text = ((Data_Manager.m_Data.Dungeon_Clear_Level[0] + 1) * 50).ToString();
        ClearAssets[1].text = StringMethod.ToCurrencyString(value);

        Key01ArrowButton[0].onClick.AddListener(() => ArrowButton(0, -1));
        Key01ArrowButton[1].onClick.AddListener(() => ArrowButton(0, 1));
        Key02ArrowButton[0].onClick.AddListener(() => ArrowButton(1, -1));
        Key02ArrowButton[1].onClick.AddListener(() => ArrowButton(1, 1));

        return base.Init();
    }

    private void Update()
    {
        TimerText.text = Utils.NextDayTimer();
    }

    public void GetDungeon(int value)
    {
        if(Data_Manager.m_Data.Key[value] + Data_Manager.m_Data.KeyAssets[value] <= 0)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["Dungeon_Need"].Get_Data(), Color.red);
            return;
        }
        Stage_Manager.DungeonLevel = Level[value];
        Stage_Manager.State_Change(Stage_State.Dungeon, value);

        Data_Manager.m_Data.Dungeon++; //  추후 던전 클리어 시로 변경할지 생각

        Utils.CloseAllPopupUI();
    }

    public void ArrowButton(int keyValue, int value)
    {
        Level[keyValue] += value;
        if(Level[keyValue] <= 0)
        {
            Level[keyValue] = 0;
        }
        else if(Level[keyValue] >= Data_Manager.m_Data.Dungeon_Clear_Level[keyValue])
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["Clear_CurrentLevel"].Get_Data(), Color.white);
            Level[keyValue] = Data_Manager.m_Data.Dungeon_Clear_Level[keyValue];
        }

        Levels[keyValue].text = (Level[keyValue]+1).ToString();
    }

    public override void DisableOBJ()
    {
        Main_UI.instance.LayerCheck(-1);
        for(int i =0; i< Render_Manager.instance.DUNGEON.characters.Length; i++)
        {
            Render_Manager.instance.DUNGEON.characters[i].DisableCoroutine();
        }
        base.DisableOBJ();
    }

}
