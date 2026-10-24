using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
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
        // 결제 초기화 전에 상점을 열면 상품 정보가 없다. 예외 대신 가격 표시를 비워 둔다.
        Product product = Base_Manager.IAP.GetProduct(productID);
        string price = product != null
            ? string.Format("{0} {1}", product.metadata.localizedPrice, product.metadata.isoCurrencyCode)
            : "";

        if(GetComponent<TextMeshProUGUI>() != null)
        {
            GetComponent<TextMeshProUGUI>().text = price;
        }
        else
        {
            GetComponent<Text>().text = price;
        }
    }
}
