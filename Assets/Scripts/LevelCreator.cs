using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Events;

// 1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12-    one level

public class LevelCreator : MonoBehaviour
{
    public List<GameObject> tubes;
    public List<GameObject> balls;

    public List<Material> mats;

    int lastLevelLoaded = 0;

    public List<List<List<int>>> levels = new List<List<List<int>>>(); // a list of level

    public List<int> chosen;

    public string savedLevels = "";

    public List<GameObject> spots;
    public GameObject chooseButtonPrefab;

    public GameObject saveButton;

    GameObject button;

    public GameObject pageLeftButton;
    public GameObject pageRightButton;
    List<List<GameObject>> levelButtons = new List<List<GameObject>>();
    int currentLevelPage = 0;

    int spotCount = 0;
    int actualCount = 1;

    public Material completedMat;
    public Material blankMat;
    public string completedSave = "";

    // Start is called before the first frame update
    void Start()
    {
        
        savedLevels = GetLevels("Assets/Resources/Levels.txt");
        LoadGame();
        LoadLevelChooseList();
        LoadCompleted();
        Debug.Log(levelButtons.Count);

        
    }

    // Update is called once per frame
    void Update()
    {
        saveButton.GetComponent<Button>().interactable = FinishedMaking();

        //Debug.Log("Level Count: " + levels.Count);

    }

    public void ChangedValue(GameObject dropdown) // changed a value in the maker to update chosen
    {
        

        for (int i = 0; i < tubes.Count; ++i) // i is the tube
        {
            for (int ii = 0; ii < tubes[i].transform.childCount; ++ii) // ii is the dropdown
            {
                if (dropdown.GetComponent<TMP_Dropdown>().value != 0)
                {
                    if (tubes[i].transform.GetChild(ii).gameObject == dropdown && chosen[dropdown.GetComponent<TMP_Dropdown>().value - 1] < 4)
                    {
                        chosen[dropdown.GetComponent<TMP_Dropdown>().value - 1]++;
                        UpdateChosen();
                        return;
                    }
                    else if (tubes[i].transform.GetChild(ii).gameObject == dropdown && chosen[dropdown.GetComponent<TMP_Dropdown>().value - 1] >= 4)
                    {
                        dropdown.GetComponent<TMP_Dropdown>().value = 0;
                        return;
                    }
                }
            }
        }

        
    }

    public void SaveLevel() // save a level after completing it in the maker
    {
        if (!FinishedMaking()) { return; }

        List<List<int>> level = new List<List<int>>();

        for (int i = 0; i < tubes.Count; ++i)
        {
            List<int> newTube = new List<int>();

            for (int ii = 0; ii < tubes[i].transform.childCount; ++ii)
            {
                int add = tubes[i].transform.GetChild(ii).gameObject.GetComponent<TMP_Dropdown>().value - 1;

                newTube.Add(add);
            }

            level.Add(newTube);
        }

        List<List<int>> newLevel = level;

        levels.Add(newLevel);
        AddToLevelList(newLevel);

        AddToDatabase(levels.Count - 1);
        WriteLevels("Assets/Resources/Levels.txt");

        ResetMaker();

    }

    public void WriteLevels(string path) // save the string of levels to a text file
    {
        StreamWriter writer = new StreamWriter(path);

        writer.WriteLine(savedLevels);

        writer.Close();

    }

    void UpdateChosen() // update the level maker counter
    {
        List<int> newChosen = new List<int>();

        for (int i = 0; i < 12; ++i)
        {
            newChosen.Add(0);
        }

        
        
        for (int i = 0; i < tubes.Count; ++i)
        {

            for (int ii = 0; ii < tubes[i].transform.childCount; ++ii)
            {

                if (tubes[i].transform.GetChild(ii).gameObject.GetComponent<TMP_Dropdown>().value != 0) { newChosen[tubes[i].transform.GetChild(ii).gameObject.GetComponent<TMP_Dropdown>().value - 1]++; }
                Debug.Log(newChosen[i]);
                
            }

        }

        chosen = newChosen;
    }

    public bool FinishedMaking() // check to see if all the dropdowns are filled in the level maker
    {
        for (int i = 0; i < chosen.Count; ++i)
        {
            if (chosen[i] != 4) { return false; }
        }

        return true;
    }

    public void ResetMaker() // reset the dropdowns for the level maker
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            chosen[i] = 0;

