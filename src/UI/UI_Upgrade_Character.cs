using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Upgrade_Character : UI_Base
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
        var data = Base_Manager.Data.m_Data_Character;

        foreach (var c in data)
        {
            if (Can_LevelUp(c.Value))
            {
                var go = Instantiate(Panel, Content);
                go.gameObject.SetActive(true);
                go.GetComponent<Image>().sprite = Utils.Get_Atlas(c.Value.Data.m_Rarity.ToString());
                int before = c.Value.holder.Level + 1;
                int value = 0;
                UpgradeLevel(c.Value, ref value);

                go.Init(before, value + 1, c.Value );
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }

        var heros = parentBase.GetComponent<UI_Heros>();
        for (int i = 0; i < heros.part.Count; i++) heros.part[i].Init();
        Base_Manager.Firebase.WriteData();
    }

    private void UpgradeLevel(Character_Holder holder, ref int value)
    {

        while (holder.holder.Count >= Base_Manager.Character.LevelCount(holder))
        {
            holder.holder.Count -= Base_Manager.Character.LevelCount(holder);
            holder.holder.Level++;
        }
        value = holder.holder.Level;
    }

    private bool Can_LevelUp(Character_Holder holder)
    {
        if(holder.holder.Count >= Base_Manager.Character.LevelCount(holder))
        {
            return true;
        }
        return false;
    }
}
