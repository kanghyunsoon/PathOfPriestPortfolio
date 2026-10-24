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
    bool collected = false;

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
        GrantReward(1);
        DisableOBJ();
    }

    public void ADSCollectButton()
    {
        Base_Manager.ADS.ShowRewardedAds(() =>
        {
            // 광고 콜백이 늦게 와서 창이 이미 닫혔으면 기본 보상만 받은 상태다.
            if (this == null || collected) return;

            GrantReward(2);
            DisableOBJ();
        });

    }

    private void GrantReward(int multiplier)
    {
        if (collected) return;
        collected = true;

        Data_Manager.m_Data.Money += moneyValue * multiplier;
        foreach (var item in Items)
        {
            Base_Manager.Inventory.GetItem(item.Value.Data, item.Value.holder.Count * multiplier);
        }
        Main_UI.instance.TextCheck();
    }

    public override void DisableOBJ()
    {
        // 뒤로 가기로 닫아도 기본 보상은 지급한다. 오프라인 시간은 아래에서 비우므로 닫으면 보상이 사라졌다.
        GrantReward(1);

        // 같은 오프라인 시간으로 보상을 두 번 받지 않도록 비운다.
        Utils.ConsumeOfflineTime();

        Base_Manager.Pool.Pooling_OBJ("COIN_PARENT").Get((value) =>
        {
            value.GetComponent<COIN_PARENT>().Init(Vector3.zero, true);
        });

        // 획득 연출만 보여 준다. 실제 지급은 위에서 끝났다.
        var Items = Base_Manager.Item.GetDropSet();

        for (int i = 0; i < Items.Count; i++)
        {
            Base_Manager.Pool.Pooling_OBJ("Item_Obj").Get((value) =>
            {
                value.GetComponent<Item_OBJ>().Init(Vector3.zero, Items[i], false);
            });
        }

        base.DisableOBJ();
    }
}
