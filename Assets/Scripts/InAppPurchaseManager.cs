using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAppPurchaseManager : MonoBehaviour
{
    [SerializeField] private GameObject noAdsButton;
    private static string PURCHASE_KEY = "AD_REMOVE";

    public static Action OnRemoveAds = delegate { };

    private void Start()
    {
        if (PlayerPrefs.GetInt(PURCHASE_KEY) == 1)
        {
            RemoveAds();
        }
    }
    public void OnBuyRemoveAds()
    {
        Debug.Log("success");
        RemoveAds();
    }

    public void OnFailBuyingRemoveAds()
    {
        Debug.Log("failure");
    }

    private void RemoveAds()
    {
        PlayerPrefs.SetInt(PURCHASE_KEY, 1);
        noAdsButton.SetActive(false);
        OnRemoveAds?.Invoke();
    }

    public void OnTransactionsRestores()
    {
        Debug.Log("restored");
    }
}
