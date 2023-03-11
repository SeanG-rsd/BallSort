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

        button.onClick.AddListener(LoadLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadLevel()
    {
        GameManager.GetComponent<LevelCreator>().LoadLevel(levelValue - 1);
    }
}
