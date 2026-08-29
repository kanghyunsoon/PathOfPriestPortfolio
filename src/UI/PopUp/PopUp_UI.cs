using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PopUp_UI : MonoBehaviour
{
    RectTransform rect;
    [SerializeField] private Image IconImage;
    [SerializeField] private TextMeshProUGUI TitleText, RarityText, ExplaneText;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Base_Canvas.instance.popup = null;
            Destroy(this.gameObject);
        }
    }


    public void Item_PopUp(Item_Scriptable item, Vector2 pos)
    {
        rect.pivot = PivotPoint(pos);

        rect.anchoredPosition = pos;

        IconImage.sprite = Utils.Get_Atlas(item.name);
        TitleText.text = Local_Manager.local_Data[item.Item_Name].Get_Data();
        RarityText.text = Utils.String_Color_Rarity(item.rarity) + item.rarity.ToString() + "</color>";
        ExplaneText.text = string.Format(Local_Manager.local_Data[item.Item_DES].Get_Data(), 15, 30);
    }

    public Vector2 PivotPoint(Vector2 pos)
    {
        float xPos = pos.x > Screen.width / 2 ? 1.0f : 0.0f;
        float yPos = pos.y > Screen.height / 2 ? 1.0f : 0.0f;

        return new Vector2(xPos, yPos);
    }
}
