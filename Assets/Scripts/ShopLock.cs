using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopLock : MonoBehaviour
{
    [SerializeField] private TMP_Text priceText;

    public void SetPrice(int price)
    {
        priceText.text = price + " Coins";
    }
}
