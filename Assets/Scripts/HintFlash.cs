using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HintFlash : MonoBehaviour
{
    private int direction = -1;
    float fadeSpeed = .65f;

    private bool isOn;

    public Color color;

    void Update() // increases and decreases transparency within a range when active
    {
        if (isOn)
        { 
            if (color.a > .15f && color.a <= 1)
            {
                float col = color.a + (fadeSpeed * Time.deltaTime * direction);
                color.a = col;
                gameObject.GetComponent<Image>().color = color;
            }
            if (color.a >= 0.95f)
            {
                direction = -1;
            }
            else if (color.a < .2f)
            {
                direction = 1;
            }
        }   
    }

    private void Flip()
    {
        isOn = !isOn;
        color.a = isOn ? 1 : 0;
        gameObject.GetComponent<Image>().color = color;
    }

    public void Activate(GameObject tube)
    {
        if (!isOn)
        {
            Flip();
        }
        transform.position = tube.transform.position;
    }

    public void Deactivate()
    {
        if (isOn)
        {
            Flip();
        }
    }
    
}
