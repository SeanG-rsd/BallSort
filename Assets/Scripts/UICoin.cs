using UnityEngine;

public class UICoin : MonoBehaviour
{
    public float rotateSpeed;
    void Update() // rotate ui coin
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
