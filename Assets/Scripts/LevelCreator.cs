using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.Events;
using System.Linq;
using Unity.VisualScripting;

// 1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12-    one level

public class LevelCreator : MonoBehaviour
{
    public List<GameObject> tubes;
    public List<GameObject> balls;

    public List<Material> mats;

    public int lastLevelLoaded = 0;

    List<List<List<int>>> levels = new List<List<List<int>>>(); // a list of level

    public List<int> chosen;

    public string savedLevels = "";
    

    public List<GameObject> spots;
    public GameObject chooseButtonPrefab;

    public GameObject saveButton;

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
    string completedSave = "";
    public List<int> completed = new List<int>();

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
    private bool finishedChallenge = false;
    
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
    public List<bool> challengeSolvability = new List<bool>();

    public GameObject challengeWinScreen;
    public TMP_Text challengeWinRecordText;
    public TMP_Text challengeWinTimeText;
    public int challengeWinCoins;
    public TMP_Text challengeWinCoinText;

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
    public Button levelsPageButton;

    public Button goToWinChallengeButton;

    GameManager gameManager;

    DateTime challengeStart;

    public List<string> ballTags;

    public bool generatingChallenge = false;

    public GameObject loadingIcon;

    public bool lookingForHint = false;
    public int hintCost;

    public int loadingLevelNum;


    // Start is called before the first frame update
    void Start()
    {
        winScreen.SetActive(false);
        gameManager = GetComponent<GameManager>();
        savedLevels = GetLevels();
        LoadGame();
        LoadLevelChooseList();
        LoadCompleted();
        Debug.Log(levels.Count);
        challengeRequirement.SetActive(ChallengeRequirement());

    }

