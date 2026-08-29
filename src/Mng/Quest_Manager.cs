using UnityEngine;
using System.Collections.Generic;
using System;



public class DailyQuest
{
    public DailyQuest_Type Type;
    public string Quest_Title;
    public string Quest_Description;
    public int Goal;
    public int CurrentProgress;
    public string Reward;
    public int RewardCount;

    public bool isCompleted => CurrentProgress >= Goal;

    public DailyQuest(DailyQuest_Type type, string quest_Title, string quest_Description, int goal, string reward, int rewardCount)
    {
        Type = type;
        Quest_Title = quest_Title;
        Quest_Description = quest_Description;
        Goal = goal;
        CurrentProgress = 0;
        Reward = reward;
        RewardCount = rewardCount;
    }

    public void UpdateProgress(int value)
    {
        if (!isCompleted)
        {
            CurrentProgress += value;
            if (CurrentProgress >= Goal)
                CurrentProgress = Goal;
        }
    }
}


public class Achievement_Status
{
    public float ATK;
    public float HP;
    public float Money;
    public float Item;
    public float Skill;
    public float ATK_Speed;
    public float Critical_P;
    public float Critical_D;

    public void GetStatusData(Status_Holder holder, float value)
    {
        switch(holder)
        {
            case Status_Holder.ATK: ATK += value; break;
            case Status_Holder.HP: HP += value; break;
            case Status_Holder.Money: Money += value; break;
            case Status_Holder.Item: Item += value; break;
            case Status_Holder.Skill: Skill += value; break;
            case Status_Holder.ATK_Speed: ATK_Speed += value; break;
            case Status_Holder.Critical_P: Critical_P += value; break;
            case Status_Holder.Critical_D: Critical_D += value; break;
        }
    }
}

public class Quest_Manager
{
    List<Dictionary<string, object>> questData;
    public List<DailyQuest> activeQuests = new List<DailyQuest>();

    public Achievement_Scriptable scriptavle_Data;
    public List<Achievement> Achievement_Lists = new List<Achievement>();
    public Achievement_Status Achievement_Status_Data = new Achievement_Status();

    public void Init()
    {
        LoadQuests();
        scriptavle_Data = Resources.Load<Achievement_Scriptable>("Scriptable/Achievement_Data");
        Achievement_Lists = scriptavle_Data.Achievement_Data;
        LoadAchievement_Data();
    }

    public void LoadAchievement_Data()
    {
        Achievement_Status achievementStatus = new Achievement_Status();
        for(int i =0; i< Data_Manager.m_Data.Achievement_B.Length; i++)
        {
            if(Data_Manager.m_Data.Achievement_B[i] == true)
            {
                achievementStatus.GetStatusData(Achievement_Lists[i].GetRewardStatus, Achievement_Lists[i].RewardValue);
            }
        }
        Achievement_Status_Data = achievementStatus;
    }

    public void LoadQuests()
    {
        questData = new List<Dictionary<string, object>>(CSV_Importer.DailyQuest);
        for(int i =0; i< questData.Count; i++)
        {
            var data = questData[i];
            DailyQuest_Type type = (DailyQuest_Type)Enum.Parse(typeof(DailyQuest_Type), data["Type"].ToString());
            string questId = Local_Manager.local_Data[data["Title"].ToString()].Get_Data() ;
            string description = Local_Manager.local_Data[data["Description"].ToString()].Get_Data();
            int goal = int.Parse(data["Count"].ToString());
            string reward = data["Reward"].ToString();
            int rewardCount = int.Parse(data["Reward_Value"].ToString());
            DailyQuest quest = new DailyQuest(type, questId, description, goal, reward, rewardCount);
            activeQuests.Add(quest);
        }

    }
}
