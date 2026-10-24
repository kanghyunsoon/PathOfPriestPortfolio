using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Gacha : UI_Base
{
    Render_Gacha gachaTexture;
    public Transform[] Horizontals;
    public int Limit = 4;
    public Image Hero_Panel;

    [SerializeField] TextMeshProUGUI GachaTitle;
    [SerializeField] TextMeshProUGUI GachaPrice;
    [SerializeField] Image GachaImage;
    [SerializeField] Button GachaButton;
    [SerializeField] GameObject[] LockButtons;

    private bool isAdsGacha = false;

    List<GameObject> Garvage = new List<GameObject>();

    private void Awake()
    {
        gachaTexture = Render_Manager.instance.GACHA;
    }

    public void Initalize()
    {
        for (int i = 0; i < LockButtons.Length; i++)
        {
            LockButtons[i].SetActive(true);
        }

        gachaTexture.Init();
        for (int i =0; i<Garvage.Count; i++)
        {
            Destroy(Garvage[i]);
        }
        Garvage.Clear();
    }

    public override void DisableOBJ()
    {
        gachaTexture.Init();
        var shopUI = GameObject.Find("#Shop");
        if (shopUI != null)
        {
            shopUI.GetComponent<UI_Shop>().TextCheck();
        }
        base.DisableOBJ();
    }

    public void GetGachaHero(int value)
    {
        GachaButton.onClick.RemoveAllListeners();

        if (isAdsGacha)
        {
            value = 0;
            isAdsGacha = false;
        }

        switch (value)
        {
            case 11:
                GachaTitle.text = Local_Manager.local_Data["Summon11"].Get_Data();
                GachaPrice.text = "3000";
                GachaPrice.color = Data_Manager.m_Data.Dia < 3000 ? Color.red : Color.white;
                GachaButton.onClick.AddListener(() => OnClickReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("Dia");
                break;

            case 1:
                GachaTitle.text = Local_Manager.local_Data["Summon1"].Get_Data();
                GachaPrice.text = "300";
                GachaPrice.color = Data_Manager.m_Data.Dia < 300 ? Color.red : Color.white;
                GachaButton.onClick.AddListener(() => OnClickReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("Dia");
                break;

            case 0: // 광고 리가차
                GachaTitle.text = Local_Manager.local_Data["SummonAD"].Get_Data(); // <- 새로운 로컬 키
                GachaPrice.text = Local_Manager.local_Data["Free"].Get_Data();     // <- "무료" 텍스트
                GachaPrice.color =  Data_Manager.m_Data.Hero_ADS_Count >= 3 ? GachaPrice.color = Color.red : GachaPrice.color = Color.white;
                GachaButton.onClick.AddListener(() => OnClickReGacha(value));
                GachaImage.sprite = Utils.Get_Atlas("ADS");
                break;
        }
        StartCoroutine(Gacha_Coroutine(value == 0 ? 1 : value));
    }
    public void OnClickReGacha(int value)
    {
        if (value == 0)
        {
            if (Data_Manager.m_Data.Hero_ADS_Count >= 3)
            {
                Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["No_More_Ads"].Get_Data(), Color.red);
                return;
            }

            // 광고 소환은 광고를 끝까지 본 뒤에만 진행한다.
            Base_Manager.ADS.ShowRewardedAds(() =>
            {
                if (this == null) return;

                Data_Manager.m_Data.Hero_ADS_Count++;
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
        gachaTexture.Init();
        Initalize();
        isAdsGacha = value == 0;
        GetGachaHero(value == 0 ? 1 : value);
    }

    IEnumerator Gacha_Coroutine(int value)
    {
        int horizontalCheker = 0;
        // 캐릭터를 미리뽑는작업
        List<Character_Scriptable> gacha_list = new List<Character_Scriptable>();
        for (int i = 1; i <= value; i++)
        {
            Data_Manager.m_Data.Summon++;
            Data_Manager.m_Data.Hero_Summon_Count++;
            Data_Manager.m_Data.Hero_PickUp_Count++;
            Rarity rarity = Rarity.Common;

            if (Data_Manager.m_Data.Hero_PickUp_Count >= 110)
            {
                Data_Manager.m_Data.Hero_PickUp_Count = 0;
                rarity = Rarity.Legendary;
            }

            float R_percentage = 0.0f;
            float percentage = Random.Range(0.0f, 100.0f);

            if (rarity != Rarity.Legendary)
            {
                for (int j = 1; j < 5; j++)
                {
                    R_percentage += Utils.Gacha_Percentage()[j];
                    if (percentage <= R_percentage)
                    {
                        rarity = (Rarity)j;
                        break;
                    }
                }
            }

            // 뽑기 완료된 케릭터 데이터 결정
            Character_Scriptable data = Base_Manager.Data.Get_Rarity_Character(rarity);
            if (data == null) continue;

            // 연출 도중 창을 닫아도 결과가 남도록 연출 전에 지급하고 저장한다.
            Base_Manager.Data.Character_Holder[data.name].Count++;
            gacha_list.Add(data);
        }

        Base_Manager.Firebase.WriteData();

        // 캐릭터 생성 연출
        for (int i = 1; i <= gacha_list.Count; i++)
        {
            var data = gacha_list[i-1];
            var go = Instantiate(Hero_Panel, Horizontals[horizontalCheker]);
            Garvage.Add(go.gameObject);

            Image character_Image = go.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            go.gameObject.SetActive(true);

            go.sprite = Utils.Get_Atlas(data.m_Rarity.ToString());
            character_Image.sprite = Utils.Get_Atlas(data.name);
            character_Image.SetNativeSize();

            if (i % Limit == 0)
            {
                horizontalCheker++;
            }

            switch(value)
            {
                case 11: gachaTexture.Get_Hero(11,i-1, data); break;
                case 1: gachaTexture.Get_Hero(1,0,data); break;
            }
            go.GetComponentInChildren<Animator>().SetTrigger((int)data.m_Rarity < 3 ? "Default" : "Rare");

            yield return new WaitForSecondsRealtime((int)data.m_Rarity < 3 ? 0.1f : 0.6f);
        }

        for(int i =0; i< LockButtons.Length; i++)
        {
            LockButtons[i].SetActive(false);
        }
    }



}
