using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Upgrade_Item : UI_Base
{
    [SerializeField] private Upgrade_Part Panel;
    [SerializeField] private Transform Content;

    UI_Base parentBase;
    public void Initialize(UI_Base parent)
    {
        parentBase = parent;
        StartCoroutine(UpgradeCoroutine());
    }

    IEnumerator UpgradeCoroutine()
    {
        var data = Base_Manager.Data.m_Data_Item;

        foreach (var c in data)
        {
            if (Can_LevelUp(Base_Manager.Data.Item_Holder[c.Value.Data.name]))
            {
                if(c.Value.Data.type == ItemType.Equipment)
                {
                    var go = Instantiate(Panel, Content);
                    go.GetComponent<Image>().sprite = Utils.Get_Atlas(c.Value.Data.rarity.ToString());
                    go.gameObject.SetActive(true);


                    int before = Base_Manager.Data.Item_Holder[c.Value.Data.name].Level + 1;
                    int value = 0;
                    UpgradeLevel(Base_Manager.Data.Item_Holder[c.Value.Data.name], ref value);

                    go.Init(before, value + 1, null, c.Value);
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        var relic = parentBase.GetComponent<UI_Relic>();
        for (int i = 0; i < relic.part.Count; i++) relic.part[i].Init();
        Base_Manager.Firebase.WriteData();
    }

    private void UpgradeLevel(Holder holder, ref int value)
    {
        int realValue = (holder.Level + 1) * 5;
        while (holder.Count >= realValue)
        {
            holder.Count -= realValue;
            holder.Level++;
        }
        value = holder.Level;
    }

    private bool Can_LevelUp(Holder holder)
    {
        int realValue = (holder.Level + 1) * 5;
        if (holder.Count >= realValue)
        {
            return true;
        }
        return false;
    }
}
