using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int levelNumber;
    [SerializeField] private Image image;
    public bool isCompleted;

    [SerializeField] private Color completedColor;
    [SerializeField] private Color normalColor;

    public void Initialize(int index)
    {
        levelNumber = index;
    }

    public int GetLevelNumber()
    {
        return levelNumber;
    }

    public void SetColor(Color c, bool isCompleted)
    {
        this.isCompleted = isCompleted;
        UpdateSelf();
    }

    public void UpdateSelf()
    {
        image.color = isCompleted ? completedColor : normalColor;
    }
}
