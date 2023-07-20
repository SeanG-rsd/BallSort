using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    public float rotateSpeed;
    void Update() // rotates loading icon
    {
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}
