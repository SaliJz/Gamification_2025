using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HazardType
{
    AcidRain,      // Lluvia ácida desde nubes
    ToxicCloud,    // Nubes de gas tóxico
    Wildfire       // Bolas de fuego explosivas
}

public enum AcidRainPattern
{
    SteadyDrizzle,    // Goteo constante y uniforme
    IntensePour,      // Lluvia intensa en ráfagas
    WavingCurtain     // Cortina ondulante de gotas
}

public enum ToxicCloudPattern
{
    LinearDrift,      // Movimiento lineal simple
    ZigzagSweep,      // Patrón zigzag
    CircularOrbit     // Movimiento circular/orbital
}

public enum WildfirePattern
{
    SingleExplosion,  // Una explosión simple
    ChainReaction,    // Explosiones encadenadas
    CrossPattern      // Patrón en cruz
}

public enum SpawnSide
{
    Top,
    Bottom,
    Left,
    Right
}

/// <summary>
/// Clase base para todos los hazards específicos del juego.
/// </summary>
public abstract class BaseHazard : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected float lifetime = 10f;
    [SerializeField] protected int damage = 1;

    [Header("Warning System")]
    [SerializeField] protected GameObject warningIndicatorPrefab;
    [SerializeField] protected float warningDuration = 1f;
    [SerializeField] protected Color warningColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] protected bool useWarningFlash = true;
    [SerializeField] protected float flashSpeed = 8f;

    [Header("Audio Feedback")]
    [SerializeField] protected AudioClip warningSound;
    [SerializeField] protected AudioClip activeSound;
    [SerializeField] protected AudioClip impactSound;

    protected bool isInitialized = false;
    protected bool isActive = false;
    protected float timeAlive = 0f;
    protected Transform playerTransform;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D hazardCollider;
    protected GameObject currentWarning;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hazardCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Deshabilitar colisión durante warning
        if (hazardCollider != null)
        {
            hazardCollider.enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (!isInitialized) return;

        if (isActive)
        {
            timeAlive += Time.deltaTime;
            UpdateHazardBehavior();
            CheckLifetime();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        if (collision.CompareTag("Player"))
        {
            OnPlayerHit(collision);
        }
    }

    #region Initialization

    public virtual void Initialize(Vector2 position, SpawnSide spawnSide, Transform player)
    {
        transform.position = position;
        playerTransform = player;
        isInitialized = true;
        isActive = false;

        StartCoroutine(WarningSequence());
    }

    protected virtual IEnumerator WarningSequence()
    {
        // Crear indicador visual de advertencia
        CreateWarningIndicator();
        PlaySound(warningSound);

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (useWarningFlash)
            {
                UpdateWarningFlash(elapsed);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Activar hazard
        ActivateHazard();
        DestroyWarning();
    }

    protected virtual void CreateWarningIndicator()
    {
        if (warningIndicatorPrefab != null)
        {
            currentWarning = Instantiate(warningIndicatorPrefab, transform.position, Quaternion.identity);
            currentWarning.transform.SetParent(transform);
        }
        else
        {
            // Warning visual por defecto
            CreateDefaultWarning();
        }
    }

    protected virtual void CreateDefaultWarning()
    {
        GameObject warning = new GameObject("DefaultWarning");
        warning.transform.SetParent(transform);
        warning.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
        sr.sprite = spriteRenderer?.sprite;
        sr.color = warningColor;
        sr.sortingOrder = -1;

        currentWarning = warning;
    }

    protected virtual void UpdateWarningFlash(float time)
    {
        if (currentWarning != null)
        {
            SpriteRenderer sr = currentWarning.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = Mathf.PingPong(time * flashSpeed, 1f);
                Color color = sr.color;
                color.a = alpha * warningColor.a;
                sr.color = color;
            }
        }
    }

    protected virtual void DestroyWarning()
    {
        if (currentWarning != null)
        {
            Destroy(currentWarning);
        }
    }

    protected virtual void ActivateHazard()
    {
        isActive = true;

        if (hazardCollider != null)
        {
            hazardCollider.enabled = true;
        }

        PlaySound(activeSound);
        OnHazardActivated();
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Comportamiento específico del hazard cuando se actualiza
    /// </summary>
    protected abstract void UpdateHazardBehavior();

    /// <summary>
    /// Llamado cuando el hazard se activa después del warning
    /// </summary>
    protected abstract void OnHazardActivated();

    #endregion

    #region Common Methods

    protected virtual void OnPlayerHit(Collider2D playerCollider)
    {
        PlayerController player = playerCollider.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
            PlaySound(impactSound);
        }

        OnHazardImpact();
    }

    protected virtual void OnHazardImpact()
    {
        // Override en clases derivadas si necesitan lógica adicional
    }

    protected virtual void CheckLifetime()
    {
        if (timeAlive >= lifetime)
        {
            DestroyHazard();
        }
    }

    protected virtual void DestroyHazard()
    {
        Destroy(gameObject);
    }

    protected void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion
}