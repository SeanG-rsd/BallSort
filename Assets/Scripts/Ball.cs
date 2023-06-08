using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Ball : MonoBehaviour
{

    bool move = false;
    bool otherTube = false;
    
    public float speed = 1200.0f; // time it takes to get there

    bool hasBeenAtTop = false;

    LineRenderer line = new LineRenderer();
    Vector3 targetPoint = new Vector3();
    Vector3 secondPoint = new Vector3();
    Vector3 topPoint = new Vector3();
    public int index = 0;

    public GameObject targetSpot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void MoveBall(int tubeNum, GameObject targetTube, int spotIndex)
    {
        index = tubeNum;
        move = true;
        targetSpot = targetTube.transform.GetChild(spotIndex).gameObject;

        targetPoint = FindPoint();
    }

    Vector3 FindPoint()
    {
        GameObject extraTop = transform.parent.parent.GetChild(0).gameObject;
        topPoint = extraTop.transform.parent.localPosition + extraTop.transform.localPosition;

        if (targetSpot.transform.parent == transform.parent.parent)
        {
            Debug.Log("true");
            targetPoint = targetSpot.transform.localPosition - transform.parent.localPosition;
        }
        else
        {
            
            
            transform.SetParent(transform.parent.parent.parent);

            transform.localScale = Vector3.one;
            Debug.Log(transform.localPosition);
            if (Vector3.Distance(transform.localPosition, topPoint) > 15.0f)
            {
                hasBeenAtTop = false;
                Debug.Log("extra ball");

            }
            else
            {
                hasBeenAtTop = true;
            }

            targetPoint = targetSpot.transform.parent.localPosition + targetSpot.transform.localPosition;
            SetSecondPoint();
            Debug.Log("different tube");

            Debug.Log(targetPoint);
            Debug.Log(secondPoint);
            Debug.Log(topPoint);

            otherTube = true;
        }

        return targetPoint;
    }

    void SetSecondPoint()
    {
        if (index > 7)
        {
            secondPoint.x = targetPoint.x;
            secondPoint.y = transform.localPosition.y;
            secondPoint.z = targetPoint.z;
        }
        else
        {
            secondPoint.x = transform.localPosition.x;
            secondPoint.y = targetPoint.y;
            secondPoint.z = targetPoint.z;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetPoint = FindPoint();
            move = true;
        }

        if (move)
        {
            if (!otherTube)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, targetPoint) < 1.0f)
                {
                    Debug.Log("close");
                    transform.SetParent(targetSpot.transform);
                    transform.localPosition = Vector3.zero;
                    transform.parent.parent.gameObject.GetComponent<Tube>().UpdateSpots();
                    move = false;
                }
            }
            else if (hasBeenAtTop)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, secondPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, secondPoint) < 0.01f)
                {
                    Debug.Log("close to other");
                    otherTube = false;
                }
            }
            else
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, topPoint, speed * Time.deltaTime);

                if (Vector3.Distance(transform.localPosition, topPoint) < 0.01f)
                {
                    Debug.Log("reached top");
                    SetSecondPoint();
                    hasBeenAtTop = true;
                }
            }
        }
    }
}
