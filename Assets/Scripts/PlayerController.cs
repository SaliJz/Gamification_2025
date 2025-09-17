using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float lateralSpeed = 10f; // Velocidad de movimiento lateral

    private float screenWidthInWorldUnits;

    private void Start()
    {
        // Calcula los límites de la pantalla para que el jugador no se salga
        float playerHalfWidth = transform.localScale.x / 2f;
        screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - playerHalfWidth;
    }

    private void Update()
    {
        // 1. Obtener input del jugador
        float horizontalInput = Input.GetAxis("Horizontal");

        // 2. Calcular el vector de movimiento
        Vector3 movement = new Vector3(horizontalInput * lateralSpeed * Time.deltaTime, 0, 0);

        // 3. Aplicar el movimiento
        transform.Translate(movement);

        // 4. Mantener al jugador dentro de los límites de la pantalla
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.Clamp(currentPosition.x, -screenWidthInWorldUnits, screenWidthInWorldUnits);
        transform.position = currentPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            GameManager.Instance.PlayerTookDamage();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("CollectibleBird"))
        {
            GameManager.Instance.AddBirdToFlock();
            Destroy(other.gameObject);
            Debug.Log("Ave coleccionada!");
        }
    }
}