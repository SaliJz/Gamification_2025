using UnityEngine;

public class CollectibleMovement : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 4f;

    private Camera mainCamera;
    private float bottomScreenY;

    private void Start()
    {
        mainCamera = Camera.main;
        bottomScreenY = mainCamera.ScreenToWorldPoint(Vector3.zero).y - 2f;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            float speed = GameManager.Instance.CurrentScrollSpeed;
            transform.Translate(Vector3.down * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }

        if (transform.position.y < bottomScreenY)
        {
            Destroy(gameObject);
        }
    }
}