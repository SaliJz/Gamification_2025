using TMPro;
using UnityEngine;

public class MagnetBird : MonoBehaviour
{
    private enum MagnetState
    {
        Falling, // Antes de ser recogida
        Orbiting, // Orbitando y buscando
        Seeking,  // Yendo hacia un insecto
        Returning // Volviendo a la órbita
    }

    private MagnetState currentState = MagnetState.Falling;
    private GameObject targetInsect;

    [Header("References")]
    [SerializeField] private Animator magnetBirdAnimator;

    [Header("Movement Before Collection")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float sideMovementAmplitude = 1f;
    [SerializeField] private float sideMovementFrequency = 2f;

    [Header("Orbit Configuration")]
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 2f;
    [SerializeField] private float duration = 30f;

    [Header("Magnet Configuration")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attractionSpeed = 8f;
    [SerializeField] private LayerMask insectLayer;

    [Header("UI")]
    [SerializeField] private GameObject timerUIPrefab;

    private Transform playerTransform;
    private float movementTime = 0f;
    private float orbitAngle = 0f;
    private float remainingTime;
    private GameObject timerUI;
    private TextMeshProUGUI timerText;

    private void Start()
    {
        remainingTime = duration;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        //var playerPauseSensitiveAnimator = player.GetComponent<PauseSensitiveAnimator>();

        //if (playerPauseSensitiveAnimator != null)
        //{
        //    magnetBirdAnimator.speed = playerPauseSensitiveAnimator.GetCurrentAnimationSpeed();
        //}
    }

    private void Update()
    {
        // Si el jugador desaparece, desactivar el imán
        if (currentState != MagnetState.Falling && playerTransform == null)
        {
            DeactivateMagnet();
            return;
        }

        switch (currentState)
        {
            case MagnetState.Falling:
                UpdateFallingMovement();
                CheckOffScreen();
                break;

            // Si está activa, actualizar siempre el timer y el ángulo de órbita
            case MagnetState.Orbiting:
            case MagnetState.Seeking:
            case MagnetState.Returning:
                UpdateTimer();
                UpdateOrbitAngle();
                break;
        }

        // Lógica de movimiento para estados activos
        if (currentState == MagnetState.Orbiting)
        {
            UpdateOrbitMovement();
            DetectInsects();
        }
        else if (currentState == MagnetState.Seeking)
        {
            UpdateSeekMovement();
        }
        else if (currentState == MagnetState.Returning)
        {
            UpdateReturnMovement();
        }
    }

    #region Movement Before Collection
    private void UpdateFallingMovement()
    {
        movementTime += Time.deltaTime;

        // Movimiento de caída
        float verticalMovement = -fallSpeed * Time.deltaTime;

        // Movimiento lateral ondulante
        float sideOffset = Mathf.Sin(movementTime * sideMovementFrequency) * sideMovementAmplitude;
        float horizontalMovement = sideOffset * Time.deltaTime;

        transform.Translate(new Vector3(horizontalMovement, verticalMovement, 0f));
    }

    private void CheckOffScreen()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        if (screenPos.y < -0.1f)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Collection
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentState == MagnetState.Falling && collision.CompareTag("Player"))
        {
            CollectMagnetBird();
        }
    }

    private void CollectMagnetBird()
    {
        currentState = MagnetState.Orbiting;
        Debug.Log("[MagnetBird] ¡Ave Imán recogida! Duración: " + duration + "s");

        // Configurar UI del temporizador
        CreateTimerUI();

        // Inicializar ángulo de órbita aleatorio
        orbitAngle = Random.Range(0f, 360f);
    }
    #endregion

    #region Orbit & State Movements

    private void UpdateOrbitAngle()
    {
        orbitAngle += orbitSpeed * Time.deltaTime * 360f / duration;
    }

