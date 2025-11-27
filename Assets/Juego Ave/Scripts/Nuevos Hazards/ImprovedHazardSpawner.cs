using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Spawner mejorado que maneja los tres tipos de hazards específicos
/// Mantiene compatibilidad con AttackPattern
/// </summary>
public class ImprovedHazardSpawner : MonoBehaviour
{
    [Header("Hazard Configurations")]
    [SerializeField] private List<HazardConfiguration> hazardConfigs;
    [SerializeField] private bool spawnHazards = true;

    [Header("Legacy Attack Patterns (Bullet Hell)")]
    [SerializeField] private List<AttackPattern> attackPatterns;
    [SerializeField] private bool useLegacyPatterns = true;
    [SerializeField] private float minTimeBetweenPatterns = 3f;
    [SerializeField] private float maxTimeBetweenPatterns = 6f;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private float minSpawnInterval = 1f;

    [Header("References")]
    [SerializeField] private Transform hazardContainer; // Contenedor para organizar hazards en la jerarquia

    private Transform playerTransform;
    private Camera mainCamera;
    private Vector2 screenBounds;
    private float currentDifficultyMultiplier = 1f;
    private float gameTime = 0f;

    // Pooling basico
    private Dictionary<HazardType, Queue<GameObject>> hazardPools = new Dictionary<HazardType, Queue<GameObject>>();

    #region Initialization
    void Start()
    {
        InitializeReferences();
        InitializeHazardPools();

        if (spawnHazards && hazardConfigs.Count > 0)
        {
            StartCoroutine(HazardSpawningRoutine());
        }

        if (useLegacyPatterns && attackPatterns.Count > 0)
        {
            StartCoroutine(PatternSpawningRoutine());
        }

        if (enableDifficultyScaling)
        {
            StartCoroutine(DifficultyScalingRoutine());
        }
    }

