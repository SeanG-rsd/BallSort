using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    public List<int> spots = new List<int>();

    public int index;

    public bool GameTube;

    public bool TutorialTube;

    public bool isFull;

    public GameObject gm;

    public Button button;

    public GameObject CorkPrefab;
    public bool corked = true;

    private int InvalidIndex = -1;

    public List<GameObject> spotObjects;

    public ParticleSystem confettiPrefab;
    float loadedConfetti = 0.2f;
    bool canConfetti;

    // Start is called before the first frame update

    void Awake()
    {
        
        
        
        GameTube = true;
        if (!TutorialTube)
        {
            gm = GameObject.Find("GameManager");
            button.onClick.AddListener(Clicked);
        }
        else
        {
            gm = GameObject.Find("Manager");
            button.onClick.AddListener(TutorialClicked);
        }
        corked = false;
        spots.Clear();

        spots.Add(0);
        spots.Add(0);
        spots.Add(0);
        spots.Add(0);
        spots.Add(0);
    }

    

    

    public void SetSpot(int given, int where)
    {
        Debug.Log(spots.Count);
        spots[where] = given;
    }

    void UpdateCork()
    {
        if (FullTube() && !EmptyTube())
        {
            Cork();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!corked && GameTube)
        {
            UpdateCork();
        }

        if (!canConfetti)
        {
            loadedConfetti -= Time.deltaTime;
            if (loadedConfetti < 0)
            {
                canConfetti = true;
            }
        }
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
        for (int i = spots.Count - 1; i >= 0; i--)
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

    public int GetBottomBall() // get the lowest ball in the tube
    {
        for (int i = 0; i < spots.Count; i++)
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
        if (spots[4] == 0)
        {
            return true;
        }
        return false;
    }

    public bool FullTube() // check if the tube is completed
    {
        if (spots[0] == 0)
        {
            for (int i = 1; i < spots.Count; ++i)
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
        if (BottomIndex() != InvalidIndex)
        {
            spots[BottomIndex() - 1] = ball;
        }
        else
        {
            spots[4] = ball;
        }
        ogTube.spots[ogLocation] = 0;
    }

    public void NewBallsFromTT(int ball, TinyTube ogTube, int ogLocation)
    {
        if (BottomIndex() != InvalidIndex)
        {
            spots[BottomIndex() - 1] = ball;
        }
        else
        {
            spots[4] = ball;
        }
        ogTube.spots[ogLocation] = 0;
    }

    public bool CheckIfNextIsSameColor(int ball)
    {
        //Debug.Log("check if next is same color: " + BottomIndex());
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
        if (BottomIndex() != InvalidIndex)
        {
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
        }
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
        //Debug.Log("return next: " + BottomIndex());
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
                for (int i = BottomIndex(); i < spots.Count; ++i)
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

        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] == 0) { num++; }
        }

        //Debug.Log("open spots: " + num);
        return num;
    }

    public int NumSameAtTop()
    {
        int num = 0;
        if (spots[4] == 0) { return 0; }

        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] != 0)
            {
                if (spots[i] == spots[BottomIndex()])
                {
                    num++;
                }
                else
                {
                    return num;
                }
            }
        }
        //Debug.Log("num same at top: " + num);

        return num;
    }

    public void ResetSelf()
    { 

    }

    void Clicked()
    {
        
        gm.GetComponent<GameManager>().Clicked(gameObject);

    }

    void TutorialClicked()
    {
        gm.GetComponent<TutorialManager>().Clicked(gameObject);
    }

    public void Cork()
    {
        //Debug.Log("corked");

        corked = true;

        GameObject Cork = Instantiate(CorkPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ParticleSystem confetti = Instantiate(confettiPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        //confetti.gameObject.transform.SetParent(gameObject.transform);
        Vector3 pos = spotObjects[spotObjects.Count - 1].transform.position;
        pos.z = -1;
        pos.y = pos.y - 0.1f;
        confetti.gameObject.transform.position = pos;
        confetti.gameObject.transform.localScale = new Vector3(1, 1, 1);
        
        

        if (canConfetti)
        {
            Debug.Log("confetti");
            confetti.Play();
            canConfetti = false;
        }
        else { Destroy(confetti.gameObject); }

        Cork.transform.SetParent(gameObject.transform);
        Cork.transform.localPosition = new Vector3(0, 104.5f, 0);
        Cork.transform.localScale = new Vector3(1, 1, 1);
    }
}

