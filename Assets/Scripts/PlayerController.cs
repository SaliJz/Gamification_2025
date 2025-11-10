using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [SerializeField] private float speed = 10f;

    [Header("Permisos de Movimiento")]
    public bool canMoveX = true;
    public bool canMoveY = false;
    public bool canMoveDiagonal = false;

    [Header("Control Movil")]
    [SerializeField] private bool useMobileInput = true;
    [SerializeField] private Joystick joystick;

    [Header("Sistema de Vida")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int birdProtectionHealth = 5; // Vida que absorbe cada ave
    private int currentHealth;

    [Header("Sistema de Curacion")]
    [Tooltip("Tiempo en segundos sin recibir danio para curarse")]
    [SerializeField] private float timeToHeal = 15f;
    [Tooltip("Cantidad de vida a recuperar")]
    [SerializeField] private int healAmount = 20;
    private float healTimer = 0f;

    [Header("Barra de Vida")]
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private UnityEngine.UI.Slider healthSlider;
    [SerializeField] private UnityEngine.UI.Image healthFillImage;
    [SerializeField] private float healthBarDisplayDuration = 3f;
    private float healthBarTimer = 0f;

    [Header("Sistema de Invencibilidad")]
    [SerializeField] private float invincibilityDuration = 0.5f;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    private Vector3 startPosition;
    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;
    private float playerWidth;
    private float playerHeight;

    private bool isSlowed = false;
    private float slowFactor = 1f;
    private float baseSpeed = 10;

    private void Start()
    {
        startPosition = transform.position;

        Camera cam = Camera.main;
        screenBoundsMin = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        screenBoundsMax = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane));

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            playerWidth = spriteRenderer.bounds.size.x / 2;
            playerHeight = spriteRenderer.bounds.size.y / 2;
            screenBoundsMin += new Vector2(playerWidth, playerHeight);
            screenBoundsMax -= new Vector2(playerWidth, playerHeight);
        }

        currentHealth = maxHealth;
        baseSpeed = speed;

        if (healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }

        UpdateHealthBar();

        // Detectar automaticamente si es dispositivo movil
#if UNITY_ANDROID || UNITY_IOS
        useMobileInput = true;
#else
        useMobileInput = false;
#endif
    }

    private void Update()
    {
        // Solo permitir movimiento si el juego esta activo
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.Tutorial)
        {
            return;
        }

        HandleMovement();
        UpdateHealthBarVisibility();
        HandlePassiveHealing();
    }

    private void HandleMovement()
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
        if (useMobileInput && joystick != null)
        {
            return new Vector2(joystick.Horizontal, joystick.Vertical);
        }
        else
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");
            return new Vector2(inputX, inputY);
        }
    }

    #region Damage System
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hazard"))
        {
            TakeDamageFromHazard();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("CollectibleBird"))
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.FlockSize < GameManager.Instance.MaxFlockSize)
                {
                    CollectBird();
                    Destroy(other.gameObject);
                }
            }
        }
        else if (other.CompareTag("Insect"))
        {
            CollectInsect();
            Destroy(other.gameObject);
        }
    }

    private void TakeDamageFromHazard()
    {
        if (isInvincible) return;

        healTimer = 0f;

        // Iniciar rutina de invencibilidad
        StartCoroutine(InvincibilityRoutine());

        // Primero verificar si hay aves protectoras
        if (GameManager.Instance != null && GameManager.Instance.FlockSize > 1)
        {
            // Un ave de la bandada absorbe el danio
            GameManager.Instance.PlayerTookDamage();
            Debug.Log("Un ave protectora absorbio el golpe!");
        }
        else
        {
            // El jugador recibe danio directo
            TakeDamage(birdProtectionHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        healTimer = 0f;

        ShowHealthBar();
        UpdateHealthBar();

        Debug.Log($"Jugador ha recibido {damage} de danio. Vida actual: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void HandlePassiveHealing()
    {
        // No curar si el juego no esta activo o si la vida esta al maximo
        if (GameManager.Instance.CurrentState != GameState.Playing || currentHealth >= maxHealth)
        {
            healTimer = 0f;
            return;
        }

        healTimer += Time.deltaTime;

        if (healTimer >= timeToHeal)
        {
            Heal(healAmount);
            healTimer = 0f;
            Debug.Log($"[PlayerController] Curacion pasiva aplicada. Vida actual: {currentHealth}");
        }
    }

    private void Die()
    {
        Debug.Log("El jugador ha muerto!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }
    }

    /// <summary>
    /// Activa la invencibilidad temporal y un parpadeo visual.
    /// </summary>
    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Feedback visual
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float timer = 0f;

            while (timer < invincibilityDuration)
            {
                // Alterna la opacidad para crear el parpadeo
                float alpha = Mathf.PingPong(Time.time * 15f, 1.0f) < 0.5f ? 0.3f : 1.0f;
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                timer += Time.deltaTime;
                yield return null;
            }

            // Restaurar color original
            spriteRenderer.color = originalColor;
        }
        else
        {
            // Si no hay sprite, solo esperar el tiempo
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvincible = false;
    }
    #endregion

    #region Health Bar
    private void ShowHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
            healthBarTimer = healthBarDisplayDuration;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthFillImage != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            if (healthPercent > 0.5f)
            {
                healthFillImage.color = Color.green;
            }
            else if (healthPercent > 0.25f)
            {
                healthFillImage.color = Color.yellow;
            }
            else
            {
                healthFillImage.color = Color.red;
            }
        }
    }

    private void UpdateHealthBarVisibility()
    {
        if (healthBarTimer > 0)
        {
            healthBarTimer -= Time.deltaTime;

            if (healthBarTimer <= 0 && healthBarUI != null)
            {
                healthBarUI.SetActive(false);
            }
        }
    }
    #endregion

    #region Collectibles
    private void CollectBird()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddBirdToFlock();
        }

        Debug.Log("Ave coleccionada!");
    }

    private void CollectInsect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
        }

        Debug.Log("Insecto coleccionado! +10 puntos");
    }
    #endregion

    #region Slow Effect
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
    #endregion

    #region Public Methods
    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        speed = baseSpeed;
        isSlowed = false;

        transform.position = startPosition;

        isInvincible = false; // Asegurarse de no estar invencible al reiniciar
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white; // Restaurar color/alfa
        }

        UpdateHealthBar();

        if (healthBarUI != null)
        {
            healthBarUI.SetActive(false);
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        ShowHealthBar();
        UpdateHealthBar();
    }
    #endregion
}