using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    public List<GameObject> ballObjects = new List<GameObject>();

    public List<int> spots = new List<int>();

    [SerializeField] public bool isTinyTube; 
    public bool isFull;

    public Button button;

    public GameObject CorkPrefab;
    public bool corked;
    private bool isWaitingToBeCorked;

    private int InvalidIndex = -1;

    public List<GameObject> spotObjects;

    public ParticleSystem confettiPrefab;
    float loadedConfetti = 0.2f;
    bool canConfetti;

    public GameObject ballPrefab;

    public int siblingIndex = 0;

    [SerializeField] private int tubeSize;

    [SerializeField] private GameObject topSpot;

    // Start is called before the first frame update

    void Awake()
    {        
        button.onClick.AddListener(Clicked);
        
        for (int i = 0; i < tubeSize; i++)
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

        Ball.OnMadeItHome += HandleBallMadeItHome;
    }

    private void OnDestroy()
    {
        Ball.OnMadeItHome -= HandleBallMadeItHome;
    }

    public void SetSpot(int given, int where) // sets a spot in the tube
    {        
        spots[where] = given;
    }
    void Update() // checks if the tube is able to play confetti
    {
        if (!canConfetti)
        {
            loadedConfetti -= Time.deltaTime;
            if (loadedConfetti < 0)
            {
                canConfetti = true;
            }
        }

        if (isWaitingToBeCorked)
        {
            if (!IsMovement())
            {
                isWaitingToBeCorked = false;
                CloseTube();
            }
        }
    }

    private bool IsMovement()
    {
        for (int i = 0; i < ballObjects.Count; i++)
        {
            if (ballObjects[i] != null)
            {
                if (ballObjects[i].GetComponent<Ball>().IsMoving())
                {
                    
                    return true;
                }
                Debug.Log(ballObjects[i].transform.localPosition);
            }
        }

        return false;
    }

    public void MoveBottomToTop() // moves the first ball to the top of the tube
    { 
        int index = BottomIndex();
        spots[0] = spots[index];
        ballObjects[0] = ballObjects[index];
        spots[index] = 0;
        ballObjects[index] = null;

        MoveBallToHome(true, 0, this);
    }

    public int BottomIndex() // returns the index of the last ball
    {
        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] != 0)
            {
                return i;
            }
        }

        return tubeSize - 1;
    }

    public void MoveTopToBottom() // 
    {
        for (int i = spots.Count - 1; i >= 0; i--)
        {
            if (spots[i] == 0 && spots[0] != 0)
            {
                spots[i] = spots[0];
                ballObjects[i] = ballObjects[0];
                MoveBallToHome(true, i, this);
                spots[0] = 0;
                ballObjects[0] = null;
                
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
        if (spots[tubeSize - 1] == 0)
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

    public int NumberMoving() // returns how many balls from the bottom index ball are the same color as the top ball
        // only valid given the top ball is not null
    {
        int num = 1;
        if (BottomIndex() != InvalidIndex)
        {
            for (int i = BottomIndex(); i < tubeSize; ++i)
            {
                if (spots[i] != 0)
                {
                    if (spots[i] != GetTopBall()) { return num; }
                    if (spots[i] == GetTopBall())
                    {
                        num++;

                    }
                }
            }
        }
        return num;
    }

    public int NumOpenSpots() // returns the number of open spots at the top of the tube
    {
        int num = 0;

        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] == 0) { num++; }
        }

        //Debug.Log("open spots: " + num);
        return num;
    }

    public void NewBallToBottom(int ball, GameObject ballObject, Tube originalTube) // puts a new ball into the bottom of this tube from another tube
    {

        int openSpot = GetOpenSpot();
        ballObjects[openSpot] = ballObject;

        spots[openSpot] = ball;

        MoveBallToHome(false, openSpot, originalTube);
    }

    public void RemoveTopBall()
    {
        spots[0] = 0;
        ballObjects[0] = null;
    }

    public void RemoveBottomBall()
    {
        ballObjects[BottomIndex()] = null;
        spots[BottomIndex()] = 0;
    }

    public void RemoveBall(int position)
    {
        Destroy(ballObjects[position]);
        ballObjects[position] = null;
        spots[position] = 0;
    }

    public void AddBall(int position, Color color, int colorIndex)
    {
        if (spots[position] == 0 && ballObjects[position] == null)
        {
            GameObject newBall = Instantiate(ballPrefab, spotObjects[position].transform);
            newBall.GetComponent<Image>().color = color;
            ballObjects[position] = newBall;
            spots[position] = colorIndex;
        }
        else
        {
            spots[position] = colorIndex;
            ballObjects[position].GetComponent<Image>().color = color;
        }
    }

    public void EmptyEntireTube()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            if (ballObjects[i] != null)
            {
                RemoveBall(i);
            }
        }
    }

    public int GetOpenSpot()
    {
        for (int i = tubeSize - 1; i >= 1; i--)
        {
            if (spots[i] == 0)
            {
                return i;
            }
        }

        return InvalidIndex;
    }

    public void SetBall(int position, Color color, int colorIndex)
    {
        ballObjects[position].GetComponent<Image>().color = color;
        ballObjects[position].tag = "C" + (colorIndex + 1).ToString();
        SetSpot(colorIndex + 1, position);
    }

    public int ReturnNumOpenSpots() // returns the number of open spots at the top of the tube
    {
        int num = 0;

        for (int i = 1; i < spots.Count; ++i)
        {
            if (spots[i] == 0) { num++; }
        }

        //Debug.Log("open spots: " + num);
        return num;
    }

    public int NumSameAtTop() // checks how many at the top of the tube are the same
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

        return num;
    }

    private void MoveBallToHome(bool isFromThisTube, int position, Tube originalTube)
    {
        List<Vector2> targetSpots = new List<Vector2>();

        if (!isFromThisTube)
        {
            targetSpots.Add(originalTube.topSpot.transform.position);
            
            if (transform.GetSiblingIndex() > originalTube.transform.GetSiblingIndex())
            {
                ballObjects[position].transform.SetParent(spotObjects[position].transform);
                ballObjects[position].transform.localScale = Vector3.one;
            }

            if (originalTube.topSpot.transform.position.y > topSpot.transform.position.y) // top to bottom
            {
                targetSpots.Add(new Vector2(spotObjects[position].transform.position.x, ballObjects[position].transform.position.y));
                targetSpots.Add(topSpot.transform.position);
            }
            else if (originalTube.topSpot.transform.position.y < topSpot.transform.position.y) // bottom to top
            {
                targetSpots.Add(new Vector2(ballObjects[position].transform.position.x, spotObjects[position].transform.position.y));
                targetSpots.Add(topSpot.transform.position);
            }
            else // same to same
            {
                targetSpots.Add(topSpot.transform.position);
            }
        }

        targetSpots.Add(spotObjects[position].transform.position);

        ballObjects[position].GetComponent<Ball>().MoveBall(targetSpots, position, this);
    }

    private void HandleBallMadeItHome(GameObject ball, int position, Tube homeTube)
    {
        if (this == homeTube)
        {
            ball.transform.SetParent(spotObjects[position].transform);
            ball.transform.localScale = Vector3.one;
        }
    }

    void Clicked() // setup for button click
    {
        LevelManager.instance.OnClickTube(gameObject);
    }
    public void Cork() // corks the tube and play confetti
    {
        corked = true;

        isWaitingToBeCorked = true;
    }

    private void CloseTube()
    {
        GameObject Cork = Instantiate(CorkPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ParticleSystem confetti = Instantiate(confettiPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        Vector3 pos = spotObjects[spotObjects.Count - 1].transform.position;
        pos.z = -1;
        pos.y = pos.y - 0.1f;
        confetti.gameObject.transform.position = pos;
        confetti.gameObject.transform.localScale = new Vector3(1, 1, 1);

        if (canConfetti)
        {
            confetti.Play();
            canConfetti = false;
        }
        else { Destroy(confetti.gameObject); }

        Cork.transform.SetParent(gameObject.transform);
        Cork.transform.localPosition = new Vector3(0, 104.5f, 0);
        Cork.transform.localScale = new Vector3(1, 1, 1);
    }
}

