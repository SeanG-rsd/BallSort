using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]

public class NonConsumableItem
{
    public string Name;
    public string Id;
    public string Description;
    public float price;
}

[Serializable]

public class ConsumableItem
{
    public string Name;
    public string Id;
    public string Description;
    public float price;
}

public class InAppPurchaseManager : MonoBehaviour, IStoreListener
{
    public static InAppPurchaseManager instance;

    public NonConsumableItem catBackground;
    public NonConsumableItem nonConsumableItem;

    IStoreController storeController;

    [SerializeField] private GameObject removeAdsObj;
    [SerializeField] private GameObject removeAdsScreen;
    [SerializeField] private TMP_Text removeAdsPrice;
    [SerializeField] private TMP_Text removeAdsText;

    public static Action OnRemoveAds = delegate { };
    public static Action OnShowAds = delegate { };


    [Obsolete]
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Obsolete]
    public void SetupBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(catBackground.Id, ProductType.NonConsumable);
        builder.AddProduct(nonConsumableItem.Id, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);

        
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("Success");
        storeController = controller;
        CheckNonConsumable(nonConsumableItem.Id);
        CheckNonConsumable(catBackground.Id);
    }

    public void OnOpenRemoveAdsScreen()
    {
        removeAdsPrice.text = "$" + nonConsumableItem.price.ToString();
        removeAdsText.text = "Would you like to Remove all Ads from the game?";
        removeAdsScreen.SetActive(true);
    }

    public void OnCloseRemoveAdsScreen()
    {
        removeAdsScreen.SetActive(false);
    }

    public void OnBuyRemoveAds()
    {
        storeController.InitiatePurchase(nonConsumableItem.Id);
    }

    public ShopItem catShopItem;
    public void OnBuyCatBackground()
    {
        storeController.InitiatePurchase(catBackground.Id);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;

        Debug.Log($"Purchase Complete" + product.definition.id);

        if (product.definition.id == catBackground.Id)
        {
            BuyCatBackground();
        }
        else if (product.definition.id == nonConsumableItem.Id)
        {
            RemoveAds();
        }

        return PurchaseProcessingResult.Complete;
    }

    private void RemoveAds()
    {
        removeAdsScreen.SetActive(false);
        removeAdsObj.SetActive(false);
        OnRemoveAds?.Invoke();
    }

    private void BuyCatBackground()
    {
        catShopItem.SetBought();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("initialize failed : " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("initialize failed : " + error + message);
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("purchase failed : " + failureReason);
        if (product.definition.id == nonConsumableItem.Id)
        {
            removeAdsScreen.SetActive(false);
        }
    }

    private void CheckNonConsumable(string id)
    {
        var product = storeController.products.WithID(id);
        if (product != null)
        {
            if (product.definition.id == nonConsumableItem.Id)
            {
                RemoveAds();
            }
            else if (product.definition.id == catBackground.Id)
            {
                BuyCatBackground();
            }
        }
        else if (id == nonConsumableItem.Id)
        {
            OnShowAds?.Invoke();
        }
    }
}
