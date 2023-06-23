using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HintFlash : MonoBehaviour
{
    public int direction = -1;
    public int index = 0;
    public Vector2 tubes = Vector2.zero;

    float fadeSpeed = .65f;

    public bool playerMadeMove = false;

    public Color color;
    GameObject gm;

    // Start is called before the first frame update
    void Awake()
    {
        color = gameObject.GetComponent<Image>().color;
        gm = GameObject.Find("GameManager");
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
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

        if (playerMadeMove)
        {
            tubes = Vector2.zero;
            gm.GetComponent<LevelCreator>().lookingForHint = false;
            Debug.Log("reset");
            index = 0;
            playerMadeMove = false;
            gameObject.SetActive(false);
        }
    
    }
    
}
