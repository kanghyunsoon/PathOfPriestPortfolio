using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IAP_LocalText : MonoBehaviour
{
    public string productID;

    private void Start()
    {
        InitIAPText();
    }

    private void InitIAPText()
    {
        if(GetComponent<TextMeshProUGUI>() != null)
        {
            GetComponent<TextMeshProUGUI>().text = string.Format("{0} {1}",
                Base_Manager.IAP.GetProduct(productID).metadata.localizedPrice, // 그리스 기준 2.69
                Base_Manager.IAP.GetProduct(productID).metadata.isoCurrencyCode); // 그리스 기준(EUR)
        }
        else
        {
            GetComponent<Text>().text = string.Format("{0} {1}",
                Base_Manager.IAP.GetProduct(productID).metadata.localizedPrice,
                Base_Manager.IAP.GetProduct(productID).metadata.isoCurrencyCode);
        }
    }
}
