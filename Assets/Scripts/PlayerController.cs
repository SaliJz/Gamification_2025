using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 10f;

    [Header("Permisos de Movimiento")]
    public bool canMoveX = true;
    public bool canMoveY = false;
    public bool canMoveDiagonal = false;

    [Header("Control Móvil")]
    [SerializeField] private bool useMobileInput = true;
    [SerializeField] private Joystick joystick;

    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;
    private float playerWidth;
    private float playerHeight;

    private bool isSlowed = false;
    private float slowFactor = 1f;
    public int maxHealth = 10;
    private int currentHealth;
    private float baseSpeed = 0;

    public string gameOverSceneName = "GameOver";

    private void Start()
    {
        Camera cam = Camera.main;
        screenBoundsMin = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        screenBoundsMax = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            playerWidth = sr.bounds.size.x / 2;
            playerHeight = sr.bounds.size.y / 2;
            screenBoundsMin += new Vector2(playerWidth, playerHeight);
            screenBoundsMax -= new Vector2(playerWidth, playerHeight);
        }
        currentHealth = maxHealth;
        baseSpeed = speed;

        // Detectar automáticamente si es dispositivo móvil
#if UNITY_ANDROID || UNITY_IOS
        useMobileInput = true;
#else
        useMobileInput = false;
#endif
    }

    private void Update()
    {
        Vector2 inputDirection = GetInputDirection();

        Vector2 moveDirection = Vector2.zero;
        if (canMoveX)
        {
            moveDirection.x = inputDirection.x;
        }
        if (canMoveY)
        {
            moveDirection.y = inputDirection.y;
        }

        if (!canMoveDiagonal && moveDirection.x != 0 && moveDirection.y != 0)
        {
            moveDirection.y = 0;
        }

        if (moveDirection != Vector2.zero)
        {
            Vector3 finalMovement = moveDirection.normalized * speed * Time.deltaTime;
            transform.position += finalMovement;
        }

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, screenBoundsMin.x, screenBoundsMax.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, screenBoundsMin.y, screenBoundsMax.y);
        transform.position = clampedPosition;
    }

    private Vector2 GetInputDirection()
    {
        // Usar joystick virtual en móvil o cuando esté habilitado
        if (useMobileInput && joystick != null)
        {
            return new Vector2(joystick.Horizontal, joystick.Vertical);
        }
        // Fallback a teclado/gamepad para testing en editor
        else
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");
            return new Vector2(inputX, inputY);
        }
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
        if (isSlowed) return;

        slowFactor = Mathf.Clamp(factor, 0f, 1f);
        isSlowed = true;
        speed = speed * slowFactor;
    }

    public void RemoveSlow()
    {
        if (!isSlowed) return;

        slowFactor = 1f;
        isSlowed = false;
        speed = baseSpeed;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        //if (GameManager.Instance.FlockSize > 1)
        //{
        //    GameManager.Instance.FlockSize--;
        //    return;
        //}

        currentHealth -= damage;
        Debug.Log("Jugador ha recibido " + damage + " de daño. Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("¡El jugador ha muerto!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
        gameObject.SetActive(false);
    }
}