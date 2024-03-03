using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("---Menu---")]
    [SerializeField] private GameObject[] screens;
    [SerializeField] private int levelScreenIndex, gameScreenIndex, settingsScreenIndex, howToPlayScreenIndex, winScreenIndex, tutorialScreenIndex;

    [Header("---Mode---")]
    [SerializeField] private GameObject modeButton;
    [SerializeField] private TMP_Text modeButtonText;
    private bool modeState = false; // false is normal, true is easy
    [SerializeField] private TMP_Text coinText;
    private string coinKeyString = "CoinCount";

    // Tutorial
    private string tutorialKeyString = "TUTORIAL";


    public static Action<bool> OnChangeMode = delegate { };
    public static Action OnGoToLevelsMenu = delegate { };
    public static Action OnStartTutorial = delegate { };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (PlayerPrefs.HasKey(tutorialKeyString))
        {
            screens[tutorialScreenIndex].SetActive(false);
        }
        else
        {
            PlayerPrefs.SetString(tutorialKeyString, tutorialKeyString);
        }

        LevelManager.OnLoadLevel += HandleLoadLevel;
    }

    private void OnDestroy()
    {
        LevelManager.OnLoadLevel -= HandleLoadLevel;
    }

    private void Update()
    {
        coinText.text = LevelManager.instance.Coin.ToString();
        PlayerPrefs.SetInt(coinKeyString, LevelManager.instance.Coin);
    }

    private void HandleLoadLevel()
    {
        OpenMenuNumber(gameScreenIndex);
    }

    public void OnClickStartGame()
    {
        OpenMenuNumber(levelScreenIndex);
    }

    public void OnClickTutorial()
    {
        OnStartTutorial?.Invoke();
        OpenMenuNumber(tutorialScreenIndex);
    }

    public void OnClickModeChange()
    {
        modeState = !modeState;

        if (modeState)
        {
            modeButtonText.text = "Mode:   Easy";
        }
        else
        {
            modeButtonText.text = "Mode: Normal";
        }

        OnChangeMode?.Invoke(modeState);
    }

    public void ToggleWinScreen()
    {
        OnToggleScreen(winScreenIndex);
    }

    public void ToggleSettingsScreen()
    {
        if (screens[settingsScreenIndex].activeSelf)
        {
            screens[tutorialScreenIndex].SetActive(false);
        }
        OnToggleScreen(settingsScreenIndex);
        
    }

    public void ToggleHowToScreen()
    {
        OnToggleScreen(howToPlayScreenIndex);
    }

    private void OnToggleScreen(int screenIndex)
    {
        screens[screenIndex].SetActive(!screens[screenIndex].activeSelf);
    }

    public void OnClickLevelsButton()
    {
        OnGoToLevelsMenu?.Invoke();
        OpenMenuNumber(levelScreenIndex);
    }

    public void OnClickPlayTutorial()
    {
        PlayerPrefs.SetInt("Tutorial", 0);

        SceneManager.LoadScene("Tutorial");
    }

    private void OpenMenuNumber(int number)
    {
        for (int index = 0; index < screens.Length; index++)
        {
            if (index == number)
            {
                //Debug.Log(number);
                screens[number].SetActive(true);
            }
            else
            {
                screens[index].SetActive(false);
            }
        }
    }
}
