using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("---Menu---")]
    [SerializeField] private GameObject[] screens;
    [SerializeField] private int mainScreenIndex, levelScreenIndex, gameScreenIndex, makeScreenIndex, settingsScreenIndex, howToPlayScreenIndex;

    [Header("---Mode---")]
    [SerializeField] private GameObject modeButton;
    [SerializeField] private TMP_Text modeButtonText;
    private bool modeState = false; // false is normal, true is easy

    public static Action<bool> OnChangeMode = delegate { };
    public static Action OnGoToLevelsMenu = delegate { };

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

    public void OnClickSettingsScreen()
    {
        screens[settingsScreenIndex].SetActive(!screens[settingsScreenIndex].activeSelf);
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
                screens[index].SetActive(true);
            }
            else
            {
                screens[index].SetActive(false);
            }
        }
    }
}
