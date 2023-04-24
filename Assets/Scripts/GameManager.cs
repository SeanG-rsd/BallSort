using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{ 
    public List<GameObject> tubes;


    public bool clickState;
    public GameObject firstTubeClicked;

    Material currentMat;

    public List<GameObject> screens;
    public GameObject gameScreen;

    public int menuNum = 0;

    public List<GameObject> resetTubes = new List<GameObject>();
    
    public GameObject fullTubePrefab;

    public GameObject undoButton;
    public GameObject modeButton;
    public TMP_Text modeButtonText;
    public List<GameObject> undoTubes = new List<GameObject>();
    private int firstUndoTube;
    public GameObject moveHolder;
    public bool canUndo;
    public List<List<GameObject>> undoHolster = new List<List<GameObject>>();
    private int undosLeft;
    public int givenUndos;
    public int undoCost;
    public TMP_Text undosLeftText;

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

    public GameObject NoMovesLeftBox;
    public float noMoveTime;
    float noMoveTimer;
    bool noMoves;

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


    // Start is called before the first frame update
    void Start()
    {

        insult.SetActive(false);
        NoMovesLeftBox.SetActive(false);
        SettingsScreen.SetActive(false);
        insultTimer = insultTime;
        noMoveTimer = noMoveTime;
        winTimer = winTime;
        //ModeChange();
        OpenMenuNum(1);
        

        ResetGame();
        LoadBackgrounds();
    }

    // Update is called once per frame
    void Update()
    {
        if (win)
        {
            if (winTimer < 0)
            {
                win = false;
                winTimer = winTime;
                gameObject.GetComponent<LevelCreator>().WinScreen();
            }

            winTimer -= Time.deltaTime;
        }

        if (insult.activeSelf)
        {
            if (doInsult)
            {
                insultText.text = insults[Random.Range(0, insults.Count)];
                doInsult = false;
            }
            insultTimer -= Time.deltaTime;

            if (insultTimer < 0)
            {
                insult.SetActive(false);
                insultTimer = insultTime;
            }
        }

        if (noMoves)
        {
            if (noMoveTimer < 0)
            {
                noMoves = false;
                NoMovesLeftBox.SetActive(false);
                noMoveTimer = noMoveTime;
            }

            noMoveTimer -= Time.deltaTime;
        }
        
        

        for (int i = 0; i < screens.Count; ++i)
        {
            if (i == menuNum && !screens[i].activeSelf)
            {
                OpenMenuNum(i);
            }
        }
    }

    public void ResetGame()
    {
        //Debug.Log("reset game");
        undosLeft = givenUndos;
        undosLeftText.text = "Free Undos Left: " + undosLeft.ToString();
        if (undosLeft == 0) { undosLeftText.text = "No Undos Left: " + undoCost.ToString() + " coins"; }
        TTTExt.text = "Buy Tiny Tube for " + TinyTubeCost.ToString() + " coins";

        noMoves = false;
        NoMovesLeftBox.SetActive(false);
        noMoveTimer = noMoveTime;


        List<GameObject> testTubes = new List<GameObject>();

        for (int i = 0; i < resetTubes.Count; ++i)
        {
            GameObject test = Instantiate(resetTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

            test.transform.SetParent(gameScreen.transform.GetChild(0));


            test.transform.position = tubes[i].transform.position;
            test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

            

            testTubes.Add(test);
            Destroy(tubes[i]);
            

            clickState = false;
            
        }

        tubes.Clear();
        tubes = testTubes;

        //menuNum = 2;
        if (tinyTubeActive)
        {
            tinyTubeActive = false;
            TinyTube.SetActive(false);
            TinyTubeButton.interactable = true;

            if (TinyTube.GetComponent<TinyTube>().FullTube()) { Destroy(TinyTube.GetComponent<TinyTube>().GetBottomBall()); }
        }
        
        //menuNum = 1;
    }

    public void OpenMenuNum(int index)
    {
        for (int i = 0; i < screens.Count; ++i)
        {
            screens[i].SetActive(false);
        }

        screens[index].SetActive(true);
    }

    public void OpenLevelScreen()
    {
        //Debug.Log("openlevelscreen");
        menuNum = 1;
        OpenMenuNum(menuNum);
        ResetGame();
        canUndo = false;
    }

    public void ResetCurrent()
    {
        ResetGame();
        gameObject.GetComponent<LevelCreator>().LoadLastLevel();
        SetUndoTubes();
        canUndo = false;

        for (int i = 0; i < moveHolder.transform.childCount; ++i)
        {
            Destroy(moveHolder.transform.GetChild(i).gameObject);
            
        }
        undoHolster.Clear();
        TTHolster.Clear();
    }

    public void ResetChallenge()
    {
        ResetGame();
        gameObject.GetComponent<LevelCreator>().LoadLastChallengeLevel();
    }

    bool CheckForWin() // check to see if all tubes are the right color
    {
        if (!CheckIfAnyMovesLeft())
        {
            noMoves = true;
            NoMovesLeftBox.SetActive(true);
            noMoveTimer = noMoveTime;
        }
        for (int i = 0; i < tubes.Count; ++i)
        {
            
            if (!tubes[i].GetComponent<Tube>().FullTube() && !tubes[i].GetComponent<Tube>().EmptyTube()) { return false; } 
        }

        return true;
    }

    public void Cork()
    {
        //Debug.Log("cork");

        for (int i = 0; i < tubes.Count; ++i)
        {
            if (!tubes[i].GetComponent<Tube>().corked) { if (tubes[i].GetComponent<Tube>().FullTube() && !tubes[i].GetComponent<Tube>().EmptyTube()) { tubes[i].GetComponent<Tube>().Cork(); } }
            
            
        }
    }

    public void Clicked(GameObject tube)
    {
        if (!tube.GetComponent<Tube>().corked)
        {
            if (!clickState && !tube.GetComponent<Tube>().FullTube() && !tube.GetComponent<Tube>().EmptyTube()) // move ball to the top
            {
                SetUndoTubes();
                clickState = true;
                firstTubeClicked = tube;
                //Debug.Log("tubeFirstClicked");

                firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
            }
            else if (!tinyTubeTime && clickState && firstTubeClicked != tube) // move balls into empty tube
            {
                GameObject ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

                Tube second = tube.GetComponent<Tube>();
                //Debug.Log(ball.GetComponent<Image>().color);

                if (second.EmptyTube())
                {

                    clickState = false;

                    //Debug.Log("tubeSecondClick");
                    second.NewBallsToBottom(ball);



                    for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {

                        if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != null)
                        {
                            if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                            {
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                            }
                        }
                    }



                    firstTubeClicked = null;
                    canUndo = true;
                    Cork();
                    if (CheckForWin()) { Win(); }
                }
                else if (ball.tag == second.GetBottomBall().tag && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
                {
                    clickState = false;



                    second.NewBallsToBottom(ball);

                    if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball)) // fix this for only going to the last ball to get rid of error
                    {
                        second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                        if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
                        {
                            second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                            if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
                            {
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                            }
                        }
                    }

                    firstTubeClicked = null;
                    canUndo = true;
                    Cork();
                    if (CheckForWin()) { Win(); }
                }
                else if (!second.FullTube())// move ball from different tube to the top
                {
                    firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();

                    //Debug.Log("move from dif");

                    firstTubeClicked = tube;



                    firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
                }

                

            }
            else if (tinyTubeTime && clickState && firstTubeClicked != tube) // move balls into empty tube
            {
                GameObject ball = firstTubeClicked.GetComponent<TinyTube>().GetTopBall();

                Tube second = tube.GetComponent<Tube>();
                //Debug.Log(ball.GetComponent<Image>().color);

                if (second.EmptyTube())
                {

                    clickState = false;

                    //Debug.Log("tubeSecondClick");
                    second.NewBallsToBottom(ball);



                    for (int i = 0; i <= firstTubeClicked.GetComponent<TinyTube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {

                        if (firstTubeClicked.GetComponent<TinyTube>().ReturnNext() != null)
                        {
                            if (firstTubeClicked.GetComponent<TinyTube>().CheckTwo(firstTubeClicked.GetComponent<TinyTube>().ReturnNext(), ball))
                            {
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<TinyTube>().ReturnNext());
                            }
                        }
                    }



                    firstTubeClicked = null;
                    canUndo = true;
                    Cork();
                    if (CheckForWin()) { Win(); }
                }

                else if (ball.tag == second.GetBottomBall().tag && !second.FullTube() && firstTubeClicked.GetComponent<TinyTube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
                {
                    clickState = false;



                    second.NewBallsToBottom(ball);

                    if (firstTubeClicked.GetComponent<TinyTube>().CheckIfNextIsSameColor(ball)) // fix this for only going to the last ball to get rid of error
                    {
                        second.NewBallsToBottom(firstTubeClicked.GetComponent<TinyTube>().ReturnNext());
                        if (firstTubeClicked.GetComponent<TinyTube>().CheckIfNextIsSameColor(ball))
                        {
                            second.NewBallsToBottom(firstTubeClicked.GetComponent<TinyTube>().ReturnNext());
                            if (firstTubeClicked.GetComponent<TinyTube>().CheckIfNextIsSameColor(ball))
                            {
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<TinyTube>().ReturnNext());
                            }
                        }
                    }

                    firstTubeClicked = null;
                    canUndo = true;
                    Cork();
                    if (CheckForWin()) { Win(); }
                }
                else if (!second.FullTube())// move ball from different tube to the top
                {
                    firstTubeClicked.GetComponent<TinyTube>().MoveTopToBottom();

                    Debug.Log("move from dif");

                    firstTubeClicked = tube;



                    firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
                }

                //else { Debug.Log("not win"); }
                

                tinyTubeTime = false;
            }
            
            
            
            else if (clickState && firstTubeClicked == tube) // move ball back into the tube
            {
                tube.GetComponent<Tube>().MoveTopToBottom();

                //Debug.Log("same tube");
                undoHolster.RemoveAt(undoHolster.Count - 1);
                if (TinyTube.activeSelf) { TTHolster.RemoveAt(TTHolster.Count - 1); }
                tube = null;
                clickState = false;
            }

        }
    } // main game function

    public void ClickedTinyTube(GameObject tube)
    {
        if (!clickState && !tube.GetComponent<TinyTube>().EmptyTube()) // move ball to the top
        {
            SetUndoTubes();
            clickState = true;
            firstTubeClicked = tube;
            //Debug.Log("tubeFirstClicked");
            tinyTubeTime = true;

            firstTubeClicked.GetComponent<TinyTube>().MoveBottomToTop();
        }
        else if (!tinyTubeTime && clickState && firstTubeClicked != tube) // coming from normal tube (firstTubeClicked is a normal tube)
        {
            GameObject ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

            TinyTube second = tube.GetComponent<TinyTube>();
            //Debug.Log(ball.GetComponent<Image>().color);

            if (second.EmptyTube())
            {

                clickState = false;

                //Debug.Log("tubeSecondClick");
                second.NewBallsToBottom(ball);



                for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                {

                    if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != null)
                    {
                        if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                        {
                            second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                        }
                    }
                }



                firstTubeClicked = null;
                tinyTubeTime = false;
            }
            else if (ball.tag == second.GetBottomBall().tag && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
            {
                clickState = false;



                second.NewBallsToBottom(ball);

                if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball)) // fix this for only going to the last ball to get rid of error
                {
                    second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                    if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
                    {
                        second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                        if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
                        {
                            second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                        }
                    }
                }

                firstTubeClicked = null;
                tinyTubeTime = false;
            }
            else if (!second.EmptyTube())// move ball from different tube to the top
            {
                firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();

                //Debug.Log("move from dif");

                firstTubeClicked = tube;



                firstTubeClicked.GetComponent<TinyTube>().MoveBottomToTop();
            }
            canUndo = true;
            Cork();
            if (CheckForWin()) { Win(); }


            //else { Debug.Log("not win"); }

            

        }
        
        else if (clickState && firstTubeClicked == tube) // move ball back into the tube
        {
            tube.GetComponent<TinyTube>().MoveTopToBottom();

            //Debug.Log("same tube");
            undoHolster.RemoveAt(undoHolster.Count - 1);
            if (TinyTube.activeSelf) { TTHolster.RemoveAt(TTHolster.Count - 1); }
            tube = null;
            clickState = false;
        }

    }


    void Win()
    {
        /*if (!gameObject.GetComponent<LevelCreator>().inChallenge)
        {
            menuNum = 1;
            OpenMenuNum(menuNum);
            gameObject.GetComponent<LevelCreator>().BeatLastLevel();
            gameObject.GetComponent<LevelCreator>().UpdateCompleted();
            ResetGame();
            SetUndoTubes();
            canUndo = false;
        }
        else if (gameObject.GetComponent<LevelCreator>().inChallenge)
        {
            menuNum = 1;
            OpenMenuNum(menuNum);
            gameObject.GetComponent<LevelCreator>().BeatLastChallengeLevel();
            ResetGame();
            //gameObject.GetComponent<LevelCreator>().UpdateCompleted();
        }*/

        win = true;
    }

    public void SetUndoTubes()
    {
        //for (int i = 0; i < undoTubes.Count; ++i) { Destroy(undoTubes[i]); }
        List<GameObject> newTubes = new List<GameObject>();

        for (int i = 0; i < tubes.Count; ++i)
        {
            GameObject test = Instantiate(fullTubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            test.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            test.transform.SetParent(moveHolder.transform);
            Tube tube = test.GetComponent<Tube>();

            for (int ii = 0; ii < 5; ++ii)
            {
                if (tubes[i].transform.GetChild(ii).childCount != 0 && test.transform.GetChild(ii).childCount != 0) // is one in actual and in new
                {
                    GameObject newBall = tubes[i].transform.GetChild(ii).GetChild(0).gameObject;
                    test.transform.GetChild(ii).GetChild(0).gameObject.tag = newBall.tag;
                    test.transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = newBall.GetComponent<Image>().color;
                }
                else if (tubes[i].transform.GetChild(ii).childCount != 0 && test.transform.GetChild(ii).childCount == 0) // is one in actual but not in new
                {
                    GameObject newBall = tubes[i].transform.GetChild(ii).GetChild(0).gameObject;
                    GameObject added = Instantiate(newBall, new Vector3(0, 0, 0), Quaternion.identity);

                    added.transform.SetParent(test.transform.GetChild(ii));
                    added.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    added.transform.localPosition = new Vector3(0, 0, 0);
                }
                else if (tubes[i].transform.GetChild(ii).childCount == 0 && test.transform.GetChild(ii).childCount != 0) // in not one in actual but in new
                {
                    Destroy(test.transform.GetChild(ii).GetChild(0).gameObject);
                }
            }

            newTubes.Add(test);

            
        }

        if (TinyTube.activeSelf)
        {
            GameObject tt = Instantiate(TinyTubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            tt.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            tt.transform.SetParent(moveHolder.transform);
            for (int ii = 0; ii < 2; ++ii)
            {
                if (TinyTube.transform.GetChild(ii).childCount != 0 && tt.transform.GetChild(ii).childCount != 0) // is one in actual and in new
                {
                    GameObject newBall = TinyTube.transform.GetChild(ii).GetChild(0).gameObject;
                    tt.transform.GetChild(ii).GetChild(0).gameObject.tag = newBall.tag;
                    tt.transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = newBall.GetComponent<Image>().color;
                }
                else if (TinyTube.transform.GetChild(ii).childCount != 0 && tt.transform.GetChild(ii).childCount == 0) // is one in actual but not in new
                {
                    GameObject newBall = TinyTube.transform.GetChild(ii).GetChild(0).gameObject;
                    GameObject added = Instantiate(newBall, new Vector3(0, 0, 0), Quaternion.identity);

                    added.transform.SetParent(tt.transform.GetChild(ii));
                    added.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    added.transform.localPosition = new Vector3(0, 0, 0);
                }
                else if (TinyTube.transform.GetChild(ii).childCount == 0 && tt.transform.GetChild(ii).childCount != 0) // in not one in actual but in new
                {
                    Destroy(tt.transform.GetChild(ii).GetChild(0).gameObject);
                }
            }

            TTHolster.Add(tt);
        }

        undoHolster.Add(newTubes);
        

        //Debug.LogWarning(undoHolster.Count);
        
    }

    public void UNDO()
    {
        int coins = gameObject.GetComponent<LevelCreator>().coins;

        

        if (canUndo && undoHolster.Count != 0 && undosLeft > 0)
        {
            undosLeft--;
            //Debug.LogWarning("undo");
            //insult.SetActive(true);
            //if (insultTimer == insultTime) { doInsult = true; }

            GameObject undoTT = new GameObject();
            undoTubes = undoHolster[undoHolster.Count - 1];
            if (TinyTube.activeSelf) { undoTT = TTHolster[TTHolster.Count - 1]; }

                List<GameObject> testTubes = new List<GameObject>();

            for (int i = 0; i < undoTubes.Count; ++i)
            {
                GameObject test = Instantiate(undoTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));


                test.transform.position = tubes[i].transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);



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
                Destroy(TinyTube);
                TinyTube = test;
                TTHolster.RemoveAt(TTHolster.Count - 1);
                
            }

            

            if (undoHolster.Count == 0) { canUndo = false; }
        }
        else if (canUndo && undoHolster.Count != 0 && coins >= undoCost && undosLeft == 0)
        {
            //insult.SetActive(true);
            coins -= undoCost;
            gameObject.GetComponent<LevelCreator>().coins = coins;
            //if (insultTimer == insultTime) { doInsult = true; }

            GameObject undoTT = new GameObject();
            undoTubes = undoHolster[undoHolster.Count - 1];
            if (TinyTube.activeSelf) { undoTT = TTHolster[TTHolster.Count - 1]; }

            List<GameObject> testTubes = new List<GameObject>();

            for (int i = 0; i < undoTubes.Count; ++i)
            {
                GameObject test = Instantiate(undoTubes[i], new Vector3(0, 0, 0), Quaternion.identity);

                test.transform.SetParent(gameScreen.transform.GetChild(0));


                test.transform.position = tubes[i].transform.position;
                test.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);



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
                Destroy(TinyTube);
                TinyTube = test;
                TTHolster.RemoveAt(TTHolster.Count - 1);

            }
            //Debug.LogWarning(undoHolster.Count);

            if (undoHolster.Count == 0) { canUndo = false; }
        }

        undosLeftText.text = "Free Undos Left: " + undosLeft.ToString();
        if (undosLeft == 0) { undosLeftText.text = "No Undos Left: " + undoCost.ToString() + " coins"; }
        
    }

    public void TinyTubeTime()
    {
        int coins = gameObject.GetComponent<LevelCreator>().coins;

        if (!tinyTubeActive && coins >= TinyTubeCost)
        {
            //if (insultTimer == insultTime) { doInsult = true; }

            
            TinyTubeButton.interactable = false;

            TinyTube.SetActive(true);
            coins -= TinyTubeCost;
            gameObject.GetComponent<LevelCreator>().coins = coins;
            tinyTubeActive = true;
        }
    }
    
    public bool CheckIfAnyMovesLeft()
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            for (int ii = 0; ii < tubes.Count; ++ii)
            {
                if (ii != i)
                {
                    
                    Tube tube1 = tubes[i].GetComponent<Tube>();
                    Tube tube2 = tubes[ii].GetComponent<Tube>();
                    GameObject ball1 = null;
                    GameObject ball2 = null;

                    if (!tube1.EmptyTube() && !tube2.EmptyTube())
                    {
                        ball1 = tubes[i].GetComponent<Tube>().GetBottomBall();
                        ball2 = tubes[ii].GetComponent<Tube>().GetBottomBall();
                    }
                    else
                    {
                        //Debug.Log(i + " " + ii);
                        //Debug.LogWarning("there is a move");
                        return true;
                    }


                    //Debug.Log("num same at top: " + tube1.NumSameAtTop());
                    //Debug.Log("open spots: " + tube2.ReturnNumOpenSpots());

                    if (ball1.tag == ball2.tag)
                    {
                        if (tube1.NumSameAtTop() <= tube2.ReturnNumOpenSpots())
                        {
                            //Debug.Log(i + " " + ii);
                            //Debug.LogWarning("there is a move");
                            return true;
                        }
                        
                    }
                       
                }
            }
        }
        
        return false;
    }

    public void ModeChange()
    {
        if (undoButton.activeSelf)
        {
            undoButton.SetActive(false);
            TinyTubeButton.gameObject.SetActive(false);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode: Normal";
            modeButton.GetComponent<Image>().color = blank.color;
        }
        else if (!undoButton.activeSelf)
        {
            undoButton.SetActive(true);
            TinyTubeButton.gameObject.SetActive(true);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode:   Easy";
            modeButton.GetComponent<Image>().color = badRed.color;
        }
    }

    public void OpenSettingsScreen()
    {
        if (SettingsScreen.activeSelf) { SettingsScreen.SetActive(false); }
        else if (!SettingsScreen.activeSelf) { SettingsScreen.SetActive(true); }
    }

    public void BuyBackground(int index)
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

    public void SetBackground(int index)
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

    public void LoadBackgrounds()
    {
        if (PlayerPrefs.HasKey("lastBackground"))
        {
            Debug.Log(PlayerPrefs.GetInt("lastBackground"));
            SetBackground(PlayerPrefs.GetInt("lastBackground"));
        }
        else
        {
            PlayerPrefs.SetInt("lastBackground", 0);
        }

        if (PlayerPrefs.HasKey("redLock")) { if (PlayerPrefs.GetInt("redLock") == 1) { redLockButton.gameObject.SetActive(false); } }
        if (PlayerPrefs.HasKey("greenLock")) { if (PlayerPrefs.GetInt("greenLock") == 1) { greenLockButton.gameObject.SetActive(false); } }
        if (PlayerPrefs.HasKey("blueLock")) { if (PlayerPrefs.GetInt("blueLock") == 1) { blueLockButton.gameObject.SetActive(false); } }

        greenLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        redLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        blueLockButton.gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = BackgroundCost.ToString() + " Coins";
        
    }
    
}
