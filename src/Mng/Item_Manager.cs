using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Item_Manager
{
    public bool SetItemCheck(string name)
    {
        for(int i=0; i<Base_Manager.Data.m_Set_Item.Length; i++)
        {
            if(Base_Manager.Data.m_Set_Item[i] != null)
            {
                if (Base_Manager.Data.m_Set_Item[i].name == name)
                {

                    return true;
                }
            }
        }
        return false;
    }

    public void GetItem(int value, string item_name)
    {

        Base_Manager.Data.m_Set_Item[value] = Base_Manager.Data.m_Data_Item[item_name].Data;
        Base_Manager.Player.Set_Relic_DefaultAbility(true, Base_Manager.Data.m_Set_Item[value]);
    }

    public void DisableItem(int value)
    {
        var item = Base_Manager.Data.m_Set_Item[value];
        Base_Manager.Data.m_Set_Item[value] = null;
        Base_Manager.Player.Set_Relic_DefaultAbility(false, item);
    }

    public int LevelCount(Item_Holder holder)
    {
        return (holder.holder.Level + 1) * 5;
    }

    public List<Item_Scriptable> GetDropSet()
    {
        List<Item_Scriptable> objs = new List<Item_Scriptable>();

        foreach (var data in Base_Manager.Data.m_Data_Item)
        {
            if(data.Value.Data.MinLevel <= Data_Manager.m_Data.Stage)
            {
                float valueCount = Random.Range(0.0f, 100.0f);
                if (valueCount <= data.Value.Data.Item_Chance)
                {
                    objs.Add(data.Value.Data);
                }
            }
        }
        return objs;
    }


}
