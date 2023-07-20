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
    void Start()
    {
        gameManager = GetComponent<GameManager>();
        savedLevels = GetLevels();
        LoadGame();
        LoadLevelChooseList();
        LoadCompleted();
        challengeRequirement.SetActive(ChallengeRequirement());
    }
    void Update()
    {
        SaveCompleted();
        UpdateCoins();

        if (generatingChallenge && challengeSolvability.Count < 5) // checks if the challenge is done generating or not
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

        if (lookingForHint && gameObject.GetComponent<LevelSolver>().hintFlash.tubes == Vector2.zero) // checks if the user is looking for a hint but doesnt have it yet
        {
            if (gameManager.menuNum == 2)
            {
                loadingIcon.SetActive(true);
            }
        }
        else if (gameObject.GetComponent<LevelSolver>().hintFlash.tubes != Vector2.zero && lookingForHint) // when the user got a hint and is waiting for them to use it
        {
            loadingIcon.SetActive(false);
            List<GameObject> gameTubes = gameManager.tubes;
            HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;

            flash.transform.position = gameTubes[(int)flash.tubes[flash.index]].transform.position;
            flash.gameObject.SetActive(true);       
        }
        else if (!lookingForHint && gameObject.GetComponent<LevelSolver>().hintFlash.tubes == Vector2.zero) // when the hint phase is over
        {
            loadingIcon.SetActive(false);
        }

        if (inChallenge && startChallenge && !finishedChallenge) // updating challenge times while challenge is being played
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

    void UpdateCoins() // saves the coins a player has
    {
        coinsText.text = coins.ToString();
        PlayerPrefs.SetInt("CoinCount", coins);
    }

    public void WriteLevels(string path) // save the string of levels to a text file
    {
        StreamWriter writer = new (path);

        writer.WriteLine(savedLevels);

        writer.Close();
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

    public int tubeCount;
    public void GenerateLevelsButton() // generate a certain number of levels and then solve them
    {
        for (int i = 0; i < genXLevels; ++i)
        {
            List<List<int>> newLevel = GenerateLevel(i, tubeCount - 2);
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

    List<int> FindPossibleChoices(int tubeCount) // find the possible ball options for generating a level
    {
        List<int> choices = new List<int>();

        for (int i = 0; i < tubeCount; ++i)
        {
            if (chosen[i] < 4)
            {
                choices.Add(i);
            }
        }
        return choices;
    }

    bool CompletedTube(List<int> tube) // checks if a tube has been completed when generating
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

    List<List<int>> GenerateLevel(int index, int tubeCount) // generates a random new level then solves it
    {
        ResetMaker(tubeCount);

        if (!FinishedMaking())
        {           
            List<List<int>> newLevel = new List<List<int>>(tubeCount);

            for (int ii = 0; ii < tubeCount; ii++)
            {
                List<int> newTube = new List<int>(4);
                
                for (int i = 0; i < 4; ++i)
                {
                    List<int> choices = FindPossibleChoices(tubeCount);
                    int add = UnityEngine.Random.Range(0, choices.Count);               
                    newTube.Add(choices[add]);
                    
                    chosen[choices[add]]++;           
                }

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

                if (!output)
                {
                    Debug.Log("no solution");
                    GenerateLevel(index, tubeCount);
                }

                return newLevel;
            }
        }

        return null;
    }
    bool ChallengeRequirement() // checks if the player can play the challenge
    {
        for (int i = 0; i < levelButtons[0].Count; ++i)
        {
            if (levelButtons[0][i].GetComponent<Image>().color != completedMat.color) { return true; }
        }
        return false;
    }

    string OrganizeRecordTime(float time) // organizes the input time into minutes:seconds.milliseconds
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
        

        if (Minutes == 0)
        {
            min = "00";
        }
        else if (Minutes < 10)
        { 
            min = "0" + Minutes.ToString();
        }
        else 
        {
            min = Minutes.ToString();
        }

        if (Seconds == 0) 
        {
            sec = "00";
        }
        else if (Seconds < 10) 
        {
            sec = "0" + Seconds.ToString();
        }
        else
        {
            sec = Seconds.ToString();
        }

        if (milli == 0)
        {
            ms = "00";
        }
        else if (milli < 10) 
        {
            ms = "0" + milli.ToString();
        }
        else
        { 
            ms = milli.ToString();
        }

        end = min + ":" + sec + "." + ms;
        return end;
    }
    public void StartChallengeCoroutine() // starts making the challenge levels
    {
        challengeSolvability.Clear();
        StartCoroutine(MakeChallengeLevels());
        StartChallenge();
    }
    IEnumerator MakeChallengeLevels() // makes all challenge levels
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

    public void StartChallenge() // starts the challenge, updates ui, creates list button, and sets bools
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

        if (challengeSpots.Count > 0)
        {
            challengeSpots.Clear();
        }

        Vector2 listRect = new Vector2(challengeList.GetComponent<RectTransform>().rect.width, challengeList.GetComponent<RectTransform>().rect.height);
        Vector2 chooseButtonRect = new Vector2(chooseButtonPrefab.GetComponent<RectTransform>().rect.width, chooseButtonPrefab.GetComponent<RectTransform>().rect.height);

        float sizeX = listRect.x / challengeLevelsPerPage.x;
        float sizeY = listRect.y / challengeLevelsPerPage.y;
        float scaleX = sizeX / chooseButtonRect.x;
        float scaleY = sizeY / chooseButtonRect.y;

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

                if (challengeSpotCount > LPP - 1)
                {
                    challengeSpotCount = 0;
                    List<GameObject> add = new List<GameObject>();

                    challengeLevelButtons.Add(page);

                    page = add;
                }
            }
        }

        if (page.Count != 0)
        { 
            challengeLevelButtons.Add(page);
        }
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
                }
            }          
        }

        gameObject.GetComponent<GameManager>().menuNum = 2;
        lastLevelLoaded = index;      
    }
    public void LoadLastChallengeLevel() // loads the last challenge level
    {
        LoadChallengeLevel(lastLevelLoaded);
    }
    public bool BeatLastChallengeLevel() // sets values for beating the last loaded challenge level
    {
        for (int i = 0; i < challengeLevelButtons.Count; ++i)
        {
            for (int ii = 0; ii < challengeLevelButtons[i].Count; ++ii)
            {
                if (lastLevelLoaded + 1 == challengeLevelButtons[i][ii].GetComponent<ChooseButton>().levelValue && challengeLevelButtons[i][ii].GetComponent<Image>().color != completedMat.color)
                {
                    challengeLevelButtons[i][ii].GetComponent<Image>().color = completedMat.color;
                    return true;
                }
            }
        }
        return false;
    }

    public void GiveUpChallenge() // ends the challenge, sets ui, sets bools
    {
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
            Destroy(challengeSpots[i]);
        }

        challengeSpots.Clear();

        UpdateListPage();
        UpdatePageButtons();

        WinLevels();
    }

    bool BeatChallenge() // checks if the player has beat the challenge or not
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
    public void SaveChallengeTime() // saves the challenge time to playerprefs
    {
        if (challengeTime < loadedChallengeTime || loadedChallengeTime == 0)
        {
            
            PlayerPrefs.SetFloat("ChallengeTime", challengeTime); 
        }
        else { PlayerPrefs.SetFloat("ChallengeTime", loadedChallengeTime); }
    }
    string OrganizeChallengeTime(float time) // organizes the challenge time to minutes:seconds.milliseconds
    {
        string end = "00:00.00";
        DateTime challengeCurrent = DateTime.Now;

        TimeSpan ts = challengeCurrent - challengeStart;
        string min = "";
        string sec = "";
        string ms = "";
        float milli = Mathf.Round(ts.Milliseconds / 10);

        if (ts.Minutes == 0)
        {
            min = "00"; 
        }
        else if (ts.Minutes < 10)
        {
            min = "0" + ts.Minutes.ToString();
        }
        else 
        { 
            min = ts.Minutes.ToString();
        }

        if (ts.Seconds == 0)
        { 
            sec = "00";
        }
        else if (ts.Seconds < 10)
        {
            sec = "0" + ts.Seconds.ToString(); 
        }
        else
        { 
            sec = ts.Seconds.ToString();
        }

        if (milli == 0)
        {
            ms = "00";
        }
        else if (milli < 10)
        { 
            ms = "0" + milli.ToString();
        }
        else
        {
            ms = milli.ToString();
        }

        end = min + ":" + sec + "." + ms;
        return end;
    }

    public void AddToLevelList(List<List<int>> newLevel) // creates button for the user to press
    {
        GameObject newButton = Instantiate(chooseButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        newButton.transform.SetParent(spots[spotCount].transform);
        newButton.transform.position = spots[spotCount].transform.position;
        newButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        newButton.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = "Level " + actualCount;
        newButton.GetComponent<ChooseButton>().levelValue = actualCount;
        spotCount++;
        actualCount++;

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

    float GenerateSpots() // generates the spots that the buttons for the user to press get placed on
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

        if (sizeX == sizeY) 
        {
            return scaleX - 0.05f;
        }

        return 1;
    }

    public void LoadLevelChooseList() // create the initial list of levels that you can click on to load
    {
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

                if (spotCount > LPP - 1)
                { 
                    spotCount = 0;
                    List<GameObject> add = new List<GameObject>();
                    
                    levelButtons.Add(page);

                    page = add;
                }
            }
        }
        if (page.Count != 0)
        { 
            levelButtons.Add(page);
        }  

        currentLevelPage = 0;
        UpdateListPage();
        UpdatePageButtons();
    }

    void UpdateListPage() // updates the buttons to be the correct page
    {
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            if (currentLevelPage == i)
            {              
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
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
        if (requirementBox.activeSelf)
        {
            requirementText.text = "Complete levels " + levelButtons[currentLevelPage - 1][0].GetComponent<ChooseButton>().levelValue + "-" + levelButtons[currentLevelPage - 1][levelButtons[currentLevelPage - 1].Count - 1].GetComponent<ChooseButton>().levelValue + " to unlock";
        }
    }

    bool CheckRequirement(int page) // checks if a certain page has been completed or not
    {
        if (page != 0 && page != 1 && page != 2)
        {
            for (int i = 0; i < levelButtons[page - 1].Count; ++i)
            {
                if (levelButtons[page - 1][i].GetComponent<Image>().color != completedMat.color)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void UpdatePageButtons() // updates the page buttons to be interactable or not
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
    public void PageLeft() // is called whe the single left button is pressed
    {
        currentLevelPage--;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageLeftFar() // is called whe the double left button is pressed
    {
        currentLevelPage = 0;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageRight() // is called whe the single right button is pressed
    {
        currentLevelPage++;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void PageRightFar() // is called whe the double right button is pressed
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

        if (cp != levelButtons.Count - 1)
        {
            cp--;
        }

        if (cp > levelButtons.Count - 1)
        {
            cp = levelButtons.Count - 1;
        }

        currentLevelPage = cp;
        UpdateListPage();
        UpdatePageButtons();
    }

    public void LoadLevel(int index) // load a certain level based on the index given
    {
        gameManager.undoHolster.Clear();
        gameManager.TTHolster.Clear();

        Debug.Log($"Loaded level {index} with {levels[index].Count} tubes");
        gameManager.SetResetTubes(levels[index].Count);
        gameManager.ResetGame();

        List<GameObject> Gametubes = gameObject.GetComponent<GameManager>().tubes;

        levelNumText.text = "Level " + (index + 1).ToString();

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
                }
            }
            gameObject.GetComponent<GameManager>().Cork();
        }

        gameManager.menuNum = 2;
        lastLevelLoaded = index;
        gameManager.SetUndoTubes();       
    }
    public void LoadLastLevel() // loads the last level
    {
        LoadLevel(lastLevelLoaded);
    }

    public void WinScreen() // is called when the player completes a level
    {
        winScreen.SetActive(true);
        winCoinText.text = "+" + coinIncriment.ToString() + " Coins";

        for (int i = 0; i < confettiSpots.Count; ++i)
        {
            ParticleSystem confetti = Instantiate(confettiPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            confetti.gameObject.transform.localScale = new Vector3(1, 1, 1);
            Vector3 pos = confettiSpots[i].transform.position;
            pos.z = -1;

            confetti.gameObject.transform.position = pos;

            confetti.Play();
        }

        if (!inChallenge)
        {
            if (!BeatLastLevel())
            {
                winCoinText.text = "You've Already Beat This Level!"; 
            }

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
            if (!BeatLastChallengeLevel())
            { 
                winCoinText.text = "You've Already Beat This Challenge Level!";
            }

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

    public void WinChallengeScreen() // this is called when the player completes the challenge
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
    public void EndChallenge() // this is called when the player is done with the win challenge screen
    {
        challengeWinScreen.SetActive(false);

        winNextButton.gameObject.SetActive(true);
        levelsPageButton.gameObject.SetActive(true);

        goToWinChallengeButton.gameObject.SetActive(false);

        GiveUpChallenge();
    }

    public void WinNext() // this is called when the player wants to move onto the next level on the win screen
    {
        gameObject.GetComponent<GameManager>().ResetGame();
        UpdateCompleted();
        
        if (!inChallenge)
        {
            if (lastLevelLoaded < levels.Count)
            {
                for (int i = 1; i < 150;  i++)
                {
                    int test = lastLevelLoaded + i;
                    if (!completed.Contains(test))
                    {
                        LoadLevel(test);
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
    public void WinLevels() // this is called when the player wants to go back to the levels screen
    {
        gameObject.GetComponent<GameManager>().menuNum = 1;
        gameObject.GetComponent<GameManager>().ResetGame();
        winScreen.SetActive(false);
    }

    public bool BeatLastLevel() // this is called when the player beats the level they're on
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
            }
        }

        challengeRequirement.SetActive(ChallengeRequirement());

        return false;    
    }
    public void BeatIndexLevel(int index) // this is called to set the beaten levels at the start
    {
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

    public void LoadCompleted() // this loads the completed levels of the player
    {    
        if (PlayerPrefs.HasKey("SavedString"))
        {
            completedSave = PlayerPrefs.GetString("SavedString");
        }
        if (PlayerPrefs.HasKey("CoinCount"))
        {
            coins = PlayerPrefs.GetInt("CoinCount");
        }

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

                    if (!PlayerPrefs.HasKey("BEGINNER"))
                    {
                        add += 120;
                    }

                    getCompleted.Add(add);
                    
                    set = "";
                }
            }

            PlayerPrefs.SetInt("o", 0);
            PlayerPrefs.SetInt("FIXTUBES", 0);
            PlayerPrefs.SetInt("BEGINNER", 0);
        }

        completed = getCompleted;

        for (int index = 0; index < getCompleted.Count; ++index)
        {
            for (int i = 0; i < levelButtons.Count; ++i)
            {
                for (int ii = 0; ii < levelButtons[i].Count; ++ii)
                {
                    if (levelButtons[i][ii].GetComponent<ChooseButton>().levelValue == getCompleted[index])
                    {
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

    public void UpdateCompleted() // this updates the completed levels of the player then saves them
    {
        string newCompleted = "";
        completed.Clear();

        for (int i = 0; i < levelButtons.Count; ++i)
        {
            for (int ii = 0; ii < levelButtons[i].Count; ++ii)
            {
                if (levelButtons[i][ii].GetComponent<Image>().color == completedMat.color)
                {
                    newCompleted = newCompleted + levelButtons[i][ii].GetComponent<ChooseButton>().levelValue.ToString() + ",";
                    completed.Add(levelButtons[i][ii].GetComponent<ChooseButton>().levelValue - 1);
                }               
            }
        }

        completedSave = newCompleted;
        SaveCompleted();
    }
    public void SaveCompleted()
    {
        PlayerPrefs.SetString("SavedString", completedSave);
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
                            ball = "";
                            int newBall = loadBall;

                            loadTube.Add(newBall);
                        }

                        List<int> newTube = loadTube;

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
                            ball = "";
                            int newBall = loadBall;

                            loadTube.Add(newBall);
                        }

                        List<int> newTube = loadTube;

                        loadLevel.Add(newTube);

                        loadTube = new List<int>();
                    }

                    if (loadLevel.Count > 0)
                    {
                        List<List<int>> newLevel = new List<List<int>>();

                        newLevel = loadLevel;
                        levels.Add(newLevel);
                        
                        loadLevel = new List<List<int>>();

                        level = "";                        
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

    public void Hint() // this is called when the player presses the hint button
    {
        if (coins > hintCost)
        {
            coins -= hintCost;

            DoHint();
            lookingForHint = true;
        }
    }

    public void DoHint() // this solves the current board and gives the player a hint
    {       
        List<List<int>> solvePoint = new List<List<int>>();
        List<GameObject> gameTubes = new List<GameObject>();

        if (gameManager.undoHolster.Count > 0)
        {
            gameTubes = gameManager.undoHolster[gameManager.undoHolster.Count - 1];
        }
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
    public bool SolveList(List<List<int>> generated, int index) // this solves a list of a list of ints
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

    public void AddPageToCompleted() // Debug feature
    {
        for (int j = 0; j < 5; ++j)
        {
            for (int i = 0; i < levelButtons[j].Count; ++i)
            {
                levelButtons[j][i].GetComponent<Image>().color = completedMat.color;
            }
        }

        UpdateCompleted();
    }
}
