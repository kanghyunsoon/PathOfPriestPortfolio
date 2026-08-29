using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Shop : UI_Base
{
    [SerializeField] TextMeshProUGUI H_LevelText, H_CountText, H_PickUpText, H_RelicLevelText, H_RelicCountText, H_RelicPickUpText;
    [SerializeField] Image H_Count_Fill, H_PickUp_Fill;
    [SerializeField] Image H_RelicCount_Fill, H_RelicPickUp_Fill;

    [SerializeField] TextMeshProUGUI[] Percentage_Texts;
    [SerializeField] TextMeshProUGUI[] RelicPercentage_Texts;
    [SerializeField] TextMeshProUGUI Percentage_LevelText;
    [SerializeField] TextMeshProUGUI RelicPercentage_LevelText;
    [SerializeField] GameObject InformationPanel;
    [SerializeField] GameObject RelicInformationPanel;
    [SerializeField] TextMeshProUGUI DiaText, GoldText;

    [SerializeField] TextMeshProUGUI HeroGachaDia1, HeroGachaDia2, RelicGachaDia1, RelicGachaDia2, HeroAdsCount, RelicAdsCount;

    public GameObject RemoveOBJ;
    public static GameObject RemoveADS_LockOBJ;
    int BackBoard_Level;
    int RelicBackBoard_Level;
    private bool isAdsGacha = false;


    public override bool Init()
    {
        GetInit();
        TextCheck();
        return base.Init();

    }

    public void TextCheck()
    {
        DiaText.text = Data_Manager.m_Data.Dia.ToString();
        GoldText.text = StringMethod.ToCurrencyString(Data_Manager.m_Data.Money);

        Color Ads1Color = Data_Manager.m_Data.Hero_ADS_Count >= 3 ? Color.red : Color.green;
        Color Ads2Color = Data_Manager.m_Data.Relic_ADS_Count >= 3 ? Color.red : Color.green;
        HeroAdsCount.text = "(" + Data_Manager.m_Data.Hero_ADS_Count.ToString() + "/3)";
        RelicAdsCount.text = "(" + Data_Manager.m_Data.Relic_ADS_Count.ToString() + "/3)";
        HeroAdsCount.color = Ads1Color;
        RelicAdsCount.color = Ads2Color;


        Color Gacha1Color = Data_Manager.m_Data.Dia < 300 ? Color.red : Color.green;
        Color Gacha2Color = Data_Manager.m_Data.Dia < 3000 ? Color.red : Color.green;
        HeroGachaDia1.color = Gacha1Color;
        RelicGachaDia1.color = Gacha1Color;
        HeroGachaDia2.color = Gacha2Color;
        RelicGachaDia2.color = Gacha2Color;

        GetInit();
    }

    public void GetProduct(string name)
    {
        Base_Manager.IAP.Purchase(name);
        TextCheck();
    }

    public void GetInit()
    {
        RemoveADS_LockOBJ = RemoveOBJ;

        //Hero
        int valueCount = Data_Manager.m_Data.Hero_Summon_Count;
        int level = Utils.Summon_Level(valueCount);
        if (level < 9)
        {
            int maximumValueCount = Utils.summon_level[level];

            H_LevelText.text = Local_Manager.local_Data["SummonLevel"].Get_Data() + (level + 1).ToString();
            H_CountText.text = "(" + valueCount.ToString() + "/" + maximumValueCount.ToString() + ")";
            H_Count_Fill.fillAmount = (float)valueCount / (float)maximumValueCount;
        }
        else if (level >= 9)
        {
            H_CountText.text = "Max Level";
            H_CountText.color = Color.yellow;
            H_Count_Fill.fillAmount = 1.0f;
        }

        float valuePickUp = Data_Manager.m_Data.Hero_PickUp_Count;
        H_PickUpText.text = "(" + valuePickUp.ToString() + "/110)";
        H_PickUp_Fill.fillAmount = (float)valuePickUp / 110.0f;



        //Relic
        int valueRelicCount = Data_Manager.m_Data.Relic_Summon_Count;
        int Reliclevel = Utils.Summon_Level(valueRelicCount);
        if (Reliclevel < 9)
        {
            int maximumValueCount = Utils.summon_level[Reliclevel];

            H_RelicLevelText.text = Local_Manager.local_Data["SummonLevel"].Get_Data() + (Reliclevel + 1).ToString();
            H_RelicCountText.text = "(" + valueRelicCount.ToString() + "/" + maximumValueCount.ToString() + ")";
            H_RelicCount_Fill.fillAmount = (float)valueRelicCount / (float)maximumValueCount;
        }
        else if (Reliclevel >= 9)
        {
            H_RelicCountText.text = "Max Level";
            H_RelicCountText.color = Color.yellow;
            H_RelicCount_Fill.fillAmount = 1.0f;
        }

        float valueRelicPickUp = Data_Manager.m_Data.Relic_PickUp_Count;
        H_RelicPickUpText.text = "(" + valueRelicPickUp.ToString() + "/110)";
        H_RelicPickUp_Fill.fillAmount = (float)valueRelicPickUp / 110.0f;

        if (Data_Manager.m_Data.ADS_Remove)
        {
            RemoveADS_LockOBJ.SetActive(true);
        }
    }

    public void GetInformation()
    {
        InformationPanel.SetActive(true);

        Percentage_Check(Utils.Summon_Level(Data_Manager.m_Data.Hero_Summon_Count));
    }



    public void ArrowButton(int value)
    {
        BackBoard_Level += value;
        if (BackBoard_Level < 0) BackBoard_Level = 9;
        else if (BackBoard_Level > 9) BackBoard_Level = 0;

        Percentage_Check(BackBoard_Level);
    }

    private void Percentage_Check(int value)
    {
        BackBoard_Level = value;
        for (int i = 0; i < Percentage_Texts.Length; i++)
        {
            float temp = float.Parse(CSV_Importer.Summon[value][((Rarity)i).ToString()].ToString());
            Percentage_Texts[i].text = temp.ToString("F4") + "%";
        }
        Percentage_LevelText.text = "LEVEL." + (value+1).ToString();
    }


    public void DisableInformation() => InformationPanel.SetActive(false);
    public void RelicInformation() => RelicInformationPanel.SetActive(false);

    public void GachaButton(int value)
    {
        if (!isAdsGacha) // 광고 가차가 아닐 경우, 다이아 차감
        {
            int cost = (value == 11) ? 3000 : 300;

            if (Data_Manager.m_Data.Dia < cost)
            {
                Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NeedDia"].Get_Data(), Color.red);
                return;
            }
            Data_Manager.m_Data.Dia -= cost;
            Data_Manager.m_Data.UseDiamond += cost;
        }
        else
        {
            isAdsGacha = false; // 광고 가차 실행 후 다시 일반 가차로 변경
        }

        TextCheck();
        Base_Canvas.instance.Get_UI("#Gacha");
        var ui = Utils.UI_Holder.Peek().GetComponent<UI_Gacha>();
        ui.GetGachaHero(value);
    }

    public void GachaButtonADS()
    {
        if(Data_Manager.m_Data.Hero_ADS_Count >=3)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["No_More_Ads"].Get_Data(), Color.white);
            return;
        }
        Base_Manager.ADS.ShowRewardedAds(() =>
        {
            isAdsGacha = true;
            Data_Manager.m_Data.Hero_ADS_Count++;
            GachaButton(0);
        });
    }

    public void Relic_GachaButton(int value)
    {
        if (!isAdsGacha) // 광고 가차가 아닐 경우, 다이아 차감
        {
            int cost = (value == 11) ? 3000 : 300;
            if (Data_Manager.m_Data.Dia < cost)
            {
                Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NeedDia"].Get_Data(), Color.red);
                return;
            }
            Data_Manager.m_Data.Dia -= cost;
            Data_Manager.m_Data.UseDiamond += cost;
        }
        else
        {
            isAdsGacha = false; // 광고 가차 실행 후 다시 일반 가차로 변경
        }


        TextCheck();
        Base_Canvas.instance.Get_UI("#RelicGacha");
        var ui = Utils.UI_Holder.Peek().GetComponent<UI_RelicGacha>();
        ui.GetGachaRelic(value);
    }

    public void Relic_GachaButtonADS()
    {
        if (Data_Manager.m_Data.Relic_ADS_Count >= 3)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["No_More_Ads"].Get_Data(), Color.white);
            return;
        }
        Base_Manager.ADS.ShowRewardedAds(() =>
        {
            isAdsGacha = true;
            Data_Manager.m_Data.Relic_ADS_Count++; // 광고 성공 시에만 증가
            Relic_GachaButton(0);
        });
    }

    public override void DisableOBJ()
    {
        Main_UI.instance.TextCheck();
        Main_UI.instance.LayerCheck(-1);
        base.DisableOBJ();
    }

    public void RelicArrowButton(int value)
    {
        RelicBackBoard_Level += value;
        if (RelicBackBoard_Level < 0) RelicBackBoard_Level = 9;
        else if (RelicBackBoard_Level > 9) RelicBackBoard_Level = 0;

        RelicPercentage_Check(RelicBackBoard_Level);
    }

    private void RelicPercentage_Check(int value)
    {
        RelicBackBoard_Level = value;
        for (int i = 0; i < RelicPercentage_Texts.Length; i++)
        {
            float temp = float.Parse(CSV_Importer.Summon[value][((Rarity)i).ToString()].ToString());
            RelicPercentage_Texts[i].text = temp.ToString("F4") + "%";
        }
        RelicPercentage_LevelText.text = "LEVEL." + (value + 1).ToString();
    }

    public void GetRelicInformation()
    {
        RelicInformationPanel.SetActive(true);

        RelicPercentage_Check(Utils.Summon_Level(Data_Manager.m_Data.Relic_Summon_Count));
    }

    public void DisableRelicInformation() => RelicInformationPanel.SetActive(false);
}
