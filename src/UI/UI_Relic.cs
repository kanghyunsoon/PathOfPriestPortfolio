using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class UI_Relic : UI_Base
{
    public Transform Content;
    public GameObject Part;
    public List<UI_Relic_Part> part = new List<UI_Relic_Part>();

    Dictionary<string, Item_Scriptable> m_Dictionarys = new Dictionary<string, Item_Scriptable>();
    Item_Scriptable m_Items;

    public GameObject[] RericPanelObjects;
    public Color[] Colors;

    // Hero Info
    [Header("## Item Infomation")]
    [SerializeField] private GameObject Information;
    [SerializeField] private TextMeshProUGUI NameText, RarityText, DescriptionText;
    [SerializeField] private TextMeshProUGUI Level, Count;
    [SerializeField] private TextMeshProUGUI Default_Skill;
    [SerializeField] private TextMeshProUGUI SkillName, SkillDescription;
    [SerializeField] private Image CountFill;
    [SerializeField] private Image ItemIcon, SkillIcon, RarityIcon;

    [SerializeField] private Button Upgrade;

    public void UpgradeButton(Item_Holder holder)
    {
        int value = (Base_Manager.Data.Item_Holder[holder.Data.name].Level + 1) * 5;
        if (Base_Manager.Data.Item_Holder[holder.Data.name].Count >= value)
        {
            Base_Manager.Data.Item_Holder[holder.Data.name].Count -= value;
            Base_Manager.Data.Item_Holder[holder.Data.name].Level++;
        }
        else
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["Shard_Not_EnoughRelic"].Get_Data(), Color.white);
            return;
        }
        GetItemInformation(holder.Data);

        for (int i = 0; i < part.Count; i++) part[i].Init();
    }

    public void AllUpgrade()
    {
        if (!GetAllUpgrade())
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NoRelicForEnhancement"].Get_Data(), Color.white);
            return;
        }
        Base_Canvas.instance.Get_UI("#Upgrade");
        Utils.UI_Holder.Peek().GetComponent<UI_Upgrade_Item>().Initialize(this);
    }

    private bool GetAllUpgrade()
    {

        bool Can = false;
        foreach (var data in Base_Manager.Data.m_Data_Item)
        {
            int value = (Base_Manager.Data.Item_Holder[data.Value.Data.name].Level + 1) * 5;

            if (Base_Manager.Data.Item_Holder[data.Value.Data.name].Count >= value)
            {
                Can = true;
            }
        }
        return Can;
    }

    public void GetShop() => Base_Canvas.instance.Get_UI("#Shop", false, true, true);

    public void GetSpawn()
    {
       Information.SetActive(false);
    }

    public void GetItemInformation(Item_Scriptable data)
    {
        int value = (Base_Manager.Data.Item_Holder[data.name].Level + 1) * 5;

        Information.SetActive(true);
        NameText.text = Local_Manager.local_Data[data.Item_Name].Get_Data(); ;
        RarityText.text = Utils.String_Color_Rarity(data.rarity) + data.rarity.ToString();
        DescriptionText.text = Local_Manager.local_Data[data.Description].Get_Data();
        Default_Skill.text = Local_Manager.local_Data[data.Skill_Title].Get_Data();
        SkillName.text = Local_Manager.local_Data[data.Skill_Name].Get_Data();
        SkillDescription.text = Local_Manager.local_Data[data.Skill_Description].Get_Data(); ;

        Level.text = "Lv." + (Base_Manager.Data.Item_Holder[data.name].Level + 1).ToString();
        Count.text = "(" + Base_Manager.Data.Item_Holder[data.name].Count.ToString() + "/" + value.ToString() + ")";
        CountFill.fillAmount = (float)Base_Manager.Data.Item_Holder[data.name].Count / (float)value;
        ItemIcon.sprite = Utils.Get_Atlas(data.name);
        ItemIcon.SetNativeSize();
        RarityIcon.sprite = Utils.Get_Atlas(data.rarity.ToString());
        SkillIcon.sprite = Utils.Get_Atlas(data.Skill_Image);

        Upgrade.onClick.RemoveAllListeners();
        Upgrade.onClick.AddListener(() => UpgradeButton(Base_Manager.Data.m_Data_Item[data.name]));
    }



    public override bool Init()
    {
        var Datas = Base_Manager.Data.GetOwnedItems();
        Get_Item_Check();

        foreach (var data in Datas)
        {
            if(data.Value.Data.type == ItemType.Equipment)
            {
                m_Dictionarys.Add(data.Value.Data.name, data.Value.Data);
            }

        }

        var sort_dictionary = m_Dictionarys.OrderByDescending(x => x.Value.rarity);

        foreach (var data in sort_dictionary)
        {
            var go = Instantiate(Part, Content).GetComponent<UI_Relic_Part>();
            part.Add(go);
            go.Initialize(data.Value, this);
        }

        return base.Init();
    }

    public void Set_Item_Button(int value)
    {
        Base_Manager.Item.GetItem(value, m_Items.name);
        Initialized();
    }

    public void Initialized()
    {
        Set_Click(null);

        for (int i = 0; i < part.Count; i++) part[i].Get_Item_Check();

        Delegate_Holder.ClearEvent();
        Relic_Manager.instance.Initialize();

        Get_Item_Check();
        // Main_UI.instance.Set_Character_Data();
    }

    public void Get_Item_Check()
    {
        //  현재 스테이지 기반으로 활성화할 슬롯 개수 설정 (최대 6개)
        int unlockedSlots = Mathf.Clamp((Data_Manager.m_Data.Level - 50) / 50 + 1, 0, RericPanelObjects.Length);

        for (int i = 0; i < RericPanelObjects.Length; i++)
        {
            Button slotButton = RericPanelObjects[i].GetComponent<Button>();

            if (i < unlockedSlots)
            {
                //  스테이지에 따라 슬롯 활성화
                RericPanelObjects[i].SetActive(true);
                RericPanelObjects[i].GetComponent<Image>().color = Color.white;

                //  자물쇠 이미지는 숨김 (잠금 해제됨)
                RericPanelObjects[i].transform.GetChild(1).gameObject.SetActive(false);

                if (Base_Manager.Data.m_Set_Item[i] != null)  // 유물이 장착된 경우
                {
                    var item = Base_Manager.Data.m_Set_Item[i];
                    RericPanelObjects[i].GetComponent<Image>().color = Colors[(int)item.rarity];

                    //  유물 아이콘 표시
                    RericPanelObjects[i].transform.GetChild(2).gameObject.SetActive(true);
                    RericPanelObjects[i].transform.GetChild(2).GetComponent<Image>().sprite = Utils.Get_Atlas(item.name);
                    RericPanelObjects[i].transform.GetChild(2).GetComponent<Image>().SetNativeSize();

                    //  레벨 텍스트 표시
                    RericPanelObjects[i].transform.GetChild(3).gameObject.SetActive(true);
                    RericPanelObjects[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text =
                        "Lv." + (Base_Manager.Data.Item_Holder[item.name].Level + 1).ToString();
                }
                else
                {
                    RericPanelObjects[i].transform.GetChild(2).gameObject.SetActive(false);
                    RericPanelObjects[i].transform.GetChild(3).gameObject.SetActive(false);

                    slotButton.interactable = true;
                }

            }
            else
            {
                //  아직 잠긴 슬롯 (비활성화)
                RericPanelObjects[i].SetActive(true);
                RericPanelObjects[i].transform.GetChild(1).gameObject.SetActive(true); //  자물쇠 활성화
                RericPanelObjects[i].transform.GetChild(2).gameObject.SetActive(false); //  아이콘 비활성화
                RericPanelObjects[i].transform.GetChild(3).gameObject.SetActive(false); //  레벨 비활성화

                slotButton.interactable = false;
            }
        }
    }

    public void Set_Click(UI_Relic_Part s_Part)
    {
        if (s_Part == null)
        {
            for (int i = 0; i < part.Count; i++)
            {
                part[i].LockOBJ.SetActive(false);
                part[i].GetComponent<Outline>().enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < Base_Manager.Data.m_Set_Item.Length; i++)
            {
                var data = Base_Manager.Data.m_Set_Item[i];
                if (data != null)
                {
                    if (data == s_Part.m_Item)
                    {
                        Base_Manager.Item.DisableItem(i);
                        Initialized();
                        return;
                    }
                }
            }

            m_Items = s_Part.m_Item;

            for (int i = 0; i < part.Count; i++)
            {
                part[i].LockOBJ.SetActive(true);
                part[i].GetComponent<Outline>().enabled = false;
            }

            s_Part.LockOBJ.SetActive(false);
            s_Part.GetComponent<Outline>().enabled = true;
        }

    }

    public override void DisableOBJ()
    {
        Main_UI.instance.LayerCheck(-1);
        base.DisableOBJ();
    }




}
