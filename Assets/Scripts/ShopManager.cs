using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [SerializeField] private Image[] backgroundImages;


    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform scrollBar;

    [SerializeField] private Color selectedTabColor;
    [SerializeField] private Color unSelectedTabColor;

    [SerializeField] private Image backgroundTab;
    [SerializeField] private Image ballTab;

    [Header("---Backgrounds---")]

    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private Sprite[] shopItemBackgroundImages;

    [SerializeField] private int[] backgroundCosts;
    [SerializeField] private string[] backgroundKeys;

    private List<GameObject> backgroundShopItems;
    [SerializeField] private int numberOfPurchaseBackgrounds;
    [SerializeField] private float[] purchaseBackgroundCosts;

    [Header("---Balls---")]

    [SerializeField] private BallPallete[] balls;
    [SerializeField] private Sprite[] shopItemBallImages;

    [SerializeField] private int[] ballCosts;
    [SerializeField] private string[] ballKeys;

    private List<GameObject> ballShopItems;

    private string LAST_BACKGROUND_KEY = "lastBackground";
    private string LAST_BALL_KEY = "lastBall";

    [Header("---Scroll---")]
    [SerializeField] private VerticalLayoutGroup content;
    [SerializeField] private RectTransform contentBox;

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

        ballShopItems = new List<GameObject>();
        backgroundShopItems = new List<GameObject>();

        LoadBackgrounds();
        LoadBalls();

        InAppPurchaseManager.instance.SetupBuilder();

        ClickBackgroundTab();
    }

    public void SetBackground(int index)
    {
        foreach (Image image in backgroundImages)
        {
            image.sprite = backgrounds[index];
        }
        PlayerPrefs.SetInt(LAST_BACKGROUND_KEY, index);

        for (int i = 0; i < backgroundShopItems.Count; i++)
        {
            if (index == i)
            {
                backgroundShopItems[i].GetComponent<ShopItem>().Select();
            }
            else
            {
                backgroundShopItems[i].GetComponent<ShopItem>().DeSelect();
            }
        }
    }

    public void SetBalls(int index)
    {
        LevelManager.instance.HandleBallColorChange(balls[index].GetBallColors());
        PlayerPrefs.SetInt(LAST_BALL_KEY, index);

        for (int i = 0; i < ballShopItems.Count; i++)
        {
            if (index == i)
            {
                ballShopItems[i].GetComponent<ShopItem>().Select();
            }
            else
            {
                ballShopItems[i].GetComponent<ShopItem>().DeSelect();
            }
        }
    }

    public void ClickBallTab()
    {
        ballTab.color = selectedTabColor;
        backgroundTab.color = unSelectedTabColor;

        foreach (GameObject shopItem in ballShopItems)
        {
            shopItem.SetActive(true);
        }

        foreach (GameObject item in backgroundShopItems)
        {
            item.SetActive(false);
        }

        float height = content.padding.top + content.padding.bottom + ((ballShopItems.Count - 1) * content.spacing) + (ballShopItems.Count * shopItemPrefab.GetComponent<RectTransform>().rect.size.y);
        contentBox.sizeDelta = new Vector2(0, height);
    }

    public void ClickBackgroundTab()
    {
        backgroundTab.color = selectedTabColor;
        ballTab.color = unSelectedTabColor;

        foreach (GameObject shopItem in backgroundShopItems)
        {
            shopItem.SetActive(true);
        }

        foreach (GameObject item in ballShopItems)
        {
            item.SetActive(false);
        }

        float height = content.padding.top + content.padding.bottom + ((backgroundShopItems.Count - 1) * content.spacing) + (backgroundShopItems.Count * shopItemPrefab.GetComponent<RectTransform>().rect.size.y);
        contentBox.sizeDelta = new Vector2(0, height);
    }

    private void LoadBackgrounds()
    {
        for (int i = 0; i < backgroundKeys.Length; i++)
        {
            GameObject backgroundItem = Instantiate(shopItemPrefab, scrollBar);
            ShopItem newShopItem = backgroundItem.GetComponent<ShopItem>();

            if (PlayerPrefs.HasKey(backgroundKeys[i]))
            {
                if (PlayerPrefs.GetInt(backgroundKeys[i]) == 1)
                {
                    newShopItem.Initialize(shopItemBackgroundImages[i], backgroundCosts[i], true, backgroundKeys[i], i, true, i >= backgroundKeys.Length - numberOfPurchaseBackgrounds);
                    if (i >= backgroundKeys.Length - numberOfPurchaseBackgrounds)
                    {
                        InAppPurchaseManager.instance.catShopItem = newShopItem;
                    }
                    backgroundShopItems.Add(backgroundItem);
                    continue;
                }
            }

            newShopItem.Initialize(shopItemBackgroundImages[i], backgroundCosts[i], false, backgroundKeys[i], i, true, i >= backgroundKeys.Length - numberOfPurchaseBackgrounds);
            if (i >= backgroundKeys.Length - numberOfPurchaseBackgrounds)
            {
                InAppPurchaseManager.instance.catShopItem = newShopItem;
            }
            backgroundShopItems.Add(backgroundItem);
        }

        if (PlayerPrefs.HasKey(LAST_BACKGROUND_KEY))
        {
            SetBackground(PlayerPrefs.GetInt(LAST_BACKGROUND_KEY));
        }
        else
        {
            PlayerPrefs.SetInt(LAST_BACKGROUND_KEY, 0);
            SetBackground(0);
        }
    }

    private void LoadBalls()
    {
        for (int i = 0; i < ballKeys.Length; i++)
        {
            GameObject backgroundItem = Instantiate(shopItemPrefab, scrollBar);
            ShopItem newShopItem = backgroundItem.GetComponent<ShopItem>();

            if (PlayerPrefs.HasKey(ballKeys[i]))
            {
                if (PlayerPrefs.GetInt(ballKeys[i]) == 1)
                {
                    newShopItem.Initialize(shopItemBallImages[i], ballCosts[i], true, ballKeys[i], i, false, false);
                    ballShopItems.Add(backgroundItem);
                    continue;
                }
            }

            newShopItem.Initialize(shopItemBallImages[i], ballCosts[i], false, ballKeys[i], i, false, false);
            ballShopItems.Add(backgroundItem);
        }

        if (PlayerPrefs.HasKey(LAST_BALL_KEY))
        {
            SetBalls(PlayerPrefs.GetInt(LAST_BALL_KEY));
        }
        else
        {
            PlayerPrefs.SetInt(LAST_BALL_KEY, 0);
            SetBalls(0);
        }
    }

    public float GetPriceForBackgroundPurchase(int index)
    {
        return purchaseBackgroundCosts[index - (backgroundKeys.Length - numberOfPurchaseBackgrounds)];
    }
}
