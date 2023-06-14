using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Ball : MonoBehaviour
{

    bool move = false;
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

    GameObject background;

    public int destinationSpot;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        tutorialManager = GameObject.Find("Manager");

        background = transform.parent.parent.parent.gameObject;

    }

    public void MoveBall(int tubeNum, GameObject targetTube, int spotIndex)
    {
        
        index = tubeNum;
        move = true;
        destinationSpot = spotIndex;
        if (targetTube.GetComponent<Tube>() != null)
        {
            tutorial = targetTube.GetComponent<Tube>().TutorialTube;
            targetTube.GetComponent<Tube>().IncomingBall(gameObject);
        }
        targetSpot = targetTube.transform.GetChild(spotIndex).gameObject;
        
        targetPoint = FindPoint();
    }

    Vector3 FindPoint()
    {
        GameObject extraTop = transform.parent.parent.GetChild(0).gameObject;
        topPoint = extraTop.transform.parent.localPosition + extraTop.transform.localPosition;

        if (targetSpot.transform.parent == transform.parent.parent)
        {
            //Debug.Log("true");
            targetPoint = targetSpot.transform.localPosition - transform.parent.localPosition;
        }
        else
        {
            thisIndex = transform.parent.parent.GetSiblingIndex();
            
            
            transform.SetParent(transform.parent.parent.parent);

            //transform.localScale = Vector3.one;
            //Debug.Log(transform.localPosition);
            if (Vector3.Distance(transform.localPosition, topPoint) > 15.0f)
            {
                hasBeenAtTop = false;
            }
            else
            {
                hasBeenAtTop = true;
            }

            targetPoint = targetSpot.transform.parent.localPosition + targetSpot.transform.localPosition;
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

    void SetSecondPoint()
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
            secondPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y;
            secondPoint.z = targetPoint.z;

            topPoint.x = transform.localPosition.x;

            topPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y;
            topPoint.z = targetPoint.z;

            //Debug.LogWarning("b to t");

            hasBeenAtTop = false;
        }
        else if (index == tinyTubeIndex)
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y;
            secondPoint.z = targetPoint.z;

            topPoint.x = transform.localPosition.x;

            topPoint.y = targetSpot.transform.parent.GetChild(0).localPosition.y + targetSpot.transform.parent.localPosition.y;
            topPoint.z = targetPoint.z;

            //Debug.LogWarning("b to t");

            hasBeenAtTop = false;
        }
        else
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = transform.localPosition.y;
            secondPoint.z = targetPoint.z;
        }

        
    }

    void SetTutorialPoint()
    {
        secondPoint.x = targetPoint.x;
        secondPoint.y = transform.localPosition.y;
        secondPoint.z = targetPoint.z;

        topPoint.x = transform.localPosition.x;

        topPoint.y = (targetSpot.transform.parent.GetChild(0).localPosition.y * targetSpot.transform.parent.localScale.x) + targetSpot.transform.parent.localPosition.y;
        topPoint.z = targetPoint.z;
    }
    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            if (!otherTube)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, targetPoint) < 1.0f)
                {
                    //Debug.Log("close");
                    transform.SetParent(targetSpot.transform);
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;

                    if (!tutorial)
                    {
                        if (gameManager.GetComponent<GameManager>().CheckForWin())
                        {
                            gameManager.GetComponent<GameManager>().Win();
                        }
                    }
                    else if (tutorial)
                    {
                        if (tutorialManager.GetComponent<TutorialManager>().CheckForWin())
                        {
                            tutorialManager.GetComponent<TutorialManager>().Win();
                        }
                    }
                    gameManager.GetComponent<GameManager>().Cork();
                    move = false;
                }
            }
            else if (hasBeenAtTop)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, secondPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, secondPoint) < 0.01f)
                {
                    //Debug.Log("close to other");
                    otherTube = false;
                }
            }
            else
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
