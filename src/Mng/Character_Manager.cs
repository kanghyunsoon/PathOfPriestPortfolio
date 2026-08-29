using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Manager
{
    public void GetCharacter(int value, string character_Name)
    {
        Base_Manager.Data.m_Set_Character[value] = Base_Manager.Data.m_Data_Character[character_Name];

        Base_Manager.Player.Set_Character_DefaultAbility(true, Base_Manager.Data.m_Set_Character[value].Data);
    }

    public void DisableCharacter(int value)
    {
        // 캐릭터 제거 시 보너스 감소
        Base_Manager.Player.Set_Character_DefaultAbility(false, Base_Manager.Data.m_Set_Character[value].Data);

        Base_Manager.Data.m_Set_Character[value] = null;


    }

    public int LevelCount(Character_Holder holder)
    {
       return (holder.holder.Level + 1) * 5;
    }
}
