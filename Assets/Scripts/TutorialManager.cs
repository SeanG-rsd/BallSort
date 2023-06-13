using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class TutorialManager : MonoBehaviour
{
    public List<GameObject> tubes;


    public bool clickState;
    public GameObject firstTubeClicked;

    bool win;

    public List<Material> mats;

    private int InvalidIndex = -1;

    List<List<int>> level = new List<List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("Tutorial"))
        {
            if (PlayerPrefs.GetInt("Tutorial") == 1)
            {
                SceneManager.LoadScene(2);
            }
        }

        PlayerPrefs.SetInt("Tutorial", 1);

        level.Add(new List<int>());
        level[0].Add(0);
        level[0].Add(0);
        level[0].Add(1);
        level[0].Add(0);

        level.Add(new List<int>());
        level[1].Add(1);
        level[1].Add(1);
        level[1].Add(0);
        level[1].Add(1);

        LoadTutorial();
    }

    void LoadTutorial()
    {
        for (int i = 0; i < level.Count; ++i) // each tube
        {
            
            for (int ii = 1; ii <= level[i].Count; ++ii) // each ball
            {


                tubes[i].transform.GetChild(ii).GetChild(0).gameObject.GetComponent<Image>().color = mats[level[i][ii - 1]].color;
                tubes[i].transform.GetChild(ii).GetChild(0).gameObject.tag = "C" + (level[i][ii - 1] + 1).ToString();
                tubes[i].GetComponent<Tube>().SetSpot(level[i][ii - 1] + 1, ii);
                if (!tubes[i].GetComponent<Tube>().corked) { if (tubes[i].GetComponent<Tube>().FullTube() && !tubes[i].GetComponent<Tube>().EmptyTube()) { tubes[i].GetComponent<Tube>().Cork(); } }
                else { Debug.Log("dont cork"); }
                

                //Debug.Log("changed color");
            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CheckForWin() // check to see if all tubes are the right color
    {
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
                //SetUndoTubes();
                clickState = true;
                firstTubeClicked = tube;
                Tube t = firstTubeClicked.GetComponent<Tube>();

                tube.transform.GetChild(t.BottomIndex()).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, tube, 0);
                t.MoveBottomToTop();
            }
            else if (clickState && firstTubeClicked != tube) // move balls into empty tube
            {

                int ball = firstTubeClicked.GetComponent<Tube>().GetTopBall();

                Tube second = tube.GetComponent<Tube>();
                //Debug.Log(ball.GetComponent<Image>().color);

                if (second.EmptyTube())
                {

                    clickState = false;
                    int moveIndex = 4;

                    firstTubeClicked.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, tube, moveIndex);
                    second.NewBallsToBottom(ball, firstTubeClicked.GetComponent<Tube>(), 0);



                    for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {

                        if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != InvalidIndex)
                        {
                            if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                            {

                                moveIndex--;
                                //second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                                //Debug.LogWarning("sent new ball");

                                firstTubeClicked.transform.GetChild(firstTubeClicked.GetComponent<Tube>().BottomIndex()).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, tube, moveIndex);
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext(), firstTubeClicked.GetComponent<Tube>(), firstTubeClicked.GetComponent<Tube>().BottomIndex());
                            }
                        }
                    }



                    firstTubeClicked = null;


                    if (CheckForWin()) { Win(); }

                }
                else if (ball == second.GetBottomBall() && !second.FullTube() && firstTubeClicked.GetComponent<Tube>().CanMoveIntoNextTube(tube)) // move more than one ball from partially full tube
                {
                    clickState = false;



                    Tube t = firstTubeClicked.GetComponent<Tube>();

                    int moveIndex = second.BottomIndex() - 1;

                    second.NewBallsToBottom(ball, firstTubeClicked.GetComponent<Tube>(), 0);
                    firstTubeClicked.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, tube, moveIndex);

                    for (int i = 0; i <= firstTubeClicked.GetComponent<Tube>().CheckHowManyNextIsSameColor(ball); ++i)
                    {

                        if (firstTubeClicked.GetComponent<Tube>().ReturnNext() != InvalidIndex)
                        {
                            if (firstTubeClicked.GetComponent<Tube>().CheckTwo(firstTubeClicked.GetComponent<Tube>().ReturnNext(), ball))
                            {
                                moveIndex--;
                                //second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext());
                                Debug.LogWarning("sent new ball");

                                firstTubeClicked.transform.GetChild(firstTubeClicked.GetComponent<Tube>().BottomIndex()).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, tube, moveIndex);
                                second.NewBallsToBottom(firstTubeClicked.GetComponent<Tube>().ReturnNext(), firstTubeClicked.GetComponent<Tube>(), firstTubeClicked.GetComponent<Tube>().BottomIndex());
                            }
                        }
                    }

                    firstTubeClicked = null;


                    if (CheckForWin()) { Win(); }

                }
                else if (!second.FullTube())// move ball from different tube to the top
                {



                    firstTubeClicked.GetComponent<Tube>().MoveTopToBottom();
                    firstTubeClicked.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, firstTubeClicked, firstTubeClicked.GetComponent<Tube>().BottomIndex());

                    firstTubeClicked = tube;


                    //firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
                    firstTubeClicked.transform.GetChild(firstTubeClicked.GetComponent<Tube>().BottomIndex()).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(2, firstTubeClicked, 0);
                    firstTubeClicked.GetComponent<Tube>().MoveBottomToTop();
                }



            }
            else if (clickState && firstTubeClicked == tube) // move ball back into the tube
            {
                tube.GetComponent<Tube>().MoveTopToBottom();
                tube.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Ball>().MoveBall(10, tube, tube.GetComponent<Tube>().BottomIndex());

                //Debug.Log("same tube");
                //undoHolster.RemoveAt(undoHolster.Count - 1);
                //if (TinyTube.activeSelf) { TTHolster.RemoveAt(TTHolster.Count - 1); }
                tube = null;
                clickState = false;
            }

        }
    } // main game function

    public void Win()
    {
        

        win = true;
    }

    public void StartGame()
    {
        if (win)
        {
            
            SceneManager.LoadScene(2);
        }
    }
}
