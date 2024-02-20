using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private bool move = false;

    [SerializeField] private float speed;

    private List<Vector2> currentTargets;

    private int position;
    private Tube tube;

    // Start is called before the first frame update

    public static Action<GameObject, int, Tube> OnMadeItHome = delegate { };

    private void Awake()
    {
        currentTargets = new List<Vector2>();
    }

    private Vector3 currentDirection;
    private Vector3 currentTarget;

    private void Update()
    {
        if (currentTargets.Count > 0)
        {
            if (currentTarget == Vector3.zero)
            {
                currentTarget = currentTargets[0];
                currentDirection = currentTarget - transform.position;
            }

            Vector3 ballPos = transform.position;

            if (ballPos == currentTarget)
            {
                currentTargets.RemoveAt(0);
                currentTarget = Vector3.zero;
                

                if (currentTargets.Count == 0)
                {
                    move = false;
                    OnMadeItHome?.Invoke(gameObject, position, tube);
                }
            }
            else
            {
                transform.localPosition += speed * Time.deltaTime * currentDirection.normalized;
                
                if (currentDirection.y > 0 && (currentTarget - transform.position).y < 0)
                {
                    transform.position = currentTarget;
                }
                else if (currentDirection.y < 0 && (currentTarget - transform.position).y > 0)
                {
                    transform.position = currentTarget;
                }
                else if (currentDirection.x > 0 && (currentTarget - transform.position).x < 0)
                {
                    transform.position = currentTarget;
                }
                else if (currentDirection.x < 0 && (currentTarget - transform.position).x > 0)
                {
                    transform.position = currentTarget;
                }
            }
        }
    }

    public void MoveBall(List<Vector2> targetSpots, int position, Tube homeTube)
    {
        Debug.Log("move ball");
        currentTargets = targetSpots;
        move = true;
        this.position = position;
        tube = homeTube;
    }

    public bool IsMoving()
    {
        return move;
    }
}
