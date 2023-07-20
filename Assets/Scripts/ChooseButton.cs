using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseButton : MonoBehaviour
{
    public int levelValue;

    public GameObject GameManager;

    public Button button;
    void Awake()
    {
        GameManager = GameObject.Find("GameManager");
    }
    void LoadLevel() // load the level value
    {
        GameManager.GetComponent<LevelCreator>().LoadLevel(levelValue - 1);
    }

    void LoadChallenge() // load the level value
    {
        GameManager.GetComponent<LevelCreator>().LoadChallengeLevel(levelValue - 1);
    }

    public void Challenge() // button initialization
    {
        button.onClick.AddListener(LoadChallenge);
    }

    public void Level() // button initialization
    {
        button.onClick.AddListener(LoadLevel);
    }
}
