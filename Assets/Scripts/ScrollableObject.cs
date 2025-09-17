using UnityEngine;

public class ScrollableObject : MonoBehaviour
{
    [SerializeField] private float distanceToDestroy = -10f;
    [SerializeField] private bool isDestructible = true;

    public bool IsDestructible { get => isDestructible; set => isDestructible = value; }

    private void Update()
    {
        // Mueve el objeto hacia abajo basado en la velocidad actual del GameManager
        float speed = GameManager.Instance.CurrentScrollSpeed;
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (!isDestructible) return;

        // Destruye el objeto cuando sale de la pantalla para limpiar memoria
        if (transform.position.y < distanceToDestroy)
        {
            Destroy(gameObject);
        }
    }
}