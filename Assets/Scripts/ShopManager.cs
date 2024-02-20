using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private Image[] backgroundImages;
    [SerializeField] private Sprite[] backgrounds;

    [SerializeField] private GameObject[] locks;
    [SerializeField] private int[] costs;
    [SerializeField] private string[] keys;

    private string LAST_KEY = "lastBackground";

    private void Awake()
    {
        LoadBackgrounds();
    }

    public void BuyBackground(int index)
    {
        int coins = LevelManager.instance.Coin;

        if (coins >= costs[index])
        {
            locks[index].SetActive(false);
            PlayerPrefs.SetInt(keys[index], 1);
            LevelManager.instance.RemoveCoins(costs[index]);
        }
    }

    public void SetBackground(int index)
    {
        foreach (Image image in backgroundImages)
        {
            image.sprite = backgrounds[index];
        }
        PlayerPrefs.SetInt(LAST_KEY, index);
    }

    private void LoadBackgrounds()
    {
        if (PlayerPrefs.HasKey(LAST_KEY))
        {
            SetBackground(PlayerPrefs.GetInt(LAST_KEY));
        }
        else
        {
            PlayerPrefs.SetInt(LAST_KEY, 0);
            SetBackground(0);
        }

        for (int i = 1; i < keys.Length; i++)
        {
            if (PlayerPrefs.HasKey(keys[i]))
            {
                if (PlayerPrefs.GetInt(keys[i]) == 1)
                {
                    locks[i].SetActive(false);
                }
            }

            locks[i].GetComponent<ShopLock>().SetPrice(costs[i]);
        }
    }
}