    private void InitializeReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        mainCamera = Camera.main;
        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        if (hazardContainer == null)
        {
            GameObject container = new GameObject("HazardContainer");
            hazardContainer = container.transform;
        }
    }

    private void InitializeHazardPools()
    {
        foreach (var config in hazardConfigs)
        {
            if (!hazardPools.ContainsKey(config.hazardType))
            {
                hazardPools[config.hazardType] = new Queue<GameObject>();
            }
        }
    }

    #endregion

    #region Hazard Spawning (New System)

    private IEnumerator HazardSpawningRoutine()
    {
        while (true)
        {
            // Seleccionar configuracion basada en probabilidades
            HazardConfiguration config = SelectHazardConfiguration();

            if (config != null)
            {
                SpawnHazard(config);
            }

            float waitTime = Random.Range(
                config.minTimeBetweenSpawns / currentDifficultyMultiplier,
                config.maxTimeBetweenSpawns / currentDifficultyMultiplier
            );

            waitTime = Mathf.Max(waitTime, minSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private HazardConfiguration SelectHazardConfiguration()
    {
        // Filtrar configs disponibles basados en probabilidad
        List<HazardConfiguration> availableConfigs = new List<HazardConfiguration>();

        foreach (var config in hazardConfigs)
        {
            if (Random.value <= config.spawnChance)
            {
                availableConfigs.Add(config);
            }
        }

        if (availableConfigs.Count == 0)
        {
            availableConfigs.AddRange(hazardConfigs);
        }

        return availableConfigs[Random.Range(0, availableConfigs.Count)];
    }

    private void SpawnHazard(HazardConfiguration config)
    {
        // Seleccionar lado de spawn
        SpawnSide spawnSide = config.allowedSpawnSides[Random.Range(0, config.allowedSpawnSides.Count)];

        // Calcular posicion de spawn
        Vector2 spawnPosition = GetHazardSpawnPosition(spawnSide, config.hazardType);

        // Instanciar hazard
        GameObject hazardObj = Instantiate(config.hazardPrefab, spawnPosition, Quaternion.identity, hazardContainer);

        // Inicializar segun tipo
        BaseHazard hazard = hazardObj.GetComponent<BaseHazard>();
        if (hazard != null)
        {
            hazard.Initialize(spawnPosition, spawnSide, playerTransform);
        }

        // Configurar patron especifico
        ConfigureHazardPattern(hazardObj, config);
    }

    private void ConfigureHazardPattern(GameObject hazardObj, HazardConfiguration config)
    {
        switch (config.hazardType)
        {
            case HazardType.AcidRain:
                var acidRain = hazardObj.GetComponent<AcidRainHazard>();
                if (acidRain != null)
                {
                    // El patron se configura en el inspector del prefab
                    // o mediante reflection si es necesario
                }
                break;

            case HazardType.ToxicCloud:
                var toxicCloud = hazardObj.GetComponent<ToxicCloudHazard>();
                if (toxicCloud != null)
                {
                    // Configuracion adicional si es necesario
                }
                break;

            case HazardType.Wildfire:
                var wildfire = hazardObj.GetComponent<WildfireHazard>();
                if (wildfire != null)
                {
                    // Configuracion adicional si es necesario
                }
                break;
        }
    }

    private Vector2 GetHazardSpawnPosition(SpawnSide side, HazardType type)
    {
        float margin = 1f;
        Vector2 position = Vector2.zero;

        switch (side)
        {
            case SpawnSide.Top:
                position = new Vector2(
                    Random.Range(-screenBounds.x * 0.8f, screenBounds.x * 0.8f),
                    screenBounds.y + margin
                );
                break;

            case SpawnSide.Bottom:
                position = new Vector2(
                    Random.Range(-screenBounds.x * 0.8f, screenBounds.x * 0.8f),
                    -screenBounds.y - margin
                );
                break;

            case SpawnSide.Left:
                position = new Vector2(
                    -screenBounds.x - margin,
                    Random.Range(-screenBounds.y * 0.8f, screenBounds.y * 0.8f)
                );
                break;

            case SpawnSide.Right:
                position = new Vector2(
                    screenBounds.x + margin,
                    Random.Range(-screenBounds.y * 0.8f, screenBounds.y * 0.8f)
                );
                break;
        }

        // Ajustes especificos por tipo
        switch (type)
        {
            case HazardType.AcidRain:
                // Las nubes siempre en la parte superior
                if (side != SpawnSide.Top)
                {
                    position.y = screenBounds.y + margin;
                }
                break;

            case HazardType.ToxicCloud:
                // Las nubes toxicas pueden aparecer en cualquier borde
                break;

            case HazardType.Wildfire:
                // Las bolas de fuego aparecen en los bordes
                break;
        }

        return position;
    }

    #endregion

    #region Legacy Pattern Spawning (Bullet Hell)

    private IEnumerator PatternSpawningRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenPatterns, maxTimeBetweenPatterns);
            waitTime /= currentDifficultyMultiplier;
            waitTime = Mathf.Max(waitTime, minSpawnInterval);

            yield return new WaitForSeconds(waitTime);

            AttackPattern randomPattern = attackPatterns[Random.Range(0, attackPatterns.Count)];
            StartCoroutine(ExecutePattern(randomPattern));
        }
    }

    private IEnumerator ExecutePattern(AttackPattern pattern)
    {
        Vector2 spawnPos = GetSpawnPosition(pattern.spawnOrigin);
        float currentAngle = pattern.baseAngle;

        if (pattern.aimAtPlayer && playerTransform != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - (Vector3)spawnPos).normalized;
            currentAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        }

        if (pattern.patternType == PatternType.Burst)
        {
            float angleStep = pattern.spreadAngle / Mathf.Max(1, pattern.projectileCount);
            float startAngle = currentAngle - (pattern.spreadAngle / 2f);

            for (int i = 0; i < pattern.projectileCount; i++)
            {
                float angle = startAngle + (i * angleStep) + (angleStep / 2f);
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                SpawnProjectile(pattern, spawnPos, direction);
            }
        }
        else if (pattern.patternType == PatternType.Stream)
        {
            float timer = 0f;
            float timeBetweenShots = 1f / pattern.fireRate;
            float nextShotTime = 0f;

            while (timer < pattern.duration)
            {
                if (Time.time >= nextShotTime)
                {
                    float angleStep = pattern.spreadAngle / Mathf.Max(1, pattern.projectileCount);
                    float startAngle = currentAngle - (pattern.spreadAngle / 2f);

                    for (int i = 0; i < pattern.projectileCount; i++)
                    {
                        float angle = startAngle + (i * angleStep) + (angleStep / 2f);
                        Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                        SpawnProjectile(pattern, spawnPos, direction);
                    }
                    nextShotTime = Time.time + timeBetweenShots;
                }

                currentAngle += pattern.spinSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void SpawnProjectile(AttackPattern pattern, Vector2 position, Vector2 direction)
    {
        GameObject projectileObj = Instantiate(pattern.hazardPrefab, position, Quaternion.identity, hazardContainer);
        HazardProjectile projectile = projectileObj.GetComponent<HazardProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, pattern.projectileSpeed, pattern.movementType,
                                pattern.waveAmplitude, pattern.waveFrequency);
        }
    }

    private Vector2 GetSpawnPosition(SpawnOrigin origin)
    {
        float margin = 1f;
        if (origin == SpawnOrigin.AtPlayerLocation && playerTransform != null)
        {
            return playerTransform.position;
        }

        float spawnX = 0, spawnY = 0;
        switch (origin)
        {
            case SpawnOrigin.Top:
                spawnX = Random.Range(-screenBounds.x, screenBounds.x);
                spawnY = screenBounds.y + margin;
                break;
            case SpawnOrigin.Bottom:
                spawnX = Random.Range(-screenBounds.x, screenBounds.x);
                spawnY = -screenBounds.y - margin;
                break;
            case SpawnOrigin.Left:
                spawnX = -screenBounds.x - margin;
                spawnY = Random.Range(-screenBounds.y, screenBounds.y);
                break;
            case SpawnOrigin.Right:
                spawnX = screenBounds.x + margin;
                spawnY = Random.Range(-screenBounds.y, screenBounds.y);
                break;
            case SpawnOrigin.Center:
                return mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        return new Vector2(spawnX, spawnY);
    }

    #endregion

    #region Difficulty Scaling

    private IEnumerator DifficultyScalingRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // Cada 10 segundos

            gameTime += 10f;
            currentDifficultyMultiplier = 1f + (gameTime * difficultyIncreaseRate / 60f);
            currentDifficultyMultiplier = Mathf.Clamp(currentDifficultyMultiplier, 1f, 3f);

            Debug.Log($"Dificultad actual: {currentDifficultyMultiplier:F2}x");
        }
    }

    #endregion

    #region Public Methods

    public void SetDifficulty(float multiplier)
    {
        currentDifficultyMultiplier = Mathf.Clamp(multiplier, 0.5f, 5f);
    }

    public void PauseSpawning()
    {
        spawnHazards = false;
        useLegacyPatterns = false;
        StopAllCoroutines();
    }

    public void ResumeSpawning()
    {
        spawnHazards = true;
        useLegacyPatterns = true;

        if (hazardConfigs.Count > 0)
        {
            StartCoroutine(HazardSpawningRoutine());
        }

        if (attackPatterns.Count > 0)
        {
            StartCoroutine(PatternSpawningRoutine());
        }
    }

    #endregion
}