using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public float activationTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            activationTime -= Time.deltaTime;

            if (activationTime < 0 )
            {
                gameObject.SetActive (false);
            }
        }
    }

    public void Activate(float time)
    {
        activationTime = time;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        activationTime = 0;
    }
}
