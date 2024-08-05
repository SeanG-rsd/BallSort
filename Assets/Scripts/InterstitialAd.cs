using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";
    string _adUnitId;

    private int winsBeforeAd;

    private int currentWinsLeft;

    private string CURRENT_ADS = "CURRENT_ADS_KEY";
    private bool removeAds = false;

    private int currentNumberOfLevelsCompleted;

    private void Awake()
    {
        if (PlayerPrefs.HasKey(CURRENT_ADS))
        {
            currentWinsLeft = PlayerPrefs.GetInt(CURRENT_ADS);
        }
        else
        {
            currentWinsLeft = winsBeforeAd;
            PlayerPrefs.SetInt(CURRENT_ADS, currentWinsLeft);
        }
        GameManager.OnWinScreen += LevelCompletion;
        InAppPurchaseManager.OnRemoveAds += HandleRemoveAds;

        // Get the Ad Unit ID for the current platform:
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;

        LoadAd();
    }

    private void OnDestroy()
    {
        GameManager.OnWinScreen -= LevelCompletion;
        InAppPurchaseManager.OnRemoveAds -= HandleRemoveAds;
    }

    private void LevelCompletion()
    {
        currentWinsLeft--;
        PlayerPrefs.SetInt(CURRENT_ADS, currentWinsLeft);
        Debug.Log("win");
        if (currentWinsLeft <= 0)
        {
            ShowAd();
        }
    }


    private void HandleRemoveAds()
    {
        removeAds = true;
    }
    // Load content to the Ad Unit:
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // Show the loaded content in the Ad Unit:
    public void ShowAd()
    {
        if (!removeAds)
        {
            Debug.Log("show ad");
            // Note that if the ad content wasn't previously loaded, this method will fail
            currentWinsLeft = winsBeforeAd;
            PlayerPrefs.SetInt(CURRENT_ADS, currentWinsLeft);
            Advertisement.Show(_adUnitId, this);
        }
    }

    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
    }

    public void OnUnityAdsShowStart(string _adUnitId) { }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState) { }
}
