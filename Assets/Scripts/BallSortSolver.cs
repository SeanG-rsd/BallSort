using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace BallSortSolver
{
    class SolverBall
    {
        public SolverBall(char color)
        {
            this.Color = color;
        }

        public char Color { get; set; }

        public override string ToString()
        {
            return this.Color.ToString();
        }

        public static bool operator ==(SolverBall a, SolverBall b)
        {
            return a.Color == b.Color;
        }

        public static bool operator !=(SolverBall a, SolverBall b)
        {
            return a.Color != b.Color;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class SolverBoard
    {
        public List<SolverTube> Tubes { get; set; }

        public int NumColors { get; set; }

        public SolverBoard(int numTubes, int numBallsPerTube, int numColors)
        {
            this.MaxBallsPerTube = numBallsPerTube;
            this.Tubes = new List<SolverTube>();
            for (var ii = 0; ii < numTubes; ii++)
            {
                this.Tubes.Add(new SolverTube(this, ii));
            }
            this.NumColors = numColors;
        }

        public SolverBoard(SolverBoard board)
        {
            this.Tubes = new List<SolverTube>(board.NumTubes);
            this.NumColors = board.NumColors;
            this.MaxBallsPerTube = board.MaxBallsPerTube;

            foreach (var tube in board.Tubes)
            {
                this.Tubes.Add(new SolverTube(this, tube));
            }
        }

        public void WriteToFile(string fullpath)
        {
            using (var textWriter = File.CreateText(fullpath))
            {
                foreach (var tube in this.Tubes)
                {
                    textWriter.Write($"{tube}\n");
                }
            }
        }

        public static SolverBoard CreateFromFile(string fullpath)
        {
            if (!File.Exists(fullpath))
            {
                return null;
            }

            var lines = File.ReadLines(fullpath);
            var board = new SolverBoard(lines.Count() + 2, 4, lines.Count());
            var index = 0;
            foreach (var line in lines)
            {
                board.SetTube(index, new SolverTube(board, index, line));
                index++;
            }

            return board;
        }


        public SolverBoard Parent { get; set; }

        // This is the move that takes us from Parent to This.

        public SolverMove Move { get; set; }

        public int MaxBallsPerTube { get; set; }


        public int NumTubes => this.Tubes.Count;


        public bool IsWinner => this.Tubes.All(t => t.IsEmpty || t.IsComplete);

        public void SetTube(int index, SolverTube tube)
        {
            this.Tubes[index] = tube;
        }

        public void SortTubes()
        {
            this.Tubes.Sort((a, b) => a.Value - b.Value);
        }

        public bool TryMove(int source, int target, out SolverBoard board)
        {
            board = null;
            if (SolverTube.CanMove(this.Tubes[source], this.Tubes[target]))
            {
                board = new SolverBoard(this);
                var result = SolverTube.TryMove(board.Tubes[source], board.Tubes[target]);
                if (!result)
                {
                    throw new Exception("TryMove should not have failed.");
                }
                return result;
            }

            return false;
        }

        public List<SolverBoard> GetNextBoards()
        {
            var result = new List<SolverBoard>();

            for (var ii = 0; ii < this.Tubes.Count; ii++)
            {
                for (var kk = 0; kk < this.Tubes.Count; kk++)
                {
                    if (ii == kk)
                    {
                        continue;
                    }

                    if (this.TryMove(ii, kk, out var board))
                    {
                        board.SortTubes();
                        if (!result.Contains(board))
                        {
                            board.Parent = this;
                            // ii -> kk
                            board.Move = new SolverMove(this.Tubes[ii].Id, this.Tubes[kk].Id);
                            result.Add(board);
                        }
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            var tubes = new List<SolverTube>(this.Tubes);
            tubes.Sort((x, y) => x.Id - y.Id);
            var result = new StringBuilder();
            for (var ii = MaxBallsPerTube - 1; ii >= 0; ii--)
            {
                foreach (var tube in tubes)
                {
                    result.Append($"{tube.GetBallString(ii)}  ");
                }
                result.AppendLine();
            }

            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            var board = obj as SolverBoard;

            if (board == null)
            {
                return false;
            }

            for (var ii = 0; ii < this.Tubes.Count; ii++)
            {
                if (!SolverTube.Equals(this.Tubes[ii], board.Tubes[ii]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class SolverTube
    {
        public SolverTube(SolverBoard board, int id)
        {
            this.MaxBalls = board.MaxBallsPerTube;
            this.Balls = new List<SolverBall>(this.MaxBalls);
            this.Id = id;
        }

        // balls are ordered BOTTOM to TOP
        public SolverTube(SolverBoard board, int id, string balls)
            : this(board, id)
        {
            foreach (var color in balls)
            {
                this.Balls.Add(new SolverBall(color));
            }
        }

        public SolverTube(SolverBoard board, SolverTube tube)
            : this(board, tube.Id)
        {
            this.Balls = tube.Balls.Select(b => new SolverBall(b.Color)).ToList();
        }

        public List<SolverBall> Balls { get; set; }

        public int MaxBalls { get; set; }

        public bool IsEmpty => !Balls.Any();

        public bool IsFull => Balls.Count == MaxBalls;

        public int Space => MaxBalls - Balls.Count;

        public int Value => this.Balls.Sum(b => b.Color);

        public int Id { get; set; }

        public SolverBall Top => this.Balls.Last();

        public bool IsComplete => this.IsFull && this.Balls.All(b => b == this.Balls.First());

        public List<SolverBall> GetTopBalls()
        {
            var result = new List<SolverBall>();
            if (this.IsEmpty)
            {
                return result;
            }

            result.Add(this.Top);
            for (var ii = this.Balls.Count - 2; ii >= 0; ii--)
            {
                if (this.Balls[ii] != result[0])
                {
                    break;
                }

                result.Add(this.Balls[ii]);
            }

            return result;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string GetBallString(int index)
        {
            if (index < this.Balls.Count)
            {
                return this.Balls[index].ToString();
            }

            return ".";
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var ball in this.Balls)
            {
                result.Append(ball.ToString());
            }
            for (var ii = result.Length; ii < this.MaxBalls; ii++)
            {
                result.Append(".");
            }

            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            var tube = obj as SolverTube;
            if (tube == null)
            {
                return false;
            }

            if (tube.Balls.Count != this.Balls.Count)
            {
                return false;
            }

            for (var ii = 0; ii < this.Balls.Count; ii++)
            {
                if (tube.Balls[ii] != this.Balls[ii])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryMove(SolverTube source, SolverTube target)
        {
            if (!CanMove(source, target))
            {
                return false;
            }

            // remove these balls from the source
            var move = source.GetTopBalls();
            foreach (var ball in move)
            {
                source.Balls.Remove(ball);
                target.Balls.Add(ball);
            }

            return true;
        }

        public static bool CanMove(SolverTube source, SolverTube target)
        {
            if (source.IsEmpty)
            {
                return false;
            }

            if (target.IsFull)
            {
                return false;
            }

            if (target.IsEmpty)
            {
                return true;
            }

            var move = source.GetTopBalls();

            if (target.Space < move.Count)
            {
                return false;
            }

            if (target.Top != move.First())
            {
                return false;
            }

            return true;
        }
    }

    public class SolverMove
    {
        public SolverMove(int source, int target)
        {
            this.Source = source;
            this.Target = target;
        }

        public int Source { get; }

        public int Target { get; }
    }

    class Solver
    {
        public Solver()
        {
            this.Visited = new List<SolverBoard>();
        }

        public SolverBoard OriginBoard { get; set; }

        public List<SolverBoard> Visited { get; set; }

        public SolverBoard FindSolution(SolverBoard origin)
        {
            var open = new List<SolverBoard>
            {
                origin
            };
            Visited.Add(origin);

            //origin.PrintBoard("Starting board");
            if (origin.IsWinner)
            {
                return origin;
            }

            while (open.Any())
            {
                var board = open.First();
                open.RemoveAt(0);
                Debug.Log($"Current Board\n{board.ToString()}");

                var children = board.GetNextBoards();
                //Debug.Log($"Found {children.Count} moves.");

                //var index = 0;
                foreach (var child in children)
                {
                    //child.PrintBoard($"Child Board {index}");
                    if (Visited.Contains(child))
                    {
                        // we've already seen this board
                        continue;
                    }

                    Visited.Add(child);

                    if (child.IsWinner)
                    {
                        //child.PrintBoard("Winner!!!");
                        return child;
                    }

                    //open.Add(child);
                    open.Insert(0, child);
                }
            }

            return null;
        }
    }
}
