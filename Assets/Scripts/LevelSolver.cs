using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSolver : MonoBehaviour
{
    List<List<int>> initialLevel = new List<List<int>>();

    public TMP_Text winText;
    public Button showButton;

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
            //currentState.Add(tube);

        }


    }

    public void InitiateLevel(List<List<int>> input)
    {
        for (int i = 0; i < input.Count; ++i)
        {
            for (int ii = 0; ii < input[i].Count; ++ii)
            {
                initialLevel[i][ii] = input[i][ii];

            }
        }

        // Test to see if "Board Equal Function" works

        /*List<List<int>> trivial = new List<List<int>>(initialLevel);

        for (int i = 0; i < trivial.Count; ++i)
        {
            trivial[i] = new List<int>(initialLevel[i]);
        }

        int slot = 0;
        initialLevel[0][slot++] = 1;
        initialLevel[0][slot++] = 4;
        initialLevel[0][slot++] = 4;
        initialLevel[0][slot++] = 2;

        slot = 0;
        initialLevel[1][slot++] = 2;
        initialLevel[1][slot++] = 3;
        initialLevel[1][slot++] = 1;
        initialLevel[1][slot++] = 3;

        slot = 0;
        initialLevel[2][slot++] = 1;
        initialLevel[2][slot++] = 2;
        initialLevel[2][slot++] = 3;
        initialLevel[2][slot++] = 4;

        slot = 0;
        initialLevel[3][slot++] = 1;
        initialLevel[3][slot++] = 3;
        initialLevel[3][slot++] = 2;
        initialLevel[3][slot++] = 4;

        slot = 0;
        trivial[0][slot++] = 2;
        trivial[0][slot++] = 3;
        trivial[0][slot++] = 1;
        trivial[0][slot++] = 3;

        slot = 0;
        trivial[1][slot++] = 1;
        trivial[1][slot++] = 4;
        trivial[1][slot++] = 4;
        trivial[1][slot++] = 2;

        slot = 0;
        trivial[2][slot++] = 1;
        trivial[2][slot++] = 3;
        trivial[2][slot++] = 2;
        trivial[2][slot++] = 4;

        slot = 0;
        trivial[3][slot++] = 1;
        trivial[3][slot++] = 2;
        trivial[3][slot++] = 3;
        trivial[3][slot++] = 4;

        Debug.Log(CheckIfEqualStates(trivial, initialLevel));*/

        SolveLevel();

    }


    private void SolveLevel()
    {
        List<List<int>> currentState = new List<List<int>>(initialLevel);

        for (int i = 0; i < currentState.Count; ++i)
        {
            currentState[i] = new List<int>(initialLevel[i]);
        }


        List<List<List<int>>> statesMade = new List<List<List<int>>>();
        List<List<Vector2>> movesForEachState = new List<List<Vector2>>();
        List<int> indexForState = new List<int>();
        List<Vector2> possibleMoves = new List<Vector2>();

        List<List<List<int>>> statesVisited = new List<List<List<int>>>();

        int iteration = 0;
        bool same = false;

        bool stepBack = false;
        bool dontStepBack = false;

        while (!CheckForWin(currentState))
        {



            List<List<int>> add = new List<List<int>>(currentState);

            for (int i = 0; i < add.Count; ++i)
            {
                add[i] = new List<int>(currentState[i]);

            }

            // attempt at statesVisited list

            /*for (int i = 0; i < statesVisited.Count; ++i)
            {
                if (CheckIfEqualStates(statesVisited[i], currentState) && !stepBack)
                {
                    for (int j = 0; j < statesMade.Count; ++j)
                    {
                        if (CheckIfEqualStates(statesMade[j], currentState))
                        {
                            Debug.Log("same");
                            OutputState(statesMade[j]);
                            OutputState(currentState);

                            same = true;
                            dontStepBack = true;

                            break;
                        }

                        if (same) { break; }
                    }
                }
            }*/

            


            if (!same && !stepBack)
            {
                statesMade.Add(add);
                statesVisited.Add(add);

                possibleMoves = ReturnPossibleMoves(statesMade[statesMade.Count - 1]);
                indexForState.Add(0);
                movesForEachState.Add(new List<Vector2>(possibleMoves));


            }

            if (movesForEachState[movesForEachState.Count - 1].Count >= indexForState[indexForState.Count - 1] + 1 && !same)
            {
                Debug.Log("move");
                Debug.Log(possibleMoves[indexForState[indexForState.Count - 1]]);
                Debug.Log(possibleMoves.Count);

                MakeMove(currentState, possibleMoves[indexForState[indexForState.Count - 1]]);
                OutputState(currentState);


                // checks if this board is creating a loop in the statesMade

                for (int j = 0; j < statesMade.Count; ++j)
                {
                    if (CheckIfEqualStates(statesMade[j], currentState))
                    {
                        Debug.Log("same");
                        OutputState(statesMade[j]);
                        OutputState(currentState);

                        same = true;
                        dontStepBack = true;

                        break;
                    }


                }


                stepBack = false;




                if (CheckForWin(currentState))
                {
                    int other = 0;
                    Debug.LogError("found win");
                    Debug.Log(iteration);
                    for (int i = 0; i < statesMade.Count; ++i)
                    {
                        //Debug.Log(movesForEachState[i][indexForState[i]]);
                        winText.text = winText.text + movesForEachState[i][indexForState[i]];
                        if (other != 2)
                        {
                            winText.text = winText.text + "  ";
                            other++;
                        }
                        else
                        {
                            winText.text = winText.text + "\n";
                            other = 0;
                        }
                        //StopCoroutine(SolveLevel());
                    }
                    showButton.interactable = true;

                }
            }
            else
            {
                Debug.Log("no moves on current state");
                Debug.Log(movesForEachState[movesForEachState.Count - 1].Count + "   " + indexForState[indexForState.Count - 1]);

                statesMade.RemoveAt(statesMade.Count - 1);



                movesForEachState.RemoveAt(movesForEachState.Count - 1);
                possibleMoves = movesForEachState[movesForEachState.Count - 1];
                indexForState.RemoveAt(indexForState.Count - 1);
                indexForState[indexForState.Count - 1]++;
                Debug.Log(movesForEachState[movesForEachState.Count - 1].Count + "   " + indexForState[indexForState.Count - 1]);


                if (movesForEachState[0].Count < indexForState[0] + 1)
                {
                    Debug.Log("no win");
                    Debug.Log(iteration);
                    Debug.Log(movesForEachState[0].Count + "   " + indexForState[0]);

                }

                for (int i = 0; i < currentState.Count; ++i)
                {
                    currentState[i] = new List<int>(statesMade[statesMade.Count - 1][i]);
                }

                same = false;
                if (dontStepBack)
                {
                    stepBack = false;
                    dontStepBack = false;
                }
                else
                {
                    stepBack = true;
                }
                Debug.Log("removed last index.");
                OutputState(currentState);

                
            }

            Debug.LogWarning(statesMade.Count);
            Debug.Log(iteration);
            iteration++;

            if (iteration > 10000)
            {
                Debug.LogError(movesForEachState[0].Count + "   " + indexForState[0]);
                Debug.LogError("no solution");


                break;
            }

            //yield return null;


        }



        bool CheckIfEqualStates(List<List<int>> first, List<List<int>> second)
        {

            List<int> sameOnFirst = new List<int>();
            List<int> sameOnSecond = new List<int>();

            bool same = false;


            for (int i = 0; i < first.Count; ++i)
            {
                for (int j = 0; j < second.Count; ++j)
                {
                    if (!sameOnFirst.Contains(i) && !sameOnSecond.Contains(j))
                    {
                        for (int ii = 0; ii < first[i].Count; ++ii)
                        {
                            if (first[i][ii] != second[j][ii])
                            {
                                same = false;
                                break;
                            }

                            same = true;

                        }
                        if (same)
                        {
                            sameOnFirst.Add(i);
                            sameOnSecond.Add(j);
                        }
                    }
                }
            }

            if (sameOnFirst.Count < 14)
            {
                return false;
            }

            return true;

            /*for (int i = 0; i < first.Count; ++i)
            {
                for (int ii = 0; ii < first[i].Count; ++ii)
                {
                    if (first[i][ii] != second[i][ii])
                    {
                        return false;

                    }



                }
            }

            return true;*/
        }

        void MakeMove(List<List<int>> initial, Vector2 move)
        {



            int numMoving = NumSameAtTop(initial, (int)move.x);



            for (int i = 0; i < numMoving; ++i)
            {
                int bottom1 = BottomIndex(initial, (int)move.x);
                int bottom2 = BottomIndex(initial, (int)move.y);
                if (bottom2 != 5)
                {


                    initial[(int)move.y][bottom2 - 1] = initial[(int)move.x][bottom1];
                    initial[(int)move.x][bottom1] = 0;
                }
                else
                {
                    initial[(int)move.y][initial[(int)move.y].Count - 1] = initial[(int)move.x][bottom1];
                    initial[(int)move.x][bottom1] = 0;
                }
            }


        }

        bool CheckForWin(List<List<int>> initial)
        {
            for (int i = 0; i < initial.Count; ++i)
            {
                if (!FullTube(initial, i) && !EmptyTube(initial, i)) { return false; }
            }
            //Debug.Log("win");
            return true;
        }

        List<Vector2> ReturnPossibleMoves(List<List<int>> state)
        {
            List<Vector2> moves = new List<Vector2>();
            bool oneEmpty = false;
            int emptyIndex = 0;

            for (int i = 0; i < state.Count; ++i)
            {
                for (int ii = 0; ii < state.Count; ++ii)
                {
                    if (ii != i)
                    {
                        if (!FullTube(state, i))
                        {
                            if (!EmptyTube(state, i) && !EmptyTube(state, ii))
                            {
                                //Debug.Log("num same at top: " + NumSameAtTop(i));
                                //Debug.Log("open spots: " + ReturnNumOpenSpots(ii));

                                if (state[i][BottomIndex(state, i)] == state[ii][BottomIndex(state, ii)])
                                {
                                    if (NumSameAtTop(state, i) <= ReturnNumOpenSpots(state, ii))
                                    {
                                        //Debug.Log(i + " " + ii);
                                        //Debug.LogWarning("there is a move");
                                        moves.Add(new Vector2(i, ii));
                                    }

                                }
                            }
                            else if (!EmptyTube(state, i) && EmptyTube(state, ii))
                            {
                                if (NumSameAtTop(state, i) != NumBallInTube(state, i))
                                {
                                    //Debug.Log(NumSameAtTop(state, i) + "   " + NumBallInTube(state, i));

                                    if (!oneEmpty)
                                    {
                                        emptyIndex = ii;
                                        oneEmpty = true;
                                    }

                                    if (ii == emptyIndex)
                                    {
                                        //Debug.Log(i + " " + ii);
                                        //Debug.LogWarning("there is a move");
                                        oneEmpty = true;
                                        moves.Add(new Vector2(i, ii));
                                    }
                                }
                            }
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

        int NumBallInTube(List<List<int>> initial, int tube)
        {
            int num = 0;

            for (int i = 0; i < initial[tube].Count; ++i)
            {
                if (initial[tube][i] != 0) { num++; }
            }

            return num;
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

        bool FullTube(List<List<int>> initial, int tube) // check if the tube is completed
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
}
