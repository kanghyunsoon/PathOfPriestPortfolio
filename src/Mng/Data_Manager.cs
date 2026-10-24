using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 서버에서 관리할 DB를 저장하는 용도
[System.Serializable]
public class Percentage
{
    public Rarity rarity;
    public float Min, Max;
}

public class Smelt_Holder
{
    public Status_Holder holder;
    public Rarity rarity;
    public float Value;
}


public class Character_Holder
{
    public Character_Scriptable Data;
    public Holder holder;
    public bool isDebuff = false;
}

public class Item_Holder
{
    public Item_Scriptable Data;
    public Holder holder;
}

public class Holder
{
    public int Level;
    public int Count;
}
public class Data
{
    public double Money;
    public double ATK;
    public double HP;
    public int Dia;
    public int Level;
    public int UpgradeCount;
    public int GoldDungeon_Count;
    public int DiaDungeon_Count;
    public int Quest_Count;
    public double EXP;
    public int Stage;

    public float[] Buff_Timers = { 0.0f, 0.0f, 0.0f };
    public float Buff_x2 = 0.0f;
    public int BuffLevel, BuffCount;

    //shop
    public int Hero_Summon_Count;
    public int Hero_PickUp_Count;
    public int Relic_Summon_Count;
    public int Relic_PickUp_Count;
    public int Hero_ADS_Count;
    public int Relic_ADS_Count;

    public string StartDate;
    public string EndDate;

    // Dungeon
    public int[] Key = { 2, 2 };
    public int[] KeyAssets = { 0, 0 };

    public int[] Dungeon_Clear_Level = { 0, 0 };

    public bool ADS_Remove = false;

    //DailyQuest
    public int DailyAttendance = 1;
    public int LevelUp;
    public int ADS;
    public int Dungeon;
    public int Summon;
    public int UseDiamond;
    public int MonsterKill;
    public int StayTime;


    public bool[] DailyQuests = new bool[8];


    // Achievement_Data
    public bool[] Achievement_B = new bool[11];

}

public class Data_Manager
{
    public static Data m_Data = new Data();

    public Dictionary<string, Holder> Character_Holder = new Dictionary<string, Holder>();
    public Dictionary<string, Holder> Item_Holder = new Dictionary<string, Holder>();
    public Dictionary<string, Character_Holder> m_Data_Character = new Dictionary<string, Character_Holder>(); // 플레이어가 가지고 있는 케릭터 저장
    public Dictionary<string, Item_Holder> m_Data_Item = new Dictionary<string, Item_Holder>();

    public Dictionary<Status_Holder, float> m_Data_Mastery = new Dictionary<Status_Holder, float>();
    public List<Smelt_Holder> m_Data_Smelt = new List<Smelt_Holder>();
    public Character_Holder[] m_Set_Character = new Character_Holder[6];
    public Item_Scriptable[] m_Set_Item = new Item_Scriptable[6];

    public void Init()
    {
        Set_Character();
        Set_Item();

        if (m_Data.DailyQuests == null || m_Data.DailyQuests.Length != 8)
        {
            Debug.LogWarning("[DataManager] DailyQuests 배열이 잘못되어 다시 초기화합니다.");
            m_Data.DailyQuests = new bool[8];
        }

        // 디버프 적용 (Firebase에서 받은 임시 데이터 사용)
        foreach (var pair in Base_Manager.Firebase._loadedDebuffDict)
        {
            if (m_Data_Character.ContainsKey(pair.Key))
            {
                m_Data_Character[pair.Key].isDebuff = pair.Value;
            }
        }

        CheckDebuffFromOfflineTime(); // 새로 적용

    }



    public Character_Scriptable Get_Rarity_Character(Rarity rarity)
    {
        List<Character_Scriptable> holder = new List<Character_Scriptable>();
        while (true)
        {
            foreach (var data in m_Data_Character)
            {
                if (data.Value.Data.m_Rarity == rarity && data.Value.Data.Main_Character == false)
                {
                    holder.Add(data.Value.Data);
                }
            }

            // 해당 등급의 영웅이 없으면 유물 뽑기와 같이 한 단계 높은 등급에서 찾는다.
            if (holder.Count > 0 || rarity == Rarity.Legendary) break;
            rarity = GetNextAvailableRarity(rarity);
        }

        return holder.Count > 0 ? holder[UnityEngine.Random.Range(0, holder.Count)] : null;
    }

