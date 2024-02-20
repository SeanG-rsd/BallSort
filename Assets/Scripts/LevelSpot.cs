using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpot : MonoBehaviour
{
    public List<GameObject> levelSpots;

    private void Awake()
    {
        levelSpots = new List<GameObject>();
    }

    public void SetLevel(int level, bool completed)
    {
        levelSpots[level].GetComponent<LevelButton>().SetColor(completed);
    }

    public int GetLevelNumber(int levelIndex)
    {
        return levelSpots[levelIndex].GetComponent<LevelButton>().levelNumber;
    }

    public bool GetLevel(int levelIndex)
    {
        return levelSpots[levelIndex].GetComponent<LevelButton>().isCompleted;
    }

    public void UpdateLevel(int levelIndex)
    {
        levelSpots[levelIndex].GetComponent<LevelButton>().UpdateSelf();
    }

    public void AddNewLevel(GameObject level, int index)
    {
        level.GetComponent<LevelButton>().Initialize(index);
        levelSpots.Add(level);
    }

    public void SetPage(int page)
    {
        for (int index = 0;  index < levelSpots.Count; index++)
        {
            if (index == page)
            {
                levelSpots[index].SetActive(true);
            }
            else
            {
                levelSpots[index].SetActive(false);
            }
        }
    }
}
