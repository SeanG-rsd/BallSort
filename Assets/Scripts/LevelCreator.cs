using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCreator : MonoBehaviour
{
    public List<GameObject> tubes;
    public List<GameObject> balls;

    public List<Material> mats;

    List<List<int>> level; // a list of each tube in a level, containing a list of each ball in each level

    public List<List<List<int>>> levels; // a list of level

    public List<int> chosen;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(levels[0][0]);
    }

    public void ChangedValue(GameObject dropdown)
    {

    }

    public void LoadLevel(int index)
    {
        List<GameObject> Gametubes = GetComponent<GameManager>().tubes;
         
        for (int i = 0; i < levels[index].Count; ++i) // each tube
        {
            level = levels[i];
            for (int ii = 0; ii < levels[index][i].Count; ++ii) // each ball
            {
                Gametubes[i].transform.GetChild(i).GetChild(0).gameObject.GetComponent<Image>().color = mats[level[i][ii]].color;
            }
        }
    }
}
