using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    [SerializeField] private float lateralSpeed = 10f; // Velocidad de movimiento lateral

    private float screenWidthInWorldUnits;
    private bool isSlowed = false;
    private float slowFactor = 1f;
    public int maxHealth = 10;
    private int currentHealth;

    private void Start()
    {
        // Calcula los l�mites de la pantalla para que el jugador no se salga
        float playerHalfWidth = transform.localScale.x / 2f;
        screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - playerHalfWidth;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // 1. Obtener input del jugador
        float horizontalInput = Input.GetAxis("Horizontal");

        // 2. Calcular el vector de movimiento
        Vector3 movement = new Vector3(horizontalInput * lateralSpeed * Time.deltaTime, 0, 0);

        // 3. Aplicar el movimiento
        transform.Translate(movement);

        // 4. Mantener al jugador dentro de los l�mites de la pantalla
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
    public void ApplySlow(float factor)
    {
        slowFactor = Mathf.Clamp(factor, 0f, 1f);
        isSlowed = true;
    }
    public void RemoveSlow()
    {
        slowFactor = 1f;
        isSlowed = false;
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Jugador ha recibido " + damage + " de da�o. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("�El jugador ha muerto!");
        Destroy(gameObject);
    }
}