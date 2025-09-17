using UnityEngine;

public class VerticalParallaxLayer : MonoBehaviour
{
    // Controla qué tan rápido se mueve la capa en relación con la cámara.
    // Un valor cercano a 0: se mueve lento (lejos).
    // Un valor cercano a 1: se mueve rápido (cerca).
    [SerializeField, Range(0f, 1f)]
    private float parallaxFactor;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitHeight;

    private void Start()
    {
        // Cachea la transformación de la cámara para optimizar.
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        // Calcula la altura de la textura en unidades del mundo.
        Sprite sprite = GetComponent<SpriteRenderer>()?.sprite;
        if (sprite != null)
        {
            // Si es un SpriteRenderer
            textureUnitHeight = sprite.texture.height / sprite.pixelsPerUnit;
        }
        else
        {
            // Si es un Quad (MeshRenderer)
            textureUnitHeight = transform.localScale.y;
        }
    }

    private void LateUpdate()
    {
        // Calcula cuánto se ha movido la cámara desde el último frame.
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Mueve la capa en proporción al movimiento de la cámara.
        transform.position += new Vector3(0, deltaMovement.y * parallaxFactor, 0);

        // Actualiza la última posición de la cámara para el siguiente frame.
        lastCameraPosition = cameraTransform.position;

        // Si la capa se ha desplazado más allá de su punto medio relativo a la cámara, la reposiciona.
        if (Mathf.Abs(cameraTransform.position.y - transform.position.y) >= textureUnitHeight)
        {
            float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitHeight;
            transform.position = new Vector3(transform.position.x, cameraTransform.position.y + offsetPositionY, transform.position.z);
        }
    }
}