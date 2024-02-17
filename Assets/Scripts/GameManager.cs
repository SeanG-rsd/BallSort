using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

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

    [Header("---Pages---")]
    [SerializeField] private Button[] pageButtons;
    [SerializeField] private int leftIndex, rightIndex, farLeftIndex, farRightIndex;
    private int currentPage;
    private int numberOfPages;

    [SerializeField] private GameObject pageRequirementBox;
    [SerializeField] private TMP_Text pageRequirementText;

    [SerializeField] private TMP_Text pageNumberText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        savedLevels = GetLevels();
        LoadGame();
        LoadLevelChooseList();
        LoadCompleted();

        currentPage = 0;
        UpdateListPage();
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
                    levelButtonSpots[spot].UpdateLevel(level);
                }
            }
        }

        UpdateCompleted();
    }

    public void UpdateCompleted() // this updates the completed levels of the player then saves them
    {
        string newCompleted = "";
        completedLevels.Clear();

        for (int spot = 0; spot < levelButtonSpots.Count; ++spot)
        {
            for (int level = 0; level < levelButtonSpots[spot].levelSpots.Count; level++)
            {
                if (levelButtonSpots[spot].GetLevel(level))
                {
                    int value = levelButtonSpots[spot].GetLevelNumber(level);
                    newCompleted += value + ",";
                    completedLevels.Add(value - 1);
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



    #endregion

    #region Page Initializer

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

    private void LoadLevelChooseList()
    {
        GenerateLevelSpots();

        int levelsPerPage = pageLevelLayout.x * pageLevelLayout.y;

        for (int current = 0; current < levels.Count; current++)
        {
            LevelSpot currentSpot = levelButtonSpots[current % levelsPerPage];

            GameObject newButton = Instantiate(levelButtonPrefab, currentSpot.gameObject.transform);
            newButton.transform.localScale = Vector3.one;

            currentSpot.AddNewLevel(newButton, current + 1);
        }

        numberOfPages = levels.Count / levelsPerPage;

        if (levels.Count % levelsPerPage != 0)
        {
            numberOfPages++;
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
        int currentCheck = currentPage;

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
            levelButtonSpots[spotIndex].SetPage(currentPage + 1);
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
                if (!levelButtonSpots[i].GetLevel(page))
                {
                    return true;
                }
            }
        }
        return false;
    }

    #endregion

    // new stuff is anything above
    /*public List<GameObject> tubes;
    private int tinyTubeIndex = -1;

    public bool clickState;
    public GameObject firstTubeClicked;

    private int InvalidIndex = -1;

    Material currentMat;

    public List<GameObject> screens;
    public GameObject gameScreen;

    public int menuNum = 0;

    public List<GameObject> resetTubes = new List<GameObject>();
    
    public GameObject fullTubePrefab;
    public GameObject emptyTubePrefab;

    public GameObject undoButton;
    public GameObject modeButton;
    public TMP_Text modeButtonText;
    public List<GameObject> undoTubes = new List<GameObject>();
    public GameObject moveHolder;
    public bool canUndo;
    public List<List<GameObject>> undoHolster = new List<List<GameObject>>();
    private int undosLeft;
    public int givenUndos;
    public int undoCost;
    public TMP_Text undosLeftText;
    public GameObject undosLeftCoin;

    public GameObject insult;
    public TMP_Text insultText;
    public List<string> insults;
    bool doInsult;
    public float insultTime;
    float insultTimer;
    

    public Material blank;
    public Material badRed;

    public bool tinyTubeTime;
    bool tinyTubeActive;
    public GameObject TinyTube;
    public int TinyTubeCost;
    public Button TinyTubeButton;
    public TMP_Text TTTExt;
    public List<GameObject> TTHolster = new List<GameObject>();
    public GameObject TinyTubePrefab;

    public Popup NoMovesLeftBox;
    public float noMoveTime;

    bool win;
    static float winTime = 1.0f;
    float winTimer;

    public GameObject SettingsScreen;

    public Image gameBackground;
    public Image levelBackground;
    public Image settingsBackground;

    public Sprite greenSetting;
    public Sprite blueSetting;
    public Sprite redSetting;
    public Sprite defaultSetting;

    public GameObject greenSettingButton;
    public Button greenLockButton;

    public GameObject redSettingButton;
    public Button redLockButton;

    public GameObject blueSettingButton;
    public Button blueLockButton;

    public GameObject defaultSettingButton;

    public int BackgroundCost;

    public GameObject ballPathPrefab;

    public GameObject solveText;
    public Button solve;

    public GameObject HowToScreen;
    public GameObject hintButton;
    public TMP_Text hintCostText;

    public Transform tubeContainer;
    public TubeContainer tubeContainerObj;

    void Start()
    {

        insult.SetActive(false);
        SettingsScreen.SetActive(false);
        insultTimer = insultTime;
        winTimer = winTime;
        OpenMenuNum(1);
        

        ResetGame();
        LoadBackgrounds();
    }

    void Update()
    {
        if (win) // checks if the player has won the game and then waits to open the win screen
        {
            if (winTimer < 0)
            {
                win = false;
                winTimer = winTime;
                gameObject.GetComponent<LevelCreator>().WinScreen();
            }

            winTimer -= Time.deltaTime;
        }   

        for (int i = 0; i < screens.Count; ++i) // this is so the correct screen is always open when it is changed somewhere
        {
            if (i == menuNum && !screens[i].activeSelf)
            {
                OpenMenuNum(i);
            }
        }
    }

    public void SetResetTubes(int tubeCount) // sets the number of tubes for a level that is being loaded so that the level can load
    {
        resetTubes.Clear();

        for (int i = 0; i < tubeCount; ++i)
        {
            resetTubes.Add(fullTubePrefab);
        }

        resetTubes.Add(emptyTubePrefab);
        resetTubes.Add(emptyTubePrefab);

        int row = (tubeCount + 2) / 8;
        row++;

        tubeContainerObj.SetGrid(row);
    }

    public void ResetGame() // resets everything related to the game. this includes undos, tinytubes, moveHolders, the board
    {
        undosLeft = givenUndos;
        undosLeftText.text = undosLeft.ToString() + " Free";
        hintCostText.text = gameObject.GetComponent<LevelCreator>().hintCost.ToString();
        undosLeftCoin.SetActive(false);
        gameObject.GetComponent<LevelSolver>().resetFlash.SetActive(false);
        NoMovesLeftBox.Deactivate();
        gameObject.GetComponent<LevelSolver>().noSolution.Deactivate();

        if (undosLeft == 0)
        {
            undosLeftCoin.SetActive(true);
            undosLeftText.text = "     " + undoCost.ToString();
        }
        TTTExt.text = TinyTubeCost.ToString();

        for (int i = 0; i < tubes.Count; ++i)
        {
            Destroy(tubes[i]);
        }

        tubes.Clear();

        List<GameObject> testTubes = new List<GameObject>();

        for (int i = 0; i < resetTubes.Count; ++i)
        {
            GameObject test = Instantiate(resetTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

            test.transform.SetParent(tubeContainer);

            test.GetComponent<Tube>().siblingIndex = i;
            test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);         
            testTubes.Add(test);
        }
        tubes = testTubes;

        TinyTube.GetComponent<TinyTube>().ClearTube();

        canUndo = false;
        clickState = false;

        for (int i = 0; i < moveHolder.transform.childCount; ++i)
        {
            Destroy(moveHolder.transform.GetChild(i).gameObject);

        }
        undoHolster.Clear();
        TTHolster.Clear();
    }

    public void OpenMenuNum(int index) // opens a certain menu number based off of the index
    {
        for (int i = 0; i < screens.Count; ++i)
        {
            screens[i].SetActive(false);
        }

        screens[index].SetActive(true);
    }

    public void OpenLevelScreen() // opens the level screen, reseting the tiny tube and checking for challenge generation
    {
        menuNum = 1;
        OpenMenuNum(menuNum);
        ResetGame();

        if (gameObject.GetComponent<LevelCreator>().generatingChallenge)
        {
            gameObject.GetComponent<LevelCreator>().loadingIcon.SetActive(true);
        }

        if (tinyTubeActive)
        {
            tinyTubeActive = false;
            TinyTube.SetActive(false);
            TinyTubeButton.gameObject.SetActive(true);
        }
    }

    public void ResetCurrent() // this resets the game while in the game
    {
        ResetGame();
        gameObject.GetComponent<LevelCreator>().LoadLastLevel(); 
    }

    public void ResetChallenge() // this reset the game while in a challenge level
    {
        ResetGame();
        gameObject.GetComponent<LevelCreator>().LoadLastChallengeLevel();
    }

    bool CheckForMovement() // this checks if any balls are in motion at a moment, return true if there is movement and false if not
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            Tube tube = tubes[i].GetComponent<Tube>();

            for (int j = 0; j < tube.ballObjects.Count; ++j)
            {
                if (tube.ballObjects[j] != null)
                {
                    if (tube.ballObjects[j].GetComponent<Ball>().move)
                    {
                        return true;
                    }
                }
            }
        }

        if (TinyTube.activeSelf)
        {
            for (int j = 0; j < TinyTube.GetComponent<TinyTube>().ballObjects.Count; ++j)
            {
                if (TinyTube.GetComponent<TinyTube>().ballObjects[j] != null)
                {
                    if (TinyTube.GetComponent<TinyTube>().ballObjects[j].GetComponent<Ball>().move)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool CheckForWin() // check to see if all tubes are the right color or checks if there are any moves left or not
    {
        if (!CheckIfAnyMovesLeft() && !CheckForMovement())
        {
            NoMovesLeftBox.Activate(noMoveTime);
        }

        if (CheckForMovement())
        {
            return false;
        }

        for (int i = 0; i < tubes.Count; ++i)
        { 
            if (!tubes[i].GetComponent<Tube>().FullTube() && !tubes[i].GetComponent<Tube>().EmptyTube())
            {
                return false;
            } 
        }

        return true;
    }

    public void Cork() // corks the tubes if they are full
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            if (!tubes[i].GetComponent<Tube>().corked && !tubes[i].GetComponent<Tube>().CheckForMovement())
            { 
                if (tubes[i].GetComponent<Tube>().FullTube())
                { 
                    tubes[i].GetComponent<Tube>().Cork();
                }
            }       
        }
    }

    void EndHint() // moves the hint to the next tube in the hint sequence
    {
        if (gameObject.GetComponent<LevelCreator>().lookingForHint)
        {
            HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;
            flash.playerMadeMove = true;
        }
    }

    public void Clicked(GameObject tube)
    {
        if (!tube.GetComponent<Tube>().corked)
        {
            if (!clickState && !tube.GetComponent<Tube>().FullTube() && !tube.GetComponent<Tube>().EmptyTube()) // move ball to the top
            {
                clickState = true;
                firstTubeClicked = tube;
                Tube t = firstTubeClicked.GetComponent<Tube>();

                t.ballObjects[t.BottomIndex()].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, 0);
            
                t.MoveBottomToTop();
                HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;

                if (gameObject.GetComponent<LevelCreator>().lookingForHint && (int)flash.tubes[flash.index] == tube.GetComponent<Tube>().siblingIndex)
                { 
                    flash.index++;
                    flash.transform.position = tubes[(int)flash.tubes[flash.index]].transform.position;    
                }
            }
            else if (!tinyTubeTime && clickState && firstTubeClicked != tube) // move balls into empty tube
            {
                
                int ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

                Tube first = firstTubeClicked.GetComponent<Tube>();

                Tube second = tube.GetComponent<Tube>();

                if (second.EmptyTube())
                {

                    clickState = false;
                    int moveIndex = 4;
                    EndHint();

                    first.ballObjects[0].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, moveIndex);
                    
                    second.NewBallsToBottom(ball, firstTubeClicked.GetComponent<Tube>(), 0);

                    for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {

                        if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != InvalidIndex)
                        {
                            if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                            {
                                moveIndex--;
                                first.ballObjects[first.BottomIndex()].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, moveIndex);
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext(), firstTubeClicked.GetComponent<Tube>(), firstTubeClicked.GetComponent<Tube>().BottomIndex());
                            }
                        }
                    }

                    firstTubeClicked = null;
                    canUndo = true;
                    
                    SetUndoTubes();
                }
                else if (ball == second.GetBottomBall() && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
                {
                    clickState = false;
                    EndHint();

                    Tube t = firstTubeClicked.GetComponent<Tube>();

                    int moveIndex = second.BottomIndex() - 1;

                    first.ballObjects[0].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, moveIndex);
                    second.NewBallsToBottom(ball, firstTubeClicked.GetComponent<Tube>(), 0);

                    for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {
                        if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != InvalidIndex)
                        {
                            if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                            {
                                moveIndex--;
                                first.ballObjects[first.BottomIndex()].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, moveIndex);
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext(), firstTubeClicked.GetComponent<Tube>(), firstTubeClicked.GetComponent<Tube>().BottomIndex());
                            }
                        }
                    }

                    firstTubeClicked = null;
                    canUndo = true;
                    
                    SetUndoTubes();
                }
                else if (!second.FullTube())// move ball from different tube to the top
                {
                    if (!first.EmptyTube())
                    {
                        first.ballObjects[0].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, firstTubeClicked.GetComponent<Tube>().BottomIndex() - 1);
                    }
                    else
                    {
                        first.ballObjects[0].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, 4);
                    }
                    firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();
                    firstTubeClicked = tube;

                    first = firstTubeClicked.GetComponent<Tube>();
                    first.ballObjects[first.BottomIndex()].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, 0);
                    firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();

                    HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;

                    if (gameObject.GetComponent<LevelCreator>().lookingForHint && (int)flash.tubes[flash.index] != tube.GetComponent<Tube>().siblingIndex)
                    {
                        flash.index--;
                        flash.transform.position = tubes[(int)flash.tubes[flash.index]].transform.position;

                    }
                    else if (gameObject.GetComponent<LevelCreator>().lookingForHint && (int)flash.tubes[flash.index] == tube.GetComponent<Tube>().siblingIndex)
                    {
                        flash.index++;
                        flash.transform.position = tubes[(int)flash.tubes[flash.index]].transform.position;
                    }
                }
            }
            else if (tinyTubeTime && clickState && firstTubeClicked != tube) // move balls into empty tube
            {
                int ball = firstTubeClicked.GetComponent<TinyTube>().GetTopBall();

                Tube second = tube.GetComponent<Tube>();

                TinyTube first = firstTubeClicked.GetComponent<TinyTube>();
                //Debug.Log(ball.GetComponent<Image>().color);

                if (second.EmptyTube())
                {
                    clickState = false;
                    first.ballObjects[0].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, 4);      
                    
                    second.NewBallsFromTT(ball, firstTubeClicked.GetComponent<TinyTube>(), 0);

                    firstTubeClicked = null;
                    canUndo = true;
                    
                    SetUndoTubes();
                }
                else if (ball == second.GetBottomBall() && !second.FullTube() && firstTubeClicked.GetComponent<TinyTube>().CanMoveIntoNextTube(tube))
                {
                    clickState = false;

                    first.ballObjects[0].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, second.BottomIndex() - 1);
                    second.NewBallsFromTT(ball, firstTubeClicked.GetComponent<TinyTube>(), 0);

                    firstTubeClicked = null;
                    canUndo = true;

                    SetUndoTubes();
                }
                else if (!second.FullTube())// move ball from different tube to the top
                {

                    first.ballObjects[0].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, 1);
                    firstTubeClicked.GetComponent<TinyTube>().MoveTopToBottom();
                    
                    firstTubeClicked = tube;
                    
                    firstTubeClicked.GetComponent<Tube>().ballObjects[firstTubeClicked.GetComponent<Tube>().BottomIndex()].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, 0);
                    firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
                }
                tinyTubeTime = false;
            }     
            else if (clickState && firstTubeClicked == tube) // move ball back into the tube
            {
                HintFlash flash = gameObject.GetComponent<LevelSolver>().hintFlash;

                if (gameObject.GetComponent<LevelCreator>().lookingForHint)
                {
                    flash.index--;
                    flash.transform.position = tubes[(int)flash.tubes[flash.index]].transform.position;
                }

                tube.GetComponent<Tube>().MoveTopToBottom();
                tube.GetComponent<Tube>().ballObjects[tube.GetComponent<Tube>().BottomIndex()].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, tube.GetComponent<Tube>().BottomIndex());

                tube = null;
                clickState = false;
            }

        }
    } // main game function

    public void ClickedTinyTube(GameObject tube)
    {
        gameObject.GetComponent<LevelSolver>().tinyTubeFlash.gameObject.SetActive(false);

        if (!clickState && !tube.GetComponent<TinyTube>().EmptyTube()) // move ball to the top
        {
            clickState = true;
            firstTubeClicked = tube;

            tinyTubeTime = true;

            tube.GetComponent<TinyTube>().ballObjects[1].GetComponent<Ball>().MoveBall(tube.transform.GetSiblingIndex(), tube, 0);
            tube.GetComponent<TinyTube>().MoveBottomToTop();
        }
        else if (!tinyTubeTime && clickState && firstTubeClicked != tube) // coming from normal tube (firstTubeClicked is a normal tube)
        {
            int ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

            TinyTube second = tube.GetComponent<TinyTube>();

            Tube first = firstTubeClicked.GetComponent<Tube>();

            if (second.EmptyTube())
            {
                clickState = false;

                first.ballObjects[0].GetComponent<Ball>().MoveBall(-2, tube, 1);
                
                second.NewBallsToBottom(ball, firstTubeClicked.GetComponent<Tube>(), 0);

                firstTubeClicked = null;
                tinyTubeTime = false;
            }
            
            else if (!second.EmptyTube())// move ball from different tube to the top
            {
                if (!first.EmptyTube())
                {
                    first.ballObjects[0].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, firstTubeClicked.GetComponent<Tube>().BottomIndex() - 1);
                }
                else
                {
                    first.ballObjects[0].GetComponent<Ball>().MoveBall(firstTubeClicked.transform.GetSiblingIndex(), firstTubeClicked, 4);
                }
                firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();
                

                firstTubeClicked = tube;

                firstTubeClicked.GetComponent<TinyTube>().ballObjects[1].GetComponent<Ball>().MoveBall(-2, firstTubeClicked, 0);
                
                firstTubeClicked.GetComponent<TinyTube>().MoveBottomToTop();
                tinyTubeTime = true;
            }
            canUndo = true;
            SetUndoTubes();           
        }
        
        else if (clickState && firstTubeClicked == tube) // move ball back into the tube
        {
            tube.GetComponent<TinyTube>().ballObjects[0].GetComponent<Ball>().MoveBall(tinyTubeIndex, firstTubeClicked, 1);
            firstTubeClicked.GetComponent<TinyTube>().MoveTopToBottom();
            
            tube = null;
            clickState = false;
        }

    }

    public void Win()
    {
        win = true;
    }

    public void SetUndoTubes() // adds tubes to a list of boards that is used when the undo button is pressed
    {
        List<GameObject> newTubes = new List<GameObject>();

        for (int i = 0; i < tubes.Count; ++i)
        {
            GameObject test = Instantiate(fullTubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            test.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            test.transform.SetParent(moveHolder.transform);
            Tube tube = test.GetComponent<Tube>();

            for (int ii = 0; ii < 5; ++ii)
            {
                if (tubes[i].GetComponent<Tube>().spots[ii] != 0) // is one in actual and in new
                {
                    tube.SetSpot(tubes[i].GetComponent<Tube>().spots[ii], ii);
                }
                else if (tubes[i].GetComponent<Tube>().spots[ii] == 0 && test.transform.GetChild(ii).childCount != 0) // in not one in actual but in new
                {
                    Destroy(test.transform.GetChild(ii).GetChild(0).gameObject);
                    tube.SetSpot(0, ii);
                }
            }

            newTubes.Add(test);       
        }

        GameObject tt = Instantiate(TinyTubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        tt.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        tt.transform.SetParent(moveHolder.transform);

        TinyTube tinyTube = tt.GetComponent<TinyTube>();

        for (int ii = 0; ii < 2; ++ii)
        {
            if (TinyTube.GetComponent<TinyTube>().spots[ii] != 0) // is one in actual and in new
            {
                tinyTube.SetSpot(TinyTube.GetComponent<TinyTube>().spots[ii], ii);
            }
            else if (TinyTube.GetComponent<TinyTube>().spots[ii] != 0 && tt.transform.GetChild(ii).childCount != 0) // in not one in actual but in new
            {
                Destroy(tt.transform.GetChild(ii).GetChild(0).gameObject);
                tinyTube.SetSpot(0, ii);
            }
        }

        TTHolster.Add(tt);

        undoHolster.Add(newTubes);     
    }

    public void UNDO() // this is called when the undo button is pressed to undo the current board
    {
        int coins = gameObject.GetComponent<LevelCreator>().coins;

        if (canUndo && undoHolster.Count != 0 && undosLeft > 0)
        {
            undosLeft--;
            NoMovesLeftBox.Deactivate();

            GameObject undoTT = new GameObject();
            if (undoHolster.Count > 1)
            {
                undoTubes = undoHolster[undoHolster.Count - 2]; 
            }
            else
            {
                undoTubes = undoHolster[undoHolster.Count - 1];
            }

            if (TinyTube.activeSelf)
            {
                if (TTHolster.Count > 1)
                {
                    undoTT = TTHolster[TTHolster.Count - 2];
                }
                else
                {
                    undoTT = TTHolster[TTHolster.Count - 1];
                }
            }

            List<GameObject> testTubes = new List<GameObject>();

            for (int i = 0; i < undoTubes.Count; ++i)
            {
                GameObject test = Instantiate(undoTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));
                test.transform.position = tubes[i].transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                for (int ii = 0; ii < undoTubes[i].transform.childCount; ++ii)
                {
                    test.GetComponent<Tube>().SetSpot(undoTubes[i].GetComponent<Tube>().spots[ii], ii);
                }
                test.GetComponent<Tube>().ResetSelf();
                Debug.Log("set tube");


                testTubes.Add(test);
                Destroy(tubes[i]);
            }

            tubes.Clear();
            tubes = testTubes;
            undoHolster.RemoveAt(undoHolster.Count - 1);

            if (TinyTube.activeSelf)
            {
                GameObject test = Instantiate(undoTT, new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));


                test.transform.position = TinyTube.transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                for (int ii = 0; ii < undoTT.transform.childCount; ++ii)
                {
                    test.GetComponent<TinyTube>().SetSpot(undoTT.GetComponent<TinyTube>().spots[ii], ii);
                }
                test.GetComponent<TinyTube>().ResetSelf();

                Destroy(TinyTube);
                TinyTube = test;
                TTHolster.RemoveAt(TTHolster.Count - 1);
                
            }        

            if (undoHolster.Count == 0)
            { 
                canUndo = false;
            }
        }
        else if (canUndo && undoHolster.Count != 0 && coins >= undoCost && undosLeft == 0)
        {
            NoMovesLeftBox.Deactivate();
            coins -= undoCost;
            gameObject.GetComponent<LevelCreator>().coins = coins;

            GameObject undoTT = new GameObject();
            if (undoHolster.Count > 1) { undoTubes = undoHolster[undoHolster.Count - 2]; }
            else { undoTubes = undoHolster[undoHolster.Count - 2]; }
            if (TinyTube.activeSelf)
            {
                if (TTHolster.Count > 1) { undoTT = TTHolster[TTHolster.Count - 2]; }
                else { undoTT = TTHolster[TTHolster.Count - 1]; }
            }

            List<GameObject> testTubes = new List<GameObject>();

            for (int i = 0; i < undoTubes.Count; ++i)
            {
                GameObject test = Instantiate(undoTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));


                test.transform.position = tubes[i].transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                for (int ii = 0; ii < undoTubes[i].transform.childCount; ++ii)
                {
                    test.GetComponent<Tube>().SetSpot(undoTubes[i].GetComponent<Tube>().spots[ii], ii);
                }
                test.GetComponent<Tube>().ResetSelf();

                testTubes.Add(test);
                Destroy(tubes[i]);

            }

            tubes.Clear();
            tubes = testTubes;
            

            undoHolster.RemoveAt(undoHolster.Count - 1);
            if (TinyTube.activeSelf)
            {
                GameObject test = Instantiate(undoTT, new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));


                test.transform.position = TinyTube.transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                for (int ii = 0; ii < undoTT.transform.childCount; ++ii)
                {
                    test.GetComponent<TinyTube>().SetSpot(undoTT.GetComponent<TinyTube>().spots[ii], ii);
                }
                test.GetComponent<TinyTube>().ResetSelf();

                Destroy(TinyTube);
                TinyTube = test;
                TTHolster.RemoveAt(TTHolster.Count - 1);

            }

            if (undoHolster.Count == 0) { canUndo = false; }
        }

        undosLeftText.text = undosLeft.ToString() + " Free";
        undosLeftCoin.SetActive(false);
        if (undosLeft == 0)
        {
            undosLeftCoin.SetActive(true);
            undosLeftText.text = "     " + undoCost.ToString();
        }
        
        
    }

    public void TinyTubeTime() // this is called when the tinytube button is pressed
    {
        int coins = gameObject.GetComponent<LevelCreator>().coins;

        if (!tinyTubeActive && coins >= TinyTubeCost)
        {
            TinyTubeButton.gameObject.SetActive(false);

            TinyTube.SetActive(true);
            coins -= TinyTubeCost;
            gameObject.GetComponent<LevelCreator>().coins = coins;
            tinyTubeActive = true;
        }
    }
    
    public bool CheckIfAnyMovesLeft() // checks if the current state has any more moves that the player can make
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            for (int ii = 0; ii < tubes.Count; ++ii)
            {
                if (ii != i)
                {
                    
                    Tube tube1 = tubes[i].GetComponent<Tube>();
                    Tube tube2 = tubes[ii].GetComponent<Tube>();
                    int ball1 = InvalidIndex;
                    int ball2 = InvalidIndex;

                    if (!tube1.EmptyTube() && !tube2.EmptyTube())
                    {
                        ball1 = tubes[i].GetComponent<Tube>().GetBottomBall();
                        ball2 = tubes[ii].GetComponent<Tube>().GetBottomBall();
                    }
                    else
                    {
                        return true;
                    }

                    if (tube1.spots[0] != 0 || tube2.spots[0] != 0)
                    {
                        return true;
                    }

                    if (ball1 == ball2)
                    {
                        if (tube1.NumSameAtTop() <= tube2.ReturnNumOpenSpots())
                        {
                            return true;
                        }                     
                    }       
                }
            }

            if (TinyTube.activeSelf)
            {
                Tube tube = tubes[i].GetComponent<Tube>();
                TinyTube tinyTube = TinyTube.GetComponent<TinyTube>();

                int ball1 = InvalidIndex;
                int ball2 = InvalidIndex;

                if (!tube.EmptyTube() && !tinyTube.EmptyTube())
                {
                    ball1 = tube.GetBottomBall();
                    ball2 = tinyTube.GetBottomBall();
                }
                else
                {
                    return true;
                }

                if (ball1 == ball2)
                {
                    if (1 <= tube.ReturnNumOpenSpots())
                    {         
                        return true;
                    }
                }
            }
        }
        Debug.Log("NO MOVES LEFT");
        return false;
    }

    public void ModeChange() // this swaps the mode from hard to easy
    {
        if (undoButton.activeSelf)
        {
            undoButton.SetActive(false);
            TinyTubeButton.gameObject.SetActive(false);
            hintButton.SetActive(false);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode: Normal";
            modeButton.GetComponent<Image>().color = blank.color;
        }
        else if (!undoButton.activeSelf)
        {
            undoButton.SetActive(true);
            TinyTubeButton.gameObject.SetActive(true);
            hintButton.SetActive(true);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode:   Easy";
            modeButton.GetComponent<Image>().color = badRed.color;
        }
    }

    public void OpenSettingsScreen() // this opens the settings screen
    {
        if (SettingsScreen.activeSelf) { SettingsScreen.SetActive(false); }
        else if (!SettingsScreen.activeSelf) { SettingsScreen.SetActive(true); }
    }

    public void BuyBackground(int index) // this is called when a background is tapped on in the settings menu and remembers that the player bought the background
    {
        int coins = gameObject.GetComponent<LevelCreator>().coins;
        if (coins >= BackgroundCost)
        {
            if (index == 0) { greenLockButton.gameObject.SetActive(false);
                PlayerPrefs.SetInt("greenLock", 1);
            }
            else if (index == 1) { redLockButton.gameObject.SetActive(false);
                PlayerPrefs.SetInt("redLock", 1);
            }
            else if (index == 2) { blueLockButton.gameObject.SetActive(false);
                PlayerPrefs.SetInt("blueLock", 1);
            }
            coins -= BackgroundCost;
            gameObject.GetComponent<LevelCreator>().coins = coins;
        }
    }

    public void SetBackground(int index) // this is called when the player wants to change to a different background
    {
        if (index == 0)
        {
            levelBackground.sprite = defaultSetting;
            gameBackground.sprite = defaultSetting;
            settingsBackground.sprite = defaultSetting;

        }
        else if (index == 1)
        {
            levelBackground.sprite = greenSetting;
            gameBackground.sprite = greenSetting;
            settingsBackground.sprite = greenSetting;
            greenLockButton.gameObject.SetActive(false);
        }
        else if (index == 2)
        {
            levelBackground.sprite = redSetting;
            gameBackground.sprite = redSetting;
            settingsBackground.sprite = redSetting;
            redLockButton.gameObject.SetActive(false);
        }
        else if (index == 3)
        {
            levelBackground.sprite = blueSetting;
            gameBackground.sprite = blueSetting;
            settingsBackground.sprite = blueSetting;
            blueLockButton.gameObject.SetActive(false);
        }
        PlayerPrefs.SetInt("lastBackground", index);
    }

    public void LoadBackgrounds() // this loads all the backgrounds that the player has bought
    {
        if (PlayerPrefs.HasKey("lastBackground"))
        {
            SetBackground(PlayerPrefs.GetInt("lastBackground"));
        }
        else
        {
            PlayerPrefs.SetInt("lastBackground", 0);
        }

        if (PlayerPrefs.HasKey("redLock"))
        { 
            if (PlayerPrefs.GetInt("redLock") == 1) 
            { 
                redLockButton.gameObject.SetActive(false);
            }
        }
        if (PlayerPrefs.HasKey("greenLock")) 
        { 
            if (PlayerPrefs.GetInt("greenLock") == 1)
            {
                greenLockButton.gameObject.SetActive(false); 
            }
        }
        if (PlayerPrefs.HasKey("blueLock"))
        {
            if (PlayerPrefs.GetInt("blueLock") == 1) 
            {
                blueLockButton.gameObject.SetActive(false); 
            }
        }

        greenLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        redLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        blueLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        
    }


    public void ShowSolveText() // debug for showing a solution
    {
        if (solveText.activeSelf)
        {
            solveText.SetActive(false);
        }
        else { solveText.SetActive(true); }
    }

    public void ShowHowToPlay() // this shows the tutorial screen
    {
        if (!HowToScreen.activeSelf)
        {
            HowToScreen.SetActive(true);
        }
        else
        {
            HowToScreen.SetActive(false);
        }
    }

    public void PlayTutorial() // this happens when user user clicks on the tutorial button on the tutorial screen and then loads the tutorial for them to play
    {
        PlayerPrefs.SetInt("Tutorial", 0);

        SceneManager.LoadScene(1);
    }*/
}
