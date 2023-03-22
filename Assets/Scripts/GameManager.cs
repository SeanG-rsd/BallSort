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

    public GameObject insult;
    public TMP_Text insultText;
    public List<string> insults;
    bool doInsult;
    public float insultTime;
    float insultTimer;

    public Material blank;
    public Material badRed;

    // Start is called before the first frame update
    void Start()
    {

        insult.SetActive(false);
        insultTimer = insultTime;
        //ModeChange();
        OpenMenuNum(1);
    }

    // Update is called once per frame
    void Update()
    {
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
        Debug.Log("reset game");
        
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
        Debug.Log("openlevelscreen");
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
    }

    bool CheckForWin() // check to see if all tubes are the right color
    {
        for (int i = 0; i < tubes.Count; ++i)
        {
            if (!tubes[i].GetComponent<Tube>().FullTube() && !tubes[i].GetComponent<Tube>().EmptyTube()) { return false; } 
        }

        return true;
    }

    public void Clicked(GameObject tube)
    {
        
        if (!clickState && !tube.GetComponent<Tube>().FullTube()) // move ball to the top
        {
            SetUndoTubes();
            clickState = true;
            firstTubeClicked = tube;
            Debug.Log("tubeFirstClicked");

            firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
        }
        else if (clickState && firstTubeClicked != tube) // move balls into empty tube
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
            }
            else if (ball.GetComponent<Image>().color == second.GetBottomBall().GetComponent<Image>().color && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
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
            }
            else if (!second.FullTube())// move ball from different tube to the top
            {
                firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();

                Debug.Log("move from dif");

                firstTubeClicked = tube;
                
                

                firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
            }
            canUndo = true;


            if (CheckForWin()) { Win(); }
            else { Debug.Log("not win"); }

            

        }
        else if (clickState && firstTubeClicked == tube) // move ball back into the tube
        {
            tube.GetComponent<Tube>().MoveTopToBottom();

            Debug.Log("same tube");

            tube = null;
            clickState = false;
        }  
    }

    void Win()
    {
        menuNum = 1;
        OpenMenuNum(menuNum);
        gameObject.GetComponent<LevelCreator>().BeatLastLevel();
        gameObject.GetComponent<LevelCreator>().UpdateCompleted();
        ResetGame();
        SetUndoTubes();
        canUndo = false;
    }

    void SetUndoTubes()
    {
        for (int i = 0; i < undoTubes.Count; ++i) { Destroy(undoTubes[i]); }
        undoTubes.Clear();

        for (int i = 0; i < tubes.Count; ++i)
        {
            GameObject test = Instantiate(fullTubePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            test.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            test.transform.SetParent(moveHolder.transform);
            Tube tube = test.GetComponent<Tube>();

            for (int ii = 0; ii < tubes[i].transform.childCount; ++ii)
            {
                if (tubes[i].transform.GetChild(ii).childCount != 0 && test.transform.GetChild(ii).childCount != 0) // is one in actual and in new
                {
                    GameObject newBall = tubes[i].transform.GetChild(ii).GetChild(0).gameObject;
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

            undoTubes.Add(test);
        }

        
    }

    public void UNDO()
    {
        if (canUndo)
        {
            Debug.LogWarning("undo");
            insult.SetActive(true);
            if (insultTimer == insultTime) { doInsult = true; }



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

            canUndo = false;
        }
    }

    public void ModeChange()
    {
        if (undoButton.activeSelf)
        {
            undoButton.SetActive(false);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode: Normal";
            modeButton.GetComponent<Image>().color = blank.color;
        }
        else if (!undoButton.activeSelf)
        {
            undoButton.SetActive(true);
            modeButtonText.GetComponent<TMP_Text>().text = "Mode: Easy";
            modeButton.GetComponent<Image>().color = badRed.color;
        }
    }

}
