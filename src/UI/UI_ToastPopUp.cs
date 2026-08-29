using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_ToastPopUp : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ToastPopUpText;
    public void Initialize(string temp, Color color)
    {
        ToastPopUpText.color = color;
        ToastPopUpText.text = temp;
        Destroy(this.gameObject, 2.0f);
    }
}