            for (int ii = 0; ii < tubes[i].transform.childCount; ++ii)
            {
                tubes[i].transform.GetChild(ii).gameObject.GetComponent<TMP_Dropdown>().value = 0;
            }
        }
    }

    

    public void AddToLevelList(List<List<int>> newLevel)
    {
        

        GameObject newButton = Instantiate(chooseButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        newButton.transform.SetParent(spots[spotCount].transform);
        newButton.transform.position = spots[spotCount].transform.position;
        newButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        newButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "Level " + actualCount;
        newButton.GetComponent<ChooseButton>().levelValue = actualCount;



        spotCount++;
        actualCount++;

        
        //Debug.Log("page count: " + page.Count);

        if (spotCount > 7)
        {
            spotCount = 0;
            List<GameObject> page = new List<GameObject>();
            page.Add(newButton);
            levelButtons.Add(page);
        }
        else
        {
            levelButtons[levelButtons.Count - 1].Add(newButton);
        }

        UpdateListPage();
    }

    public void LoadLevelChooseList() // create the initial list of levels that you can click on to load
    {
        Debug.Log("Load Level Choose List");

        
        List<GameObject> page = new List<GameObject>();
        

        if (levels.Count > 0)
        {
            

            for (int i = 0; i < levels.Count; ++i)
            {
                



                GameObject newButton = Instantiate(chooseButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newButton.transform.SetParent(spots[spotCount].transform);
                newButton.transform.position = spots[spotCount].transform.position;
                newButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                newButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "Level " + actualCount;
                newButton.GetComponent<ChooseButton>().levelValue = actualCount;



                spotCount++;
                actualCount++;
                
                page.Add(newButton);
                //Debug.Log("page count: " + page.Count);

                if (spotCount > 7)
                { 
                    spotCount = 0;
                    List<GameObject> add = new List<GameObject>();
                    
                    levelButtons.Add(page);

                    page = add;

                    


                }
                



            }
        }

        
        levelButtons.Add(page);

        

        currentLevelPage = 0;
        UpdateListPage();
        UpdatePageButtons();
    }

    void UpdateListPage()
    {
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            if (currentLevelPage == i)
            {
               
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    //Debug.Log(levelButtons[i][ii].name);
                    levelButtons[i][ii].SetActive(true);
                }
            }
            else
            {
                
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    levelButtons[i][ii].SetActive(false);
                }
            }
        }
    }

    void UpdatePageButtons()
    {
        if (currentLevelPage == 0) { pageLeftButton.GetComponent<Button>().interactable = false; }
        else if (!pageLeftButton.GetComponent<Button>().interactable) { pageLeftButton.GetComponent<Button>().interactable = true; }
        if (currentLevelPage == levelButtons.Count - 1) { pageRightButton.GetComponent<Button>().interactable = false; }
        else if (!pageRightButton.GetComponent<Button>().interactable) { pageRightButton.GetComponent<Button>().interactable = true; }
    }

    public void PageLeft()
    {
        currentLevelPage--;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageRight()
    {
        currentLevelPage++;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void LoadLevel(int index) // load a certain level based on the index given
    {
        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;


        //index = button.GetComponent<ChooseButton>().levelValue - 1;

        //Debug.Log("Level Count: " + levels.Count);

        if (levels.Count > 0)
        {
            for (int i = 0; i < levels[index].Count; ++i) // each tube
            {
                int mat = 0;
                for (int ii = 1; ii <= levels[index][i].Count; ++ii) // each ball
                {
                    

                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = mats[levels[index][i][mat]].color;

                    mat++;

                    //Debug.Log("changed color");
                }

            }
        }
        else
        {
            Debug.Log("no levels");
            Debug.Log("Level Count: " + levels.Count);

        }

        gameObject.GetComponent<GameManager>().menuNum = 2;
        lastLevelLoaded = index;
    }

    public void LoadLastLevel()
    {
        LoadLevel(lastLevelLoaded);
    }

    public void BeatLastLevel()
    {
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (lastLevelLoaded + 1 == levelButtons[i][ii].GetComponent<ChooseButton>().levelValue)
                {
                    levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                }
            }
        }
    }

    public void BeatIndexLevel(int index)
    {
        Debug.Log(index);
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (index == levelButtons[i][ii].GetComponent<ChooseButton>().levelValue - 1)
                {
                    levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                }
            }
        }
    }

    public void AddToDatabase(int index) // add a new level to the saved string
    {
        string level = "";

        for (int i = 0; i < levels[index].Count; ++i)
        {
            string tube = "";

            for (int ii = 0; ii < levels[index][i].Count; ++ii)
            {
                string ball = levels[index][i][ii].ToString();

                if (ii == levels[index][i].Count - 1) { tube = tube + ball; }
                else { tube = tube + ball + ","; }
            }

            if (i == levels[index].Count - 1) { level = level + tube; }
            else { level = level + tube + ":"; }
        }

        level = level + "-";

        savedLevels = savedLevels + level;
    }

    public void LoadCompleted()
    {
        
        if (PlayerPrefs.HasKey("SavedString"))
        {
            completedSave = PlayerPrefs.GetString("SavedString");
            Debug.Log(completedSave);
        }
        else { Debug.LogError("There is no save data"); }

        List<int> completed = new List<int>();
        string set = "";

        if (completedSave.Length > 0)
        {
            for (int i = 0; i < completedSave.Length; ++i)
            {

                if (completedSave[i] != ',')
                {
                    set = set + completedSave[i];

                }
                else if (completedSave[i] == ',')
                {
                    int add = System.Convert.ToInt32(set);
                    completed.Add(add);
                    set = "";
                    Debug.Log(completed.Count);
                }
            }
        }

        Debug.Log(completed.Count);

        for (int index = 0; index < completed.Count; ++index)
        {
            Debug.Log("complete load: " + completed[index]);
            for (int i = 0; i < levelButtons.Count; ++i)
            {
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    if (levelButtons[i][ii].GetComponent<ChooseButton>().levelValue == completed[index])
                    {
                        Debug.Log("loadedCom: " + index);
                        levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    }
                }
            }
        }
    }

    public void ResetData() // reset database
    {
        PlayerPrefs.DeleteAll();
        completedSave = "";
        Debug.Log("Data reset complete");

        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                levelButtons[i][ii].GetComponent<Image>().color = blankMat.color;
            }
        }

    }

    public void UpdateCompleted()
    {
        string newCompleted = "";

        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (levelButtons[i][ii].GetComponent<Image>().color == completedMat.color)
                {
                    newCompleted = newCompleted + levelButtons[i][ii].GetComponent<ChooseButton>().levelValue.ToString() + ",";
                }
                
            }
        }

        completedSave = newCompleted;
        SaveCompleted();
    }

    public void SaveCompleted()
    {
        PlayerPrefs.SetString("SavedString", completedSave);
        PlayerPrefs.Save();
        Debug.Log("Game data saved!");
        
    }

    public void LoadGame() // load all the levels from a string to their list versions
    {
        string ball = "";

        int loadBall = 0;
        List<int> loadTube = new List<int>();
        List<List<int>> loadLevel = new List<List<int>>();

        // 1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12-    one level

        if (savedLevels.Length > 0)
        {
            for (int index = 0; index < savedLevels.Length; index++)
            {

                //Debug.Log("index = " + index);

                if (savedLevels[index] != '-')
                {
                    if (savedLevels[index] != ':')
                    {
                        if (savedLevels[index] != ',') // add to ball
                        {
                            ball = ball + savedLevels[index]; 
                        }
                        else if (savedLevels[index] == ',') // finish ball
                        {

                            loadBall = System.Convert.ToInt32(ball);
                            //Debug.Log("Load ball = " + loadBall);
                            ball = "";
                            int newBall = loadBall;

                            loadTube.Add(newBall);

                        }


                    }
                    else if (savedLevels[index] == ':') // finish tube
                    {
                        if (ball != "")
                        {
                            loadBall = System.Convert.ToInt32(ball);
                            //Debug.Log("Load ball = " + loadBall);
                            ball = "";
                            int newBall = loadBall;

                            loadTube.Add(newBall);
                        }

                        List<int> newTube = loadTube;
                        //Debug.Log("New Tube with Count = " + newTube.Count);

                        loadLevel.Add(newTube);

                        loadTube = new List<int>();
                    }
                }
                else if (savedLevels[index] == '-')
                {
                    if (loadTube.Count > 0)
                    {
                        if (ball != "")
                        {
                            loadBall = System.Convert.ToInt32(ball);
                            //Debug.Log("Load ball = " + loadBall);
                            ball = "";
                            int newBall = loadBall;

                            loadTube.Add(newBall);
                        }

                        List<int> newTube = loadTube;
                        //Debug.Log("New Tube with Count = " + newTube.Count);

                        loadLevel.Add(newTube);

                        loadTube = new List<int>();
                    }

                    if (loadLevel.Count == 12)
                    {
                        List<List<int>> newLevel = new List<List<int>>();

                        newLevel = loadLevel;

                        //Debug.Log("Level Count = " + levels.Count);

                        levels.Add(newLevel);

                        loadLevel = new List<List<int>>();
                    }
                    else
                    {
                        Debug.Log("Not enough in loadLevel: " + loadLevel.Count);
                    }
                }

            }
        }
    }

    public string GetLevels(string path) // open the text file containing the string of levels
    {

        StreamReader reader = new StreamReader(path);

        string l = reader.ReadToEnd();

        reader.Close();

        return l;
    }

    
}
