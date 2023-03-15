using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{ 
    public List<GameObject> tubes;


    bool clickState;
    GameObject firstTubeClicked;

    Material currentMat;

    public List<GameObject> screens;
    public GameObject gameScreen;

    public int menuNum = 0;

    public List<GameObject> resetTubes = new List<GameObject>();
    
    public GameObject resetTubePrefab;

    


    // Start is called before the first frame update
    void Start()
    {
        
        //resetTubes = tubes;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("reset");
            tubes = resetTubes;
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
    }

    public void ResetCurrent()
    {
        ResetGame();
        gameObject.GetComponent<LevelCreator>().LoadLastLevel();
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

    }

    void SaveWins()
    {

    }

    void LoadWins()
    {

    }
}
