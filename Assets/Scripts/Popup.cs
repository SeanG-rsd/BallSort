using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public float activationTime;
    void Update() // decreases time until the object get deactivated
    {
        if (gameObject.activeSelf)
        {
            activationTime -= Time.deltaTime;

            if (activationTime < 0 )
            {
                Deactivate();
            }
        }
    }
    public void Activate(float time) // activates the icon with a certain time
    {
        activationTime = time;
        gameObject.SetActive(true);
    }

    public void Deactivate() // deactivates the icon and resets it
    {
        gameObject.SetActive(false);
        activationTime = 0;
    }
}