    public Item_Scriptable Get_Rarity_Relic(Rarity rarity)
    {
        List<Item_Scriptable> holder = new List<Item_Scriptable>();
        foreach (var data in m_Data_Item)
        {
            if (data.Value.Data.rarity == rarity && data.Value.Data.type == ItemType.Equipment)
            {
                holder.Add(data.Value.Data);
            }
        }
        while (holder.Count == 0)
        {
            rarity = GetNextAvailableRarity(rarity); // 다음 가능한 레어리티 찾기

            // 다시 찾기
            foreach (var data in m_Data_Item)
            {
                if (data.Value.Data.rarity == rarity && data.Value.Data.type == ItemType.Equipment)
                {
                    holder.Add(data.Value.Data);
                }
            }

            // 모든 레어리티를 검사했음에도 찾을 수 없는 경우, null 반환 방지
            if (rarity == Rarity.Legendary) break;
        }

        return holder.Count > 0 ? holder[UnityEngine.Random.Range(0, holder.Count)] : null;
    }

    private Rarity GetNextAvailableRarity(Rarity currentRarity)
    {
        // 현재 레어리티에서 한 단계 높은 레어리티로 변경
        switch (currentRarity)
        {
            case Rarity.Common: return Rarity.UnCommon;
            case Rarity.UnCommon: return Rarity.Rare;
            case Rarity.Rare: return Rarity.Hero;
            case Rarity.Hero: return Rarity.Legendary;
            default: return Rarity.Legendary; // 최종적으로 Legendary가 최대
        }
    }

    private void Set_Character()
    {
        var datas = Resources.LoadAll<Character_Scriptable>("Scriptable/Character");

        foreach(var data in datas)
        {
            var character = new Character_Holder();

            character.Data = data;

            if (!m_Data_Character.ContainsKey(data.m_CharacterName))
            {
                m_Data_Character.Add(data.m_CharacterName, character);
            }
            else
            {
                // 기존 데이터를 덮어쓰거나 무시
                m_Data_Character[data.m_CharacterName] = character;
            }

            Holder s_holder = new Holder();
            if (Character_Holder.ContainsKey(data.m_CharacterName))
            {
                s_holder = Character_Holder[data.m_CharacterName];
            }
            else
            {
                Character_Holder.Add(data.m_CharacterName, s_holder);
            }

            character.holder = s_holder;
        }
    }

    private void Set_Item()
    {
        var datas = Resources.LoadAll<Item_Scriptable>("Scriptable/Item");

        foreach (var data in datas)
        {
            var item = new Item_Holder();

            item.Data = data;

            Holder s_holder = new Holder();
            if (Item_Holder.ContainsKey(data.name))
            {
                s_holder = Item_Holder[data.name];
            }
            else
            {
                Item_Holder.Add(data.name, s_holder);
            }

            item.holder = s_holder;

            if (!m_Data_Item.ContainsKey(data.name))
            {
                m_Data_Item.Add(data.name, item);
            }
            else
            {
                m_Data_Item[data.name] = item; // 덮어쓰기
            }

        }
    }

    public float valueSmelt(Status_Holder status)
    {
        float value = 0.0f;
        for(int i =0; i<m_Data_Smelt.Count; i++)
        {
            if(m_Data_Smelt[i].holder == status)
            {
                value += m_Data_Smelt[i].Value;
            }
        }
        return value;
    }

    public float ValueMastery(Status_Holder status)
    {
        if (m_Data_Mastery.TryGetValue(status, out float value))
        {
            return value;
        }
        return 0.0f;
    }

    public Dictionary<string, Character_Holder> GetOwnedCharacters()
    {
        Dictionary<string, Character_Holder> owned = new Dictionary<string, Character_Holder>();

        foreach (var character in m_Data_Character)
        {
            if (character.Value.holder.Count > 0)
            {
                owned.Add(character.Key, character.Value);
            }
        }

        return owned;
    }

    public Dictionary<string, Item_Holder> GetOwnedItems()
    {
        Dictionary<string, Item_Holder> owned = new Dictionary<string, Item_Holder>();

        foreach (var item in m_Data_Item)
        {
            if (item.Value.holder.Count > 0)
            {
                owned.Add(item.Key, item.Value);
            }
        }

        return owned;
    }

    public void CheckDebuffFromOfflineTime()
    {
        TimeSpan offlineDuration = TimeSpan.FromSeconds(Utils.TimerCheck());
        int debuffCountToApply = (int)(offlineDuration.TotalHours / 12);

        if (debuffCountToApply <= 0) return;

        var owned = GetOwnedCharacters();
        List<string> eligible = new List<string>();

        foreach (var pair in owned)
        {
            var chName = pair.Key;
            var chHolder = pair.Value;
            if (!chHolder.isDebuff)
            {
                eligible.Add(chName);
            }
        }

        if (eligible.Count == 0) return;


        int appliedCount = 0;
        for (int i = 0; i < debuffCountToApply && eligible.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, eligible.Count);
            string chosenName = eligible[index];

            if (m_Data_Character.ContainsKey(chosenName))
            {
                m_Data_Character[chosenName].isDebuff = true;
                eligible.RemoveAt(index);
                appliedCount++;
            }
        }
    }


}
