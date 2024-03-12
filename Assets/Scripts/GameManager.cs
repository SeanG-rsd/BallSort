using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class GameManager : MonoBehaviour
{ 
    public static GameManager instance;

    private List<List<List<int>>> levels = new List<List<List<int>>>(); // a list of levels

    private List<int> chosen = new List<int>();

    private string savedLevels = "";


    private List<LevelSpot> levelButtonSpots;

    [Header("---Levels---")]
    [SerializeField] private GameObject levelButtonPrefab;

    private List<int> completedLevels = new List<int>();
    private string completedSaveLevels = "";

    [SerializeField] private TextAsset levelsFile;

    [SerializeField] private Vector2Int pageLevelLayout;
    [SerializeField] private GameObject spotPrefab;
    [SerializeField] private Transform levelSpotContainer;

    [SerializeField] private int generateXLevels;

    [Header("---Win---")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button winNextButton;

    [SerializeField] private Transform[] confettiSpots;
    [SerializeField] private ParticleSystem confettiPrefab;
    [SerializeField] private TMP_Text winCoinText;
    [SerializeField] private int coinIncrement;

    [SerializeField] private float winScreenWaitTime;
    private float winScreenTimer;
    private bool isWaitingForWinScreen;

    public static Action OnWinScreen = delegate { };


    [Header("---Pages---")]
    [SerializeField] private Button[] pageButtons;
    [SerializeField] private int leftIndex, rightIndex, farLeftIndex, farRightIndex;
    private int currentPage;
    private int numberOfPages;
    private int numberOfTutorialPages = 2;

    [SerializeField] private GameObject pageRequirementBox;
    [SerializeField] private TMP_Text pageRequirementText;

    [SerializeField] private TMP_Text pageNumberText;

    [Header("---LoadingScreen---")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private RectTransform loadingBarMask;
    private float loadingBarStartingWidth;

    private void Awake()
    {
        loadingBarStartingWidth = loadingBarMask.rect.width;
        StartLoadingGame();
        LevelManager.OnBeatLevel += HandleBeatLevel;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        LevelManager.OnBeatLevel -= HandleBeatLevel;
    }

    DateTime time = DateTime.Now;


    private void StartLoadingGame()
    {
        loadingScreen.SetActive(true);

        savedLevels = GetLevels();
        LoadGame();
        GenerateLevelSpots();

        LoadLevelChooseList();
    }

    private void Update()
    {
        if (isWaitingForWinScreen)
        {
            winScreenTimer -= Time.deltaTime;

            if (winScreenTimer < 0)
            {
                isWaitingForWinScreen = false;
                WinScreen();
            }
        }

        if (loadingScreen.activeSelf)
        {
            loadingBarMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, highestLoadedLevel / (float)levels.Count * loadingBarStartingWidth);
        }
    }

    #region Initialize Game
    public void AddToDatabase(int index) // add a new level to the saved string
    {
        string level = "";

        for (int i = 0; i < levels[index].Count; ++i)
        {
            string tube = "";

            for (int ii = 0; ii < levels[index][i].Count; ++ii)
            {
                string ball = levels[index][i][ii].ToString();

                if (ii == levels[index][i].Count - 1) { tube += ball; }
                else { tube = tube + ball + ","; }
            }

            if (i == levels[index].Count - 1) { level += tube; }
            else { level = level + tube + ":"; }
        }

        level += "-";

        savedLevels += level;
    }

    public void LoadCompleted() // this loads the completed levels of the player
    {
        if (PlayerPrefs.HasKey("SavedString"))
        {
            completedSaveLevels = PlayerPrefs.GetString("SavedString");
        }

        completedLevels = new List<int>();
        string set = "";

        if (completedSaveLevels.Length > 0)
        {
            for (int i = 0; i < completedSaveLevels.Length; ++i)
            {
                if (completedSaveLevels[i] != ',')
                {
                    set += completedSaveLevels[i];
                }
                else
                {
                    int add = Convert.ToInt32(set);
                    completedLevels.Add(add);

                    set = "";
                }

            }
        }

        for (int spot = 0; spot < levelButtonSpots.Count; ++spot)
        {
            for (int level = 0; level < levelButtonSpots[spot].levelSpots.Count; level++)
            {
                if (completedLevels.Contains(levelButtonSpots[spot].GetLevelNumber(level)))
                {
                    levelButtonSpots[spot].SetLevel(level, true);
                }
            }
        }
    }

    public void UpdateCompleted() // this updates the completed levels of the player then saves them
    {
        string newCompleted = "";

        for (int spot = 0; spot < levelButtonSpots.Count; ++spot)
        {
            for (int level = 0; level < levelButtonSpots[spot].levelSpots.Count; level++)
            {
                if (levelButtonSpots[spot].GetLevel(level))
                {
                    int value = levelButtonSpots[spot].GetLevelNumber(level);
                    newCompleted += value + ",";
                    if (!completedLevels.Contains(value))
                    {
                        completedLevels.Add(value - 1);
                    }
                }
            }
        }

        completedSaveLevels = newCompleted;
        SaveCompleted();
    }
    public void SaveCompleted()
    {
        PlayerPrefs.SetString("SavedString", completedSaveLevels);
    }

    public int GetNumberOfCompletedLevels()
    {
        return completedLevels.Count;
    }

    public void LoadGame() // load all the levels from a string to their list versions
    {
        DateTime time = DateTime.Now;
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
                            int newBall = Convert.ToInt32(ball);
                            ball = "";

                            loadTube.Add(newBall);

                        }
                    }
                    else if (savedLevels[index] == ':') // finish tube
                    {
                        if (ball != "")
                        {
                            int newBall = Convert.ToInt32(ball);
                            ball = "";

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
                            loadBall = Convert.ToInt32(ball);
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
                        List<List<int>> newLevel = new List<List<int>>(loadLevel);

                        levels.Add(newLevel);

                        loadLevel = new List<List<int>>();

                        level = "";
                    }
                }
            }
        }

        Debug.Log((DateTime.Now - time).TotalMilliseconds);
    }
    public string GetLevels() // open the text file containing the string of levels
    {
        return levelsFile.text;
    }

    public List<List<int>> GetLevel(int levelNumber)
    {
        return levels[levelNumber];
    }
    #endregion

    #region GenerateLevels

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
        for (int i = 0; i < generateXLevels; ++i)
        {
            List<List<int>> newLevel = GenerateLevel(i, tubeCount - 2);
            if (newLevel != null)
            {
                AddToDatabase(levels.Count - 1);
                WriteLevels("Assets/Resources/Levels.txt");
            }
            else
            {
                Debug.LogError("had full tube or was not solvable");
            }
        }
    }

    public void WriteLevels(string path) // save the string of levels to a text file
    {
        StreamWriter writer = new(path);

        writer.WriteLine(savedLevels);

        writer.Close();
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
                bool output = true;// SolveList(newLevel, index);

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

    #endregion

    #region Page Initializer

    private int highestLoadedLevel = 0;

    private void GenerateLevelSpots()
    {        
        levelButtonSpots = new List<LevelSpot>();
        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        for (int current = 0; current < levelsPerPage; current++)
        {
            GameObject newSpot = Instantiate(spotPrefab, levelSpotContainer);

            levelButtonSpots.Add(newSpot.GetComponent<LevelSpot>());
        }
    }

    private IEnumerator LoadLevelSpot(int index)
    {
        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        for (int current = 0; current < numberOfPages; current++)
        {
            int currentLevel = current * levelsPerPage + index + 1;
            if (currentLevel <= levels.Count)
            {
                LevelSpot currentSpot = levelButtonSpots[index];

                GameObject newButton = Instantiate(levelButtonPrefab, currentSpot.gameObject.transform);
                newButton.transform.localScale = Vector3.one;

                currentSpot.AddNewLevel(newButton, currentLevel);

                highestLoadedLevel++;
            }

            if (currentLevel == levels.Count)
            {
                StopAllCoroutines();
                loadingScreen.SetActive(false);
                Debug.Log((DateTime.Now - time).TotalMilliseconds);

                MenuManager.instance.OnClickStartGame();
                LoadCompleted();
                PageFarRight();

                StopAllCoroutines();
            }

            if (current % 5 == 0)
            {
                yield return null;
            }
        }
    }

    private void LoadLevelChooseList()
    {
        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        numberOfPages = levels.Count / levelsPerPage;

        if (levels.Count % levelsPerPage != 0)
        {
            numberOfPages++;
        }

        for (int index = 0; index < levelsPerPage; index++)
        {
            StartCoroutine(LoadLevelSpot(index));
        }
    }

    #endregion

    #region Page Manager

    public void PageLeft()
    {
        currentPage--;
        UpdateListPage();
    }

    public void PageRight()
    {
        currentPage++;
        UpdateListPage();
    }

    public void PageFarLeft()
    {
        currentPage = 0;
        UpdateListPage();
    }

    public void PageFarRight()
    {
        int currentCheck = 2;

        while (!CheckRequirement(currentCheck))
        {
            currentCheck++;
        }

        currentPage = currentCheck - 1;

        UpdateListPage();
    }

    private void UpdateListPage()
    {
        currentPage = Mathf.Clamp(currentPage, 0, numberOfPages);

        UpdateButtons();

        pageNumberText.text = (currentPage + 1).ToString();

        for (int spotIndex = 0; spotIndex < levelButtonSpots.Count; spotIndex++)
        {
            levelButtonSpots[spotIndex].SetPage(currentPage);
        }

        pageRequirementBox.SetActive(CheckRequirement(currentPage));

        if (pageRequirementBox.activeSelf)
        {
            int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;
            pageRequirementText.text = "Complete levels " + levelButtonSpots[0].GetLevelNumber(currentPage) + "-" + (levelButtonSpots[0].GetLevelNumber(currentPage) + levelsPerPage - 1) + " to unlock";
        }
    }

    private void UpdateButtons()
    {
        if (currentPage == 0)
        {
            pageButtons[leftIndex].interactable = false;
            pageButtons[farLeftIndex].interactable = false;
        }
        else
        {
            pageButtons[leftIndex].interactable = true;
            pageButtons[farLeftIndex].interactable = true;
        }

        if (currentPage == levelButtonSpots[0].levelSpots.Count - 1)
        {
            pageButtons[rightIndex].interactable = false;
            pageButtons[farRightIndex].interactable = false;
        }
        else
        {
            pageButtons[rightIndex].interactable = true;
            pageButtons[farRightIndex].interactable = true;
        }
    }

    private bool CheckRequirement(int page)
    {
        if (page != 0 && page != 1 && page != 2)
        {
            for (int i = 0; i < levelButtonSpots.Count; ++i)
            {
                if (!levelButtonSpots[i].GetLevel(page - 1))
                {
                    return true;
                }
            }
        }
        return false;
    }

    #endregion

    #region Level Win

    private int lastLevelBeat;
    private bool isChallenge;

    public void WinNext()
    {
        int LPP = pageLevelLayout.x * pageLevelLayout.y;

        int position = lastLevelBeat % LPP;

        if (position == 0) // look for levels in the page
        {
            if (CheckRequirement(currentPage + 1))
            {
                LevelManager.instance.OnClickLoadLevel(lastLevelBeat + 1);
            }
            else
            {
                for (int index = 0;  index < LPP; index++)
                {
                    if (!levelButtonSpots[index].GetLevel(currentPage))
                    {
                        LevelManager.instance.OnClickLoadLevel(levelButtonSpots[index].GetLevelNumber(currentPage));
                        return;
                    }
                }
            }
        }
        else
        {
            for (int index = 0; index < LPP - 1; index++)
            {
                if (!levelButtonSpots[(index + lastLevelBeat) % LPP].GetLevel(currentPage))
                {
                    LevelManager.instance.OnClickLoadLevel(levelButtonSpots[(index + lastLevelBeat) % LPP].GetLevelNumber(currentPage) - 1);
                    return;
                }
            }
        }

        MenuManager.instance.ToggleWinScreen(false);
    }

    public void WinLevels()
    {
        MenuManager.instance.OnClickLevelsButton();
        MenuManager.instance.ToggleWinScreen(false);
    }

    private bool BeatLevel(int levelIndex)
    {
        int LPP = pageLevelLayout.x * pageLevelLayout.y;

        int pageNumber = levelIndex / LPP;
        int number = levelIndex % LPP;

        if (levelButtonSpots[number].GetLevel(pageNumber))
        {
            return false;
        }

        LevelManager.instance.AddCoins(coinIncrement);
        levelButtonSpots[number].SetLevel(pageNumber, true);
        return true;
    }

    private bool BeatChallengeLevel(int levelIndex)
    {
        return false;
    }

    private void WinScreen()
    {
        OnWinScreen?.Invoke();
        MenuManager.instance.ToggleWinScreen(true);
        winCoinText.text = "+" + coinIncrement.ToString() + " Coins";

        for (int i = 0; i < confettiSpots.Length; ++i)
        {
            ParticleSystem confetti = Instantiate(confettiPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            confetti.gameObject.transform.localScale = new Vector3(1, 1, 1);
            Vector3 pos = confettiSpots[i].position;
            pos.z = -1;

            confetti.gameObject.transform.position = pos;

            confetti.Play();
        }

        if (!isChallenge)
        {
            if (!BeatLevel(lastLevelBeat))
            {
                winCoinText.text = "You've Already Beaten This Level!";
            }
            else
            {
                
            }

            UpdateCompleted();


            if (lastLevelBeat >= levels.Count - 1)
            {
                winNextButton.interactable = false;
                winCoinText.text = "You've Won the Game!\n+" + coinIncrement.ToString() + " Coins";
            }
        }
        else if (isChallenge)
        {
            /*winCoinText.text = "Challenge " + (levelIndex + 1).ToString() + " completed";
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
            }*/
        }
    }

    private void HandleBeatLevel(int levelIndex, bool isChallenge)
    {
        lastLevelBeat = levelIndex;
        this.isChallenge = isChallenge;

        winScreenTimer = winScreenWaitTime;
        isWaitingForWinScreen = true;
    }

    #endregion
}
