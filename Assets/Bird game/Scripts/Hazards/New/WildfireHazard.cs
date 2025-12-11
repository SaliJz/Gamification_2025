using UnityEngine;

/// <summary>
/// Hazard de incendio forestal. Bola de fuego que explota en múltiples direcciones.
/// </summary>
public class WildfireHazard : BaseHazard
{
    [Header("Wildfire Configuration")]
    [SerializeField] private WildfirePattern pattern = WildfirePattern.SingleExplosion;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float travelSpeed = 6f;
    [SerializeField] private float explosionDelay = 1f;
    [SerializeField] private float chainExplosionInterval = 0.5f;
    [SerializeField] private int chainExplosionCount = 3;

    private int currentExplosionProjectileCount;
    private float currentExplosionProjectileSpeed;

    private Vector2 travelDirection;
    private bool hasExploded = false;
    private float explosionTimer = 0f;
    private int explosionsTriggered = 0;

    public override void Initialize(Vector2 position, SpawnSide spawnSide, Transform player)
    {
        base.Initialize(position, spawnSide, player);
        SetupTravelDirection(spawnSide);

        // Calcular valores escalados según dificultad actual
        CalculateScaledValues();
    }

    private void CalculateScaledValues()
    {
        float difficulty = DifficultyManager.Instance.CurrentDifficulty;
        DifficultyScaling scaling = DifficultyManager.Instance.scaling;

        // Escalar número de proyectiles: 4 => 16
        currentExplosionProjectileCount = DifficultyScaling.LerpInt(
            scaling.wildfireMinProjectiles,
            scaling.wildfireMaxProjectiles,
            difficulty
        );

        // Escalar velocidad de proyectiles: 3 => 12
        currentExplosionProjectileSpeed = DifficultyScaling.Lerp(
            scaling.wildfireMinSpeed,
            scaling.wildfireMaxSpeed,
            difficulty
        );

        Debug.Log($"[Wildfire] Dificultad: {difficulty:F2}x | Proyectiles: {currentExplosionProjectileCount} | Velocidad: {currentExplosionProjectileSpeed:F2}");
    }

    private void SetupTravelDirection(SpawnSide side)
    {
        switch (side)
        {
            case SpawnSide.Top:
                travelDirection = Vector2.down;
                break;
            case SpawnSide.Bottom:
                travelDirection = Vector2.up;
                break;
            case SpawnSide.Left:
                travelDirection = Vector2.right;
                break;
            case SpawnSide.Right:
                travelDirection = Vector2.left;
                break;
        }
    }

    protected override void OnHazardActivated()
    {
        explosionTimer = 0f;
        hasExploded = false;
        explosionsTriggered = 0;
    }

    protected override void UpdateHazardBehavior()
    {
        if (!hasExploded)
        {
            // Movimiento hacia el objetivo
            transform.position += (Vector3)(travelDirection * travelSpeed * Time.deltaTime);
            explosionTimer += Time.deltaTime;

            // Verificar si debe explotar
            CheckExplosionTrigger();
        }
        else if (pattern == WildfirePattern.ChainReaction)
        {
            // Gestionar explosiones encadenadas
            UpdateChainExplosions();
        }
    }

    private void CheckExplosionTrigger()
    {
        switch (pattern)
        {
            case WildfirePattern.SingleExplosion:
                if (explosionTimer >= explosionDelay)
                {
                    TriggerExplosion();
                }
                break;

            case WildfirePattern.ChainReaction:
                if (explosionTimer >= explosionDelay && explosionsTriggered == 0)
                {
                    TriggerExplosion();
                    explosionsTriggered++;
                }
                break;

            case WildfirePattern.CrossPattern:
                if (explosionTimer >= explosionDelay)
                {
                    TriggerCrossExplosion();
                }
                break;
        }
    }

    private void UpdateChainExplosions()
    {
        explosionTimer += Time.deltaTime;

        if (explosionsTriggered < chainExplosionCount &&
            explosionTimer >= chainExplosionInterval * explosionsTriggered)
        {
            TriggerExplosion();
            explosionsTriggered++;
        }

        if (explosionsTriggered >= chainExplosionCount)
        {
            DestroyHazard();
        }
    }

    private void TriggerExplosion()
    {
        hasExploded = true;

        // Explotar en círculo completo con el número escalado de proyectiles
        float angleStep = 360f / currentExplosionProjectileCount;

        for (int i = 0; i < currentExplosionProjectileCount; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            CreateExplosionProjectile(transform.position, direction);
        }

        PlaySound(impactSound);

        if (pattern != WildfirePattern.ChainReaction)
        {
            DestroyHazard();
        }
    }

    private void TriggerCrossExplosion()
    {
        hasExploded = true;

        // Patrón en cruz (4 direcciones cardinales + diagonales = 8 direcciones)
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            new Vector2(1, 1).normalized, new Vector2(-1, 1).normalized,
            new Vector2(1, -1).normalized, new Vector2(-1, -1).normalized
        };

        foreach (Vector2 dir in directions)
        {
            CreateExplosionProjectile(transform.position, dir);
        }

        PlaySound(impactSound);
        DestroyHazard();
    }

    private void CreateExplosionProjectile(Vector2 position, Vector2 direction)
    {
        if (fireballPrefab == null) return;

        GameObject projectile = Instantiate(fireballPrefab, position, Quaternion.identity);
        HazardProjectile proj = projectile.GetComponent<HazardProjectile>();
        if (proj != null)
        {
            proj.Initialize(direction, currentExplosionProjectileSpeed, MovementType.Linear);
        }
    }

    protected override void OnPlayerHit(Collider2D playerCollider)
    {
        base.OnPlayerHit(playerCollider);

        // Explotar inmediatamente al impactar con el jugador
        if (!hasExploded)
        {
            TriggerExplosion();
        }
    }
}