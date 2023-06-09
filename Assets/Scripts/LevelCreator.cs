using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
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
    public GameObject pageRightFarButton;
    public GameObject pageLeftFarButton;
    List<List<GameObject>> levelButtons = new List<List<GameObject>>();
    public int currentLevelPage = 0;

    int spotCount = 0;
    int actualCount = 1;

    public Material completedMat;
    public Material blankMat;
    public string completedSave = "";
    List<int> completed = new List<int>();

    public TextAsset textfile;

    public TMP_Text levelNumText;
    public TMP_Text pageNumText;

    public Vector2 levelsPerPage;
    public GameObject list;
    public GameObject spotPrefab;

    public int genXLevels;

   
    public List<List<List<int>>> challengeLevels = new List<List<List<int>>>();
    List<List<GameObject>> challengeLevelButtons = new List<List<GameObject>>();
    public float challengeTime;
    public float loadedChallengeTime;
    public bool inChallenge;
    
    public TMP_Text challengeTimeTextGame;
    public TMP_Text challengeTimeTextList;
    public GameObject challengeList;
    public GameObject challengeButton;
    public GameObject giveUpButton;
    public List<GameObject> challengeSpots;
    public Vector2 challengeLevelsPerPage;
    public GameObject resetButton;
    public GameObject challengeReset;
    List<int> completedChallenge = new List<int>();
    public TMP_Text recordText;
    public GameObject challengeRequirement;
    bool startChallenge;

    public GameObject requirementBox;
    public TMP_Text requirementText;

    public int coins;
    public TMP_Text coinsText;
    public int coinIncriment;


    public GameObject winScreen;
    public ParticleSystem confettiPrefab;
    public List<GameObject> confettiSpots;
    public TMP_Text winCoinText;
    public Button winNextButton;

    GameManager gameManager;

    DateTime challengeStart;

    public List<string> ballTags;

    // Start is called before the first frame update
    void Start()
    {
        winScreen.SetActive(false);
        gameManager = GetComponent<GameManager>();
        savedLevels = GetLevels();
        LoadGame();
        LoadLevelChooseList();
        LoadCompleted();

        challengeRequirement.SetActive(ChallengeRequirement());

    }

    // Update is called once per frame
    void Update()
    {
        SaveCompleted();

        saveButton.GetComponent<Button>().interactable = FinishedMaking();
        UpdateCoins();

        if (inChallenge && startChallenge)
        {
            challengeTime += Time.deltaTime;
            
            challengeTimeTextGame.text = OrganizeChallengeTime(challengeTime).ToString();
            challengeTimeTextList.text = OrganizeChallengeTime(challengeTime).ToString();

            if (BeatChallenge())
            {
                
                inChallenge = false;
                
                SaveChallengeTime();
                //Debug.LogWarning("beat challenge");
                GiveUpChallenge();
            }
        }

        //Debug.Log("Level Count: " + levels.Count);

    }

    void UpdateCoins()
    {
        coinsText.text = coins.ToString();
        PlayerPrefs.SetInt("CoinCount", coins);
        PlayerPrefs.Save();
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
        StreamWriter writer = new (path);

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
                //Debug.Log(newChosen[i]);
                
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

    public void GenerateLevelsButton()
    {
        for (int i = 0; i < genXLevels; ++i)
        {
            List<List<int>> newLevel = GenerateLevel();
            if (newLevel != null)
            {
                levels.Add(newLevel);
                AddToLevelList(newLevel);

                AddToDatabase(levels.Count - 1);
                WriteLevels("Assets/Resources/Levels.txt");
            }
        }

        
    }

    List<int> FindPossibleChoices()
    {
        List<int> choices = new List<int>();

        for (int i = 0; i < chosen.Count; ++i)
        {
            if (chosen[i] < 4)
            {
                choices.Add(i);
                
            }
        }
        //Debug.Log(choices.Count);
        return choices;
    }

    List<List<int>> GenerateLevel()
    {
        

        ResetMaker();

        if (!FinishedMaking())
        {
            
            List<List<int>> newLevel = new List<List<int>>(12);

            for (int ii = 0; ii < 12; ii++)
            {


                List<int> newTube = new List<int>(4);
                //int tryAgain = 4;
                
                for (int i = 0; i < 4; ++i)
                {
                    List<int> choices = FindPossibleChoices();
                    

                    int add = UnityEngine.Random.Range(0, choices.Count);

                    newTube.Add(choices[add]);
                    chosen[choices[add]]++;
                    
                }

                //Debug.Log("new tube size = " + newTube.Count);

                newLevel.Add(newTube);
            }





            if (FinishedMaking())
            {
                Debug.Log("new level sixe = " + newLevel.Count);
                return newLevel;
                

            }
            
            else
            {
                Debug.Log(FinishedMaking());
                Debug.LogError("Did not work");
            }
        }

        return null;
    }

    bool ChallengeRequirement()
    {
        for (int i = 0; i < levelButtons[0].Count; ++i)
        {
            if (levelButtons[0][i].GetComponent<Image>().color != completedMat.color) { return true; }
        }
        return false;
    }

    public void StartChallenge()
    {
        if (gameObject.GetComponent<GameManager>().undoButton.activeSelf)
        {
            gameObject.GetComponent<GameManager>().ModeChange();
        }

        inChallenge = true;
        completedChallenge.Clear();

        gameObject.GetComponent<RewardedAd>()._showAdButton.gameObject.SetActive(false);
        challengeStart = DateTime.Now;
        pageNumText.gameObject.SetActive(false);
        pageLeftButton.SetActive(false);
        pageLeftFarButton.SetActive(false);
        pageRightButton.SetActive(false);
        pageRightFarButton.SetActive(false);
        requirementBox.SetActive(false);

        levelNumText.gameObject.SetActive(false);
        challengeTimeTextGame.gameObject.SetActive(true);
        challengeTimeTextList.gameObject.SetActive(true);

        challengeButton.GetComponent<Button>().interactable = false;
        giveUpButton.SetActive(true);
        gameObject.GetComponent<GameManager>().modeButton.SetActive(false);
        challengeList.SetActive(true);
        resetButton.SetActive(false);
        challengeReset.SetActive(true);
        list.SetActive(false); // ^^ Setup for Challenge

        if (PlayerPrefs.HasKey("ChallengeTime"))
        {
            loadedChallengeTime = PlayerPrefs.GetFloat("ChallengeTime");
            if (loadedChallengeTime != 0) { recordText.gameObject.SetActive(true); }
            float roundedTime = Mathf.Round(loadedChallengeTime * 100) / 100;
            recordText.text = "Record: " + OrganizeChallengeTime(roundedTime).ToString();

        }


        

        if (challengeSpots.Count > 0) { challengeSpots.Clear(); }

        Vector2 listRect = new Vector2(challengeList.GetComponent<RectTransform>().rect.width, challengeList.GetComponent<RectTransform>().rect.height);
        Vector2 chooseButtonRect = new Vector2(chooseButtonPrefab.GetComponent<RectTransform>().rect.width, chooseButtonPrefab.GetComponent<RectTransform>().rect.height);

        float sizeX = listRect.x / challengeLevelsPerPage.x;
        float sizeY = listRect.y / challengeLevelsPerPage.y;
        float scaleX = sizeX / chooseButtonRect.x;
        float scaleY = sizeY / chooseButtonRect.y;
        //Debug.Log("size x = " + sizeX);

        float halfY = challengeLevelsPerPage.y / 2;


        for (float r = halfY; r > -halfY; --r)
        {
            float halfX = challengeLevelsPerPage.x / 2;


            for (float c = -halfX; c < halfX; ++c)
            {
                GameObject newSpot = Instantiate(spotPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newSpot.transform.SetParent(challengeList.transform);
                newSpot.transform.localPosition = new Vector3((c * chooseButtonRect.x * scaleX) + (chooseButtonRect.x * scaleX / 2), (r * chooseButtonRect.y * scaleY) - (chooseButtonRect.y * scaleY / 2), 0);
                newSpot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                challengeSpots.Add(newSpot);

            }
        }
        
        //Debug.LogError("generatespots no work");
        int LPP = (int)challengeLevelsPerPage.x * (int)challengeLevelsPerPage.y;

        
        //Debug.LogWarning("spots");

        challengeLevels.Clear();
        for (int i = 0; i < LPP; ++i)
        {
            List<List<int>> newLevel = GenerateLevel();
            challengeLevels.Add(newLevel);
        }
        

        List<GameObject> page = new List<GameObject>();
        int challengeSpotCount = 0;
        int challengeActualCount = 1;

        if (challengeLevels.Count > 0)
        {


            for (int i = 0; i < challengeLevels.Count; ++i)
            {

                GameObject newButton = Instantiate(chooseButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newButton.transform.SetParent(challengeSpots[challengeSpotCount].transform);
                newButton.transform.position = challengeSpots[challengeSpotCount].transform.position;
                newButton.transform.localScale = new Vector3(scaleX - 0.05f, scaleY - 0.05f, 1.0f);

                newButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = challengeActualCount.ToString();
                newButton.GetComponent<ChooseButton>().levelValue = challengeActualCount;
                newButton.GetComponent<ChooseButton>().Challenge();




                challengeSpotCount++;
                challengeActualCount++;

                page.Add(newButton);
                //Debug.Log("page count: " + page.Count);

                if (challengeSpotCount > LPP - 1)
                {
                    challengeSpotCount = 0;
                    List<GameObject> add = new List<GameObject>();

                    challengeLevelButtons.Add(page);

                    page = add;
                }
            }



        }


        if (page.Count != 0) { challengeLevelButtons.Add(page); }



        //currentLevelPage = 0;
     
        
        //Debug.LogWarning("complete");

        //Debug.LogWarning(challengeLevels.Count);
    }

    public void LoadChallengeLevel(int index) // load a certain level based on the index given
    {
        gameManager.undoHolster.Clear();
        gameManager.TTHolster.Clear();
        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;
        startChallenge = true;
        

        if (challengeLevels.Count > 0)
        {
            for (int i = 0; i < challengeLevels[index].Count; ++i) // each tube
            {
                int mat = 0;
                for (int ii = 1; ii <= challengeLevels[index][i].Count; ++ii) // each ball
                {


                    
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = mats[challengeLevels[index][i][mat]].color;
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.tag = "C" + (challengeLevels[index][i][mat] + 1).ToString();
                    if (!Gametubes[i].GetComponent<Tube>().corked) { if (Gametubes[i].GetComponent<Tube>().FullTube() && !Gametubes[i].GetComponent<Tube>().EmptyTube()) { Gametubes[i].GetComponent<Tube>().Cork(); } }
                    else { Debug.Log("dont cork"); }

                    mat++;

                    //Debug.Log("changed color");
                }

            }

            
        }
        else
        {
            Debug.Log("no levels");
            Debug.Log("Level Count: " + challengeLevels.Count);

        }

        gameObject.GetComponent<GameManager>().menuNum = 2;
        lastLevelLoaded = index;

        
    }

    public void LoadLastChallengeLevel()
    {
        LoadChallengeLevel(lastLevelLoaded);
    }

    public bool BeatLastChallengeLevel()
    {
        for (int i = 0; i < challengeLevelButtons.Count; ++i)
        {
            for (int ii = 0; ii < challengeLevelButtons[i].Count; ++ii)
            {
                if (lastLevelLoaded + 1 == challengeLevelButtons[i][ii].GetComponent<ChooseButton>().levelValue && challengeLevelButtons[i][ii].GetComponent<Image>().color != completedMat.color)
                {
                    challengeLevelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    Debug.LogWarning("Beat Last Challenge Level");
                    return true;
                }
            }
        }
        return false;
    }

    public void GiveUpChallenge()
    {
        startChallenge = false;
        inChallenge = false;
        challengeTime = 0;

        pageNumText.gameObject.SetActive(true);
        pageLeftButton.SetActive(true);
        pageLeftFarButton.SetActive(true);
        pageRightButton.SetActive(true);
        pageRightFarButton.SetActive(true);

        gameObject.GetComponent<RewardedAd>()._showAdButton.gameObject.SetActive(true);


        levelNumText.gameObject.SetActive(true);
        challengeTimeTextGame.gameObject.SetActive(false);
        challengeTimeTextList.gameObject.SetActive(false);

        challengeButton.GetComponent<Button>().interactable = true;
        giveUpButton.SetActive(false);
        gameObject.GetComponent<GameManager>().modeButton.SetActive(true);
        challengeList.SetActive(false);
        list.SetActive(true);
        resetButton.SetActive(true);
        challengeReset.SetActive(false);
        recordText.gameObject.SetActive(false);

        challengeTimeTextGame.text = OrganizeChallengeTime(challengeTime).ToString();
        challengeTimeTextList.text = OrganizeChallengeTime(challengeTime).ToString();

        UpdateListPage();
        UpdatePageButtons();
    }

    bool BeatChallenge()
    {
        for (int i = 0; i < challengeLevelButtons.Count; ++i)
        {
            for (int ii = 0; ii < challengeLevelButtons[i].Count; ++ii)
            {
                if (challengeLevelButtons[i][ii].GetComponent<Image>().color != completedMat.color)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SaveChallengeTime()
    {
        if (challengeTime < loadedChallengeTime || loadedChallengeTime == 0)
        {
            
            PlayerPrefs.SetFloat("ChallengeTime", challengeTime); 
        }
        else { PlayerPrefs.SetFloat("ChallengeTime", loadedChallengeTime); }
        //Debug.Log(PlayerPrefs.GetFloat("ChallengeTime"));
        PlayerPrefs.Save();
        //Debug.Log("Game data saved!");
    }

    string OrganizeChallengeTime(float time)
    {
        string end = "00:00.00";
        DateTime challengeCurrent = DateTime.Now;

        TimeSpan ts = challengeCurrent - challengeStart;
        string min = "";
        string sec = "";
        string ms = "";
        float milli = Mathf.Round(ts.Milliseconds / 10);

        if (ts.Minutes == 0) { min = "00"; }
        else if (ts.Minutes < 10) { min = "0" + ts.Minutes.ToString(); }
        else { min = ts.Minutes.ToString(); }

        if (ts.Seconds == 0) { sec = "00"; }
        else if (ts.Seconds < 10) { sec = "0" + ts.Seconds.ToString(); }
        else { sec = ts.Seconds.ToString(); }

        if (milli == 0) { ms = "00"; }
        else if (milli < 10) { ms = "0" + milli.ToString(); }
        else { ms = milli.ToString(); }

        end = min + ":" + sec + "." + ms;
        Debug.Log(end);
        return end;
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

    float GenerateSpots()
    {
        spots.Clear();

        Vector2 listRect = new Vector2(list.GetComponent<RectTransform>().rect.width, list.GetComponent<RectTransform>().rect.height);
        Vector2 chooseButtonRect = new Vector2(chooseButtonPrefab.GetComponent<RectTransform>().rect.width, chooseButtonPrefab.GetComponent<RectTransform>().rect.height);

        float sizeX = listRect.x / levelsPerPage.x;
        float sizeY = listRect.y / levelsPerPage.y;
        float scaleX = sizeX / chooseButtonRect.x;
        float scaleY = sizeY / chooseButtonRect.y;
        //Debug.Log("size x = " + sizeX);

        float halfY = levelsPerPage.y / 2;
        

        for (float r = halfY; r > -halfY; --r)
        {
            float halfX = levelsPerPage.x / 2;
            

            for (float c = -halfX; c < halfX; ++c)
            {
                GameObject newSpot = Instantiate(spotPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newSpot.transform.SetParent(list.transform);
                newSpot.transform.localPosition = new Vector3((c * chooseButtonRect.x * scaleX) + (chooseButtonRect.x * scaleX / 2), (r * chooseButtonRect.y * scaleY) - (chooseButtonRect.y * scaleY / 2), 0);
                newSpot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                spots.Add(newSpot);
                
            }
        }

        if (sizeX == sizeY) { return scaleX - 0.05f; }
        //Debug.LogError("generatespots no work");
        return 1;
    }

    public void LoadLevelChooseList() // create the initial list of levels that you can click on to load
    {
        //Debug.Log("Load Level Choose List");
        float scale = GenerateSpots();

        int LPP = (int)levelsPerPage.x * (int)levelsPerPage.y;

        List<GameObject> page = new List<GameObject>();
        

        if (levels.Count > 0)
        {
            

            for (int i = 0; i < levels.Count; ++i)
            {

                GameObject newButton = Instantiate(chooseButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                newButton.transform.SetParent(spots[spotCount].transform);
                newButton.transform.position = spots[spotCount].transform.position;
                newButton.transform.localScale = new Vector3(scale, scale, 1.0f);

                newButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = actualCount.ToString();
                newButton.GetComponent<ChooseButton>().levelValue = actualCount;
                newButton.GetComponent<ChooseButton>().Level();



                spotCount++;
                actualCount++;
                
                page.Add(newButton);
                //Debug.Log("page count: " + page.Count);

                if (spotCount > LPP - 1)
                { 
                    spotCount = 0;
                    List<GameObject> add = new List<GameObject>();
                    
                    levelButtons.Add(page);

                    page = add;

                    


                }
            }

            

        }


        if (page.Count != 0) { levelButtons.Add(page); }

        

        currentLevelPage = 0;
        //Debug.LogWarning(levelButtons.Count);
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

        requirementBox.SetActive(CheckRequirement(currentLevelPage));
        if (requirementBox.activeSelf) { requirementText.text = "Complete levels " + levelButtons[currentLevelPage - 1][0].GetComponent<ChooseButton>().levelValue + "-" + levelButtons[currentLevelPage - 1][levelButtons[currentLevelPage - 1].Count - 1].GetComponent<ChooseButton>().levelValue + " to unlock"; }
    }

    bool CheckRequirement(int page)
    {
        if (page != 0)
        {
            for (int i = 0; i < levelButtons[page - 1].Count; ++i)
            {
                if (levelButtons[page - 1][i].GetComponent<Image>().color != completedMat.color) { return true; }
            }
        }
        return false;
    }

    void UpdatePageButtons()
    {

        pageNumText.text = (currentLevelPage + 1).ToString() + " / " + (levelButtons.Count).ToString();

        if (currentLevelPage == 0)
        { 
            pageLeftButton.GetComponent<Button>().interactable = false;
            pageLeftFarButton.GetComponent<Button>().interactable = false;
        }
        else if (!pageLeftButton.GetComponent<Button>().interactable) 
        {
            pageLeftButton.GetComponent<Button>().interactable = true;
            pageLeftFarButton.GetComponent<Button>().interactable = true;
        }
        if (currentLevelPage == levelButtons.Count - 1)
        {
            pageRightButton.GetComponent<Button>().interactable = false;
            pageRightFarButton.GetComponent<Button>().interactable = false;
        }
        else if (!pageRightButton.GetComponent<Button>().interactable)
        { 
            pageRightButton.GetComponent<Button>().interactable = true;
            pageRightFarButton.GetComponent<Button>().interactable = true;
        }
    }

    public void PageLeft()
    {
        currentLevelPage--;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageLeftFar()
    {
        currentLevelPage = 0;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageRight()
    {
        currentLevelPage++;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageRightFar()
    {
        int cp = currentLevelPage;
        

        while (!CheckRequirement(currentLevelPage))
        {
            currentLevelPage++;
        }
        Debug.Log(cp + " " + currentLevelPage);
        if (currentLevelPage == cp + 1) { currentLevelPage = levelButtons.Count; }
        


        currentLevelPage--;
        if (cp + 1 > currentLevelPage) { currentLevelPage = levelButtons.Count - 1; }
        UpdateListPage();
        UpdatePageButtons();
    }

    public void LoadLevel(int index) // load a certain level based on the index given
    {
        gameManager.undoHolster.Clear();
        gameManager.TTHolster.Clear();
        Debug.LogWarning("Loaded Level " + index);
        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;

        levelNumText.text = "Level " + (index + 1).ToString();
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
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.tag = "C" + (levels[index][i][mat] + 1).ToString();
                    if (!Gametubes[i].GetComponent<Tube>().corked) { if (Gametubes[i].GetComponent<Tube>().FullTube() && !Gametubes[i].GetComponent<Tube>().EmptyTube()) { Gametubes[i].GetComponent<Tube>().Cork(); } }
                    else { Debug.Log("dont cork"); }
                    mat++;

                    //Debug.Log("changed color");
                }

            }

            gameObject.GetComponent<GameManager>().Cork();
        }
        else
        {
            Debug.Log("no levels");
            Debug.Log("Level Count: " + levels.Count);

        }

        gameManager.menuNum = 2;
        lastLevelLoaded = index;
        gameManager.SetUndoTubes();
        
    }

    public void LoadLastLevel()
    {
        LoadLevel(lastLevelLoaded);
    }

    public void WinScreen()
    {
        winScreen.SetActive(true);
        winCoinText.text = "+" + coinIncriment.ToString() + " Coins";

        for (int i = 0; i < confettiSpots.Count; ++i)
        {
            ParticleSystem confetti = Instantiate(confettiPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            //confetti.gameObject.transform.SetParent(confettiSpots[i].transform);
            confetti.gameObject.transform.localScale = new Vector3(1, 1, 1);
            Vector3 pos = confettiSpots[i].transform.position;
            pos.z = -1;

            confetti.gameObject.transform.position = pos;

            confetti.Play();
        }

        if (!inChallenge)
        {
            if (!BeatLastLevel()) { winCoinText.text = "You've Already Beat This Level!"; }
            UpdateCompleted();
            gameObject.GetComponent<GameManager>().SetUndoTubes();
            if (lastLevelLoaded >= levels.Count - 1)
            {
                winNextButton.interactable = false;
                winCoinText.text = "You've Beat the Game!\n" + "+" + coinIncriment.ToString() + " Coins";
            }

            int LPP = (int)levelsPerPage.x * (int)levelsPerPage.y;

            if ((lastLevelLoaded + 1) % LPP == 0 && CheckRequirement(currentLevelPage + 1))
            {
                
                winNextButton.interactable = false;
                
            }
        }
        else if (inChallenge)
        {
            winCoinText.text = "Challenge " + (lastLevelLoaded + 1).ToString() + " completed";
            if (!BeatLastChallengeLevel()) { winCoinText.text = "You've Already Beat This Challenge Level!"; }
            BeatLastChallengeLevel();
            if (lastLevelLoaded >= challengeLevels.Count - 1)
            {
                winNextButton.interactable = false;
                winCoinText.text = "You've Beat The Challenge!";
            }
        }

        
    }

    public void WinNext()
    {
        gameObject.GetComponent<GameManager>().ResetGame();
        

        if (!inChallenge)
        {
            if (lastLevelLoaded < levels.Count) { LoadLevel(lastLevelLoaded + 1); }
        }
        else if (inChallenge) { if (lastLevelLoaded < challengeLevels.Count - 1) { LoadChallengeLevel(lastLevelLoaded + 1); } }

        winScreen.SetActive(false);
    }

    public void WinLevels()
    {

        gameObject.GetComponent<GameManager>().menuNum = 1;
        gameObject.GetComponent<GameManager>().ResetGame();
        winScreen.SetActive(false);
    }

    public bool BeatLastLevel()
    {
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (lastLevelLoaded + 1 == levelButtons[i][ii].GetComponent<ChooseButton>().levelValue && levelButtons[i][ii].GetComponent<Image>().color != completedMat.color)
                {
                    levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    coins += coinIncriment;
                    challengeRequirement.SetActive(ChallengeRequirement());
                    return true;
                    
                }
                //else if (lastLevelLoaded + 1 == 45) { Unlock(); }
            }
        }

        challengeRequirement.SetActive(ChallengeRequirement());
        return false;
        
    }

    public void BeatIndexLevel(int index)
    {
        //Debug.Log(index);
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

        challengeRequirement.SetActive(ChallengeRequirement());

        
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
            //Debug.Log(completedSave);
        }
        if (PlayerPrefs.HasKey("CoinCount"))
        {
            coins = PlayerPrefs.GetInt("CoinCount");
        }
        //else { Debug.LogWarning("There is no save data"); }

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
                    //Debug.Log(completed.Count);
                }
            }
        }

        //Debug.Log(completed.Count);

        for (int index = 0; index < completed.Count; ++index)
        {
            //Debug.Log("complete load: " + completed[index]);
            for (int i = 0; i < levelButtons.Count; ++i)
            {
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    if (levelButtons[i][ii].GetComponent<ChooseButton>().levelValue == completed[index])
                    {
                        //Debug.Log("loadedCom: " + index);
                        levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    }
                }
            }
        }
    }

    public void ResetData(GameObject button) // reset database
    {
        PlayerPrefs.DeleteAll();
        completedSave = "";
        loadedChallengeTime = 0;
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
        //Debug.Log("Game data saved!");  
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

    public string GetLevels() // open the text file containing the string of levels
    {

        return textfile.text;
    }

    /*public void FixCompleted() // temporary
    {
        string newCompleted = "";
        

        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (levelButtons[i][ii].GetComponent<ChooseButton>().levelValue <= 312)
                {
                    newCompleted = newCompleted + levelButtons[i][ii].GetComponent<ChooseButton>().levelValue.ToString() + ",";
                    coins += coinIncriment;
                }

            }
        }

        completedSave = newCompleted;
        SaveCompleted();
        Destroy(fixCompleted);
    }

    public void Unlock() // temporary
    {
        fixCompleted.SetActive(true);
        
    }*/

    public void Test()
    {
        
        List<List<int>> solvePoint = new List<List<int>>();
        List<GameObject> gameTubes = new List<GameObject>();

        if (gameManager.undoHolster.Count > 0) { gameTubes = gameManager.undoHolster[gameManager.undoHolster.Count - 1]; }
        else
        {
            solvePoint = levels[lastLevelLoaded];
            solvePoint.Add(new List<int>(4));
            solvePoint.Add(new List<int>(4));
            gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, lastLevelLoaded);
            
            return;
        }

        //Debug.Log(gameTubes.Count);

        for (int i = 0; i < gameTubes.Count; ++i)
        {
            solvePoint.Add(new List<int>());
            for (int j = 1; j < gameTubes[i].transform.childCount; ++j)
            {
                
                if (gameTubes[i].transform.GetChild(j).childCount != 0)
                {

                    for (int index = 0; index < ballTags.Count; ++index)
                    {
                        if (ballTags[index] == gameTubes[i].transform.GetChild(j).GetChild(0).gameObject.tag)
                        {
                            solvePoint[i].Add(index + 1);
                            //Debug.Log(j);
                            break;
                        }

                    }
                }
                else
                {
                    //Debug.Log("emptySlot");
                    solvePoint[i].Add(0);
                }
            }
        }
        gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, lastLevelLoaded);
        
    }

    public void SolveAll()
    {
        string add = "";
        List<List<int>> solvePoint = new List<List<int>>();

        for (int i = 0; i < levels.Count; ++i)
        {
            
            solvePoint = levels[i];

            for (int j = 0; j < solvePoint.Count; ++j)
            {
                for (int ii = 0; ii < solvePoint[j].Count; ++ii)
                {
                    solvePoint[j][ii]++;
                }
            }

            solvePoint.Add(new List<int>(4));
            solvePoint.Add(new List<int>(4));
            
            add = add + gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, i);
        }

        StreamWriter writer = new("Assets/Resources/SolveCheck.txt");

        writer.WriteLine(add);

        writer.Close();
    }
}
