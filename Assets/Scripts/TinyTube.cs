using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TinyTube : MonoBehaviour
{
    public List<int> spots = new List<int> { 0, 0 };
    public List<GameObject> ballObjects = new List<GameObject>();

    private int InvalidIndex;
    public int index;

    public bool GameTube;

    public bool isFull;

    public GameObject gm;

    public Button button;

    public List<GameObject> spotObjects;

    public GameObject ballPrefab;

    // Start is called before the first frame update

    void Awake()
    {
        gm = GameObject.Find("GameManager");
        GameTube = true;
        button.onClick.AddListener(Clicked);

        for (int i = 0; i < ballObjects.Count; i++)
        {
            if (transform.GetChild(i).childCount != 0)
            {
                ballObjects[i] = transform.GetChild(i).GetChild(0).gameObject;
            }
            else
            {
                ballObjects[i] = null;
            }
        }
    }

    

    public void SetSpot(int given, int where)
    {

        spots[where] = given;
    }

    public void ResetSelf()
    {
        Debug.Log("reset self");
        for (int i = 0; i < spots.Count; ++i)
        {
            if (spots[i] != 0)
            {
                GameObject ball = Instantiate(ballPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                ball.GetComponent<Image>().color = gm.GetComponent<LevelCreator>().mats[spots[i] - 1].color;
                ball.tag = "C" + spots[i].ToString();

                if (transform.GetChild(i).childCount == 0 && i != 0)
                {
                    ball.transform.SetParent(transform.GetChild(i));
                    ball.transform.localPosition = Vector3.zero;
                    ball.transform.localScale = Vector3.one;
                }
                else if (transform.GetChild(i).childCount != 0 && i != 0)
                {
                    Destroy(transform.GetChild(i).GetChild(0).gameObject);

                    ball.transform.SetParent(transform.GetChild(i));
                    ball.transform.localPosition = Vector3.zero;
                    ball.transform.localScale = Vector3.one;
                }
                else if (i == 0)
                {
                    Destroy(transform.GetChild(i).GetChild(0).gameObject);
                }
            }

        }
    }


    public void ClearTube()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            spots[i] = 0;
            if (spotObjects[i].transform.childCount != 0)
            {
                Destroy(spotObjects[i].transform.GetChild(i).gameObject);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
       
    }

    public void MoveBottomToTop()
    {
        index = BottomIndex();
        spots[0] = spots[index];
        spots[index] = 0;
        index = BottomIndex();
    }

    public int BottomIndex()
    {
        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] != 0)
            {
                return i;
            }
        }

        return InvalidIndex;
    }

    public void MoveTopToBottom() // move the lowest ball in the tube to the above spot on the tube
    {
        for (int i = spotObjects.Count - 1; i >= 0; i--)
        {
            if (spots[i] == 0 && spots[0] != 0)
            {
                spots[i] = spots[0];
                spots[0] = 0;

                return;
            }
        }
    }

    public int GetTopBall() // get the ball sitting above the tube
    {
        if (spots[0] != 0)
        {
            return spots[0];
        }
        Debug.LogWarning("not getting top ball");
        return InvalidIndex;
    }

    public int GetBottomBall() // get the last ball in the tube
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (spots[i] != 0)
            {
                return spots[i];
            }


        }

        return InvalidIndex;
    }

    public bool EmptyTube() // check if the tube is empty
    {
        if (spots[1] == 0)
        {
            return true;
        }
        return false;
    }

    public bool FullTube() // check if the tube is completed
    {
        if (spots[0] == 0)
        {
            for (int i = 1; i < spotObjects.Count; ++i)
            {


                if (spots[i] == 0)
                {
                    return false;
                }
                else
                {
                    if (spots[1] != 0 && spots[i] != 0)
                    {
                        int ball = spots[1];
                        int check = spots[i];

                        if (ball != check) { return false; }
                    }
                }
            }

            isFull = true;

            return true;
        }

        return false;
    }

    public void NewBallsToBottom(int ball, Tube ogTube, int ogLocation)
    {
        spots[1] = ball;
        ogTube.spots[ogLocation] = 0;
    }

    public bool CheckIfNextIsSameColor(int ball)
    {
        if (BottomIndex() != InvalidIndex)
        {
            if (spots[BottomIndex()] != 0)
            {
                if (spots[BottomIndex()] == ball)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int CheckHowManyNextIsSameColor(int ball)
    {
        int num = 0;

        for (int i = BottomIndex(); i < 5; ++i)
        {
            if (spots[i] != 0)
            {
                if (spots[i] != ball) { return num; }
                if (spots[i] == ball)
                {
                    num++;

                }
            }
        }
        //Debug.Log("num same: " + num);
        return num;
    }

    public bool CheckTwo(int ball, int other)
    {
        if (other == ball)
        {
            return true;
        }

        return false;
    }

    public int ReturnNext()
    {
        if (BottomIndex() != InvalidIndex)
        {
            if (spots[BottomIndex()] != 0)
            {
                return spots[BottomIndex()];

            }
        }
        return InvalidIndex;
    }

    public bool CanMoveIntoNextTube(GameObject tube)
    {
        //Debug.Log("check if moveable");

        if (tube.GetComponent<Tube>().ReturnNumOpenSpots() != 0)
        {
            int ball = InvalidIndex;
            if (spots[0] != 0) { ball = GetTopBall(); }
            else { ball = GetBottomBall(); }

            int numMoving = 1;
            if (!EmptyTube())
            {
                for (int i = BottomIndex(); i < spotObjects.Count; ++i)
                {
                    if (CheckTwo(ball, spots[i]))
                    {
                        //Debug.Log("is same color");
                        numMoving++;
                    }
                    else
                    {
                        i = 4;
                    }
                }
            }

            //Debug.Log("numMoving: " + numMoving);

            if (numMoving <= tube.GetComponent<Tube>().ReturnNumOpenSpots())
            {
                return true;
            }
        }

        return false;
    }

    public int ReturnNumOpenSpots()
    {
        int num = 0;

        for (int i = 1; i < spotObjects.Count; ++i)
        {
            if (spots[i] == 0) { num++; }
        }

        //Debug.Log("open spots: " + num);
        return num;
    }

    void Clicked()
    {

        gm.GetComponent<GameManager>().ClickedTinyTube(gameObject);

    }
}

