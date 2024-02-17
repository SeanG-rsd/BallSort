using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public bool move = false;
    
    public float speed = 1500.0f; // time it takes to get there

    private List<Vector2> currentTargets;

    // Start is called before the first frame update

    private void Awake()
    {
        currentTargets = new List<Vector2>();
    }
    private void Update()
    {
        if (currentTargets.Count > 0)
        {
            Vector2 currentTarget = currentTargets[0];

            Vector2 ballPos = transform.position;

            if (ballPos == currentTarget)
            {
                currentTargets.RemoveAt(0);
            }
            else
            {
                transform.position = currentTarget;
            }
        }
    }

    public void MoveBall(List<Vector2> targetSpots)
    {
        currentTargets = targetSpots;
        move = true;
    }

    /*public void MoveBall(int tubeNum, GameObject targetTube, int spotIndex) // setting the balls destination and figuring out the target spot where the ball is to be placed
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
                        /*gameManager.GetComponent<GameManager>().Cork();
                        if (gameManager.GetComponent<GameManager>().CheckForWin())
                        {
                            gameManager.GetComponent<GameManager>().Win();
                        }
                    }
                    else if (tutorial)
                    {
                        //tutorialManager.GetComponent<TutorialManager>().Cork();
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
    }*/
}
