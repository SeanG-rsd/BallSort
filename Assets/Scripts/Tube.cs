using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tube : MonoBehaviour
{
    public List<bool> spots = new List<bool> { false, false, false, false, false };

    public int index;

    public bool GameTube;

    // Start is called before the first frame update
    void Start()
    {
        if (GameTube) { UpdateSpots(); }
    }

    public void UpdateSpots()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).childCount == 0)
            {
                spots[i] = false;
            }    
            else { spots[i] = true; }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameTube) { UpdateSpots(); }
    }

    public void MoveBottomToTop()
    {
        for (int i = 1; i < transform.childCount; ++i)
        {
            if (spots[i] && !spots[0])
            {
                transform.GetChild(i).GetChild(0).SetParent(transform.GetChild(0));
                transform.GetChild(0).GetChild(0).position = transform.GetChild(0).position;
                Debug.Log(i);
                index = i;
                return;
            }
        }
    }

    public int BottomIndex()
    {
        UpdateSpots();

        for (int i = 1; i < transform.childCount; ++i)
        {
            if (spots[i])
            {
                return i;
            }
        }

        return 5;
    }

    public void MoveTopToBottom()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (!spots[i] && spots[0])
            {
                transform.GetChild(0).GetChild(0).SetParent(transform.GetChild(i));
                transform.GetChild(i).GetChild(0).position = transform.GetChild(i).position;
                return;
            }
        }
    }

    public GameObject GetTopBall()
    {
        if (spots[0])
        {
            return transform.GetChild(0).GetChild(0).gameObject;
        }

        return null;
    }

    public GameObject GetBottomBall()
    {
        UpdateSpots();

        for (int i = 0; i < transform.childCount; i++)
        {
            if (spots[i])
            {
                return transform.GetChild(i).GetChild(0).gameObject;
            }

            
        }

        return null;
    }

    public bool EmptyTube()
    {
        if (!spots[4])
        {
            return true;
        }
        return false;
    }

    public bool FullTube()
    {
        for (int i = 1; i < transform.childCount; ++i)
        {
            

            if (!spots[i])
            {
                return false;
            }
            else
            {
                if (spots[1] && spots[i])
                {
                    GameObject ball = transform.GetChild(1).GetChild(0).gameObject;
                    GameObject check = transform.GetChild(i).GetChild(0).gameObject;

                    if (ball.GetComponent<Image>().color != check.GetComponent<Image>().color) { return false; }
                }
            }
        }
        return true;
    }

    public void NewBallsToBottom(GameObject ball)
    {
        for (int i = 1; i < transform.childCount; ++i)
        {
            if (!spots[i])
            {
                ball.transform.SetParent(transform.GetChild(i));
                ball.transform.position = transform.GetChild(i).position;
            }
        }

        UpdateSpots();
    }

    public bool CheckIfNextIsSameColor(GameObject ball)
    {
        Debug.Log("check if next is same color: " + BottomIndex());
        if (spots[BottomIndex()] && BottomIndex() != 5)
        {
            
            if (gameObject.transform.GetChild(BottomIndex()).GetChild(0).gameObject.GetComponent<Image>().color == ball.GetComponent<Image>().color)
            {
                return true;
            }
        }

        return false;
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
        Debug.Log("return next: " + BottomIndex());
        if (spots[BottomIndex()])
        {
            return transform.GetChild(BottomIndex()).GetChild(0).gameObject;
            
        }

        return null;
    }

    public bool CanMoveIntoNextTube(GameObject tube)
    {
        Debug.Log("check if moveable");

        if (tube.GetComponent<Tube>().ReturnNumOpenSpots() != 0)
        {
            GameObject ball = GetTopBall();

            int numMoving = 1;
            if (!EmptyTube())
            {
                for (int i = BottomIndex(); i < transform.childCount; ++i)
                {
                    if (CheckTwo(ball, transform.GetChild(i).GetChild(0).gameObject))
                    {
                        Debug.Log("is same color");
                        numMoving++;
                    }
                    else
                    {
                        i = 4;
                    }
                }
            }

            Debug.Log("numMoving: " + numMoving);

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

        for (int i = 1; i < transform.childCount; ++i)
        {
            if (!spots[i]) { num++; }
        }

        Debug.Log("open spots: " + num);
        return num;
    }
}

