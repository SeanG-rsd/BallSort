using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public int x;
    public int y;
    public int ball;
    public int count;
    public bool tinyTube;

    public Move(int x, int y, int ball, int count)
    {
        this.x = x;
        this.y = y;
        this.ball = ball;
        this.count = count;
    }

    public Move(Move move, bool reverse)
    {
        this.x = reverse ? move.y : move.x;
        this.y = reverse ? move.x : move.y;
        this.ball = move.ball;
        this.count = move.count;
    }

    public Move()
    {

    }

    public void SetTinyTube(bool tube)
    {
        tinyTube = tube;
    }
}
