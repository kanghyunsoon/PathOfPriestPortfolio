using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
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

    // 게임 데이터를 불러오기 전에 들어온 결제 (결제 중 앱이 종료된 경우 재실행 시 다시 들어온다)
    private readonly List<Product> pendingProducts = new List<Product>();

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
        Product product = purchaseEvent.purchasedProduct;

        // 세이브를 불러오기 전에 지급하면 불러온 데이터에 덮여서 보상이 사라진다.
        // Pending으로 두고 게임이 시작되면 지급한 뒤 확정한다.
        if (!Base_Manager.GetGameStart)
        {
            if (!pendingProducts.Contains(product))
            {
                pendingProducts.Add(product);
            }
            return PurchaseProcessingResult.Pending;
        }

        return GrantPurchase(product) ? PurchaseProcessingResult.Complete : PurchaseProcessingResult.Pending;
    }

    public void ProcessPendingPurchases()
    {
        if (storeController == null || pendingProducts.Count == 0)
        {
            return;
        }

        foreach (Product product in pendingProducts.ToArray())
        {
            if (GrantPurchase(product))
            {
                pendingProducts.Remove(product);
                storeController.ConfirmPendingPurchase(product);
            }
        }
    }

    private bool GrantPurchase(Product product)
    {
        string purchaseEventName = product.definition.id;
        if (!Enum.TryParse(purchaseEventName, out IAP_Holder iap))
        {
            // 확정하지 않으면 스토어가 일정 기간 후 환불 처리한다.
            Debug.LogError("알 수 없는 상품입니다 : " + purchaseEventName);
            return false;
        }

        Base_Canvas.instance.Get_UI("#Reward");
        Utils.UI_Holder.Peek().GetComponent<UI_Reward>().GetIAPReward(iap);

        // 결제 보상은 10초 자동 저장을 기다리지 않고 바로 저장한다.
        Base_Manager.Firebase.WriteData();
        return true;
    }

    public void Purchase(string productId)
    {
        // 결제 초기화에 실패했거나 아직 끝나지 않았으면 storeController가 없다.
        Product product = GetProduct(productId);
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
            storeExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result, error) =>
            {
                Debug.Log("Restore Purchase completed : " + result + (string.IsNullOrEmpty(error) ? "" : " / " + error));
            });

        }
        else
        {
            Debug.Log("Restore Purchase is not supported on this platform");
        }
    }

    public Product GetProduct(string _productId)
    {
        return storeController != null ? storeController.products.WithID(_productId) : null;
    }

}
