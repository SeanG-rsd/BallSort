using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private TMP_Text price;
    private int cost;
    private string key;
    private int index;

    public void Initialize(Sprite image, int price, bool bought, string key, int index)
    {
        itemImage.sprite = image;
        this.price.text = price + " Coins";
        lockObject.SetActive(!bought);
        cost = price;
        this.key = key;
        this.index = index;

        if (price == 0)
        {
            lockObject.SetActive(false);
        }
    }

    public void Buy()
    {
        int coins = LevelManager.instance.Coin;

        if (coins >= cost)
        {
            lockObject.SetActive(false);
            LevelManager.instance.RemoveCoins(cost);
            PlayerPrefs.SetInt(key, 1);
        }
    }

    public void Activate()
    {
        ShopManager.instance.SetBackground(index);
    }
}
