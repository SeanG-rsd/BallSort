using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSolver : MonoBehaviour
{
    List<List<int>> initialLevel = new List<List<int>>();

    List<List<int>> currentState = new List<List<int>>();

    // Start is called before the first frame update
    void Start()
    {
        
        
        for (int i = 0; i < 14; ++i)
        {
            List<int> tube = new List<int>();

            for (int ii = 0; ii < 4; ++ii)
            {
                tube.Add(0);
            }

            initialLevel.Add(tube);
            currentState.Add(tube);
            
        }

        
    }

    public void InitiateLevel(List<List<int>> input)
    {
        for (int i = 0; i < 12; ++i)
        {
            for (int ii = 0; ii < 4; ++ii)
            {
                initialLevel[i][ii] = input[i][ii] + 1;
                currentState[i][ii] = input[i][ii] + 1;
            }
        }
        
        SolveLevel();
    }

    void SolveLevel()
    {

        List<Vector2> possibleMoves = ReturnPossibleMoves(currentState);

        if (possibleMoves.Count == 0) { Debug.LogWarning("there are no moves"); }

    }

    List<List<int>> MakeMove(List<List<int>> initial, Vector2 move)
    {
        List<List<int>> final = new List<List<int>>();









        return final;
    }

    List <Vector2> ReturnPossibleMoves(List<List<int>> state)
    {
        List<Vector2> moves = new List<Vector2>();

        for (int i = 0; i < state.Count; ++i)
        {
            for (int ii = 0; ii < state.Count; ++ii)
            {
                if (ii != i)
                {

                    if (!EmptyTube(i) && !EmptyTube(ii))
                    {
                        //Debug.Log("num same at top: " + NumSameAtTop(i));
                        //Debug.Log("open spots: " + ReturnNumOpenSpots(ii));

                        if (state[i][BottomIndex(i)] == state[ii][BottomIndex(ii)])
                        {
                            if (NumSameAtTop(i) <= ReturnNumOpenSpots(ii))
                            {
                                Debug.Log(i + " " + ii);
                                Debug.LogWarning("there is a move");
                                moves.Add(new Vector2(i, ii));
                            }

                        }
                    }
                    else
                    {
                        Debug.Log(i + " " + ii);
                        Debug.LogWarning("there is a move");
                        moves.Add(new Vector2(i, ii));
                    }
                }
            }
        }

        return moves;
    }

    void OutputState()
    {
        string level = "";

        for (int i = 0; i < 4; ++i)
        {
            for (int ii = 0; ii < 14; ++ii)
            {
                level = level + currentState[ii][i].ToString() + " ";
                if (currentState[ii][i] < 10) { level += "  "; }
            }

            level += "\n";
        }

        Debug.Log(level);
    }

    int ReturnNumOpenSpots(int tube)
    {
        int num = 0;

        for (int i = 0; i < currentState[tube].Count; ++i)
        {
            if (currentState[tube][i] == 0) { num++; }
        }
        return num;
    }

    int NumSameAtTop(int tube)
    {
        int num = 0;
        if (currentState[tube][currentState[tube].Count - 1] == 0) { return 0; }

        for (int i = 1; i < currentState[tube].Count; ++i)
        {
            if (currentState[tube][i] != 0)
            {
                if (currentState[tube][i] == currentState[tube][BottomIndex(tube)])
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

    int BottomIndex(int tube)
    {
        for (int i = 0; i < currentState[tube].Count; ++i)
        {
            if (currentState[tube][i] != 0)
            {
                return i;
            }
        }
        Debug.Log("empty");
        return 5;
    }

    bool EmptyTube(int tube) // check if the tube is empty
    {
        if (currentState[tube][currentState[tube].Count - 1] == 0)
        {
            return true;
        }
        return false;
    }

    public bool FullTube(int tube) // check if the tube is completed
    {
        for (int i = 0; i < currentState[tube].Count; ++i)
        {


            if (currentState[tube][i] == 0)
            {
                return false;
            }
            else
            {
                if (currentState[tube][0] != 0 && currentState[tube][i] != 0)
                {
                    if (currentState[tube][0] == currentState[tube][i]) { return true; }
                    
                }
            }
        }
        return false;
    }
}
