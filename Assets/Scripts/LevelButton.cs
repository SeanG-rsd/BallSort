using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int currentLevel;
    [SerializeField] private Image image;
    private bool currentCompleted;

    [SerializeField] private Color completedColor;
    [SerializeField] private Color normalColor;

    [SerializeField] private TMP_Text levelNumberText;

    [SerializeField] private Button button;

    public void SetLevel(int index, bool isCompleted)
    {
        currentLevel = index;
        currentCompleted = isCompleted;
        UpdateSelf();
    }

    public int GetLevelNumber()
    {
        return currentLevel;
    }
    public void SetColor(bool completed)
    {
        currentCompleted = completed;
        UpdateSelf();
    }

    public void UpdateSelf()
    {
        image.color = currentCompleted ? completedColor : normalColor;
        levelNumberText.text = currentLevel.ToString();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate { LevelManager.instance.OnClickLoadLevel(currentLevel); });
    }
}
