using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialHelper : MonoBehaviour
{
    public List<string> infoStrings;
    private int index = 0;

    public GameObject gameLock;

    public TMP_Text infoText;

    public GameObject moveOnButton;
    public GameObject startGameButton;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        infoText.text = infoStrings[index];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveOn()
    {
        index++;
        if (index < infoStrings.Count - 1)
        {
            infoText.text = infoStrings[index];
        }
        else
        {
            moveOnButton.SetActive(false);
            startGameButton.SetActive(true);
        }

        if (index > 1)
        {
            gameLock.SetActive(false);
        }
    }


}
