using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Inventory : UI_Base
{
    [SerializeField] private ItemType m_InventoryState;
    [SerializeField] private RectTransform m_Bar;
    [SerializeField] private Button[] m_Top_Buttons;
    [SerializeField] private RectTransform TopContent;
    [SerializeField] TextMeshProUGUI GoldText;
    [SerializeField] private Transform Content;
    public UI_Inventory_Part part;
    List<GameObject> Garvage = new List<GameObject>();
    public override bool Init()
    {
        if(Garvage.Count > 0)
        {
            for (int i = 0; i < Garvage.Count; i++) Destroy(Garvage[i]);
            Garvage.Clear();
        }

        var sort_dictionary = Base_Manager.Data.GetOwnedItems().OrderByDescending(x => x.Value.Data.rarity);

        foreach (var item in sort_dictionary)
        {


            if(Base_Manager.Data.Item_Holder[item.Key].Count > 0)
            {
                if(m_InventoryState == ItemType.ALL)
                {
                    var go = Instantiate(part, Content);
                    go.Init(item.Key, Base_Manager.Data.Item_Holder[item.Key]);

                    Garvage.Add(go.gameObject);
                }
                else
                {
                    if(Base_Manager.Data.Item_Holder[item.Key].Count > 0 && m_InventoryState == Base_Manager.Data.m_Data_Item[item.Key].Data.type)
                    {
                        var go = Instantiate(part, Content);
                        go.Init(item.Key, Base_Manager.Data.Item_Holder[item.Key]);

                        Garvage.Add(go.gameObject);
                    }
                }

            }
        }

        for(int i =0; i< m_Top_Buttons.Length; i++)
        {
            int index = i;
            m_Top_Buttons[index].onClick.RemoveAllListeners();
            m_Top_Buttons[index].onClick.AddListener(() => Item_Inventory_Check((ItemType)index));
        }

        GoldText.text = StringMethod.ToCurrencyString(Data_Manager.m_Data.Money);

        return base.Init();
    }

    public void Item_Inventory_Check(ItemType m_State)
    {
        m_InventoryState = m_State;
        StartCoroutine(barMovementCoroutine(m_Top_Buttons[(int)m_State].GetComponent<RectTransform>().anchoredPosition,
            m_Top_Buttons[(int)m_State].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x));
        Init();
    }

    IEnumerator barMovementCoroutine(Vector2 endPos, float endXPos)
    {
        float current = 0;
        float percent = 0;
        Vector2 start = m_Bar.anchoredPosition;
        Vector2 end = new Vector2(endPos.x, TopContent.anchoredPosition.y);

        float startX = m_Bar.sizeDelta.x;
        float endX = endXPos + 50.0f;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.1f;
            Vector2 LerpPos = Vector2.Lerp(start, end, percent);
            float LerpPosX = Mathf.Lerp(startX, endX,percent);

            m_Bar.anchoredPosition = LerpPos;
            m_Bar.sizeDelta = new Vector2(Mathf.Clamp(LerpPosX,200.0f,Mathf.Infinity), m_Bar.sizeDelta.y);
            yield return null;
        }
    }
}
