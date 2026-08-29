using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UI_Offline : UI_Base
{
    double moneyValue;
    [SerializeField] private TextMeshProUGUI moneyCount;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private Transform Content;
    [SerializeField] private UI_Inventory_Part Item;
    Dictionary<string, Item_Holder> Items = new Dictionary<string, Item_Holder>();

    public override bool Init()
    {
        double offlineTime = Mathf.Min((float)Utils.TimerCheck(), 7200f);

        moneyValue = (Utils.Data.stageData.MONEY() * offlineTime) / 10.0f;
        moneyCount.text = StringMethod.ToCurrencyString(moneyValue);

        TimeSpan span = TimeSpan.FromSeconds(offlineTime);
        TimerText.text = span.Hours + " " + Local_Manager.local_Data["Hour"].Get_Data() + " " + span.Minutes + " " + Local_Manager.local_Data["Minute"].Get_Data();

        Item_Collect(offlineTime);

        foreach (var item in Items)
        {
            var go = Instantiate(Item, Content);
            go.Init(item.Key, item.Value.holder);
        }

        return base.Init();
    }

    private void Item_Collect(double offlineTime)
    {
        int value = Mathf.Min((int)(offlineTime / 600), 12);

        for (int i = 0; i < value; i++)
        {
            var GetItem = Base_Manager.Item.GetDropSet();

            for (int j = 0; j < GetItem.Count; j++)
            {
                if (Items.ContainsKey(GetItem[j].name))
                {
                    Items[GetItem[j].name].holder.Count++;
                }
                else
                {
                    var ItemData = new Item_Holder();
                    ItemData.Data = GetItem[j];
                    ItemData.holder = new Holder();
                    ItemData.holder.Count = 1;
                    Items.Add(GetItem[j].name, ItemData);
                }
            }
        }
    }

    public void CollectButton()
    {
        Data_Manager.m_Data.Money += moneyValue;
        foreach (var item in Items)
        {
            Base_Manager.Inventory.GetItem(item.Value.Data, item.Value.holder.Count);
        }
        Main_UI.instance.TextCheck();
        DisableOBJ();
    }

    public void ADSCollectButton()
    {
        Base_Manager.ADS.ShowRewardedAds(() =>
        {
            Data_Manager.m_Data.Money += (moneyValue * 2);

            foreach (var item in Items)
            {
                Base_Manager.Inventory.GetItem(item.Value.Data, item.Value.holder.Count * 2);
            }
            Main_UI.instance.TextCheck();
            DisableOBJ();
        });

    }

    public override void DisableOBJ()
    {

        Base_Manager.Pool.Pooling_OBJ("COIN_PARENT").Get((value) =>
        {
            value.GetComponent<COIN_PARENT>().Init(Vector3.zero, true);
        });

        var Items = Base_Manager.Item.GetDropSet();

        for (int i = 0; i < Items.Count; i++)
        {
            Base_Manager.Pool.Pooling_OBJ("Item_Obj").Get((value) =>
            {
                value.GetComponent<Item_OBJ>().Init(Vector3.zero, Items[i]);
            });
        }

        base.DisableOBJ();
    }
}
