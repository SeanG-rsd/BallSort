using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public bool move = false;
    bool otherTube = false;
    
    public float speed = 1500.0f; // time it takes to get there

    bool hasBeenAtTop = false;

    private int tinyTubeIndex = -1;
    Vector3 targetPoint = new Vector3();
    Vector3 secondPoint = new Vector3();
    Vector3 topPoint = new();
    int index = 0;

    public GameObject targetSpot;

    int thisIndex = 0;

    GameObject gameManager;

    GameObject tutorialManager;
    bool tutorial;

    GameObject lastTube;

    public int destinationSpot;

    private Transform moveSpace;
    private Transform tubeSpace;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        tutorialManager = GameObject.Find("Manager");
        tubeSpace = gameManager.GetComponent<GameManager>().tubeContainer;

        moveSpace = transform.parent.parent.parent.parent;
    }

    public void MoveBall(int tubeNum, GameObject targetTube, int spotIndex) // setting the balls destination and figuring out the target spot where the ball is to be placed
    {
        if (!move)
        {
            lastTube = transform.parent.parent.gameObject;
        }
        else
        {
            lastTube = targetSpot.transform.parent.gameObject;
        }

        index = tubeNum;
        move = true;
        destinationSpot = spotIndex;
        if (targetTube.GetComponent<Tube>() != null)
        {
            tutorial = targetTube.GetComponent<Tube>().TutorialTube;      
        }
        targetSpot = targetTube.transform.GetChild(spotIndex).gameObject;
        
        targetPoint = FindPoint();
    }

    Vector3 FindPoint() // finds the point of the target spot relative to the tubecontainer object
    {
        GameObject extraTop = lastTube.transform.GetChild(0).gameObject;
        topPoint = extraTop.transform.parent.localPosition + extraTop.transform.localPosition + tubeSpace.localPosition;

        if (targetSpot.transform.parent == transform.parent.parent) // if the ball is moving within its own tube
        {
            targetPoint = targetSpot.transform.localPosition - transform.parent.localPosition;
        }
        else // if the ball is moving to a different tube
        {
            thisIndex = transform.parent.parent.GetSiblingIndex();
            
            
            transform.SetParent(moveSpace);
            if (Vector3.Distance(transform.localPosition, topPoint) > 15.0f) // checks if the ball needs to go out the tube before moving the the second point
            {
                Debug.Log(transform.position);
                hasBeenAtTop = false;
            }
            else
            {
                hasBeenAtTop = true;
            }

            targetPoint = targetSpot.transform.parent.localPosition + targetSpot.transform.localPosition + tubeSpace.localPosition; // the target spot point
            if (!tutorial)
            {
                SetSecondPoint();
            }
            else
            {
                SetTutorialPoint();
            }
            
            otherTube = true;
        }

        return targetPoint;
    }

    void SetSecondPoint() // finding the top of the second tube, either traveling vertically first if the tube is on a different row or traveling horizontally if the target tube is on the same row
    {
        if (index > 7 && thisIndex < 8 && index != -2)
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = transform.localPosition.y;
            secondPoint.z = targetPoint.z;
            
        }
        else if (index < 8 && thisIndex > 7 && index != -2)
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y + tubeSpace.localPosition.y;
            secondPoint.z = targetPoint.z;

            topPoint.x = transform.localPosition.x;

            topPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y + tubeSpace.localPosition.y;
            topPoint.z = targetPoint.z;

            hasBeenAtTop = false;
        }
        else if (index == tinyTubeIndex)
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y + tubeSpace.localPosition.y;
            secondPoint.z = targetPoint.z;

            topPoint.x = transform.localPosition.x;

            topPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y + tubeSpace.localPosition.y;
            topPoint.z = targetPoint.z;

            hasBeenAtTop = false;
        }
        else
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = transform.localPosition.y;
            secondPoint.z = targetPoint.z;
        }     
    }

    void SetTutorialPoint() // this is for the tutorial
    {
        secondPoint.x = targetPoint.x;
        secondPoint.y = transform.localPosition.y;
        secondPoint.z = targetPoint.z;

        topPoint.x = transform.localPosition.x;

        topPoint.y = (targetSpot.transform.parent.GetChild(0).localPosition.y * targetSpot.transform.parent.localScale.x) + targetSpot.transform.parent.localPosition.y;
        topPoint.z = targetPoint.z;
    }
    void Update()
    {
        if (move)
        {
            if (!otherTube) // last
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, targetPoint) < 1.0f)
                {
                    move = false;
                    transform.SetParent(targetSpot.transform);
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;

                    if (!tutorial)
                    {
                        gameManager.GetComponent<GameManager>().Cork();
                        if (gameManager.GetComponent<GameManager>().CheckForWin())
                        {
                            gameManager.GetComponent<GameManager>().Win();
                        }
                    }
                    else if (tutorial)
                    {
                        tutorialManager.GetComponent<TutorialManager>().Cork();
                    }         
                }
            }
            else if (hasBeenAtTop) // second
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, secondPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, secondPoint) < 0.01f)
                {
                    otherTube = false;
                }
            }
            else // first
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, topPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, topPoint) < 0.01f)
                {
                    
                    if (!tutorial)
                    {
                        SetSecondPoint();
                    }
                    else
                    {
                        SetTutorialPoint();
                    }
                    hasBeenAtTop = true;                  
                }
            }
        }   
    }
}
