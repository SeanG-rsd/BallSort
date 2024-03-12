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
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private GameObject selectedObject;
    private int cost;
    private string key;
    private int index;
    private bool isBackground;

    public void Initialize(Sprite image, int price, bool bought, string key, int index, bool isBackground)
    {
        itemImage.sprite = image;
        this.price.text = price + " Coins";
        lockObject.SetActive(!bought);
        cost = price;
        this.key = key;
        this.index = index;
        this.isBackground = isBackground;

        if (price == 0)
        {
            lockObject.SetActive(false);
            itemName.text = "DEFAULT";
        }
    }

    public void Buy()
    {
        int coins = LevelManager.instance.Coin;

        if (coins >= cost)
        {
	    Debug.Log("can't buy");
            lockObject.SetActive(false);
            LevelManager.instance.RemoveCoins(cost);
            PlayerPrefs.SetInt(key, 1);
        }
    }

    public void Activate()
    {
        if (isBackground)
        {
            ShopManager.instance.SetBackground(index);
        }
        else
        {
            ShopManager.instance.SetBalls(index);
        }
    }

    public void Select()
    {
        selectedObject.SetActive(true);
    }

    public void DeSelect()
    {
        selectedObject.SetActive(false);
    }
}
