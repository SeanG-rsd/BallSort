using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LevelSolver : MonoBehaviour
{
    private int InvalidIndex = 5;

    List<List<int>> initialLevel = new List<List<int>>();

    public TMP_Text winText;
    public Button showButton;

    public TextAsset solveCheck;

    int levelIndex;
    string path = "Assets/Resources/SolveCheck.txt";
    string original = "";
    bool solvable = true;

    public HintFlash hintFlash;

    public GameObject resetFlash;
    public GameObject tinyTubeFlash;

    public Popup noSolution;
    void Start() // initializes the original board
    {
        for (int i = 0; i < 14; ++i)
        {
            List<int> tube = new List<int>();

            for (int ii = 0; ii < 4; ++ii)
            {
                tube.Add(0);
            }

            initialLevel.Add(tube);
        }
    }

    public bool InitiateLevel(List<List<int>> input, int index) // takes the input and creates a copy then starts to solve it
    {
        levelIndex = index;
        initialLevel = CopyBoard(input);
        StartCoroutine(SolveLevel());
        
        Debug.LogError("finish");
        return solvable;
    }

    public string GetLevels() // open the text file containing the string of levels
    {
        return solveCheck.text;
    }
    private IEnumerator SolveLevel() // main solve function
    {
        List<List<int>> currentState = new List<List<int>>(initialLevel);

        // Create a copy of InitialLevel
        for (int i = 0; i < currentState.Count; ++i)
        {
            currentState[i] = new List<int>(initialLevel[i]);
        }

        // Initialize all lists for use
        List<List<List<int>>> statesMade = new List<List<List<int>>>();
        List<List<Vector2>> movesForEachState = new List<List<Vector2>>();
        List<int> indexForState = new List<int>();
        List<Vector2> possibleMoves = new List<Vector2>();

        List<List<List<int>>> statesVisited = new List<List<List<int>>>();

        // the iteration count so the solver doesn't go on for too long
        int iteration = 0;

        // all bools help move to the next branch and not go into a loop
        bool same = false;

        bool stepBack = false;

        while (!CheckForWin(currentState))
        {
            
            List<List<int>> add = CopyBoard(currentState); // Create a copy of CurrentState

            // Add a new state to all lists
            if (!same && !stepBack)
            {
                statesMade.Add(add);
                statesVisited.Add(add);

                possibleMoves = ReturnPossibleMoves(statesMade[statesMade.Count - 1]);
                indexForState.Add(0);
                movesForEachState.Add(new List<Vector2>(possibleMoves));
            }

            // If the number of moves in the last state is greater than the index of the last state then create a new state
            if (movesForEachState[movesForEachState.Count - 1].Count >= indexForState[indexForState.Count - 1] + 1)
            {
                // make a move on currentState based on the index of its possible moves and edit current state
                MakeMove(currentState, possibleMoves[indexForState[indexForState.Count - 1]]);

                // checks if this board is creating a loop in the statesMade
                // same portion
                for (int j = 0; j < statesVisited.Count; ++j)
                {
                    if (CheckIfEqualStates(statesVisited[j], currentState))
                    {
                        indexForState[indexForState.Count - 1]++;
                        currentState = CopyBoard(statesMade[statesMade.Count - 1]);
                        same = true;

                        break;
                    }
                    else
                    {
                        same = false;
                    }
                }

                stepBack = false;

                // check if the new state made is winning
                if (CheckForWin(currentState))
                {
                    int other = 0;
                    Debug.LogError("found win");
                    tinyTubeFlash.SetActive(false);
                    Debug.Log(movesForEachState[0][indexForState[0]]);
                    gameObject.GetComponent<LevelCreator>().hintTubes = movesForEachState[0][indexForState[0]];
                    
                    hintFlash.tubes = movesForEachState[0][indexForState[0]];
                    hintFlash.index = 0;

                    Debug.Log(iteration);
                    for (int i = 0; i < statesMade.Count; ++i)
                    {
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
                    }
                    showButton.interactable = true;

                    solvable = true;
                    gameObject.GetComponent<LevelCreator>().challengeSolvability.Add(true);
                    
                    original = "Level " + (levelIndex + 1).ToString() + ": Solution in " + (statesMade.Count + 1).ToString() + " Moves and Iteration = " + iteration.ToString() + "\n";
                    yield return null;
                    
                }
            }
            else
            {
                // if there are no moves then remove the last item in each list and get the new moves for the new last state
                statesMade.RemoveAt(statesMade.Count - 1);
                movesForEachState.RemoveAt(movesForEachState.Count - 1);            
                indexForState.RemoveAt(indexForState.Count - 1);

                // if the new index for the first state is greater than the number of moves, there is no win
                if (statesMade.Count == 0)
                {
                    Debug.LogError("no win");
                    Debug.Log(iteration);
                    solvable = false;

                    if (gameObject.GetComponent<GameManager>().TinyTube.activeSelf && !gameObject.GetComponent<GameManager>().TinyTube.GetComponent<TinyTube>().FullTube())
                    {
                        tinyTubeFlash.SetActive(true);
                        Debug.Log("tinytube flash");
                    }
                    else
                    {
                        resetFlash.SetActive(true);
                        noSolution.Activate(5f);
                        gameObject.GetComponent<LevelCreator>().lookingForHint = false;
                    }

                    yield break;
                }
                else if (movesForEachState[0].Count < indexForState[0] + 1)
                {
                    Debug.LogError("no win");
                    Debug.Log(iteration);
                    solvable = false;

                    if (gameObject.GetComponent<GameManager>().TinyTube.activeSelf && !gameObject.GetComponent<GameManager>().TinyTube.GetComponent<TinyTube>().FullTube())
                    {
                        tinyTubeFlash.SetActive(true);
                    }
                    else
                    {
                        resetFlash.SetActive(true);
                        noSolution.Activate(5f);
                        gameObject.GetComponent<LevelCreator>().lookingForHint = false;
                    }

                    yield break;
                    
                }

                possibleMoves = movesForEachState[movesForEachState.Count - 1];
                indexForState[indexForState.Count - 1]++;

                currentState = CopyBoard(statesMade[statesMade.Count - 1]);
                // Create a copy of current state

                // step backwards if the last "same" portion was not triggered ^^ and do not step backwards if it was triggered
                same = false;
                stepBack = true;
            }
            iteration++;

            if (iteration > 10000)
            {
                Debug.LogError(movesForEachState[0].Count + "   " + indexForState[0]);
                Debug.LogError("no solution");

                if (gameObject.GetComponent<GameManager>().TinyTube.activeSelf && !gameObject.GetComponent<GameManager>().TinyTube.GetComponent<TinyTube>().FullTube())
                {
                    tinyTubeFlash.SetActive(true);
                }
                else
                {
                    resetFlash.SetActive(true);
                    noSolution.Activate(5f);
                    gameObject.GetComponent<LevelCreator>().lookingForHint = false;
                }
                solvable = false;

                yield break;
            }

            yield return null;
        }
        yield return null;
        
    }

    public void WriteLevels() // save the string of levels to a text file
    {
        StreamWriter writer = new(path);

        writer.WriteLine(original);

        writer.Close();
    }

    public List<List<int>> CopyBoard(List<List<int>> board) // creates a copy of the parameter board and returns it
    {
        List<List<int>> copy = new List<List<int>>(board);

        for (int i = 0; i < copy.Count; ++i)
        {
            copy[i] = new List<int>(board[i]);
        }

        return copy;
    }

    bool CheckIfEqualStates(List<List<int>> first, List<List<int>> second) // checks if two boards are equal
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
    }

    void MakeMove(List<List<int>> initial, Vector2 move) // makes a particular move on a board
    {
        int numMoving = NumSameAtTop(initial, (int)move.x);

        for (int i = 0; i < numMoving; ++i)
        {
            int bottom1 = BottomIndex(initial, (int)move.x);
            int bottom2 = BottomIndex(initial, (int)move.y);
            if (bottom2 != InvalidIndex)
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

    // Determine if state is a winner.
    bool CheckForWin(List<List<int>> state)
    {
        for (int i = 0; i < state.Count; ++i)
        {
            if (!FullTube(state, i) && !EmptyTube(state, i)) 
            { 
                return false; 
            }
        }
        return true;
    }

    List<Vector2> ReturnPossibleMoves(List<List<int>> state) // finds all the posible moves in a current state
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
                            if (state[i][BottomIndex(state, i)] == state[ii][BottomIndex(state, ii)])
                            {
                                if (NumSameAtTop(state, i) <= ReturnNumOpenSpots(state, ii))
                                {
                                    moves.Add(new Vector2(i, ii));
                                }

                            }
                        }
                        else if (!EmptyTube(state, i) && EmptyTube(state, ii))
                        {
                            if (NumSameAtTop(state, i) != NumBallInTube(state, i))
                            {
                                if (!oneEmpty)
                                {
                                    emptyIndex = ii;
                                    oneEmpty = true;
                                }

                                if (ii == emptyIndex)
                                {
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

    void OutputState(List<List<int>> state) // outputs a state to the log
    {
        string level = "";

        for (int i = 0; i < 4; ++i)
        {
            for (int ii = 0; ii < state.Count; ++ii)
            {
                level = level + state[ii][i].ToString() + " ";
                if (state[ii][i] < 10) 
                { 
                    level += "  "; 
                }
            }

            level += "\n";
        }

        Debug.Log(level);
    }

    int NumBallInTube(List<List<int>> initial, int tube) // finds the number of ball in a tube
    {
        int num = 0;

        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] != 0)
            {
                num++;
            }
        }

        return num;
    }

    int ReturnNumOpenSpots(List<List<int>> initial, int tube) // returns the number of open spots in a tube
    {
        int num = 0;

        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] == 0) 
            { 
                num++; 
            }
        }

        return num;
    }

    int NumSameAtTop(List<List<int>> initial, int tube) // returns the number of same balls at the top of a tube
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

        return num;
    }

    int BottomIndex(List<List<int>> initial, int tube) // returns the bottom index of a tube
    {
        for (int i = 0; i < initial[tube].Count; ++i)
        {
            if (initial[tube][i] != 0)
            {
                return i;
            }
        }

        return InvalidIndex;
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
                    if (initial[tube][0] != initial[tube][i]) 
                    { 
                        return false; 
                    }
                }
            }
        }

        return true;
    }
}