    // Update is called once per frame
    void Update()
    {
        SaveCompleted();

        saveButton.GetComponent<Button>().interactable = FinishedMaking();
        UpdateCoins();

        if (generatingChallenge && challengeSolvability.Count < 5)
        {
            if (gameManager.menuNum == 1)
            {
                loadingIcon.SetActive(true);
            }
            for (int i = 0; i < challengeSolvability.Count; i++)
            {
                if (challengeSolvability[i] == true && challengeSpots[i].transform.GetChild(0).gameObject.GetComponent<Button>().interactable == false)
                {
                    challengeSpots[i].transform.GetChild(0).gameObject.GetComponent<Button>().interactable = true;
                }
            }

            if (challengeSolvability.Count == 4)
            {
                generatingChallenge = false;
                challengeSolvability.Clear();
                loadingIcon.SetActive(false);
            }
        }

        if (lookingForHint && gameObject.GetComponent<LevelSolver>().hintFlash.tubes == Vector2.zero)
        {
            if (gameManager.menuNum == 2)
            {
                loadingIcon.SetActive(true);
            }
        }
        else if (gameObject.GetComponent<LevelSolver>().hintFlash.tubes != Vector2.zero && lookingForHint)
        {
            loadingIcon.SetActive(false);
            List<GameObject> gameTubes = gameManager.tubes;
            HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;

            flash.transform.position = gameTubes[(int)flash.tubes[flash.index]].transform.position;
            flash.gameObject.SetActive(true);
            
        }
        else if (!lookingForHint && gameObject.GetComponent<LevelSolver>().hintFlash.tubes == Vector2.zero)
        {
            loadingIcon.SetActive(false);
        }

        if (inChallenge && startChallenge && !finishedChallenge)
        {
            challengeTime += Time.deltaTime;
            
            challengeTimeTextGame.text = OrganizeChallengeTime(challengeTime).ToString();
            challengeTimeTextList.text = OrganizeChallengeTime(challengeTime).ToString();

            if (BeatChallenge())
            {
                SaveChallengeTime();
                finishedChallenge = true;
            }
        }

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

        ResetMaker(12);

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

    public void ResetMaker(int tubeCount) // reset the dropdowns for the level maker
    {
        chosen.Clear();

        for (int i = 0; i < tubeCount; ++i)
        {
            chosen.Add(0);
        }
    }

    public void GenerateLevelsButton()
    {
        for (int i = 0; i < genXLevels; ++i)
        {
            List<List<int>> newLevel = GenerateLevel(i, 10);
            if (newLevel != null)
            {
                levels.Add(newLevel);
                
                AddToLevelList(newLevel);

                AddToDatabase(levels.Count - 1);
                WriteLevels("Assets/Resources/Levels.txt");
            }
            else
            {
                Debug.LogError("had full tube or was not solvable");
            }
        }

        
    }

    List<int> FindPossibleChoices(int tubeCount)
    {
        List<int> choices = new List<int>();

        for (int i = 0; i < tubeCount; ++i)
        {
            if (chosen[i] < 4)
            {
                choices.Add(i);
                
            }
        }
        //Debug.Log(choices.Count);
        return choices;
    }

    bool CompletedTube(List<int> tube)
    {
        

        for (int i = 0; i < tube.Count; ++i)
        {
            if (tube[i] != tube[0])
            {
                return false;
            }
        }

        return true;
    }

    List<List<int>> GenerateLevel(int index, int tubeCount)
    {
        

        ResetMaker(tubeCount);

        if (!FinishedMaking())
        {
            
            List<List<int>> newLevel = new List<List<int>>(tubeCount);

            for (int ii = 0; ii < tubeCount; ii++)
            {


                List<int> newTube = new List<int>(4);
                //int tryAgain = 4;
                
                for (int i = 0; i < 4; ++i)
                {
                    List<int> choices = FindPossibleChoices(tubeCount);
                    

                    int add = UnityEngine.Random.Range(0, choices.Count);

                    
                    newTube.Add(choices[add]);

                    
                    chosen[choices[add]]++;
                    
                }

                //Debug.Log("new tube size = " + newTube.Count);
                if (CompletedTube(newTube))
                {
                    Debug.Log("error");
                    GenerateLevel(index, tubeCount);
                    return null;
                }
                newLevel.Add(newTube);
            }





            if (FinishedMaking())
            {
                bool output = SolveList(newLevel, index);

                //StreamWriter writer = new("Assets/Resources/SolveCheck.txt");

                //writer.WriteLine(output);

                //writer.Close();
                Debug.Log("new level sixe = " + newLevel.Count);
                if (!output)
                {
                    Debug.Log("no solution");
                    GenerateLevel(index, tubeCount);
                }
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

    string OrganizeRecordTime(float time)
    {
        string end = "00:00.00";

        float check = time;

        int Minutes = (int)time / 60;
        check = check - (Minutes * 60);
        int Seconds = (int)check;
        check = check - Seconds;
        float temp = check * 100;
        int milli = Mathf.RoundToInt(temp);

        string min = "";
        string sec = "";
        string ms = "";
        

        if (Minutes == 0) { min = "00"; }
        else if (Minutes < 10) { min = "0" + Minutes.ToString(); }
        else { min = Minutes.ToString(); }

        if (Seconds == 0) { sec = "00"; }
        else if (Seconds < 10) { sec = "0" + Seconds.ToString(); }
        else { sec = Seconds.ToString(); }

        if (milli == 0) { ms = "00"; }
        else if (milli < 10) { ms = "0" + milli.ToString(); }
        else { ms = milli.ToString(); }

        end = min + ":" + sec + "." + ms;
        Debug.Log(end);
        return end;
    }

    public void StartChallengeCoroutine()
    {
        challengeSolvability.Clear();
        StartCoroutine(MakeChallengeLevels());
        StartChallenge();
    }

    IEnumerator MakeChallengeLevels()
    {
        generatingChallenge = true;
        int LPP = (int)challengeLevelsPerPage.x * (int)challengeLevelsPerPage.y;

        challengeLevels.Clear();
        for (int i = 0; i < LPP; ++i)
        {
            List<List<int>> newLevel = GenerateLevel(i, 12);
            challengeLevels.Add(newLevel);
        }

        yield return null;
    }

    public void StartChallenge()
    {
        int LPP = (int)challengeLevelsPerPage.x * (int)challengeLevelsPerPage.y;
        if (gameManager.undoButton.activeSelf)
        {
            gameManager.ModeChange();
            
        }

        inChallenge = true;
        completedChallenge.Clear();
        loadingIcon.SetActive(true);

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
            recordText.text = "Record: " + OrganizeRecordTime(roundedTime).ToString();

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
                newButton.GetComponent<Button>().interactable = false;
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
    }

    public void LoadChallengeLevel(int index) // load a certain level based on the index given
    {
        gameManager.undoHolster.Clear();
        gameManager.TTHolster.Clear();
        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;
        if (!startChallenge)
        {
            challengeStart = DateTime.Now;
            startChallenge = true;
        }
        loadingIcon.SetActive(false);
        

        if (challengeLevels.Count > 0)
        {
            for (int i = 0; i < challengeLevels[index].Count; ++i) // each tube
            {
                
                for (int ii = 1; ii <= challengeLevels[index][i].Count; ++ii) // each ball
                {


                    
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = mats[challengeLevels[index][i][ii - 1]].color;
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.tag = "C" + (challengeLevels[index][i][ii - 1] + 1).ToString();
                    Gametubes[i].GetComponent<Tube>().SetSpot(challengeLevels[index][i][ii - 1] + 1, ii);
                    if (!Gametubes[i].GetComponent<Tube>().corked)
                    {
                        if (Gametubes[i].GetComponent<Tube>().FullTube() && !Gametubes[i].GetComponent<Tube>().EmptyTube())
                        {
                            Gametubes[i].GetComponent<Tube>().Cork();
                        }
                    }
                    else
                    {
                        Debug.Log("dont cork");
                    }

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
        Debug.Log("give up challenge");

        startChallenge = false;
        inChallenge = false;
        challengeTime = 0;
        challengeStart = DateTime.Now;
        challengeTimeTextGame.text = OrganizeChallengeTime(challengeTime).ToString();
        challengeTimeTextList.text = OrganizeChallengeTime(challengeTime).ToString();

        pageNumText.gameObject.SetActive(true);
        pageLeftButton.SetActive(true);
        pageLeftFarButton.SetActive(true);
        pageRightButton.SetActive(true);
        pageRightFarButton.SetActive(true);
        loadingIcon.SetActive(false);
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

        challengeLevels.Clear();
        challengeSolvability.Clear();
        generatingChallenge = false;

        StopAllCoroutines();

        for (int i = 0; i < challengeSpots.Count; i++)
        {
            //challengeSpots[i].transform.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
            Destroy(challengeSpots[i]);
        }

        challengeSpots.Clear();

        UpdateListPage();
        UpdatePageButtons();

        WinLevels();
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

    bool CheckRequirement(int page) // checks if a certain page has been completed or not
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

        pageNumText.text = (currentLevelPage + 1).ToString();

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
        bool exit = false;

        while (!exit)
        {
            if (cp < levelButtons.Count - 1)
            {
                if (!CheckRequirement(cp))
                {
                    cp++;
                }
                else { exit = true; }
            }
            else { exit = true; }
        }

        if (cp == currentLevelPage + 1 || cp == currentLevelPage)
        {
            cp = levelButtons.Count - 1;
        }
        else
        {
            Debug.Log($"{cp} is not current page: {currentLevelPage}");
        }

        if (cp != levelButtons.Count - 1)
        {
            cp--;
        }


        if (cp > levelButtons.Count - 1)
        {
            cp = levelButtons.Count - 1;
        }


        currentLevelPage = cp;
        //if (cp + 1 > currentLevelPage) { currentLevelPage = levelButtons.Count - 1; }
        UpdateListPage();
        UpdatePageButtons();
    }

    public void LoadLevel(int index) // load a certain level based on the index given
    {
        gameManager.undoHolster.Clear();
        gameManager.TTHolster.Clear();
        Debug.LogWarning("Loaded Level " + index);

        gameManager.SetResetTubes(levels[index].Count);
        gameManager.ResetGame();

        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;

        levelNumText.text = "Level " + (index + 1).ToString();
        //index = button.GetComponent<ChooseButton>().levelValue - 1;

        //Debug.Log("Level Count: " + levels.Count);

        if (levels.Count > 0)
        {
            for (int i = 0; i < levels[index].Count; ++i) // each tube
            {
                
                for (int ii = 1; ii <= levels[index][i].Count; ii++) // each ball
                {
                    

                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = mats[levels[index][i][ii - 1]].color;
                    Gametubes[i].transform.GetChild(ii).GetChild(0).gameObject.tag = "C" + (levels[index][i][ii - 1] + 1).ToString();
                    Gametubes[i].GetComponent<Tube>().SetSpot(levels[index][i][ii - 1] + 1, ii);
                    if (!Gametubes[i].GetComponent<Tube>().corked)
                    {
                        if (Gametubes[i].GetComponent<Tube>().FullTube())
                        {
                            Gametubes[i].GetComponent<Tube>().Cork(); 
                        }
                    }
                    else { Debug.Log("dont cork"); }
                    

                    //Debug.Log("changed color");
                }

            }

            gameObject.GetComponent<GameManager>().Cork();
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
            else
            {
                winNextButton.interactable = true;
            }
        }
        else if (inChallenge)
        {
            winCoinText.text = "Challenge " + (lastLevelLoaded + 1).ToString() + " completed";
            if (!BeatLastChallengeLevel()) { winCoinText.text = "You've Already Beat This Challenge Level!"; }
            BeatLastChallengeLevel();
            if (BeatChallenge())
            {
                winCoinText.text = "You've Beat The Challenge!";
                winNextButton.gameObject.SetActive(false);
                levelsPageButton.gameObject.SetActive(false);

                goToWinChallengeButton.gameObject.SetActive(true);
            }
        }

        
    }

    public void WinChallengeScreen()
    {
        challengeWinScreen.SetActive(true);
        challengeWinCoinText.text = $"+ {challengeWinCoins} Coins!";

        loadedChallengeTime = PlayerPrefs.GetFloat("ChallengeTime");
        float roundedTime = Mathf.Round(loadedChallengeTime * 100) / 100;
        

        challengeWinRecordText.text = $"Record: {OrganizeChallengeTime(roundedTime)}";
        challengeWinTimeText.text = $"Time: {OrganizeChallengeTime(challengeTime)}";

        coins += challengeWinCoins;
        finishedChallenge = false;
    }

    public void EndChallenge()
    {
        challengeWinScreen.SetActive(false);

        winNextButton.gameObject.SetActive(true);
        levelsPageButton.gameObject.SetActive(true);

        goToWinChallengeButton.gameObject.SetActive(false);

        GiveUpChallenge();
    }

    public void WinNext()
    {
        gameObject.GetComponent<GameManager>().ResetGame();
        

        if (!inChallenge)
        {
            if (lastLevelLoaded < levels.Count)
            {
                for (int i = 1; i < 100;  i++)
                {
                    int test = lastLevelLoaded + i;
                    if (!completed.Contains(test))
                    {
                        Debug.Log($"Loaded level: {test}");
                        LoadLevel(test - 1);
                        break;
                    }
                } 
            }
        }
        else if (inChallenge)
        {
            if (lastLevelLoaded < challengeLevels.Count - 1)
            {
                LoadChallengeLevel(lastLevelLoaded + 1); 
            }
        }

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

        List<int> getCompleted = new List<int>();
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
                    if (!PlayerPrefs.HasKey("o"))
                    {
                        if (add > 477)
                        {
                            add -= 5;
                        }
                        else if (add > 433)
                        {
                            add -= 4;
                        }
                        else if (add > 4)
                        {
                            add -= 3;
                        }

                    }
                    if (!PlayerPrefs.HasKey("FIXTUBES"))
                    {
                        if (add > 988)
                        {
                            add -= 18;
                        }
                        else if (add > 947)
                        {
                            add -= 17;
                        }
                        else if (add > 874)
                        {
                            add -= 16;
                        }
                        else if (add > 857)
                        {
                            add -= 15;
                        }
                        else if (add > 834)
                        {
                            add -= 14;
                        }
                        else if (add > 712)
                        {
                            add -= 13;
                        }
                        else if (add > 708)
                        {
                            add -= 12;
                        }
                        else if (add > 634)
                        {
                            add -= 11;
                        }
                        else if (add > 569)
                        {
                            add -= 10;
                        }
                        else if (add > 553)
                        {
                            add -= 9;
                        }
                        else if (add > 437)
                        {
                            add -= 8;
                        }
                        else if (add > 353)
                        {
                            add -= 7;
                        }
                        else if (add > 347)
                        {
                            add -= 6;
                        }
                        else if (add > 332)
                        {
                            add -= 5;
                        }
                        else if (add > 269)
                        {
                            add -= 4;
                        }
                        else if (add > 173)
                        {
                            add -= 3;
                        }
                        else if (add > 139)
                        {
                            add -= 2;
                        }
                        else if (add > 50)
                        {
                            add -= 1;
                        }
                    }

                    getCompleted.Add(add);
                    
                    set = "";
                    //Debug.Log(completed.Count);
                }
            }

            PlayerPrefs.SetInt("o", 0);
            PlayerPrefs.SetInt("FIXTUBES", 0);
        }

        completed = getCompleted;

        for (int index = 0; index < getCompleted.Count; ++index)
        {
            //Debug.Log("complete load: " + completed[index]);
            for (int i = 0; i < levelButtons.Count; ++i)
            {
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    if (levelButtons[i][ii].GetComponent<ChooseButton>().levelValue == getCompleted[index])
                    {
                        //Debug.Log("loadedCom: " + index);
                        levelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    }
                }
            }
        }

        UpdateCompleted();
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

        string level = "";
        

        // 1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12-    one level

        if (savedLevels.Length > 0)
        {
            for (int index = 0; index < savedLevels.Length; index++)
            {
                level += savedLevels[index];
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

                    if (loadLevel.Count > 0)
                    {
                        List<List<int>> newLevel = new List<List<int>>();

                        newLevel = loadLevel;

                        //Debug.Log("Level Count = " + levels.Count);

                        levels.Add(newLevel);
                        

                        
                        loadLevel = new List<List<int>>();

                        

                        level = "";
                        
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

    public Vector2 hintTubes = Vector2.zero;

    public void Hint()
    {
        if (coins > hintCost)
        {
            coins -= hintCost;

            Test();
            lookingForHint = true;
            Debug.LogError("hint");
        }
    }

    public void Test()
    {
        
        List<List<int>> solvePoint = new List<List<int>>();
        List<GameObject> gameTubes = new List<GameObject>();

        if (gameManager.undoHolster.Count > 0) { gameTubes = gameManager.undoHolster[gameManager.undoHolster.Count - 1]; }
        else
        {
            solvePoint = gameObject.GetComponent<LevelSolver>().CopyBoard(levels[lastLevelLoaded]);
            solvePoint.Add(new List<int>());
            solvePoint[12].Add(0);
            solvePoint[12].Add(0);
            solvePoint[12].Add(0);
            solvePoint[12].Add(0);
            solvePoint.Add(new List<int>());
            solvePoint[13].Add(0);
            solvePoint[13].Add(0);
            solvePoint[13].Add(0);
            solvePoint[13].Add(0);
            gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, lastLevelLoaded);
            
            return;
        }

        //Debug.Log(gameTubes.Count);

        for (int i = 0; i < gameTubes.Count; ++i)
        {
            solvePoint.Add(new List<int>());
            for (int j = 1; j < gameTubes[i].GetComponent<Tube>().spots.Count; ++j)
            {
                solvePoint[i].Add(gameTubes[i].GetComponent<Tube>().spots[j]);
            }
        }
        gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, lastLevelLoaded);
        
    }

    public TextAsset solveCheck;
    public bool SolveList(List<List<int>> generated, int index)
    {
        bool solvable = false;
        List<List<int>> solvePoint = new List<List<int>>(generated);

        for (int i = 0; i < generated.Count; ++i)
        {
            solvePoint[i] = new List<int>(generated[i]);
        }

        for (int j = 0; j < solvePoint.Count; ++j)
        {
            for (int ii = 0; ii < solvePoint[j].Count; ++ii)
            {
                solvePoint[j][ii]++;
            }
        }

        solvePoint.Add(new List<int>());
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint.Add(new List<int>());
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);
        solvePoint[solvePoint.Count - 1].Add(0);

        solvable = gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, index);
        
        return solvable;
        
    }

    public void AddPageToCompleted()
    {
        for (int j = 0; j < levelButtons.Count; ++j)
        {
            for (int i = 0; i < levelButtons[j].Count; ++i)
            {
                levelButtons[j][i].GetComponent<Image>().color = completedMat.color;
            }
        }

        UpdateCompleted();
    }
}
