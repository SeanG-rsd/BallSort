using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    public List<bool> spots = new List<bool> { false, false, false, false, false };

    public int index;

    public bool GameTube;

    public bool isFull;

    public GameObject gm;

    public Button button;

    public GameObject CorkPrefab;
    public bool corked;

    public List<GameObject> spotObjects;

    // Start is called before the first frame update

    void Awake()
    {
        gm = GameObject.Find("GameManager");
        GameTube = true;
        button.onClick.AddListener(Clicked);
        corked = false;
    }

    

    void Start()
    {
        if (!corked && GameTube) { UpdateSpots(); }
    }

    public void UpdateSpots()
    {
        for (int i = 0; i < spotObjects.Count; ++i)
        {
            if (spotObjects[i].transform.childCount == 0)
            {
                spots[i] = false;
            }    
            else { spots[i] = true; }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!corked && GameTube) { UpdateSpots(); }
    }

    public void MoveBottomToTop()
    {
        
        for (int i = 1; i < spotObjects.Count; ++i)
        {
            if (spots[i] && !spots[0])
            {
                spotObjects[i].transform.GetChild(0).SetParent(spotObjects[0].transform);
                spotObjects[0].transform.GetChild(0).position = spotObjects[0].transform.position;
                //Debug.Log(i);
                index = i;
                return;
            }
        }
    }

    public int BottomIndex()
    {
        UpdateSpots();

        for (int i = 1; i < spotObjects.Count; ++i)
        {
            if (spots[i])
            {
                return i;
            }
        }

        return 5;
    }

    public void MoveTopToBottom() // move the lowest ball in the tube to the above spot on the tube
    {
        for (int i = spotObjects.Count - 1; i >= 0; i--)
        {
            if (!spots[i] && spots[0])
            {
                spotObjects[0].transform.GetChild(0).SetParent(spotObjects[i].transform);
                spotObjects[i].transform.GetChild(0).position = spotObjects[i].transform.position;
                return;
            }
        }
    }

    public GameObject GetTopBall() // get the ball sitting above the tube
    {
        if (spots[0])
        {
            return spotObjects[0].transform.GetChild(0).gameObject;
        }
        Debug.LogWarning("not getting top ball");
        return null;
    }

    public GameObject GetBottomBall() // get the last ball in the tube
    {
        UpdateSpots();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (spots[i])
            {
                return spotObjects[i].transform.GetChild(0).gameObject;
            }

            
        }

        return null;
    }

    public bool EmptyTube() // check if the tube is empty
    {
        if (!spots[4])
        {
            return true;
        }
        return false;
    }

    public bool FullTube() // check if the tube is completed
    {
        if (!spots[0])
        {
            for (int i = 1; i < spotObjects.Count; ++i)
            {


                if (!spots[i])
                {
                    return false;
                }
                else
                {
                    if (spots[1] && spots[i])
                    {
                        GameObject ball = spotObjects[1].transform.GetChild(0).gameObject;
                        GameObject check = spotObjects[i].transform.GetChild(0).gameObject;

                        if (ball.GetComponent<Image>().color != check.GetComponent<Image>().color) { return false; }
                    }
                }
            }

            isFull = true;

            return true;
        }

        return false;
    }

    public void NewBallsToBottom(GameObject ball)
    {
        for (int i = 1; i < spotObjects.Count; ++i)
        {
            if (!spots[i])
            {
                ball.transform.SetParent(spotObjects[i].transform);
                ball.transform.position = spotObjects[i].transform.position;
            }
        }

        UpdateSpots();
    }

    public bool CheckIfNextIsSameColor(GameObject ball)
    {
        //Debug.Log("check if next is same color: " + BottomIndex());
        if (BottomIndex() != 5)
        {
            if (spots[BottomIndex()])
            {

                if (spotObjects[BottomIndex()].transform.GetChild(0).gameObject.GetComponent<Image>().color == ball.GetComponent<Image>().color)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int CheckHowManyNextIsSameColor(GameObject ball)
    {
        int num = 0;

        for (int i = BottomIndex(); i < 5; ++i)
        {
            if (spots[i])
            {
                if (spotObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color != ball.GetComponent<Image>().color) { return num; }
                if (spotObjects[i].transform.GetChild(0).gameObject.GetComponent<Image>().color == ball.GetComponent<Image>().color)
                {
                    num++;
                    
                }
            }
        }
        //Debug.Log("num same: " + num);
        return num;
    }

    public bool CheckTwo(GameObject ball, GameObject other)
    {
        if (other.GetComponent<Image>().color == ball.GetComponent<Image>().color)
        {
            return true;
        }

        return false;
    }

    public GameObject ReturnNext()
    {
        //Debug.Log("return next: " + BottomIndex());
        if (BottomIndex() != 5)
        {
            if (spots[BottomIndex()])
            {
                return spotObjects[BottomIndex()].transform.GetChild(0).gameObject;

            }
        }
        return null;
    }

    public bool CanMoveIntoNextTube(GameObject tube)
    {
        //Debug.Log("check if moveable");

        if (tube.GetComponent<Tube>().ReturnNumOpenSpots() != 0)
        {
            GameObject ball = GetTopBall();

            int numMoving = 1;
            if (!EmptyTube())
            {
                for (int i = BottomIndex(); i < spotObjects.Count; ++i)
                {
                    if (CheckTwo(ball, spotObjects[i].transform.GetChild(0).gameObject))
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
            if (!spots[i]) { num++; }
        }

        //Debug.Log("open spots: " + num);
        return num;
    }

    void Clicked()
    {
        
        gm.GetComponent<GameManager>().Clicked(gameObject);

    }

    public void Cork()
    {
        Debug.Log("corked");

        corked = true;

        GameObject Cork = Instantiate(CorkPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        Cork.transform.SetParent(gameObject.transform);
        Cork.transform.localPosition = new Vector3(0, 108, 0);
        Cork.transform.localScale = new Vector3(1, 1, 1);
    }
}

