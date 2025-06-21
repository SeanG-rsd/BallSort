using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

using BallSortSolver;


public class LevelSolver : MonoBehaviour
{

    private Dictionary<string, char> ColorMap = new Dictionary<string, char>()
        {
            { "0", 'r' }, // red
            { "1", 'b' }, // blue
            { "2", 'w' }, // brown
            { "3", 'y' }, // yellow
            { "4", 'o' }, // orange
            { "5", 'p' }, // purple
            { "6", 'k' }, // pink
            { "7", 'l' }, // light-blue
            { "8", 'd' }, // dark-green
            { "9", 'e' }, // grey
            {"10", 'g' }, // green
            {"11", 'a' }, // aqua
        };
    public List<SolverMove> SolveFromCurrent(List<GameObject> level)
    {
        var board = new SolverBoard(level.Count, 4, level.Count - 2);
        var tubeIndex = 0;
        foreach (var tube in level)
        {
            var tubeString = new StringBuilder();
            for (int i = 1; i < 5; i++)
            {
                int ball = tube.GetComponent<Tube>().spots[i];
                if (ball == 0) continue;
                tubeString.Append(ColorMap[(ball - 1).ToString()]);
            }

            board.SetTube(tubeIndex, new SolverTube(board, tubeIndex, new string(tubeString.ToString().Reverse().ToArray())));
            tubeIndex++;
        }

        var solver = new Solver();
        List<SolverMove> solution = new List<SolverMove>();
        var ret = solver.FindSolution(board);

        if (ret == null)
        {
            Debug.Log($"No solution!!! Visited = {solver.Visited.Count}");
            return solution;
        }

        //Console.WriteLine($"Solved!!! Visited = {solver.Visited.Count}");
        // boards are in "reverse order" at this point (walking up looking at parents).
        // make understanding the moves easier now by reversing the order.
        while (ret != null)
        {
            solution.Insert(0, ret.Move);
            ret = ret.Parent;
        }

        Debug.Log(solution.Count);
        foreach (SolverMove move in solution)
        {
            if (move != null)
            {
                Debug.Log($"{move.Source} to {move.Target}");
            }
        }

        return solution;
    }

    public bool SolveFromList(List<List<int>> level)
    {
        var board = new SolverBoard(level.Count + 2, 4, level.Count);
        var tubeIndex = 0;
        foreach (var tube in level)
        {
            var tubeString = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                int ball = tube[i];
                tubeString.Append(ColorMap[(ball).ToString()]);
            }

            board.SetTube(tubeIndex, new SolverTube(board, tubeIndex, new string(tubeString.ToString().Reverse().ToArray())));
            tubeIndex++;
        }

        var solver = new Solver();
        List<SolverMove> solution = new List<SolverMove>();
        var ret = solver.FindSolution(board);

        return ret != null;
    }
}