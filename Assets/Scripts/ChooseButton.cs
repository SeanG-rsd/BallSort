using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseButton : MonoBehaviour
{
    public int levelValue;

    public GameObject GameManager;

    public Button button;

    

    // Start is called before the first frame update
    void Awake()
    {
        GameManager = GameObject.Find("GameManager");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadLevel()
    {
        GameManager.GetComponent<LevelCreator>().LoadLevel(levelValue - 1);
    }

    void LoadChallenge()
    {
        GameManager.GetComponent<LevelCreator>().LoadChallengeLevel(levelValue - 1);
    }

    public void Challenge()
    {
        button.onClick.AddListener(LoadChallenge);
    }

    public void Level()
    {
        button.onClick.AddListener(LoadLevel);
    }
}
