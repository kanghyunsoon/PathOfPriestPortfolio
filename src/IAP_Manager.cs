using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;
using UnityEngine.Purchasing.Extension;

public class IAP_Manager : IDetailedStoreListener
{
    public readonly string removeADS = "removeads";
    public readonly string gem01 = "dia300_";
    public readonly string gem02 = "dia900_";
    public readonly string gem03 = "dia2500_";
    public readonly string gem04 = "dia5500_";
    public readonly string gem05 = "dia8500_";
    public readonly string gem06 = "dia12000_";

    private IStoreController storeController;           // 구매 과정을 제어하는 함수 제공자
    private IExtensionProvider storeExtensionProvider;  // 여러 플랫폼을 위한 확정 처리 제공자

    public void InitUnityIAP()
    {
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(gem01, ProductType.Consumable, new IDs() { { gem01, GooglePlay.Name } });
        builder.AddProduct(gem02, ProductType.Consumable, new IDs() { { gem02, GooglePlay.Name } });
        builder.AddProduct(gem03, ProductType.Consumable, new IDs() { { gem03, GooglePlay.Name } });
        builder.AddProduct(gem04, ProductType.Consumable, new IDs() { { gem04, GooglePlay.Name } });
        builder.AddProduct(gem05, ProductType.Consumable, new IDs() { { gem05, GooglePlay.Name } });
        builder.AddProduct(gem06, ProductType.Consumable, new IDs() { { gem06, GooglePlay.Name } });
        builder.AddProduct(removeADS, ProductType.NonConsumable, new IDs() { { removeADS, GooglePlay.Name } });

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("초기화에 성공하였습니다.");

        storeController = controller;
        storeExtensionProvider = extensions;
    }

    void IStoreListener.OnInitializeFailed(UnityEngine.Purchasing.InitializationFailureReason error)
    {
        Debug.LogError("초기화에 실패하였습니다.");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError("초기화에 실패하였습니다.");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["PurchaseFailed"].Get_Data(), Color.red);
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["PurchaseFailed"].Get_Data(), Color.red);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        string purchaseEventName = purchaseEvent.purchasedProduct.definition.id;
        IAP_Holder iap = (IAP_Holder)Enum.Parse(typeof(IAP_Holder), purchaseEventName);

        Base_Canvas.instance.Get_UI("#Reward");
        Utils.UI_Holder.Peek().GetComponent<UI_Reward>().GetIAPReward(iap);

        return PurchaseProcessingResult.Complete;
    }

    public void Purchase(string productId)
    {
        Product product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            storeController.InitiatePurchase(product);
        }
        else
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["PurchaseFailed"].Get_Data(), Color.red);
        }
    }

    public void RestorePurchase()
    {
        if(storeController == null)
        {
            Debug.Log("IAP is not initailize");
            return;
        }

        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            storeExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
            {
                Debug.Log("Restore Purchase completed : " + result);
            });

        }
        else
        {
            Debug.Log("Restore Purchase is not supported on this platform");
        }
    }

    public Product GetProduct(string _productId)
    {
        return storeController.products.WithID(_productId);
    }

}
