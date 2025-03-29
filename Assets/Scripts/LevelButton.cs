using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    private int currentPage = 0;
    public int currentLevel;
    [SerializeField] private Image image;

    List<int> levelNumbers = new();
    List<bool> isCompleted = new();

    [SerializeField] private Color completedColor;
    [SerializeField] private Color normalColor;

    [SerializeField] private TMP_Text levelNumberText;

    [SerializeField] private Button button;

    public void AddNewLevel(int index, bool isCompleted)
    {
        levelNumbers.Add(index);
        this.isCompleted.Add(isCompleted);
    }

    public void SetPage(int newPage)
    {
        currentPage = newPage;
        currentLevel = levelNumbers[currentPage];
        UpdateSelf();
        levelNumberText.text = currentLevel.ToString();
        button.onClick.AddListener(delegate { LevelManager.instance.OnClickLoadLevel(currentLevel); });
    }

    public int GetLevelNumber()
    {
        return currentLevel;
    }

    public bool isCompletedAtPage(int page)
    {
        return isCompleted[page];
    }

    public void SetColor(bool completed)
    {
        this.isCompleted[currentPage] = completed;
        UpdateSelf();
    }

    public void UpdateSelf()
    {
        image.color = isCompleted[currentPage] ? completedColor : normalColor;
    }
}
