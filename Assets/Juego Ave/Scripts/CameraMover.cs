using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float speed = 2f;

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}