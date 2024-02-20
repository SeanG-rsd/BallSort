using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int levelNumber;
    [SerializeField] private Image image;
    public bool isCompleted;

    [SerializeField] private Color completedColor;
    [SerializeField] private Color normalColor;

    [SerializeField] private TMP_Text levelNumberText;

    [SerializeField] private Button button;

    public void Initialize(int index)
    {
        levelNumber = index;
        levelNumberText.text = index.ToString();
        button.onClick.AddListener(delegate { LevelManager.instance.OnClickLoadLevel(index - 1); });
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public void SetColor(bool isCompleted)
    {
        this.isCompleted = isCompleted;
        UpdateSelf();
    }

    public void UpdateSelf()
    {
        image.color = isCompleted ? completedColor : normalColor;
    }
}
