using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UI_Smelt : UI_Base
{
    Smelt_Scriptable smelt_Data;
    public GameObject Smelt_Panel;
    public Transform Content;
    bool Opening = false;
    public TextMeshProUGUI CountText;
    [HideInInspector] public List<GameObject> Garvage = new List<GameObject>();

    public override bool Init()
    {
        smelt_Data = Resources.Load<Smelt_Scriptable>("Scriptable/Smelt_Data");

        if(Base_Manager.Data.m_Data_Smelt.Count > 0)
        {
            for (int i = 0; i < Base_Manager.Data.m_Data_Smelt.Count; i++)
            {
               GetSmelt_Panel(
                    (int)Base_Manager.Data.m_Data_Smelt[i].rarity,
                    Base_Manager.Data.m_Data_Smelt[i].holder,
                    Base_Manager.Data.m_Data_Smelt[i].Value
                    );
            }
        }

        CountCheck();

        return base.Init();
    }

    public void Smelt_Change()
    {
        if (Opening) return;
        if (!Utils.Item_Count("Monster_Energy", 100))
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NotEnoughRefineResource"].Get_Data(), Color.red);
            return;
        }

        Opening = true;

        Base_Manager.Data.Item_Holder["Monster_Energy"].Count -= 100;
        CountCheck();

        if (Garvage.Count > 0)
        {
            for (int i = 0; i< Garvage.Count; i++) Destroy(Garvage[i]);
            Garvage.Clear();
        }

        StartCoroutine(OpenCoroutine(CountPercentage()));

    }

    IEnumerator OpenCoroutine(int count)
    {
        Base_Manager.Data.m_Data_Smelt.Clear();

        for (int i = 0; i < count; i++)
        {
            Status_Holder status = (Status_Holder)Random.Range(0, 8);
            int value = CountPercentage();
            float valueCount = Random.Range(StatusHolder(status)[value].Min, StatusHolder(status)[value].Max);

            GetSmelt_Panel(value, status, valueCount);

            Base_Manager.Data.m_Data_Smelt.Add(new Smelt_Holder { holder = status, rarity = (Rarity)value,  Value = valueCount });

            yield return new WaitForSeconds(0.05f);

        }

        for(int i =0; i< Spawner.m_Players.Count; i++)
        {
            Spawner.m_Players[i].Set_ATKHP();
        }
        Opening = false;
    }

    private void CountCheck()
    {
        CountText.text = string.Format("({0}/{1})", Base_Manager.Data.Item_Holder["Monster_Energy"].Count, 100);
        CountText.color = Utils.Item_Count("Monster_Energy", 100) ? Color.green : Color.red;
    }

    private void  GetSmelt_Panel(int rarityValue, Status_Holder status, float valueCount)
    {

        var go = Instantiate(Smelt_Panel, Content);
        Garvage.Add(go);
        go.SetActive(true);

        go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Utils.String_Color_Rarity((Rarity)rarityValue) + StatusString(status);
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = Utils.String_Color_Rarity((Rarity)rarityValue) + string.Format("{0:0.00}%", valueCount);


        go.GetComponent<Animator>().SetTrigger("Open");
    }

    private int CountPercentage()
    {
        int randomCount = Random.Range(0, 100);
        float randomValue = 0.0f;
        int count = 0;
        for (int i = 0; i < smelt_Data.Count_Value.Length; i++)
        {
            randomValue += smelt_Data.Count_Value[i];
            if (randomValue >= randomCount)
            {
                count = i + 1;
                return count;
            }
        }
        return 1;
    }

    private string StatusString(Status_Holder holder)
    {
        string Increase = " " + Local_Manager.local_Data["Increase"].Get_Data();
        switch (holder)
        {
            case Status_Holder.ATK: return Local_Manager.local_Data["ATK"].Get_Data() + Increase;
            case Status_Holder.HP: return Local_Manager.local_Data["HP"].Get_Data() + Increase;
            case Status_Holder.Money: return Local_Manager.local_Data["Money"].Get_Data() + Increase;
            case Status_Holder.Item: return Local_Manager.local_Data["Item"].Get_Data() + Increase;
            case Status_Holder.Skill: return Local_Manager.local_Data["Skill"].Get_Data() + " " + Local_Manager.local_Data["Decrease"].Get_Data();
            case Status_Holder.ATK_Speed: return Local_Manager.local_Data["ATK_Speed"].Get_Data() + Increase;
            case Status_Holder.Critical_P: return Local_Manager.local_Data["Critical_P"].Get_Data() + Increase;
            case Status_Holder.Critical_D: return Local_Manager.local_Data["Critical_D"].Get_Data() + Increase;
        }
        return  "";
    }

    private Percentage[] StatusHolder(Status_Holder holder)
    {
        switch (holder)
        {
            case Status_Holder.ATK: return smelt_Data.ATK_percentage;
            case Status_Holder.HP: return smelt_Data.HP_percentage;
            case Status_Holder.Money: return smelt_Data.Money_percentage;
            case Status_Holder.Item: return smelt_Data.Item_percentage;
            case Status_Holder.Skill: return smelt_Data.Skill_percentage;
            case Status_Holder.ATK_Speed: return smelt_Data.ATK_Speed_percentage;
            case Status_Holder.Critical_P: return smelt_Data.Critical_P_percentage;
            case Status_Holder.Critical_D: return smelt_Data.Critical_D_percentage;
        }
        return null;
    }

    public override void DisableOBJ()
    {
        Main_UI.instance.LayerCheck(-1);
        base.DisableOBJ();
    }



}
