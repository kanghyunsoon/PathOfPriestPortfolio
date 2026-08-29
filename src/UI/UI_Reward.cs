using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Reward : UI_Base
{
    public Image ItemImage;
    public TextMeshProUGUI CountText;



    public void GetIAPReward(IAP_Holder iapName)
    {
        switch(iapName)
        {
            case IAP_Holder.removeads: GetRewardInit("ADS", 0); break;
            case IAP_Holder.dia300_: GetRewardInit("Dia", 300); break;
            case IAP_Holder.dia900_: GetRewardInit("Dia", 660); break;
            case IAP_Holder.dia2500_: GetRewardInit("Dia", 3000); break;
            case IAP_Holder.dia5500_: GetRewardInit("Dia", 6000); break;
            case IAP_Holder.dia8500_: GetRewardInit("Dia", 10000); break;
            case IAP_Holder.dia12000_: GetRewardInit("Dia", 15000); break;
        }
    }

    public void GetRewardInit(string itemName, int count)
    {
        ItemImage.sprite = Utils.Get_Atlas(itemName);
        CountText.text = count <= 1 ? "" : "x" + count.ToString();

        switch(itemName)
        {
            case "Dia": Data_Manager.m_Data.Dia += count; break;
            case "ADS":
                Data_Manager.m_Data.ADS_Remove = true;
                UI_Shop.RemoveADS_LockOBJ.SetActive(true);
                break;
        }

        Main_UI.instance.TextCheck();
    }
}
