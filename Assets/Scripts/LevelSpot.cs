using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpot : MonoBehaviour
{
    public List<GameObject> levelSpots = new List<GameObject>();

    public void SetLevel(int level, bool completed)
    {
        
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

    public void AddNewLevel(GameObject level)
    {
        levelSpots.Add(level);
    }
}
