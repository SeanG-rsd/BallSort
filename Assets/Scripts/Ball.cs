using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    bool move = false;
    
    public float speed = 500.0f; // time it takes to get there


    LineRenderer line = new LineRenderer();
    Vector3 initialPos = new Vector3();
    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            move = true;
        }

        if (move)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, (new Vector3(0, 0, 0) - transform.parent.localPosition), speed * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, transform.parent.localPosition) < 1.0f)
            {
                move = false;
                Debug.Log("made it");
            }
        }

        /*if (move && index < line.positionCount)
        {
            float diffX = line.GetPosition(index).x - line.GetPosition(index + 1).x;
            float diffY = line.GetPosition(index).y - line.GetPosition(index + 1).y;


            Vector3 pos = transform.localPosition;

            pos.x -= diffX * Time.deltaTime * speed;
            pos.y -= diffY * Time.deltaTime * speed;

            transform.localPosition = pos;
            Debug.Log(line.GetPosition(index) + " " + transform.parent.localPosition + " " + initialPos);
            float distance = Vector3.Distance(transform.localPosition, transform.parent.localPosition - line.GetPosition(index) + initialPos);
            Debug.Log(distance);
            if (distance < 1.5f)
            {
                transform.localPosition = transform.parent.localPosition - line.GetPosition(index) + initialPos;
                Debug.Log("made it");
                index++;
                move = false;
                
                if (index < line.positionCount - 1)
                {
                    Debug.LogWarning("move again");
                    move = true;
                }
                else
                {
                    Destroy(line.gameObject);
                }
            }
        }*/
    }

    public void MoveToPoint(LineRenderer newLine)
    {
        initialPos = transform.localPosition;
        if (!move) { line = newLine; }
        //move = true;
        index = 0;
    }

    public void MoveToOther(Vector3 moveTo)
    {
        //topPoint = moveTo;
        //initialPos = transform.localPosition;
        //move = true;
        //other = true;
    }
}
