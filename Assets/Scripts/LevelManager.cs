using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private bool clickState; // true means there is a ball waiting to be moved, false is all balls are in a tube
    private GameObject firstTubeClicked = null;

    private List<GameObject> tubeObjects;
    [SerializeField] private GameObject tubePrefab;

    private List<List<GameObject>> undoHolster = new List<List<GameObject>>();
    private List<GameObject> tinyTubeUndoHolster = new List<GameObject>();

    [SerializeField] private Color[] ballColors;

    private int lastLevelLoaded = -1;


    public void OnClickTube(GameObject tubeObject)
    {
        MoveBalls(tubeObject);
    }

    public void OnClickLoadLevel(int levelNumber)
    {
        lastLevelLoaded = levelNumber;
        LoadLevel(LevelCreator.Instance.GetLevel(levelNumber));
    }

    public void OnClickResetGame()
    {

    }

    public void OnClickUndo()
    {

    }

    public void OnClickHint()
    {

    }

    public void OnClickMenu()
    {

    }

    private void MoveBalls(GameObject tubeObject)
    {
        Tube currentTube = tubeObject.GetComponent<Tube>();

        if (!currentTube.corked)
        {
            if (clickState) // move balls from firstTubeClicked to tubeObject
            {
                if (firstTubeClicked == tubeObject)
                {
                    currentTube.MoveTopToBottom();
                    clickState = false;
                }
                else
                {
                    Tube firstTube = firstTubeClicked.GetComponent<Tube>(); // move balls into tubeObject from firstTubeClicked

                    if (firstTube.NumberMoving() <= currentTube.NumOpenSpots())
                    {
                        int ballCount = firstTube.NumberMoving() - 1;

                        currentTube.NewBallToBottom(firstTube.GetTopBall());
                        firstTube.RemoveTopBall();

                        for (int i = 0; i < ballCount; ++i)
                        {
                            currentTube.NewBallToBottom(firstTube.GetBottomBall());
                            firstTube.RemoveBottomBall();
                        }

                        firstTubeClicked = null;
                        clickState = false;
                    }
                    else
                    {
                        currentTube.MoveBottomToTop();
                        firstTube.MoveTopToBottom();
                        firstTubeClicked = tubeObject;
                    }
                }
            }
            else // move ball from tubeObject to top
            {
                firstTubeClicked = tubeObject;

                currentTube.MoveBottomToTop();
            }

            clickState = !clickState;
        }
    }

    private void ResetGame()
    {

    }

    private void LoadBlankLevel()
    {

    }

    private void LoadLevel(List<List<int>> level)
    {
        undoHolster.Clear();
        tinyTubeUndoHolster.Clear();

        ResetGame();

        if (level != null)
        {
            for (int tube = 0; tube < level.Count; ++tube)
            {
                Tube currentTube = tubeObjects[tube].GetComponent<Tube>();
                for (int position = 0; position < level[tube].Count; ++position)
                {
                    if (level[tube][position] != -1)
                    {
                        currentTube.SetBall(position, ballColors[level[tube][position] - 1], level[tube][position]);
                        if (!currentTube.corked)
                        {
                            if (currentTube.FullTube())
                            {
                                currentTube.Cork();
                            }
                        }
                    }
                    else
                    {
                        currentTube.RemoveBall(position);
                    }
                }
            }
        }


    }

    private void SetUndoTubes()
    {

    }
}
