using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Manager
{
    private float totalGoldBonus = 0.0f;
    private float totalItemBonus = 0.0f;
    private float totalCritBonus = 0.0f;
    private float totalAtkBonus = 0.0f;
    private float totalHpBonus = 0.0f;
    private float totalAtkSpeedBonus = 0.0f;
    private float totalCritDmgBonus = 0.0f;





    public void EXP_UP()
    {
        double LevelUpMoneyValue = Utils.CalculatedValue(
         Utils.Data.levelData.B_MONEY,
         Data_Manager.m_Data.Level,
         Utils.Data.levelData.C_MONEY);

        if (Data_Manager.m_Data.Money < LevelUpMoneyValue)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NeedGold"].Get_Data(), Color.red);
            return;
        }

        Data_Manager.m_Data.Money -= LevelUpMoneyValue; // 골드 차감

        Data_Manager.m_Data.EXP += Utils.Data.levelData.EXP();
        Data_Manager.m_Data.UpgradeCount++;

        if (Data_Manager.m_Data.EXP >= Utils.Data.levelData.MAX_EXP())
        {
            Data_Manager.m_Data.LevelUp++;
            Data_Manager.m_Data.Level++;
            Base_Manager.GPGS.SaveScore(Data_Manager.m_Data.Level);

            Data_Manager.m_Data.EXP = 0;

            Data_Manager.m_Data.ATK += Utils.Data.levelData.ATK();
            Data_Manager.m_Data.HP += Utils.Data.levelData.HP();
            for (int i = 0; i < Spawner.m_Players.Count; i++) Spawner.m_Players[i].Set_ATKHP();
        }
        Main_UI.instance.TextMoney();
    }

    public float EXP_Percentage()
    {
        float exp = (float)Utils.Data.levelData.MAX_EXP();
        double myEXP = Data_Manager.m_Data.EXP;

        return (float)myEXP / exp;
    }

    public float Next_EXP()
    {
        float exp = (float)Utils.Data.levelData.MAX_EXP();
        float myEXP = (float)Utils.Data.levelData.EXP();

        return (myEXP / exp) * 100.0f;
    }


    public double Get_ATK(Rarity rarity, Character_Holder holder)
    {
        var Damage = Data_Manager.m_Data.ATK * ((int)rarity + 1);
        float Level = ((holder.holder.Level + 1.0f) * 10.0f) / 100.0f;
        var RealDamage = Damage + (Damage * Level);

        float smeltBonus = Base_Manager.Data.valueSmelt(Status_Holder.ATK);
        float MasteryBonus = Base_Manager.Data.ValueMastery(Status_Holder.ATK);
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.ATK;

        float totalBonus = smeltBonus + totalAtkBonus + achievementBonus + MasteryBonus;

        RealDamage += RealDamage * (totalBonus / 100.0f);

        // ATK 버프 적용 (버튼 1)
        if (Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.ATK] > 0)
        {
            RealDamage *= 1.2f;
        }

        if (holder.isDebuff)
        {
            RealDamage *= 0.5f;
        }

        return RealDamage;
    }

    public double Get_HP(Rarity rarity, Character_Holder holder)
    {
        var hp = Data_Manager.m_Data.HP * ((int)rarity + 1);
        float Level = ((holder.holder.Level + 1.0f) * 10.0f) / 100.0f;
        var RealHP = hp + (hp * Level);

        float smeltBonus = Base_Manager.Data.valueSmelt(Status_Holder.HP);
        float MasteryBonus = Base_Manager.Data.ValueMastery(Status_Holder.HP);
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.HP;

        float totalBonus = smeltBonus + totalAtkBonus + achievementBonus + MasteryBonus;

        RealHP += RealHP * (totalBonus / 100.0f);

        // HP 버프 적용 (버튼 1)
        if (Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.ATK] > 0)
        {
            RealHP *= 1.25;
        }

        if(holder.isDebuff)
        {
            RealHP *= 0.5f;
        }

        return RealHP;
    }

    public double Main_ATK()
    {
        double atk = Get_ATK(Rarity.Common,Base_Manager.Data.m_Data_Character["Cleric"]);

        int value = 1;
        for (int i =0; i<Base_Manager.Data.m_Set_Character.Length; i++)
        {
            if(Base_Manager.Data.m_Set_Character[i] != null)
            {
                var data = Base_Manager.Data.m_Set_Character[i].Data;

                atk += Get_ATK(data.m_Rarity, Base_Manager.Data.m_Data_Character[data.name]);
                value++;
            }
        }
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.ATK;
        return (atk / value) * (1.0 + (totalAtkBonus + achievementBonus) / 100.0);
    }

    public double Main_HP()
    {
        double hp = Get_HP(Rarity.Common, Base_Manager.Data.m_Data_Character["Cleric"]);
        int value = 1;
        for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
        {
            if (Base_Manager.Data.m_Set_Character[i] != null)
            {
                var data = Base_Manager.Data.m_Set_Character[i].Data;

                hp += Get_HP(data.m_Rarity, Base_Manager.Data.m_Data_Character[data.name]);
                value++;
            }
        }
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.HP;
        return (hp / value) * (1.0 + (totalHpBonus + achievementBonus) / 100.0);
    }

    public float GoldDropPercentage()
    {
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.Money;
        float buffValue = Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.Gold] > 0 ? 20.0f : 0.0f;
        float basePercentage = buffValue + Base_Manager.Data.valueSmelt(Status_Holder.Money) + totalGoldBonus + achievementBonus + Base_Manager.Data.ValueMastery(Status_Holder.Money);

        if (Base_Manager.isNight)
            return basePercentage * 1.5f;
        else
            return basePercentage;
    }

    public float ItemDropPercentage()
    {
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.Item;
        float buffValue = Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.Gold] > 0 ? 10.0f : 0.0f;

        float basePercentage = 100.0f + buffValue + Base_Manager.Data.valueSmelt(Status_Holder.Item) + totalItemBonus + achievementBonus + Base_Manager.Data.ValueMastery(Status_Holder.Item);

        if (Base_Manager.isNight)
            return basePercentage * 1.5f;
        else
            return basePercentage;
    }

    public float CriticalPercentage()
    {
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.Critical_P;
        float buffValue = Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.Critical] > 0 ? 10.0f : 0.0f;
        return 20.0f + Base_Manager.Data.valueSmelt(Status_Holder.Critical_P) + buffValue + totalCritBonus + achievementBonus + Base_Manager.Data.ValueMastery(Status_Holder.Critical_P);
    }

    public float ATKSpeed()
    {
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.ATK_Speed;
        float last = (Base_Manager.Data.valueSmelt(Status_Holder.ATK_Speed) + totalAtkSpeedBonus + achievementBonus + Base_Manager.Data.ValueMastery(Status_Holder.ATK_Speed)) / 100f;
        return 1.0f + last;
    }

    public float CriticalDamage()
    {
        float achievementBonus = Base_Manager.Quest.Achievement_Status_Data.Critical_D;
        float buffValue = Data_Manager.m_Data.Buff_Timers[(int)UI_ADS_Buff.ADS_Buff_State.Critical] > 0 ? 15.0f : 0.0f;
        return 140.0f + Base_Manager.Data.valueSmelt(Status_Holder.Critical_D) + buffValue + totalCritDmgBonus + achievementBonus + Base_Manager.Data.ValueMastery(Status_Holder.Critical_D);
    }

    public double Average_ATK()
    {
        double ATK = Main_ATK() + Main_HP();
        return ATK;
    }

    public void UpdateStats()
    {
        Main_ATK();
        Main_HP();
        GoldDropPercentage();
        ItemDropPercentage();
        CriticalPercentage();
        CriticalDamage();
        ATKSpeed();

        for (int i = 0; i < Spawner.m_Players.Count; i++)
        {
            Spawner.m_Players[i].Set_ATKHP();
        }
    }

    public void Set_Character_DefaultAbility(bool isAdding, Character_Scriptable characterHolder)
    {
        if (characterHolder == null) return;

        var character = characterHolder;
        float modifier = isAdding ? 1.0f : -1.0f; // 추가될 때는 1.0f, 제거될 때는 -1.0f



        switch (character.m_CharacterName)
        {
            case "Babarian":
                totalAtkBonus += 10.0f * modifier; // 공격력 +10%
                break;

            case "Berserker":
                totalCritBonus += 15.0f * modifier;
                totalHpBonus += 15.0f * modifier;
                break;

            case "Hunter":
                totalAtkSpeedBonus += 10.0f * modifier;
                break;

            case "Spearman":
                totalCritDmgBonus += 8.0f * modifier; // 치명타 데미지 +10%
                break;

            case "Elementalist_W":
                totalGoldBonus += 30.0f * modifier; // 골드 드롭률 +30%
                break;

            case "Elementalist_B":
                totalItemBonus += 15.0f * modifier;
                break;

            case "Paladin":
                totalHpBonus += 25.0f * modifier;
                break;

            case "Rogue":
                totalCritDmgBonus += 15.0f * modifier;
                break;

            case "Elf_Archer":
                totalAtkSpeedBonus += 15.0f * modifier;
                break;

            case "Kunoichi":
                totalAtkSpeedBonus += 10.0f * modifier;
                totalAtkBonus += 13.0f * modifier;
                break;


            default:
                break;
        }
        UpdateStats();
    }

    public void Set_Relic_DefaultAbility(bool isAdding, Item_Scriptable Item)
    {
        if (Item == null) return;

        float modifier = isAdding ? 1.0f : -1.0f; // 추가될 때는 1.0f, 제거될 때는 -1.0f

        switch (Item.Item_Name)
        {
            case "Axe":
                totalAtkBonus += 15.0f * modifier;
                break;

            case "Mana":
                totalHpBonus += 10.0f * modifier;
                break;

            case "Dice":
                totalItemBonus += 15.0f * modifier;
                break;

            default:
                break;
        }
        UpdateStats();
    }

}
