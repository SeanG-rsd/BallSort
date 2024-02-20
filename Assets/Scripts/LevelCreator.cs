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
    /* static LevelCreator Instance;

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
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

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
    bool ChallengeRequirement() // checks if the player can play the challenge
    {
        for (int j = 0; j < 2; ++j)
        {
            for (int i = 0; i < levelButtons[j].Count; ++i)
            {
                if (levelButtons[j][i].GetComponent<Image>().color != completedMat.color) { return true; }
            }
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

        //solvable = gameObject.GetComponent<LevelSolver>().InitiateLevel(solvePoint, index);
        
        return solvable;        
    }

    public void AddPageToCompleted() // Debug feature
    {
        for (int j = 0; j < 2; ++j)
        {
            for (int i = 0; i < levelButtons[j].Count; ++i)
            {
                levelButtons[j][i].GetComponent<Image>().color = completedMat.color;
            }
        }

        UpdateCompleted();
    }*/
}
