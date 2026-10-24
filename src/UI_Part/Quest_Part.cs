using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Quest_Part : MonoBehaviour
{
    public TextMeshProUGUI Title, Description, Count, RewardValue;
    public Image RewardImage, SliderFill;
    DailyQuest m_Quest;
    public GameObject CollectedPanel;
    UI_DailyQuest parent;

    public void Init(DailyQuest quest, UI_DailyQuest parentData)
    {
        parent = parentData;
        CheckInit(quest.Type);
        m_Quest = quest;
        Title.text = quest.Quest_Title;
        Description.text = quest.Quest_Description;
        RewardValue.text = quest.RewardCount.ToString();
        RewardImage.sprite = Utils.Get_Atlas(quest.Reward);
        SliderFill.fillAmount = (float)TypeCount(quest.Type) / (float)quest.Goal;
        Count.text = string.Format("({0}/{1})", TypeCount(quest.Type), quest.Goal);
    }

    public void GetReward()
    {
        if (IsCollected)
        {
            return;
        }

        if(TypeCount(m_Quest.Type) < m_Quest.Goal)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["QuestNotCompleted"].Get_Data(), Color.red);
            return;
        }

        Base_Canvas.instance.Get_UI("#Reward");
        Utils.UI_Holder.Peek().GetComponent<UI_Reward>().GetRewardInit(m_Quest.Reward, m_Quest.RewardCount);

        int index = SaveIndex(m_Quest.Type);
        if (index >= 0) Data_Manager.m_Data.DailyQuests[index] = true;

        parent.Init();
    }

    public bool IsCollected
    {
        get
        {
            int index = m_Quest != null ? SaveIndex(m_Quest.Type) : -1;
            return index >= 0 && Data_Manager.m_Data.DailyQuests[index];
        }
    }

    // 저장 데이터의 DailyQuests 순서다. Dungeon과 ADS는 enum 순서와 반대로 저장되어 있어 그대로 유지한다.
    private static int SaveIndex(DailyQuest_Type type)
    {
        switch (type)
        {
            case DailyQuest_Type.DailyAttendance: return 0;
            case DailyQuest_Type.LevelUp: return 1;
            case DailyQuest_Type.Summon: return 2;
            case DailyQuest_Type.Dungeon: return 3;
            case DailyQuest_Type.ADS: return 4;
            case DailyQuest_Type.UseDiamond: return 5;
            case DailyQuest_Type.MonsterKill: return 6;
            case DailyQuest_Type.StayTime: return 7;
        }
        return -1;
    }

    private void CheckInit(DailyQuest_Type type)
    {
        int index = SaveIndex(type);
        CollectedPanel.SetActive(index >= 0 && Data_Manager.m_Data.DailyQuests[index]);
    }



    public int TypeCount(DailyQuest_Type type)
    {
        switch(type)
        {
            case DailyQuest_Type.DailyAttendance: return Data_Manager.m_Data.DailyAttendance;
            case DailyQuest_Type.LevelUp: return Data_Manager.m_Data.LevelUp;
            case DailyQuest_Type.Summon: return Data_Manager.m_Data.Summon;
            case DailyQuest_Type.Dungeon: return Data_Manager.m_Data.Dungeon;
            case DailyQuest_Type.ADS: return Data_Manager.m_Data.ADS;
            case DailyQuest_Type.UseDiamond: return Data_Manager.m_Data.UseDiamond;
            case DailyQuest_Type.MonsterKill: return Data_Manager.m_Data.MonsterKill;
            case DailyQuest_Type.StayTime: return Data_Manager.m_Data.StayTime;
        }
        return -1;
    }
}
