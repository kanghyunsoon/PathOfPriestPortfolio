using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

public class Utils
{

    public static SpriteAtlas m_atlas = Resources.Load<SpriteAtlas>("Atlas");


    public static int[] summon_level = { 11, 22, 33, 44, 55, 66, 77, 88, 99 };
    // Queue = FIFO(First In First Out), Stack = LIFO(Last In First Out)

    public static Stack<UI_Base> UI_Holder = new Stack<UI_Base>();
    public static LevelDesign Data = Resources.Load<LevelDesign>("Scriptable/LevelDesignData");

   public static void CloseAllPopupUI()
    {
        while (UI_Holder.Count > 0) ClosePopupUI();
    }

    public static void ClosePopupUI()
    {
        if (UI_Holder.Count == 0) return;

        // push = 삽입, peek = 스택에 마지막 들어온 값을 반환, pop = 스택에 마지막 들어온 값을 반환 후 제외

        UI_Base popup = UI_Holder.Peek();
        popup.DisableOBJ();
    }

    public static Sprite Get_Atlas(string temp)
    {
        return m_atlas.GetSprite(temp);
    }


    public static string String_Color_Rarity(Rarity rare)
    {
        switch(rare)
        {
            case Rarity.Common: return "<color=#FFFFFF>";
            case Rarity.UnCommon: return "<color=#1EF118>";
            case Rarity.Hero: return "<color=#FD23CD>";
            case Rarity.Rare: return "<color=#FFB810>";
            case Rarity.Legendary: return "<color=#E00808>";
        }

        return "<color=#FFFFFF>";
    }

    public static string GetTimer(float time)
    {
        TimeSpan timespan = TimeSpan.FromSeconds(time);
        string timer = string.Format("{0:00}:{1:00}", timespan.Minutes, timespan.Seconds);

        return timer;
    }

    // 지수증가 공식. // Mathf.Pow  = 거듭제곱
    // 값이 일정한 비율로 지속적으로 증가한다
    public static double CalculatedValue(float baseValue, int Level, float value)
    {
        return baseValue * Mathf.Pow(Level + 1, value);
    }

    public static bool Coin_Check(double value)
    {
        if(Data_Manager.m_Data.Money >= value)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static float[] Gacha_Percentage()
    {
        int value = Summon_Level(Data_Manager.m_Data.Hero_Summon_Count);

        float[] valueCount = new float[5];
        for(int i =0; i<valueCount.Length; i++)
        {
            string rarityName = ((Rarity)i).ToString();
            valueCount[i] = float.Parse(CSV_Importer.Summon[value][rarityName].ToString());
        }
        return valueCount;
    }

    public static float[] Gacha_RelicPercentage()
    {
        int value = Summon_Level(Data_Manager.m_Data.Relic_Summon_Count);

        float[] valueCount = new float[5];
        for (int i = 0; i < valueCount.Length; i++)
        {
            string rarityName = ((Rarity)i).ToString();
            valueCount[i] = float.Parse(CSV_Importer.Summon[value][rarityName].ToString());
        }
        return valueCount;
    }

    public static int Summon_Level(int count)
    {
        if(count >= summon_level[8])
        {
            return 9;
        }
        for(int i = 0; i < summon_level.Length; i++)
        {
            if(count <summon_level[i])
            {
                return i;
            }
        }
        return -1;
    }

    // 접속할 때 읽은 마지막 종료 시각(UTC).
    // EndDate는 자동 저장 때마다 현재 시각으로 바뀌기 때문에 오프라인 시간 계산에는 이 값을 쓴다.
    private static DateTime? lastSessionEndUtc;
    private static double? offlineSeconds;

    public static void SetLastSessionEnd(DateTime? utc)
    {
        lastSessionEndUtc = utc;
        offlineSeconds = null;
    }

    // 오프라인 경과 시간(초). 처음 계산한 값을 보상을 받을 때까지 유지한다.
    public static double TimerCheck()
    {
        if (offlineSeconds == null)
        {
            if (lastSessionEndUtc == null || !Timer_NTP.IsSynced)
            {
                return 0.0d;
            }

            double seconds = (Timer_NTP.UtcNow - lastSessionEndUtc.Value).TotalSeconds;
            offlineSeconds = Math.Max(0.0d, seconds); // 기기 시간대 변경 등으로 음수가 나오면 0
        }

        return offlineSeconds.Value;
    }

    public static void ConsumeOfflineTime()
    {
        lastSessionEndUtc = null;
        offlineSeconds = 0.0d;
    }

    public static string NextDayTimer()
    {
        DateTime NowDate = Timer_NTP.NowTime;
        DateTime NextDate = NowDate.AddDays(1);
        NextDate = new DateTime(NextDate.Year, NextDate.Month, NextDate.Day, 0, 0, 0);

        TimeSpan timer = NextDate - NowDate;
        return string.Format("{0:00}:{1:00}:{2:00}", timer.Hours, timer.Minutes, timer.Seconds);
    }

    public static bool Item_Count(string itemName, int value)
    {
        if (Base_Manager.Data.Item_Holder[itemName].Count >= value) return true;
        else return false;
    }
}
