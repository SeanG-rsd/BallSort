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
    void Awake() 
    {
        HandleStartTutorial();
        MenuManager.OnStartTutorial += HandleStartTutorial;
    }

    private void OnDestroy()
    {
        MenuManager.OnStartTutorial -= HandleStartTutorial;
    }
    public void MoveOn() // is called when the next button is click and moves to the next message in the tutorial sequence
    {
        index++;
        if (index < infoStrings.Count - 1)
        {
            infoText.text = infoStrings[index];
        }
        else
        {
            infoText.text = infoStrings[index];
            moveOnButton.SetActive(false);
            startGameButton.SetActive(true);
        }

        if (index > 1)
        {
            gameLock.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void HandleStartTutorial()
    {
        index = 0;
        infoText.text = infoStrings[index];
        gameLock.SetActive(true);

        moveOnButton.SetActive(true);
        startGameButton.SetActive(false);
    }
}
