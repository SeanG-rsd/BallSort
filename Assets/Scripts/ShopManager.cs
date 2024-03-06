using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [SerializeField] private Image[] backgroundImages;


    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform scrollBar;

    [Header("---Backgrounds---")]

    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private Sprite[] shopItemBackgroundImages;

    [SerializeField] private int[] backgroundCosts;
    [SerializeField] private string[] backgroundKeys;

    private List<GameObject> backgroundShopItems;

    [Header("---Balls---")]

    [SerializeField] private BallPallete[] balls;
    [SerializeField] private Sprite[] shopItemBallImages;

    [SerializeField] private int[] ballCosts;
    [SerializeField] private string[] ballKeys;

    private List<GameObject> ballShopItems;

    private string LAST_BACKGROUND_KEY = "lastBackground";
    private string LAST_BALL_KEY = "lastBall";

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

        ClickBallTab();
    }

    public void SetBackground(int index)
    {
        foreach (Image image in backgroundImages)
        {
            image.sprite = backgrounds[index];
        }
        PlayerPrefs.SetInt(LAST_BACKGROUND_KEY, index);
    }

    public void SetBalls(int index)
    {

    }

    public void ClickBallTab()
    {
        foreach (GameObject shopItem in ballShopItems)
        {
            shopItem.SetActive(true);
        }

        foreach (GameObject item in backgroundShopItems)
        {
            item.SetActive(false);
        }
    }

    public void ClickBackgroundTab()
    {
        foreach (GameObject shopItem in backgroundShopItems)
        {
            shopItem.SetActive(true);
        }

        foreach (GameObject item in ballShopItems)
        {
            item.SetActive(false);
        }
    }

    private void LoadBackgrounds()
    {
        if (PlayerPrefs.HasKey(LAST_BACKGROUND_KEY))
        {
            SetBackground(PlayerPrefs.GetInt(LAST_BACKGROUND_KEY));
        }
        else
        {
            PlayerPrefs.SetInt(LAST_BACKGROUND_KEY, 0);
            SetBackground(0);
        }

        for (int i = 0; i < backgroundKeys.Length; i++)
        {
            GameObject backgroundItem = Instantiate(shopItemPrefab, scrollBar);
            ShopItem newShopItem = backgroundItem.GetComponent<ShopItem>();

            if (PlayerPrefs.HasKey(backgroundKeys[i]))
            {
                if (PlayerPrefs.GetInt(backgroundKeys[i]) == 1)
                {
                    newShopItem.Initialize(shopItemBackgroundImages[i], backgroundCosts[i], true, backgroundKeys[i], i);
                    backgroundShopItems.Add(backgroundItem);
                    continue;
                }
            }

            newShopItem.Initialize(shopItemBackgroundImages[i], backgroundCosts[i], false, backgroundKeys[i], i);
            backgroundShopItems.Add(backgroundItem);
        }
    }

    private void LoadBalls()
    {
        if (PlayerPrefs.HasKey(LAST_BALL_KEY))
        {
            SetBalls(PlayerPrefs.GetInt(LAST_BALL_KEY));
        }
        else
        {
            PlayerPrefs.SetInt(LAST_BALL_KEY, 0);
            SetBackground(0);
        }

        for (int i = 0; i < ballKeys.Length; i++)
        {
            GameObject backgroundItem = Instantiate(shopItemPrefab, scrollBar);
            ShopItem newShopItem = backgroundItem.GetComponent<ShopItem>();

            if (PlayerPrefs.HasKey(ballKeys[i]))
            {
                if (PlayerPrefs.GetInt(ballKeys[i]) == 1)
                {
                    newShopItem.Initialize(shopItemBallImages[i], ballCosts[i], true, ballKeys[i], i);
                    backgroundShopItems.Add(backgroundItem);
                    continue;
                }
            }

            newShopItem.Initialize(shopItemBallImages[i], ballCosts[i], false, ballKeys[i], i);
            ballShopItems.Add(backgroundItem);
        }
    }
}
