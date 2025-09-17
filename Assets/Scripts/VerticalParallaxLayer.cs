using UnityEngine;

public class VerticalParallaxLayer : MonoBehaviour
{
    // Controla qu� tan r�pido se mueve la capa en relaci�n con la c�mara.
    // Un valor cercano a 0: se mueve lento (lejos).
    // Un valor cercano a 1: se mueve r�pido (cerca).
    [SerializeField, Range(0f, 1f)]
    private float parallaxFactor;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitHeight;

    private void Start()
    {
        // Cachea la transformaci�n de la c�mara para optimizar.
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
        // Calcula cu�nto se ha movido la c�mara desde el �ltimo frame.
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Mueve la capa en proporci�n al movimiento de la c�mara.
        transform.position += new Vector3(0, deltaMovement.y * parallaxFactor, 0);

        // Actualiza la �ltima posici�n de la c�mara para el siguiente frame.
        lastCameraPosition = cameraTransform.position;

        // Si la capa se ha desplazado m�s all� de su punto medio relativo a la c�mara, la reposiciona.
        if (Mathf.Abs(cameraTransform.position.y - transform.position.y) >= textureUnitHeight)
        {
            float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitHeight;
            transform.position = new Vector3(transform.position.x, cameraTransform.position.y + offsetPositionY, transform.position.z);
        }
    }
}