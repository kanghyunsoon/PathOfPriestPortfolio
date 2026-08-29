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
        if(TypeCount(m_Quest.Type) < m_Quest.Goal)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["QuestNotCompleted"].Get_Data(), Color.red);
            return;
        }

        Base_Canvas.instance.Get_UI("#Reward");
        Utils.UI_Holder.Peek().GetComponent<UI_Reward>().GetRewardInit(m_Quest.Reward, m_Quest.RewardCount);

        switch (m_Quest.Type)
        {
            case DailyQuest_Type.DailyAttendance: Data_Manager.m_Data.DailyQuests[0] = true; break;
            case DailyQuest_Type.LevelUp: Data_Manager.m_Data.DailyQuests[1] = true; break;
            case DailyQuest_Type.Summon: Data_Manager.m_Data.DailyQuests[2] = true; break;
            case DailyQuest_Type.Dungeon: Data_Manager.m_Data.DailyQuests[3] = true; break;
            case DailyQuest_Type.ADS: Data_Manager.m_Data.DailyQuests[4] = true; break;
            case DailyQuest_Type.UseDiamond: Data_Manager.m_Data.DailyQuests[5] = true; break;
            case DailyQuest_Type.MonsterKill: Data_Manager.m_Data.DailyQuests[6] = true; break;
            case DailyQuest_Type.StayTime: Data_Manager.m_Data.DailyQuests[7] = true; break;
        }

        parent.Init();
    }

    private void CheckInit(DailyQuest_Type type)
    {
        bool GetCollected = false;
        switch (type)
        {
            case DailyQuest_Type.DailyAttendance: GetCollected = Data_Manager.m_Data.DailyQuests[0]; break;
            case DailyQuest_Type.LevelUp: GetCollected = Data_Manager.m_Data.DailyQuests[1]; break;
            case DailyQuest_Type.Summon: GetCollected = Data_Manager.m_Data.DailyQuests[2]; break;
            case DailyQuest_Type.Dungeon: GetCollected = Data_Manager.m_Data.DailyQuests[3]; break;
            case DailyQuest_Type.ADS: GetCollected = Data_Manager.m_Data.DailyQuests[4]; break;
            case DailyQuest_Type.UseDiamond: GetCollected = Data_Manager.m_Data.DailyQuests[5]; break;
            case DailyQuest_Type.MonsterKill: GetCollected = Data_Manager.m_Data.DailyQuests[6]; break;
            case DailyQuest_Type.StayTime: GetCollected = Data_Manager.m_Data.DailyQuests[7]; break;
        }
        CollectedPanel.SetActive(GetCollected);
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
