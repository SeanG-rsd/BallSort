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
    public string Question;
}

public class InAppPurchaseManager : IAPListener, IStoreListener
{
    public static InAppPurchaseManager instance;

    public NonConsumableItem catBackground;
    public NonConsumableItem nonConsumableItem;

    IStoreController storeController;

    [SerializeField] private GameObject removeAdsObj;
    [SerializeField] private GameObject purchaseScreen;
    [SerializeField] private TMP_Text purchasePrice;
    [SerializeField] private TMP_Text purchaseQuestion;

    public static Action OnRemoveAds = delegate { };
    public static Action OnShowAds = delegate { };

    private string currentPurchase = "";


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
        Debug.Log("setup");
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

    public void OnOpenRemoveAdsScreen(string id)
    {
        purchasePrice.text = "$" + (id == nonConsumableItem.Id ? nonConsumableItem.price.ToString() : catBackground.price.ToString());
        purchaseQuestion.text = id == nonConsumableItem.Id ? "Would you like to Remove all Ads from the game?" : "Would you like to purchase the Cat Background?";
        purchaseScreen.SetActive(true);
        Debug.Log(id);
        currentPurchase = id;
    }

    public void OnCloseRemoveAdsScreen()
    {
        purchaseScreen.SetActive(false);
    }

    public void OnBuyRemoveAds()
    {
        storeController.InitiatePurchase(nonConsumableItem.Id);
    }

    public void OnConfirmPurchase()
    {
        if (currentPurchase == nonConsumableItem.Id)
        {
            OnBuyRemoveAds();
        }
        else
        {
            OnBuyCatBackground();
        }
    }

    public ShopItem catShopItem;
    public void OnBuyCatBackground()
    {
        storeController.InitiatePurchase(catBackground.Id);
    }

    PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs purchaseEvent)
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

        purchaseScreen.SetActive(false);

        return PurchaseProcessingResult.Complete;
    }

    private void RemoveAds()
    {
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


    void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("purchase failed : " + failureReason);
        purchaseScreen.SetActive(false);
        if (product.definition.id == catBackground.Id)
        {
            catShopItem.SetNotBought();
        }
    }

    private void CheckNonConsumable(string id)
    {
        var product = storeController.products.WithID(id);
        if (product != null)
        {
            if (product.definition.id == nonConsumableItem.Id)
            {
                Debug.Log("already has no ads");
                RemoveAds();
                return;
            }
            else if (product.definition.id == catBackground.Id)
            {
                Debug.Log("already has cat background");
                BuyCatBackground();
                return;
            }
        }
        else if (id == nonConsumableItem.Id)
        {
            OnShowAds?.Invoke();
            Debug.Log("show ads");
        }
    }
}
