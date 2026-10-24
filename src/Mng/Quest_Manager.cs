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
        NormalizeAchievementFlags();
        LoadAchievement_Data();
    }

    // 저장된 업적 배열 길이가 업적 목록과 다르면 기존 값을 유지한 채 맞춘다.
    // 길이가 더 길면 LoadAchievement_Data가 목록 범위를 넘어 불러오기 도중 예외가 났다.
    private void NormalizeAchievementFlags()
    {
        bool[] old = Data_Manager.m_Data.Achievement_B;
        if (old != null && old.Length == Achievement_Lists.Count) return;

        bool[] resized = new bool[Achievement_Lists.Count];
        if (old != null)
        {
            Array.Copy(old, resized, Math.Min(old.Length, resized.Length));
        }
        Data_Manager.m_Data.Achievement_B = resized;
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
        // Init은 데이터를 불러올 때마다 호출되므로 이전 목록을 비우고 다시 만든다.
        activeQuests.Clear();
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
