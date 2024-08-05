using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using Unity.Burst;
#if ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;
#endif

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private bool clickState; // true means there is a ball waiting to be moved, false is all balls are in a tube
    private GameObject firstTubeClicked = null;

    [Header("---GamePlay---")]
    private List<GameObject> tubeObjects;
    [SerializeField] private GameObject tubePrefab;
    [SerializeField] private Transform tubeContainer;
    public int coins;
    [SerializeField] private TMP_Text levelNumberText;
    [SerializeField] private TubeContainer tubeContainerObj;
    [SerializeField] private Popup noMovesLeftPopup;

    private bool inChallenge;

    private string nextUse = "";
    [Header("---Confirm Screen---")]
    [SerializeField] private GameObject confirmScreen;
    [SerializeField] private string confirmUndoInfo;
    [SerializeField] private string confirmTinyTubeInfo;
    [SerializeField] private string confirmHintInfo;
    [SerializeField] private TMP_Text confirmInfoText;
    [SerializeField] private TMP_Text confirmCostText;

    public static Action<int, bool> OnBeatLevel = delegate { };
    public static Action OnLoadLevel = delegate { };

    [Header("---UNDO---")]
    [SerializeField] private int freeGivenUndos;
    [SerializeField] private int undoCost;
    private List<string> undoHolster = new List<string>();
    private List<string> tinyTubeUndoHolster = new List<string>();
    private int undosLeft;
    private bool canUndo;
    [SerializeField] private GameObject undoButton;

    [Header("--Challenge---")]
    private bool isInChallenge;

    [Header("--TinyTube---")]
    [SerializeField] private GameObject tinyTube;
    [SerializeField] private int tinyTubeCost;
    private bool isTinyTubeActive;
    [SerializeField] private GameObject tinyTubeButton;

    [Header("---Hint---")]
    [SerializeField] private GameObject hintButton;
    [SerializeField] private LevelSolver levelSolver;
    [SerializeField] private int hintCost;
    private Queue<Move> hintMoveQueue;
    private int hintMovesGiven = 3;
    [SerializeField] private GameObject hintFlashObj;
    [SerializeField] private HintFlash hintFlash;
    private bool isHintActive;
    private bool isTinyTubeHint;
    private Move currentHint;
    [SerializeField] private Popup noSolutionPopup;
    [SerializeField] private HintFlash resetFlash;
    [SerializeField] private HintFlash tinyTubeFlash;
    [SerializeField] private GameObject resetButtonObj;

    [SerializeField] private Color[] ballColors;

    [Header("---Tutorial---")]
    [SerializeField] private List<GameObject> tutorialTubes;
    [SerializeField] private bool isTutorial;
    [SerializeField] private Transform tutorialTubeContainer;
    private string tutorialString = "0,0,1,1:1,1,0,0-";

    private int lastLevelLoaded = -1;
    private int lastLoadedTubeCount = -1;

    private bool currentGameMode = false;

    


    private void Awake()
    {
        tubeObjects = new List<GameObject>();
        hintMoveQueue = new Queue<Move>();
        coins = PlayerPrefs.GetInt("CoinCount");

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        MenuManager.OnChangeMode += HandleModeChange;
        MenuManager.OnGoToLevelsMenu += HandleLevelsMenu;
        MenuManager.OnStartTutorial += HandleStartTutorial;
        MenuManager.OnStartGame += HandleStartGame;
    }

    private void OnDestroy()
    {
        MenuManager.OnChangeMode -= HandleModeChange;
        MenuManager.OnGoToLevelsMenu -= HandleLevelsMenu;
        MenuManager.OnStartTutorial -= HandleStartTutorial;
        MenuManager.OnStartGame -= HandleStartGame;
    }

    public int Coin
    {
        get { return coins; }
        private set { }
    }

    public void RemoveCoins(int amount)
    {
        coins -= amount;
    }

    #region OnClickEvents
    public void OnClickTube(GameObject tubeObject)
    {
        MoveBalls(tubeObject);
    }

    public void OnClickLoadLevel(int levelNumber)
    {
        OnLoadLevel?.Invoke();
        //Debug.Log("load level : " + levelNumber);
        lastLevelLoaded = levelNumber;
        levelNumberText.text = "Level " + (levelNumber + 1).ToString();

#if ENABLE_CLOUD_SERVICES_ANALYTICS
        Debug.Log("levelStart");
        Analytics.CustomEvent("levelStart");
#endif


        LoadLevel(GameManager.instance.GetLevel(levelNumber));
    }

    public void OnClickResetGame()
    {
        ResetGame();
        LoadLevel(GameManager.instance.GetLevel(lastLevelLoaded));
    }

    public void OnClickUndo()
    {
        if (canUndo)
        { 
            if (undosLeft > 0)
            {
                UndoLastMove();
                undosLeft--;
            }
            else if (coins >= undoCost)
            {
                nextUse = "undo";
                confirmScreen.SetActive(true);
                confirmInfoText.text = confirmUndoInfo;
                confirmCostText.text = undoCost.ToString();
            }
        }
    }

    public void OnClickTinyTube()
    {
        if (coins >= tinyTubeCost)
        {
            nextUse = "tinyTube";
            confirmScreen.SetActive(true);
            confirmInfoText.text = confirmTinyTubeInfo;
            confirmCostText.text = tinyTubeCost.ToString();
        }
    }

    public void OnConfirmNextFuction()
    {
        if (nextUse.Equals("undo"))
        {
            ConfirmUndo();
        }
        else if (nextUse.Equals("tinyTube"))
        {
            ConfirmTinyTube();
        }
        else if (nextUse.Equals("hint"))
        {
            ConfirmHint();
        }

        confirmScreen.SetActive(false);
    }

    public void OnClickHint()
    {
        if (coins >= hintCost)
        {
            nextUse = "hint";
            confirmScreen.SetActive(true);
            confirmCostText.text = hintCost.ToString();
            confirmInfoText.text = confirmHintInfo;
        }
    }

    public void OnCancelConfirm()
    {
        confirmScreen.SetActive(false);
        nextUse = "";
    }

    private void ConfirmTinyTube()
    {
        tinyTubeButton.SetActive(false);
        tinyTube.SetActive(true);
        coins -= tinyTubeCost;
        isTinyTubeActive = true;
    }

    private void ConfirmUndo()
    {
        UndoLastMove();
        coins -= undoCost;
    }

    private void ConfirmHint()
    {
        List<Move> movesToSolve = levelSolver.SolveFromCurrent(tubeObjects);
        coins -= hintCost;

        hintMoveQueue.Clear();
        for (int i = 0; i < hintMovesGiven; i++)
        {
            if (i < movesToSolve.Count)
            {
                hintMoveQueue.Enqueue(movesToSolve[i]);
            }
        }

        if (hintMoveQueue.Count > 0)
        {
            Move move = hintMoveQueue.Dequeue();
            currentHint = move;
            hintFlash.Activate(tubeObjects[move.x]);
            isHintActive = true;
        }
        else
        {
            if (isTinyTubeActive && tinyTube.GetComponent<Tube>().EmptyTube())
            {
                tinyTubeFlash.Activate(tinyTube);
                isHintActive = true;
                isTinyTubeHint = true;
            }
            else
            {
                noSolutionPopup.Activate(3);
                resetFlash.Activate(resetFlash.gameObject);
            }
        }
    }
    #endregion

    #region Actions

    private void HandleModeChange(bool mode)
    {
        currentGameMode = mode;
        undoButton.SetActive(true);
        tinyTubeButton.SetActive(true);
        if (!mode)
        {
            tinyTube.SetActive(false);
        }
        else if (mode)
        {
            if (isTinyTubeActive) { tinyTubeButton.SetActive(false); }
            else { tinyTubeButton.SetActive(true); }
            //hintButton.SetActive(mode);
            
        }
    }

    private void HandleLevelsMenu()
    {
        isTinyTubeActive = false;
        tinyTube.SetActive(false);
        HandleModeChange(currentGameMode);
    }

    public void HandleBallColorChange(Color[] newColor)
    {
        ballColors = newColor;
        
        for (int i = 0; i < tubeObjects.Count; ++i)
        {
            Tube tube = tubeObjects[i].GetComponent<Tube>();

            tube.SetNewPallette(newColor);
        }
    }

    #endregion

    #region Gameplay
    private bool CheckForWin()
    {
        //HandleMovesLeft();
        CorkTubes();

        if (isTutorial) return false;

        for (int tube = 0; tube < tubeObjects.Count; tube++)
        {
            Tube currentTube = tubeObjects[tube].GetComponent<Tube>();

            if (!currentTube.corked && !currentTube.EmptyTube())  
            {
                return false;
            }
        }

        return true;
    }

    private void HandleMovesLeft()
    {
        if (!CheckIfAnyMovesLeft())
        {
            noMovesLeftPopup.Activate(4);
        }
    }

    private void CorkTubes()
    {
        for (int tube = 0; tube < tubeObjects.Count; tube++)
        {
            Tube currentTube = tubeObjects[tube].GetComponent<Tube>();

            if (!currentTube.corked && currentTube.FullTube())
            {
                currentTube.Cork();
            }
            else if (currentTube.corked && !currentTube.FullTube())
            {
                currentTube.UnCork();
            }
        }
    }

    private void MoveBalls(GameObject tubeObject)
    {
        Tube currentTube = tubeObject.GetComponent<Tube>();

        if (!currentTube.corked)
        {
            if (clickState) // move balls from firstTubeClicked to tubeObject
            {
                if (firstTubeClicked == tubeObject)
                {
                    currentTube.MoveTopToBottom();
                    clickState = false;

                    if (isHintActive && !isTinyTubeHint)
                    {
                        hintFlash.Activate(tubeObjects[currentHint.x]);
                    }
                }
                else
                {
                    Tube firstTube = firstTubeClicked.GetComponent<Tube>(); // move balls into tubeObject from firstTubeClicked

                    if ((firstTube.NumberMoving() <= currentTube.NumOpenSpots() && (firstTube.GetTopBall() == currentTube.GetBottomBall() || currentTube.EmptyTube())) || (currentTube.isTinyTube && currentTube.EmptyTube()))
                    {
                        int ballCount = firstTube.NumberMoving() - 1;

                        currentTube.NewBallToBottom(firstTube.GetTopBall(), firstTube.ballObjects[0], firstTube);
                        firstTube.RemoveTopBall();

                        if (!currentTube.isTinyTube)
                        {
                            for (int i = 0; i < ballCount; ++i)
                            {
                                currentTube.NewBallToBottom(firstTube.GetBottomBall(), firstTube.ballObjects[firstTube.BottomIndex()], firstTube);
                                firstTube.RemoveBottomBall();
                            }
                        }

                        if (isHintActive)
                        {
                            if (isTinyTubeHint)
                            {
                                tinyTubeFlash.Deactivate();
                                isHintActive = false;
                                isTinyTubeHint = false;
                            }
                            else if (tubeObjects[currentHint.y] == tubeObject && firstTubeClicked == tubeObjects[currentHint.x])
                            {
                                if (hintMoveQueue.Count > 0)
                                {
                                    currentHint = hintMoveQueue.Dequeue();
                                    hintFlash.Activate(tubeObjects[currentHint.x]);
                                }
                                else
                                {
                                    hintFlash.Deactivate();
                                    isHintActive = false;
                                }
                            }
                            else
                            {
                                hintFlash.Deactivate();
                                isHintActive = false;
                            }
                        }

                        firstTubeClicked = null;
                        clickState = false;

                        SetUndoTubes();
                        HandleMovesLeft();

                    }
                    else if (!currentTube.EmptyTube())
                    {
                        currentTube.MoveBottomToTop();
                        firstTube.MoveTopToBottom();
                        firstTubeClicked = tubeObject;

                        if (isHintActive && !isTinyTubeHint)
                        {
                            if (tubeObjects[currentHint.x] == tubeObject)
                            {
                                hintFlash.Activate(tubeObjects[currentHint.y]);
                            }
                            else
                            {
                                hintFlash.Activate(tubeObjects[currentHint.x]);
                            }
                        }
                    }
                }
            }
            else if (!clickState && !currentTube.EmptyTube()) // move ball from tubeObject to top
            {
                firstTubeClicked = tubeObject;

                currentTube.MoveBottomToTop();
                clickState = true;

                if (isHintActive && !isTinyTubeHint)
                {
                    if (tubeObjects[currentHint.x] == tubeObject)
                    {
                        hintFlash.Activate(tubeObjects[currentHint.y]);
                    }
                }
            }
        }

        if (CheckForWin())
        {
            BeatLevel(lastLevelLoaded, inChallenge);
        }
    }

    public bool CheckIfAnyMovesLeft()
    {
        for (int tube = 0; tube < tubeObjects.Count; tube++)
        {
            for (int otherTube = 0; otherTube < tubeObjects.Count; otherTube++)
            {
                if (tube != otherTube)
                {
                    Tube TUBE = tubeObjects[tube].GetComponent<Tube>();
                    Tube OTHER_TUBE = tubeObjects[otherTube].GetComponent<Tube>();

                    if (TUBE.EmptyTube() || OTHER_TUBE.EmptyTube()) { return true; }

                    int ball1 = TUBE.GetBottomBall();
                    int ball2 = OTHER_TUBE.GetBottomBall();

                    if (ball1 == ball2)
                    {
                        if (TUBE.NumSameAtTop() <= OTHER_TUBE.ReturnNumOpenSpots())
                        {
                            return true;
                        }
                    }
                }
            }

            if (isTinyTubeActive) // assuming the tiny tube only has 1 spot to move into
            {
                Tube TUBE = tubeObjects[tube].GetComponent<Tube>();
                Tube OTHER_TUBE = tinyTube.GetComponent<Tube>();

                if (TUBE.EmptyTube() || OTHER_TUBE.EmptyTube()) { return true; }

                int ball1 = TUBE.GetBottomBall();
                int ball2 = OTHER_TUBE.GetBottomBall();

                if (ball1 == ball2)
                {
                    if (OTHER_TUBE.NumSameAtTop() <= TUBE.ReturnNumOpenSpots())
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void UndoLastMove()
    {
        int index = undoHolster.Count  > 1 ? undoHolster.Count - 2 : undoHolster.Count - 1;

        string lastState = undoHolster[index];
        string lastTinyTubeState = tinyTubeUndoHolster[index];
        List<List<int>> level = GetStateFromString(lastState);
        List<List<int>> tinyTubeState = GetStateFromString(lastTinyTubeState);

        noMovesLeftPopup.Deactivate();

        for (int tube = 0; tube < level.Count; tube++)
        {
            Tube currentTube = tubeObjects[tube].GetComponent<Tube>();
            for (int ball = 0; ball < level[tube].Count; ball++)
            {
                if (level[tube][ball] == 0)
                {
                    currentTube.RemoveBall(ball);
                }
                else
                {
                    currentTube.AddBall(ball, ballColors[level[tube][ball] - 1], level[tube][ball]);
                }
            }
        }

        for (int tube = 0; tube < tinyTubeState.Count; tube++)
        {
            Tube currentTube = tinyTube.GetComponent<Tube>();
            for (int ball = 0; ball < tinyTubeState[tube].Count; ball++)
            {
                if (tinyTubeState[tube][ball] == 0)
                {
                    currentTube.RemoveBall(ball);
                }
                else
                {
                    currentTube.AddBall(ball, ballColors[tinyTubeState[tube][ball] - 1], tinyTubeState[tube][ball]);
                }
            }
        }

        undoHolster.RemoveAt(undoHolster.Count - 1);
        tinyTubeUndoHolster.RemoveAt(tinyTubeUndoHolster.Count - 1);

        if (undoHolster.Count <= 1)
        {
            canUndo = false;
        }

        Debug.Log("Count : " + undoHolster.Count);
        CorkTubes();
    }
    #endregion

    #region GameLoad
    private void ResetGame()
    {
        undosLeft = freeGivenUndos;
        noMovesLeftPopup.Deactivate();
        noSolutionPopup.Deactivate();
        tinyTubeFlash.Deactivate();
        resetFlash.Deactivate();
        HandleModeChange(currentGameMode);

        LoadBlankLevel();
    }

    private void LoadBlankLevel()
    {
        for (int i = 0; i < tubeObjects.Count; ++i)
        {
            Destroy(tubeObjects[i]);
        }

        tubeObjects.Clear();

        for (int i = 0; i < lastLoadedTubeCount; i++)
        {
            GameObject newTube = Instantiate(tubePrefab, new Vector3(0, 0, 0), Quaternion.identity);

            newTube.transform.SetParent(!isTutorial ? tubeContainer : tutorialTubeContainer);
            newTube.transform.localScale = Vector3.one;

            newTube.GetComponent<Tube>().siblingIndex = i;
            tubeObjects.Add(newTube);
        }

        tubeObjects[tubeObjects.Count - 1].GetComponent<Tube>().EmptyEntireTube();
        if (!isTutorial)
        {
            tubeObjects[tubeObjects.Count - 2].GetComponent<Tube>().EmptyEntireTube();
        }

        tinyTube.GetComponent<Tube>().EmptyEntireTube();

        canUndo = false;
        clickState = false;

        int row = lastLoadedTubeCount / 8;
        row++;

        tubeContainerObj.SetGrid(row);

        EmptyMoveHolders();
    }

    private void EmptyMoveHolders()
    {
        undoHolster.Clear();
        tinyTubeUndoHolster.Clear();
    }

    private void LoadLevel(List<List<int>> level)
    {
        undoHolster.Clear();
        tinyTubeUndoHolster.Clear();

        lastLoadedTubeCount = level.Count + 2;

        if (!isTutorial)
        {
            ResetGame();
        }

        if (level != null)
        {
            for (int tube = 0; tube < level.Count; ++tube)
            {
                Tube currentTube = tubeObjects[tube].GetComponent<Tube>();
                for (int position = 0; position < level[tube].Count; ++position)
                {
                    if (level[tube][position] != -1)
                    {
                        currentTube.SetBall(position + 1, ballColors[level[tube][position]], level[tube][position]);
                        if (!currentTube.corked)
                        {
                            if (currentTube.FullTube())
                            {
                                currentTube.Cork();
                            }
                        }
                    }
                    else
                    {
                        currentTube.RemoveBall(position);
                    }
                }
            }
        }

        SetUndoTubes();
    }

    private void SetUndoTubes()
    {
        string currentState = "";

        for (int tube = 0; tube < tubeObjects.Count; tube++)
        {
            Tube currentTube = tubeObjects[tube].GetComponent<Tube>();


            currentState += AddTubeToString(currentTube);

            if (tube != tubeObjects.Count - 1) { currentState += ":"; }
        }

        string tinyTubeState = AddTubeToString(tinyTube.GetComponent<Tube>()) + "-";
        //Debug.Log(tinyTubeState);

        currentState += "-";

        undoHolster.Add(currentState);
        tinyTubeUndoHolster.Add(tinyTubeState);

        //Debug.Log(currentState);
        canUndo = true;
    }

    private string AddTubeToString(Tube currentTube)
    {
        string tube = "";
        for (int ball = 0; ball < currentTube.spots.Count; ball++)
        {
            string nextBall = currentTube.spots[ball].ToString();

            if (ball == currentTube.spots.Count - 1) { tube += nextBall; }
            else {  tube += nextBall + ","; }
        }

        return tube;
    }

    private List<List<int>> GetStateFromString(string str)
    {
        string ball = "";

        int loadBall = 0;
        List<int> loadTube = new List<int>();
        List<List<int>> loadLevel = new List<List<int>>();

        string level = "";

        // 1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12:1,2,3,4:5,6,7,8:9,10,11,12-    one level

        for (int index = 0; index < str.Length; index++)
        {
            level += str[index];

            if (str[index] != '-')
            {
                if (str[index] != ':')
                {
                    if (str[index] != ',') // add to ball
                    {
                        ball = ball + str[index];
                    }
                    else if (str[index] == ',') // finish ball
                    {

                        loadBall = Convert.ToInt32(ball);
                        ball = "";
                        int newBall = loadBall;

                        loadTube.Add(newBall);

                    }


                }
                else if (str[index] == ':') // finish tube
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
            else if (str[index] == '-')
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

                return loadLevel;
            }
        }

        return null;
    }
    #endregion

    #region Win

    private void BeatLevel(int levelIndex, bool isChallenge)
    {
        if (!isTutorial)
        {
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            Analytics.CustomEvent("beatLevel");
#endif
            OnBeatLevel?.Invoke(levelIndex, isChallenge);
        }
    }

    public void AddCoins(int add)
    {
        coins += add;
    }

    #endregion

    #region Tutorial

    private void LoadTutorial()
    {
        lastLoadedTubeCount = 3;
        LoadBlankLevel();
        LoadLevel(GetStateFromString(tutorialString));
        tubeObjects[2].GetComponent<Tube>().EmptyEntireTube();
    }

    private void HandleStartTutorial()
    {
        isTutorial = true;
        LoadTutorial();
    }

    private void HandleStartGame()
    {
        isTutorial = false;
    }

    #endregion
}
