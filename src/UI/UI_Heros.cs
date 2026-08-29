using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_Heros : UI_Base, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform Content;

    public GameObject Part;
    public List<UI_Heros_Part> part = new List<UI_Heros_Part>();

    Dictionary<string, Character_Scriptable> m_Dictionarys = new Dictionary<string, Character_Scriptable>();
    Character_Scriptable m_Character;

    // Hero Info
    [Header("## Hero Infomation")]
    [SerializeField] private GameObject Information;
    [SerializeField] private TextMeshProUGUI NameText, RarityText, DescriptionText;
    [SerializeField] private TextMeshProUGUI DPS, ATK, HP;
    [SerializeField] private TextMeshProUGUI Level, Count;
    [SerializeField] private TextMeshProUGUI Default_Skill;
    [SerializeField] private TextMeshProUGUI SkillName, SkillDescription;
    [SerializeField] private Image CountFill;
    [SerializeField] private Image CharacterIcon,SkillIcon,RarityIcon;

    [SerializeField] private Button Upgrade;

    public Image DragImage;
    public List<RectTransform> InitButtonsRect = new List<RectTransform>();
    Character_Scriptable draggingCharacter = null;

    public void Set_M_Character(Character_Scriptable character) => m_Character = character;


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (DragImage == null) return;

        Vector2 mousePos = Input.mousePosition;
        draggingCharacter = null;

        for (int i = 0; i < InitButtonsRect.Count; i++)
        {
            RectTransform slot = InitButtonsRect[i];

            if (RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos, null))
            {
                var data = Base_Manager.Data.m_Set_Character[i];
                if (data != null)
                {
                    draggingCharacter = data.Data;

                    // 해당 캐릭터 스프라이트 설정
                    DragImage.sprite = Utils.Get_Atlas(draggingCharacter.name);
                    DragImage.SetNativeSize();
                    DragImage.color = new Color(1, 1, 1, 0.5f);
                    DragImage.gameObject.SetActive(true);

                    break;
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragImage.rectTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragImage != null)
            DragImage.gameObject.SetActive(false);

        Vector2 mousePos = Input.mousePosition;

        bool placed = false;

        for (int i = 0; i < InitButtonsRect.Count; i++)
        {
            RectTransform slot = InitButtonsRect[i];
            if (RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos, null))
            {
                // 같은 캐릭터 중복 방지
                for (int j = 0; j < Base_Manager.Data.m_Set_Character.Length; j++)
                {
                    var data = Base_Manager.Data.m_Set_Character[j];
                    if (data != null && data.Data == draggingCharacter)
                    {
                        Base_Manager.Data.m_Set_Character[j] = null;
                        break;
                    }
                }

                Set_M_Character(draggingCharacter);
                Set_Character_Button(i);
                placed = true;
                break;
            }
        }

        if (!placed)
        {
            // 슬롯 외부 드롭 → 제거
            for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
            {
                var data = Base_Manager.Data.m_Set_Character[i];
                if (data != null && data.Data == draggingCharacter)
                {
                    Base_Manager.Character.DisableCharacter(i);
                    break;
                }
            }

            Initialized();
        }
    }


    public void UpgradeButton(Character_Holder holder)
    {
        int value = (holder.holder.Level + 1) * 5;
        if(holder.holder.Count >= value)
        {
            holder.holder.Count -= value;
            holder.holder.Level++;
        }
        else
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["Shard_Not_EnoughHero"].Get_Data(), Color.white);
            return;
        }
        GetHeroInformation(holder.Data);


        for(int i =0; i<part.Count; i++) part[i].Init();
    }

    public void AllUpgrade()
    {
        if(!GetAllUpgrade())
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["NoHeroForEnhancement"].Get_Data(), Color.white);
            return;
        }
        Base_Canvas.instance.Get_UI("#Upgrade");
        Utils.UI_Holder.Peek().GetComponent<UI_Upgrade_Character>().Initialize(this);
    }

    private bool GetAllUpgrade()
    {
        bool Can = false;
        foreach(var data in Base_Manager.Data.m_Data_Character)
        {
            if (data.Value.holder.Count >= Base_Manager.Character.LevelCount(data.Value))
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

    public void GetHeroInformation(Character_Scriptable data)
    {

        m_Character = data;
        Information.SetActive(true); // 무조건 켜줌

        if (Base_Manager.Data.m_Data_Character.TryGetValue(data.name, out var holder) && holder.isDebuff)
        {
            UI_Debuff.selectedDebuffCharacter = data;
            Base_Canvas.instance.Get_UI("#Debuff");
        }

        int value = (Base_Manager.Data.m_Data_Character[data.name].holder.Level + 1) * 5;

        NameText.text = Local_Manager.local_Data[data.m_CharacterName].Get_Data();
        RarityText.text = Utils.String_Color_Rarity(data.m_Rarity) + data.m_Rarity.ToString();
        DescriptionText.text = Local_Manager.local_Data[data.Description].Get_Data();
        Default_Skill.text = Local_Manager.local_Data[data.Skill_Title].Get_Data();
        SkillName.text = Local_Manager.local_Data[data.Skill_Name].Get_Data();
        SkillDescription.text = Local_Manager.local_Data[data.Skill_Description].Get_Data(); ;

        double atk = Base_Manager.Player.Get_ATK(data.m_Rarity, Base_Manager.Data.m_Data_Character[data.name]);
        double hp = Base_Manager.Player.Get_HP(data.m_Rarity, Base_Manager.Data.m_Data_Character[data.name]);
        double dps = atk + hp;
        DPS.text = StringMethod.ToCurrencyString(dps);
        ATK.text = StringMethod.ToCurrencyString(atk);
        HP.text = StringMethod.ToCurrencyString(hp);
        Level.text = "Lv." + (Base_Manager.Data.Character_Holder[data.name].Level + 1).ToString();
        Count.text = "(" + Base_Manager.Data.Character_Holder[data.name].Count.ToString() + "/" + value.ToString() + ")";
        CountFill.fillAmount = (float)Base_Manager.Data.Character_Holder[data.name].Count / (float)value;
        CharacterIcon.sprite = Utils.Get_Atlas(data.name);
        CharacterIcon.SetNativeSize();
        RarityIcon.sprite = Utils.Get_Atlas(data.m_Rarity.ToString());
        SkillIcon.sprite = Utils.Get_Atlas(data.Skill_Image);

        Upgrade.onClick.RemoveAllListeners();
        Upgrade.onClick.AddListener(() => UpgradeButton(Base_Manager.Data.m_Data_Character[data.name]));
    }

    public void InitButtons()
    {
        for(int i =0; i<Render_Manager.instance.HERO.circles.Length; i++)
        {
            int index = i;
            var go = new GameObject("Button").AddComponent<Button>(); // 새로운 버튼이라는 이름의 오브젝트를 생성 후 버튼 컴포넌트 추가

            go.onClick.AddListener(() => Set_Character_Button(index));

            go.transform.SetParent(this.transform);                   // 해당 오브젝트를 UI히어로스 팝업의 자식오브젝트로 설정
            go.gameObject.AddComponent<Image>();                      // 이미지를 해당오브젝트에 추가
            //go.gameObject.AddComponent<RectTransform>();              // 렉트 트렌스폼 컴포넌트를 추가

            RectTransform rect = go.GetComponent<RectTransform>();

            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            rect.sizeDelta = new Vector2(150.0f, 225.0f); // 버튼 크기 150 x 225 설정

            // 버튼 위치를 -75로 설정 (여기서 y 값도 조정 가능)
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -75);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            Vector3 ScreenPoint = Render_Manager.instance.ReturnScreenPoint(Render_Manager.instance.HERO.circles[i]);
            go.transform.position = new Vector3(ScreenPoint.x, ScreenPoint.y , ScreenPoint.z);

            InitButtonsRect.Add(go.GetComponent<RectTransform>());
        }

    }

    public void Set_Character_Button(int value)
    {
        Base_Manager.Character.GetCharacter(value, m_Character.m_CharacterName);

        Initialized();
    }

    public void Initialized()
    {
        Render_Manager.instance.HERO.GetParticle(false);
        Set_Click(null);
        Render_Manager.instance.HERO.InitHero();

        for (int i = 0; i < part.Count; i++) part[i].Get_Character_Check();

        Main_UI.instance.Set_Character_Data();
    }

    // 케릭터가 눌렸을때 나머지 케릭터의 음영처리
    public void Set_Click(UI_Heros_Part s_Part)
    {
        if(s_Part == null)
        {
            for (int i = 0; i < part.Count; i++)
            {
                part[i].LockOBJ.SetActive(false);
                part[i].GetComponent<Outline>().enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
            {
                var data = Base_Manager.Data.m_Set_Character[i];
                if (data != null)
                {
                    if (data.Data == s_Part.m_Character)
                    {
                        Base_Manager.Character.DisableCharacter(i);
                        Initialized();
                        return;
                    }
                }
            }

            m_Character = s_Part.m_Character;

            for (int i = 0; i < part.Count; i++)
            {
                part[i].LockOBJ.SetActive(true);
                part[i].GetComponent<Outline>().enabled = false;
            }

            s_Part.LockOBJ.SetActive(false);
            s_Part.GetComponent<Outline>().enabled = true;
        }

    }

    public override bool Init()
    {
        InitButtons();

        Render_Manager.instance.HERO.InitHero();

        Main_UI.instance.FadeInOut(true, true, null);

        var Datas = Base_Manager.Data.GetOwnedCharacters();

        foreach (var data in Datas)
        {
            m_Dictionarys.Add(data.Value.Data.m_CharacterName, data.Value.Data);
        }

        var sort_dictionary = m_Dictionarys.OrderByDescending(x => x.Value.m_Rarity);

        foreach (var data in sort_dictionary)
        {
            var go = Instantiate(Part, Content).GetComponent<UI_Heros_Part>();
            part.Add(go);
            go.Initialize(data.Value, this);
        }

        return base.Init();
    }


    public void Disable_Fade()
    {
        Main_UI.instance.FadeInOut(false, true, () =>
        {
            Main_UI.instance.FadeInOut(true, false, null);
            Main_UI.instance.LayerCheck(-1);
            if(Stage_Manager.m_State == Stage_State.Play) Stage_Manager.State_Change(Stage_State.Ready, 0 , 0);
            base.DisableOBJ();
        });
    }


}
