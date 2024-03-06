using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Schwojun/Ball Pallette", fileName = "ballPallette")]
public class BallPallete : ScriptableObject
{
    [SerializeField] private Color[] pallete;

    public Color[] GetBallColors() { return pallete; }
 }