    private Vector3 GetTargetOrbitPosition()
    {
        if (playerTransform == null) return transform.position;

        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            Mathf.Sin(rad) * orbitRadius,
            0f
        );
        return playerTransform.position + offset;
    }

    private void UpdateOrbitMovement()
    {
        // Simplemente se ajusta a la posición de órbita
        transform.position = GetTargetOrbitPosition();
    }

    // NUEVO: Lógica para volar hacia el insecto
    private void UpdateSeekMovement()
    {
        if (targetInsect == null)
        {
            // El insecto fue destruido por otra cosa
            currentState = MagnetState.Returning;
            return;
        }

        // Moverse hacia el insecto
        Vector3 direction = (targetInsect.transform.position - transform.position).normalized;
        transform.position += direction * attractionSpeed * Time.deltaTime;

        // Comprobar si llegó
        float distance = Vector3.Distance(transform.position, targetInsect.transform.position);
        if (distance < 0.5f)
        {
            // Recoger el insecto
            CollectInsect(targetInsect);
            targetInsect = null;
            currentState = MagnetState.Returning;
        }
    }

    // NUEVO: Lógica para volar de vuelta a la órbita
    private void UpdateReturnMovement()
    {
        Vector3 targetOrbitPosition = GetTargetOrbitPosition();

        // Moverse hacia la posición de órbita
        transform.position = Vector3.MoveTowards(transform.position, targetOrbitPosition, attractionSpeed * Time.deltaTime);

        // Comprobar si llegó
        float distance = Vector3.Distance(transform.position, targetOrbitPosition);
        if (distance < 0.1f)
        {
            currentState = MagnetState.Orbiting; // Volver a orbitar y buscar
        }
    }

    #endregion

    #region Magnet Behavior

    private void DetectInsects()
    {
        if (playerTransform == null) return;

        Collider2D[] insects = Physics2D.OverlapCircleAll(
            playerTransform.position,
            detectionRadius,
            insectLayer
        );

        if (insects.Length > 0)
        {
            foreach (Collider2D insect in insects)
            {
                if (insect.CompareTag("Insect"))
                {
                    targetInsect = insect.gameObject;
                    currentState = MagnetState.Seeking;
                    Debug.Log("[MagnetBird] Insecto detectado. ¡Yendo a buscar!");
                    break; // Solo perseguir uno a la vez
                }
            }
        }
    }

    private void CollectInsect(GameObject insect)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
        }

        if (insect != null)
        {
            Destroy(insect);
        }

        Debug.Log("[MagnetBird] ¡Insecto recolectado! +10 puntos");
    }

    #endregion

    #region Timer System
    private void CreateTimerUI()
    {
        if (timerUIPrefab != null)
        {
            // Instanciar en el Canvas
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                timerUI = Instantiate(timerUIPrefab, canvas.transform);
                timerText = timerUI.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        else
        {
            Debug.LogWarning("[MagnetBird] Timer UI Prefab no asignado");
        }
    }

    private void UpdateTimer()
    {
        remainingTime -= Time.deltaTime;

        // Actualizar UI
        if (timerText != null)
        {
            timerText.text = $"Ave Imán: {Mathf.CeilToInt(remainingTime)}s";
        }

        // Verificar si terminó
        if (remainingTime <= 0f)
        {
            DeactivateMagnet();
        }
    }

    private void DeactivateMagnet()
    {
        if (currentState == MagnetState.Falling) return;
        currentState = MagnetState.Falling;

        Debug.Log("[MagnetBird] Ave Imán desactivada");

        if (timerUI != null)
        {
            Destroy(timerUI);
        }
        Destroy(gameObject);
    }
    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        // Dibujar solo si está activa
        if (currentState != MagnetState.Falling && playerTransform != null)
        {
            // Rango de detección
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, detectionRadius);

            // Órbita
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(playerTransform.position, orbitRadius);

            // Línea de objetivo si está buscando
            if (currentState == MagnetState.Seeking && targetInsect != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, targetInsect.transform.position);
            }
            // Línea de regreso si está volviendo
            else if (currentState == MagnetState.Returning)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, GetTargetOrbitPosition());
            }
        }
    }
    #endregion

    private void OnDestroy()
    {
        // Limpiar UI si existe
        if (timerUI != null)
        {
            Destroy(timerUI);
        }
    }
}