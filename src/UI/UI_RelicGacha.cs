using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RelicGacha : UI_Base
{
    public Transform[] Horizontals;
    public int Limit = 4;
    public Image Relic_Panel;
    public Image SummonImage;

    [SerializeField] TextMeshProUGUI GachaTitle;
    [SerializeField] TextMeshProUGUI GachaPrice;
    [SerializeField] Image GachaImage;
    [SerializeField] Button GachaButton;
    [SerializeField] GameObject[] LockButtons;
    [SerializeField] Transform[] RelicTransforms;


    List<GameObject> Garvage = new List<GameObject>();
    List<GameObject> Garvage2 = new List<GameObject>();

    private bool isAdsRelicGacha = false;

    public void Initalize()
    {
        for (int i = 0; i < LockButtons.Length; i++)
        {
            LockButtons[i].SetActive(true);
        }

        for (int i = 0; i < Garvage.Count; i++)
        {
            Destroy(Garvage[i]);
        }
        Garvage.Clear();

        for (int i = 0; i < Garvage2.Count; i++)
        {
            Destroy(Garvage2[i]);
        }
        Garvage2.Clear();
    }

    public override void DisableOBJ()
    {
        var shopUI = GameObject.Find("#Shop");
        if (shopUI != null)
        {
            shopUI.GetComponent<UI_Shop>().TextCheck();
        }
        base.DisableOBJ();
    }

    public void GetGachaRelic(int value)
    {
        GachaButton.onClick.RemoveAllListeners();
        if(isAdsRelicGacha)
        {
            value = 0;
            isAdsRelicGacha = false;
        }
        switch (value)
        {
            case 11:
                GachaTitle.text = Local_Manager.local_Data["Summon11"].Get_Data();
                GachaPrice.text = "3000";
                GachaPrice.color = Data_Manager.m_Data.Dia < 3000 ? Color.red : Color.white;
                GachaButton.onClick.AddListener(() => OnClickRelicReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("Dia");
                break;

            case 1:
                GachaTitle.text = Local_Manager.local_Data["Summon1"].Get_Data();
                GachaPrice.text = "300";
                GachaPrice.color = Data_Manager.m_Data.Dia < 300 ? Color.red : Color.white;
                GachaButton.onClick.AddListener(() => OnClickRelicReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("Dia");
                break;

            case 0: // 광고 리가차
                GachaTitle.text = Local_Manager.local_Data["SummonAD"].Get_Data(); // <- 새로운 로컬 키
                GachaPrice.text = Local_Manager.local_Data["Free"].Get_Data();     // <- "무료" 텍스트
                GachaPrice.color = Data_Manager.m_Data.Relic_ADS_Count >= 3 ? GachaPrice.color = Color.red : GachaPrice.color = Color.white;
                GachaButton.onClick.AddListener(() => OnClickRelicReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("ADS");
                break;
        }
        StartCoroutine(RelicGacha_Coroutine(value == 0 ? 1 : value));
    }

    public void OnClickRelicReGacha(int value)
    {
        if (value == 0)
        {
            if (Data_Manager.m_Data.Relic_ADS_Count >= 3)
            {
                Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["No_More_Ads"].Get_Data(), Color.red);
                return;
            }

            // 광고 소환은 광고를 끝까지 본 뒤에만 진행한다.
            Base_Manager.ADS.ShowRewardedAds(() =>
            {
                if (this == null) return;

                Data_Manager.m_Data.Relic_ADS_Count++;
                StartReGacha(0);
            });
            return;
        }

        int cost = value == 11 ? 3000 : 300;

        if (Data_Manager.m_Data.Dia < cost)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NeedDia"].Get_Data(), Color.red);
            return;
        }

        Data_Manager.m_Data.Dia -= cost;
        Data_Manager.m_Data.UseDiamond += cost;

        StartReGacha(value);
    }

    private void StartReGacha(int value)
    {
        foreach (var obj in Garvage2)
        {
            Destroy(obj.gameObject);
        }
        Garvage2.Clear();

        Initalize();
        isAdsRelicGacha = value == 0;
        GetGachaRelic(value == 0 ? 1 : value);
    }

    IEnumerator RelicGacha_Coroutine(int value)
    {
        int horizontalCheker = 0;

        List<Item_Scriptable> gacha_list = new List<Item_Scriptable>(); // 여기까지


        for (int i = 1; i <= value; i++)
        {
            Data_Manager.m_Data.Relic_Summon_Count++;
            Data_Manager.m_Data.Relic_PickUp_Count++;
            Rarity rarity = Rarity.Common;

            if (Data_Manager.m_Data.Relic_PickUp_Count >= 110)
            {
                Data_Manager.m_Data.Relic_PickUp_Count = 0;
                rarity = Rarity.Legendary;
            }

            float R_percentage = 0.0f;
            float percentage = Random.Range(0.0f, 100.0f);

            if (rarity != Rarity.Legendary)
            {
                for (int j = 1; j < 5; j++)
                {
                    R_percentage += Utils.Gacha_RelicPercentage()[j];
                    if (percentage <= R_percentage)
                    {
                        rarity = (Rarity)j;
                        break;
                    }
                }
            }

            // 뽑기 완료된 아이템 데이터 결정
            Item_Scriptable data = Base_Manager.Data.Get_Rarity_Relic(rarity);
            if (data == null) continue;

            // 연출 도중 창을 닫아도 결과가 남도록 연출 전에 지급하고 저장한다.
            Base_Manager.Data.Item_Holder[data.name].Count++;
            gacha_list.Add(data);
        }

        Base_Manager.Firebase.WriteData();

        // 생성 연출
        for (int i = 1; i <= gacha_list.Count; i++)
        {
            var data = gacha_list[i - 1];
            var go = Instantiate(Relic_Panel, Horizontals[horizontalCheker]);

            Garvage.Add(go.gameObject);
            Image Relic_Image = go.transform.GetChild(0).GetChild(0).GetComponent<Image>();

            go.sprite = Utils.Get_Atlas(data.rarity.ToString());
            Relic_Image.sprite = Utils.Get_Atlas(data.name);
            Relic_Image.SetNativeSize();

            go.gameObject.SetActive(true);
            Animator relicAnimator = go.GetComponentInChildren<Animator>();
            // 영웅 뽑기와 같이 Hero 등급부터 희귀 연출을 쓴다. Rarity 최댓값이 4라서 5와 비교하면 항상 기본 연출이었다.
            relicAnimator.SetTrigger((int)data.rarity < 3 ? "Default" : "Rare");

            if (i % Limit == 0)
            {
                horizontalCheker++;
            }

            switch (value)
            {
                case 11: Get_Relic(11, i - 1, data); break;
                case 1:  Get_Relic(1, 0, data); break;
            }

            yield return new WaitForSecondsRealtime((int)data.rarity < 3 ? 0.2f : 0.5f);
        }

        for (int i = 0; i < LockButtons.Length; i++)
        {
            LockButtons[i].SetActive(false);
        }
    }

    public void Get_Relic(int GachaValue, int value, Item_Scriptable data)
    {
        Transform parent = Relic_Transform(GachaValue, value);

        var go = Instantiate(SummonImage, parent);
        go.gameObject.SetActive(true);
        go.SetNativeSize();

        // 부모 위치를 따라가도록 강제 조정
        go.transform.position = parent.position;
        go.transform.localPosition = Vector3.zero; // 혹시라도 높이가 다르면 0으로 조정

        Image relicImage = go.transform.GetChild(0).GetComponent<Image>();
        relicImage.sprite = Utils.Get_Atlas(data.name);

        Garvage2.Add(go.gameObject);
    }

    private Transform Relic_Transform(int GachaValue, int value)
    {
        switch (GachaValue)
        {
            case 11: return RelicTransforms[value];
            case 1: return RelicTransforms[0];
        }
        return null;
    }
}
