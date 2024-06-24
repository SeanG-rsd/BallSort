using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSolver : MonoBehaviour
{
    List<Vector2Int> lastMoves;
    List<List<int>> level;

    private void Awake()
    {
        lastMoves = new List<Vector2Int>();
    }

    public void SolveLevel(List<GameObject> tubes)
    {
        level = new List<List<int>>();
        for (int i = 0; i < tubes.Count; i++)
        {
            Tube tube = tubes[i].GetComponent<Tube>();
            level.Add(new List<int>() { tube.spots[1], tube.spots[2], tube.spots[3], tube.spots[4] });
        }

        PrintBoard();
        Debug.Log(GetPossibleMoves().Count);
        Solve();
        Debug.Log(lastMoves.Count);
        PrintBoard();
    }

    private void PrintBoard()
    {
        for (int i = 0; i < level.Count; i++)
        {
            Debug.Log(string.Join(',', level[i]));
        }
    }

    int iterator = 0;

    private void Solve()
    {
        while (!CheckForWin() && iterator < 1000)
        {
            List<Vector2Int> possibleMoves = GetPossibleMoves();

            foreach (Vector2Int move in possibleMoves)
            {
                if (!lastMoves.Contains(move))
                {
                    MakeMove(move);
                    lastMoves.Add(move);
                    Solve();
                    UnmakeMove();
                    iterator++;
                }
            }
        }
    }

    private List<Vector2Int> GetPossibleMoves()
    {
        List<Vector2Int> output = new List<Vector2Int>();

        for (int f = 0; f < level.Count; f++)
        {
            for (int t = 0; t < level.Count; t++)
            {
                if (f != t)
                {
                    if (IsEmpty(level[t]) && !IsEmpty(level[f]))
                    {
                        if (GetNumberSameAtTop(level[f]) < 4 - BottomIndex(level[f]))
                        {
                            output.Add(new Vector2Int(f, t));
                        }
                    }
                    else if (!IsFull(level[t]) && !IsEmpty(level[f]))
                    {
                        if (GetTopBall(level[f]) == GetTopBall(level[t]))
                        {                             
                            if (GetNumberSameAtTop(level[f]) <= GetNumberOfOpenSpots(level[t]))
                            {
                                output.Add(new Vector2Int(f, t));
                            }
                        }
                    }
                }
            }
        }

        return output;
    }

    private void MakeMove(Vector2Int fromTo)
    {
        int bottomIndexFrom = BottomIndex(level[fromTo.x]);
        int bottomIndexTo = FirstOpenSpot(level[fromTo.y]);

        int numberMoving = GetNumberSameAtTop(level[fromTo.x]);
        int ball = level[fromTo.x][bottomIndexFrom];

        Debug.Log(fromTo.ToString());

        for (int i = 0; i < numberMoving; i++)
        {
            level[fromTo.x][bottomIndexFrom + i] = 0;
            level[fromTo.y][bottomIndexTo - i] = ball;
            if (bottomIndexTo - i == 0)
            {
                break;
            }
        }
    }

    private void UnmakeMove()
    {
        Vector2Int lastMove = lastMoves[lastMoves.Count - 1];
        MakeMove(new Vector2Int(lastMove.y, lastMove.x));
        lastMoves.RemoveAt(lastMoves.Count - 1);
    }

    private bool CheckForWin()
    {
        for (int i = 0; i < level.Count; i++)
        {
            if (!IsFull(level[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsFull(List<int> tube)
    {
        int same = tube[0];
        for (int i = 1;  i < tube.Count; i++)
        {
            if (tube[i] != same && tube[i] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsEmpty(List<int> tube)
    {
        for (int i = 0; i < tube.Count; i++)
        {
            if (tube[i] != 0)
            {
                return false;
            }
        }

        return true;
    }

    private int GetTopBall(List<int> tube)
    {
        return tube[BottomIndex(tube)];
    }

    private int GetNumberSameAtTop(List<int> tube)
    {
        int num = 0;
        if (!IsEmpty(tube))
        {
            for (int i = BottomIndex(tube); i < 4; ++i)
            {
                if (tube[i] != 0)
                {
                    if (tube[i] != GetTopBall(tube)) { return num; }
                    if (tube[i] == GetTopBall(tube))
                    {
                        num++;
                    }
                }
            }
        }
        return num;
    }

    private int GetNumberOfOpenSpots(List<int> spots)
    {
        int num = 0;

        for (int i = 0; i < spots.Count; ++i)
        {
            if (spots[i] == 0) { num++; }
            else break;
        }

        return num;
    }

    public int BottomIndex(List<int> spots) // returns the index of the last ball
    {
        for (int i = 0; i < spots.Count; ++i)                          // 1
        {                                                             //  3      returns 1
            if (spots[i] != 0)                                       //   3
            {                                                       //    3
                return i;
            }
        }

        return 3;
    }

    private int FirstOpenSpot(List<int> spots)
    {
        for (int i = spots.Count - 1; i >= 0; i--)
        {
            if (spots[i] == 0)
            {
                return i;
            }
        }

        return 0;
    }
}