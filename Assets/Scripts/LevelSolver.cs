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
        currentState = initialLevel;
        List<List<List<int>>> statesMade = new List<List<List<int>>>();
        List<List<Vector2>> movesForEachState = new List<List<Vector2>>();
        List<int> indexForState = new List<int>();
        List<Vector2> possibleMoves = new List<Vector2>();

        while (!CheckForWin(currentState))
        {
            

            List<List<int>> t = currentState;

            statesMade.Add(t);
            possibleMoves = ReturnPossibleMoves(statesMade[statesMade.Count - 1]);
            indexForState.Add(0);
            movesForEachState.Add(possibleMoves);

            Debug.Log(statesMade.Count);
            Debug.Log(movesForEachState.Count);
            Debug.Log(indexForState.Count);

            if (movesForEachState[movesForEachState.Count - 1].Count >= indexForState[indexForState.Count - 1] + 1) 
            {
                Debug.Log("move");
                

                Debug.Log(possibleMoves.Count);
                if (possibleMoves.Count == 0) { Debug.Log("no moves"); }
                else { Debug.Log(possibleMoves[indexForState[indexForState.Count - 1]]); }

                
                currentState = MakeMove(statesMade[statesMade.Count - 1], possibleMoves[indexForState[indexForState.Count - 1]]);

                Debug.Log(statesMade.Count);
                Debug.Log(movesForEachState.Count);
                Debug.Log(indexForState.Count);
                OutputState(statesMade[statesMade.Count - 1]);
                if (CheckForWin(currentState)) { Debug.Log("found win"); }
            }
            else
            {
                Debug.Log("no moves on current state");
                statesMade.RemoveAt(statesMade.Count - 1);
                for (int i = 0; i < statesMade.Count; ++i)
                {
                    OutputState(statesMade[i]);
                }
               
                movesForEachState.RemoveAt(movesForEachState.Count - 1);
                possibleMoves = movesForEachState[movesForEachState.Count - 1];
                indexForState.RemoveAt(indexForState.Count - 1);
                indexForState[indexForState.Count - 1]++;

                Debug.Log(statesMade.Count);
                Debug.Log(movesForEachState.Count);
                Debug.Log(indexForState.Count);
                OutputState(statesMade[statesMade.Count - 1]);
            }

            currentState = statesMade[statesMade.Count - 1];
            

            if (statesMade.Count > 10)
            {
                Debug.LogError("looped");
                return;
            }
        }

        
    }

    List<List<int>> MakeMove(List<List<int>> initial, Vector2 move)
    {
        List<List<int>> final = initial;
        

        int numMoving = NumSameAtTop(final, (int)move.x);
        //Debug.Log("nummoving = " + numMoving);
        

        for (int i = 0; i < numMoving; ++i)
        {
            int bottom1 = BottomIndex(final, (int)move.x);
            int bottom2 = BottomIndex(final, (int)move.y);
            if (bottom2 != 5)
            {
                //Debug.Log(bottom2);

                final[(int)move.y][bottom2 - 1] = final[(int)move.x][bottom1];
                final[(int)move.x][bottom1] = 0;
            }
            else
            {
                final[(int)move.y][final[(int)move.y].Count - 1] = final[(int)move.x][bottom1];
                final[(int)move.x][bottom1] = 0;
            }
        }





        return final;
    }

    bool CheckForWin(List<List<int>> initial)
    {
        for (int i = 0; i < initial.Count; ++i)
        {
            if (!FullTube(initial, i) && !EmptyTube(initial, i)) { return false; }
        }
        Debug.Log("win");
        return true;
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

                    if (!EmptyTube(state, i) && !EmptyTube(state, ii))
                    {
                        //Debug.Log("num same at top: " + NumSameAtTop(i));
                        //Debug.Log("open spots: " + ReturnNumOpenSpots(ii));

                        if (state[i][BottomIndex(state, i)] == state[ii][BottomIndex(state, ii)])
                        {
                            if (NumSameAtTop(state, i) <= ReturnNumOpenSpots(state, ii))
                            {
                                Debug.Log(i + " " + ii);
                                //Debug.LogWarning("there is a move");
                                moves.Add(new Vector2(i, ii));
                            }

                        }
                    }
                    else if (EmptyTube(state, ii) && !EmptyTube(state, i))
                    {
                        Debug.Log(i + " " + ii);
                        //Debug.LogWarning("there is a move");
                        moves.Add(new Vector2(i, ii));
                    }
                }
            }
        }

        return moves;
    }

    void OutputState(List<List<int>> state)
    {
        string level = "";

        for (int i = 0; i < 4; ++i)
        {
            for (int ii = 0; ii < 14; ++ii)
            {
                level = level + state[ii][i].ToString() + " ";
                if (state[ii][i] < 10) { level += "  "; }
            }

            level += "\n";
        }

        Debug.Log(level);
    }

    int ReturnNumOpenSpots(List<List<int>> initial, int tube)
    {
        int num = 0;

        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] == 0) { num++; }
        }
        return num;
    }

    int NumSameAtTop(List<List<int>> initial, int tube)
    {
        int num = 0;
        if (initial[tube][initial[tube].Count - 1] == 0) { return 0; }

        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] != 0)
            {
                if (initial[tube][i] == initial[tube][BottomIndex(initial, tube)])
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

    int BottomIndex(List<List<int>> initial, int tube)
    {
        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] != 0)
            {
                return i;
            }
        }
        //Debug.Log("empty");
        return 5;
    }

    bool EmptyTube(List<List<int>> initial, int tube) // check if the tube is empty
    {
        if (initial[tube][initial[tube].Count - 1] == 0)
        {
            return true;
        }
        return false;
    }

    public bool FullTube(List<List<int>> initial, int tube) // check if the tube is completed
    {
        for (int i = 0; i < initial[tube].Count; ++i)
        {


            if (initial[tube][i] == 0)
            {
                return false;
            }
            else
            {
                if (initial[tube][0] != 0 && initial[tube][i] != 0)
                {
                    if (initial[tube][0] != initial[tube][i]) { return false; }
                    
                }
            }
        }
        return true;
    }
}
