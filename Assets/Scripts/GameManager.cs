using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{ 
    public List<GameObject> tubes;

    public List<Material> mats;

    public List<GameObject> tubePositions;

    bool clickState;
    GameObject firstTubeClicked;

    Material currentMat;

    public int loadedLevelNum;

    public GameObject gameScreen;
    public GameObject mainScreen;
    public GameObject levelScreen;
    public GameObject settingScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingScreen();
        }
    }

    public void OpenCurrentLevel()
    {
        gameScreen.SetActive(true);
        mainScreen.SetActive(false);
        levelScreen.SetActive(false);
        settingScreen.SetActive(false);
    }

    public void OpenPickScreen()
    {
        gameScreen.SetActive(false);
        mainScreen.SetActive(false);
        levelScreen.SetActive(true);
        settingScreen.SetActive(false);
    }

    public void OpenMainScreen()
    {
        gameScreen.SetActive(false);
        mainScreen.SetActive(true);
        levelScreen.SetActive(false);
        settingScreen.SetActive(false);
    }

    public void ToggleSettingScreen()
    {
        if (gameScreen.activeSelf && !settingScreen.activeSelf)
        {
            settingScreen.SetActive(true);
        }
        else if (gameScreen.activeSelf && settingScreen.activeSelf)
        {
            settingScreen.SetActive(false);
        }
    }

    public void Clicked(GameObject tube)
    {
        if (!clickState && !tube.GetComponent<Tube>().FullTube())
        {
            clickState = true;
            firstTubeClicked = tube;
            Debug.Log("tubeFirstClicked");

            firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
        }
        else if (clickState && firstTubeClicked != tube)
        {
            GameObject ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

            Tube second = tube.GetComponent<Tube>();
            //Debug.Log(ball.GetComponent<Image>().color);
            
            if (second.EmptyTube())
            {

                clickState = false;

                //Debug.Log("tubeSecondClick");

                second.NewBallsToBottom(ball);
                Debug.Log(firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball));

                if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
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
            else if (ball.GetComponent<Image>().color == second.GetBottomBall().GetComponent<Image>().color && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube))
            {
                clickState = false;

                

                second.NewBallsToBottom(ball);

                if (firstTubeClicked.GetComponent<Tube>().CheckIfNextIsSameColor(ball))
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
            else
            {
                firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();

                Debug.Log("moveTopToBottom");

                firstTubeClicked = tube;
                

                firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
            }
        }
        else if (clickState && firstTubeClicked == tube)
        {
            tube.GetComponent<Tube>().MoveTopToBottom();

            tube = null;
            clickState = false;
        }
    }
}
