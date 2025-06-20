using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public int x;
    public int y;
    public bool tinyTube;

    public Move(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Move(Move move, bool reverse)
    {
        this.x = reverse ? move.y : move.x;
        this.y = reverse ? move.x : move.y;
    }

    public Move()
    {

    }

    public void SetTinyTube(bool tube)
    {
        tinyTube = tube;
    }
}
