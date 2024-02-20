using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TubeContainer : MonoBehaviour
{
    public GridLayoutGroup grid;

    public void SetGrid(int rowCount)
    {
        //Debug.Log("set grid : " + rowCount);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = rowCount;
    }
}
