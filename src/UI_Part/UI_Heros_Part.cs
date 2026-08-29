using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Heros_Part : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image m_Slider, m_CharacterImage, m_RarityImage;
    [SerializeField] private TextMeshProUGUI m_Level, m_Count;
    [SerializeField] private GameObject GetLock;
    [SerializeField] private Button OnClickButton;
    [SerializeField] private Image Debuff;
    public GameObject LockOBJ;

    [SerializeField] private GameObject Plus, Minus;

    public Character_Scriptable m_Character;
    UI_Heros parent;

    private RectTransform dragRect;

    private void Start()
    {
        dragRect = parent.DragImage.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parent.DragImage == null || m_Character == null) return;

        parent.DragImage.sprite = Utils.Get_Atlas(m_Character.name);
        parent.DragImage.SetNativeSize();
        parent.DragImage.color = new Color(1, 1, 1, 0.5f);
        parent.DragImage.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragRect.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parent.DragImage != null)
            parent.DragImage.gameObject.SetActive(false);

        Vector2 mousePos = Input.mousePosition;


        for (int i = 0; i < parent.InitButtonsRect.Count; i++)
        {
            RectTransform slot = parent.InitButtonsRect[i];
            bool isInSide = RectTransformUtility.RectangleContainsScreenPoint(slot, mousePos, null);

            if (isInSide)
            {
                // 1. 해당 캐릭터가 이미 m_Set_Character에 들어가 있는지 확인
                for (int j = 0; j < Base_Manager.Data.m_Set_Character.Length; j++)
                {
                    var data = Base_Manager.Data.m_Set_Character[j];
                    if (data != null && data.Data == m_Character)
                    {
                        // 이미 있는 경우, 기존 위치 비움
                        Base_Manager.Data.m_Set_Character[j] = null;
                        break;
                    }
                }

                // 2. 새로운 위치에 설정
                parent.Set_M_Character(m_Character);
                parent.Set_Character_Button(i);

                break;
            }
        }
    }

    public void LockCheck(bool Lock)
    {
        switch(Lock)
        {
            case true:  LockOBJ.SetActive(true); Plus.SetActive(false); Minus.SetActive(true); break;
            case false: LockOBJ.SetActive(false); Plus.SetActive(true); Minus.SetActive(false); break;
        }
    }

    public void Initialize(Character_Scriptable data, UI_Heros parentBASE)
    {
        parent = parentBASE;
        m_Character = data;

        Init();


        m_RarityImage.sprite = Utils.Get_Atlas(data.m_Rarity.ToString());
        m_CharacterImage.sprite = Utils.Get_Atlas(data.m_CharacterName);
        m_CharacterImage.SetNativeSize() ;
        RectTransform rect = m_CharacterImage.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector3(rect.sizeDelta.x / 1.2f, rect.sizeDelta.y / 1.2f);

        Get_Character_Check();
    }

    public void Init()
    {
        int levelCount = (Base_Manager.Data.Character_Holder[m_Character.name].Level + 1) * 5;
        m_Slider.fillAmount = (float)Base_Manager.Data.Character_Holder[m_Character.name].Count / (float)levelCount;
        m_Count.text = Base_Manager.Data.Character_Holder[m_Character.name].Count.ToString() + "/" + levelCount.ToString();
        m_Level.text = "LV." + (Base_Manager.Data.Character_Holder[m_Character.name].Level + 1).ToString();
        Debuff.gameObject.SetActive(Base_Manager.Data.m_Data_Character[m_Character.name].isDebuff);
    }

    public void Get_Character_Check()
    {
        bool Get = false;
        for(int i = 0; i <Base_Manager.Data.m_Set_Character.Length; i++)
        {
            if(Base_Manager.Data.m_Set_Character[i] != null)
            {
                if (Base_Manager.Data.m_Set_Character[i].Data == m_Character)
                {
                    Get = true;
                }
            }

        }
        GetLock.SetActive(Get);
        Plus.SetActive(!Get);
        Minus.SetActive(Get);
    }


    public void Click_My_Button()
    {
       Render_Manager.instance.HERO.GetParticle(true);
        parent.Set_Click(this);
    }

    public void Click_My_Hero()
    {
        parent.GetHeroInformation(m_Character);
    }

}
