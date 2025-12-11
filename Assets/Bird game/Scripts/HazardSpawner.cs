using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class HazardSpawner : MonoBehaviour
{
    [Header("Configuración de Patrones")]
    [SerializeField] private List<AttackPattern> attackPatterns;
    [SerializeField] private float minTimeBetweenPatterns = 3f;
    [SerializeField] private float maxTimeBetweenPatterns = 6f;

    private Transform playerTransform;
    private Vector2 screenBounds;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        Camera cam = Camera.main;
        screenBounds = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        StartCoroutine(PatternSpawningRoutine());
    }

    private IEnumerator PatternSpawningRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTimeBetweenPatterns, maxTimeBetweenPatterns);
            yield return new WaitForSeconds(waitTime);

            AttackPattern randomPattern = attackPatterns[Random.Range(0, attackPatterns.Count)];
            StartCoroutine(ExecutePattern(randomPattern));
        }
    }

    private IEnumerator ExecutePattern(AttackPattern pattern)
    {
        Vector2 spawnPos = GetSpawnPosition(pattern.spawnOrigin);
        float currentAngle = pattern.baseAngle;

        // Si debe apuntar al jugador, calcula el ángulo inicial
        if (pattern.aimAtPlayer && playerTransform != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - (Vector3)spawnPos).normalized;
            currentAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        }

        if (pattern.patternType == PatternType.Burst)
        {
            // --- LÓGICA DE RÁFAGA (BURST) ---
            float angleStep = pattern.spreadAngle / Mathf.Max(1, pattern.projectileCount);
            float startAngle = currentAngle - (pattern.spreadAngle / 2f);

            for (int i = 0; i < pattern.projectileCount; i++)
            {
                float angle = startAngle + (i * angleStep) + (angleStep / 2f); // Centrar los disparos
                Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                SpawnProjectile(pattern, spawnPos, direction);
            }
        }
        else if (pattern.patternType == PatternType.Stream)
        {
            // --- LÓGICA DE CHORRO (STREAM) ---
            float timer = 0f;
            float timeBetweenShots = 1f / pattern.fireRate;
            float nextShotTime = 0f;

            while (timer < pattern.duration)
            {
                if (Time.time >= nextShotTime)
                {
                    // La lógica del Burst, pero ejecutada repetidamente en el tiempo
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

                // Rotar el ángulo para el siguiente disparo (espirales)
                currentAngle += pattern.spinSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    private void SpawnProjectile(AttackPattern pattern, Vector2 position, Vector2 direction)
    {
        GameObject projectileObj = Instantiate(pattern.hazardPrefab, position, Quaternion.identity);
        HazardProjectile projectile = projectileObj.GetComponent<HazardProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, pattern.projectileSpeed, pattern.movementType, pattern.waveAmplitude, pattern.waveFrequency);
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
                spawnX = Random.Range(-screenBounds.x, screenBounds.x); spawnY = screenBounds.y + margin; break;
            case SpawnOrigin.Bottom:
                spawnX = Random.Range(-screenBounds.x, screenBounds.x); spawnY = -screenBounds.y - margin; break;
            case SpawnOrigin.Left:
                spawnX = -screenBounds.x - margin; spawnY = Random.Range(-screenBounds.y, screenBounds.y); break;
            case SpawnOrigin.Right:
                spawnX = screenBounds.x + margin; spawnY = Random.Range(-screenBounds.y, screenBounds.y); break;
            case SpawnOrigin.Center:
                return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        return new Vector2(spawnX, spawnY);
    }
}