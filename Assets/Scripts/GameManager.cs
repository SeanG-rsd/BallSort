using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Data;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Analytics;
using Mono.Data.SqliteClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using BallSortSolver;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int numLevels;

    private List<int> chosen = new List<int>();

    private List<LevelButton> levelButtons;

    [Header("---Levels---")]
    [SerializeField] private GameObject levelButtonPrefab;

    private List<int> completedLevels = new List<int>();
    private string completedSaveLevels = "";

    [SerializeField] private Vector2Int pageLevelLayout;

    [SerializeField] private Transform levelButtonContainer;

    [SerializeField] private int generateXLevels;

    [SerializeField] private LevelSolver levelSolver;

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

    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader dbreader;  // not used in this example

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


    private async void StartLoadingGame()
    {
        //loadingScreen.SetActive(true);

        InitDatabase();
        GenerateLevelSpots();

        Debug.Log(numLevels);

        LoadLevelChooseList();

        MenuManager.instance.OpenMenuNumber(MenuManager.instance.levelScreenIndex);

        await LoadUnityGamingServices();

        PageFarRight();
    }
    async Task LoadUnityGamingServices()
    {
        string environment = "production";

        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName(environment);

            await UnityServices.InitializeAsync(options);

            AnalyticsService.Instance.StartDataCollection();
        }
        catch (Exception exception)
        {
            Debug.LogError($"Unity Services Init Failed: {exception.Message}");
        }
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
            loadingBarMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, highestLoadedLevel / (float)numLevels * loadingBarStartingWidth);
        }
    }

    #region Database

    private void InitDatabase()
    {
        string DATABASE_NAME = "/levels_database.s3db";

        string sourcePath = Application.streamingAssetsPath + DATABASE_NAME;
        string targetPath = Application.persistentDataPath + DATABASE_NAME;

#if UNITY_ANDROID
            UnityWebRequest www = UnityWebRequest.Get(sourcePath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(targetPath, www.downloadHandler.data);
            }
            else
            {
                Debug.LogError("Failed to load DB from StreamingAssets: " + www.error);
            }
#else
        if (!File.Exists(targetPath))
        {
            File.Copy(sourcePath, targetPath);
        }
#endif

        Debug.Log($"filepath={targetPath}");
        conn = "URI=file:" + targetPath;

        numLevels = SetNumLevels();

        //CreateLevelTable();
    }

    private int SetNumLevels()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            string text = "Not Found";

            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            sqlQuery = "SELECT COUNT(*) FROM [levels]";
            dbcmd.CommandText = sqlQuery;
            dbreader = dbcmd.ExecuteReader();
            if (dbreader.Read())
            {
                text = dbreader.GetString(0);
            }
            else
            {
                Debug.Log("QueryString - nothing to read...");
            }

            dbconn.Close();

            return Int32.Parse(text);
        }
    }

    private void CreateLevelTable()
    {
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            sqlQuery = "CREATE TABLE IF NOT EXISTS [levels] (" +
                       "[level_index] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                       "[level] VARCHAR(255) NOT NULL" +
                       "[completed] BOOLEAN NOT NULL DEFAULT 0)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
            dbconn.Close();
        }
    }

    private void InsertLevel(List<List<int>> level)
    {
        string info = "";
        for (int i = 0; i < level.Count - 1; i++)
        {
            info += string.Join(",", level[i]) + ",";
        }

        info += string.Join(",", level[level.Count - 1]);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            dbcmd = dbconn.CreateCommand();
            string sqlQuery = "INSERT OR REPLACE INTO [levels] ([level]) VALUES (@level)";

            dbcmd.CommandText = sqlQuery;
            dbcmd.Parameters.Add(new SqliteParameter("@level", info));
            dbcmd.ExecuteNonQuery();
            dbconn.Close();
        }
    }

    private List<List<int>> ReadLevel(int index)
    {
        string text = "Not Found";
        dbconn = new SqliteConnection(conn);
        dbconn.Open();

        string sqlQuery = "SELECT [level] FROM [levels] WHERE [level_index] = @level_index";
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;

        dbcmd.Parameters.Add(new SqliteParameter("@level_index", index.ToString()));

        dbreader = dbcmd.ExecuteReader();
        if (dbreader.Read())
        {
            text = dbreader.GetString(0);
        }
        else
        {
            Debug.Log("QueryString - nothing to read...");
        }
        dbreader.Close();
        dbconn.Close();

        int sublistSize = 4;

        List<int> flatList = text.Split(',').Select(int.Parse).ToList();

        List<List<int>> level = new List<List<int>>();
        for (int i = 0; i < flatList.Count; i += sublistSize)
        {
            level.Add(flatList.GetRange(i, sublistSize));
        }

        return level;
    }

    private bool ReadIsCompleted(int index)
    {
        int completed = -1;
        dbconn = new SqliteConnection(conn);
        dbconn.Open();

        string sqlQuery = "SELECT [completed] FROM [levels] WHERE [level_index] = @level_index";
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;

        dbcmd.Parameters.Add(new SqliteParameter("@level_index", index.ToString()));

        dbreader = dbcmd.ExecuteReader();
        if (dbreader.Read())
        {
            completed = dbreader.GetInt32(0); // Assuming you're retrieving the first column (id)
        }
        else
        {
            Debug.Log("QueryString - nothing to read...");
        }
        dbreader.Close();
        dbconn.Close();

        return completed == 1;
    }

    private void CompleteLevel(int index)
    {
        dbconn = new SqliteConnection(conn);
        dbconn.Open();

        string sqlQuery = "UPDATE [levels] SET [completed]=1 WHERE [level_index] = @level_index";
        dbcmd = dbconn.CreateCommand();
        dbcmd.CommandText = sqlQuery;

        dbcmd.Parameters.Add(new SqliteParameter("@level_index", index.ToString()));
        int rows = dbcmd.ExecuteNonQuery();
    }

    #endregion

    #region Dev Options

    [SerializeField] private TMP_InputField DEV_LEVEL_INPUT;
    [SerializeField] private TMP_InputField DEV_PASSWORD_INPUT;

    public void DEV_COMPLETE_LEVELS() {
        if (DEV_PASSWORD_INPUT.text == "NutronLabs") {
            PageFarLeft();
            for (int i = 1; i <= Int32.Parse(DEV_LEVEL_INPUT.text); i++) {
                BeatLevel(i);
                if (i % 60 == 0) PageRight();
            }
        }
    }

    public void DEV_TOGGLE_TAB(GameObject tab) {
        tab.SetActive(!tab.activeSelf);
    }

    #endregion

    #region Initialize Game

    public void LoadCompleted() // this loads the completed levels of the player
    {
        if (!PlayerPrefs.HasKey("DatabaseUpdate"))
        {
            PlayerPrefs.SetInt("DatabaseUpdate", 1);
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
                        CompleteLevel(add);

                        set = "";
                    }

                }
            }
        }
    }

    public int GetNumberOfCompletedLevels()
    {
        return completedLevels.Count;
    }

    public int GetNumberOfLevels()
    {
        return numLevels;
    }

    public List<List<int>> GetLevel(int levelNumber)
    {
        return ReadLevel(levelNumber);
    }

    public bool GetIsCompleted(int levelNumber)
    {
        return ReadIsCompleted(levelNumber);
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
            Debug.Log("made level");
            InsertLevel(newLevel);
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
        Debug.Log("started making");

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
                    return GenerateLevel(index, tubeCount);
                }
                newLevel.Add(newTube);
            }

            if (FinishedMaking())
            {
                bool output = levelSolver.SolveFromList(newLevel);

                if (!output)
                {
                    return GenerateLevel(index, tubeCount);
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
        levelButtons = new List<LevelButton>();
        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        for (int current = 0; current < levelsPerPage; current++)
        {
            GameObject newSpot = Instantiate(levelButtonPrefab, levelButtonContainer);

            levelButtons.Add(newSpot.GetComponent<LevelButton>());
        }
    }

    private void LoadLevelChooseList()
    {
        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        numberOfPages = numLevels / levelsPerPage;

        if (numLevels % levelsPerPage != 0)
        {
            numberOfPages++;
        }

        for (int i = 1; i <= numLevels; i++)
        {
            levelButtons[(i - 1) % levelsPerPage].AddNewLevel(i, GetIsCompleted(i));
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
        int currentCheck = 0;

        if (currentPage > numberOfTutorialPages)
        {
            currentCheck = numberOfTutorialPages + 1;
        }

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

        for (int spotIndex = 0; spotIndex < levelButtons.Count; spotIndex++)
        {
            levelButtons[spotIndex].SetPage(currentPage);
        }

        if (currentPage > 2)
        {
            pageRequirementBox.SetActive(CheckRequirement(currentPage));
        }
        else
        {
            pageRequirementBox.SetActive(false);
        }

        if (pageRequirementBox.activeSelf)
        {
            int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;
            pageRequirementText.text = "Complete levels " + levelButtons[0].GetLevelNumber() + "-" + (levelButtons[0].GetLevelNumber() + levelsPerPage - 1) + " to unlock";
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

        if (currentPage == numberOfPages - 1)
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

    private bool CheckRequirement(int page) // true if you can't go to the current page
    {
        if (page == 0 || page >= numberOfPages) return false;
        for (int i = 0; i < levelButtons.Count; ++i)
        {
            if (!levelButtons[i].isCompletedAtPage(page - 1))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Level Win

    private int lastLevelBeat;
    private bool isChallenge;

    private void NextLevel(int position, int LPP)
    {
        if (position == 0) // look for levels in the page
        {
            if (!CheckRequirement(currentPage + 1))
            {
                LevelManager.instance.OnClickLoadLevel(lastLevelBeat);
                Debug.Log("HERE");
            }
            else
            {
                for (int index = 0; index < LPP; index++)
                {
                    if (!levelButtons[index].isCompletedAtPage(currentPage))
                    {
                        LevelManager.instance.OnClickLoadLevel(levelButtons[index].GetLevelNumber());
                        currentPage = levelButtons[index].GetLevelNumber() / LPP;
                        UpdateListPage();
                        Debug.Log("also here");
                    }
                }
            }
        }
    }

    public void WinNext()
    {
        int LPP = pageLevelLayout.x * pageLevelLayout.y;
        lastLevelBeat++;

        Debug.Log(lastLevelBeat);

        int position = (lastLevelBeat - 1) % LPP;
        MenuManager.instance.ToggleWinScreen(false);

        PageFarRight();

        NextLevel(position, LPP);
    }

    public void WinLevels()
    {
        MenuManager.instance.OnClickLevelsButton();
        MenuManager.instance.ToggleWinScreen(false);
    }

    private bool BeatLevel(int levelIndex)
    {
        int LPP = pageLevelLayout.x * pageLevelLayout.y;

        int pageNumber = (levelIndex - 1) / LPP;
        int number = (levelIndex - 1) % LPP;

        if (levelButtons[number].isCompletedAtPage(pageNumber))
        {
            return false;
        }

        LevelManager.instance.AddCoins(coinIncrement);
        CompleteLevel(levelIndex);
        levelButtons[number].SetColor(true);
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

            //UpdateCompleted();


            if (lastLevelBeat >= numLevels - 1)
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
