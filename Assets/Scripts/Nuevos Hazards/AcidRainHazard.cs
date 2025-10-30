using UnityEngine;
using System.Collections;

/// <summary>
/// Hazard de lluvia ácida. Una nube que genera gotas que caen.
/// </summary>
public class AcidRainHazard : BaseHazard
{
    [Header("Acid Rain Configuration")]
    [SerializeField] private AcidRainPattern pattern = AcidRainPattern.SteadyDrizzle;
    [SerializeField] private GameObject dropletPrefab;
    [SerializeField] private float dropSpeed = 5f;
    [SerializeField] private float dropSpawnRate = 0.2f;
    [SerializeField] private int dropletsPerSpawn = 1;
    [SerializeField] private float cloudDriftSpeed = 0.5f;
    [SerializeField] private float cloudDriftRange = 2f;

    [Header("Entry Behavior")]
    [SerializeField] private float yTargetOffsetFromTop = 1.0f; // Qué tan abajo del borde superior quedará
    [SerializeField] private float entrySpeed = 3f; // Velocidad de descenso

    private Vector2 initialPosition; // La posición después de descender
    private float nextDropTime = 0f;
    private float driftTime = 0f;
    private Vector2 screenBounds;

    public override void Initialize(Vector2 position, SpawnSide spawnSide, Transform player)
    {
        transform.position = position;
        playerTransform = player;
        isInitialized = true;
        isActive = false;

        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        StartCoroutine(EnterAndActivate(position));
    }

    /// <summary>
    /// Corrutina que primero mueve la nube a su posición y luego inicia la secuencia de advertencia.
    /// </summary>
    private IEnumerator EnterAndActivate(Vector2 startPos)
    {
        // 1. Calcular la posición objetivo
        Vector2 targetPos = new Vector2(startPos.x, screenBounds.y - yTargetOffsetFromTop);

        // 2. Mover la nube a la posición objetivo
        while (Vector2.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, entrySpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        // 3. Establecer esta como la 'initialPosition' para el drift.
        this.initialPosition = targetPos;

        // 4. Ahora, y solo ahora, iniciar la secuencia de advertencia de BaseHazard.Initialize
        StartCoroutine(WarningSequence());
    }

    protected override void OnHazardActivated()
    {
        // La nube comienza a generar gotas
        nextDropTime = Time.time;
    }

    protected override void UpdateHazardBehavior()
    {
        // Movimiento vertical sutil de la nube
        UpdateCloudDrift();

        // Generar gotas según el patrón
        if (Time.time >= nextDropTime)
        {
            SpawnDroplets();
            nextDropTime = Time.time + dropSpawnRate;
        }
    }

    private void UpdateCloudDrift()
    {
        driftTime += Time.deltaTime * cloudDriftSpeed;
        float offset = Mathf.Sin(driftTime) * cloudDriftRange;
        Vector2 newPos = initialPosition;
        newPos.y += offset;
        transform.position = newPos;
    }

    private void SpawnDroplets()
    {
        switch (pattern)
        {
            case AcidRainPattern.SteadyDrizzle:
                SpawnSteadyDrizzle();
                break;
            case AcidRainPattern.IntensePour:
                SpawnIntensePour();
                break;
            case AcidRainPattern.WavingCurtain:
                SpawnWavingCurtain();
                break;
        }
    }

    private void SpawnSteadyDrizzle()
    {
        // Gotas espaciadas uniformemente
        for (int i = 0; i < dropletsPerSpawn; i++)
        {
            float xOffset = Random.Range(-1f, 1f);
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(xOffset, 0);
            CreateDroplet(spawnPos, Vector2.down);
        }
    }

    private void SpawnIntensePour()
    {
        // Ráfaga intensa de gotas
        int burstCount = dropletsPerSpawn * 3;
        for (int i = 0; i < burstCount; i++)
        {
            float xOffset = Random.Range(-2f, 2f);
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(xOffset, 0);
            CreateDroplet(spawnPos, Vector2.down);
        }
    }

    private void SpawnWavingCurtain()
    {
        // Cortina ondulante
        int curtainDrops = 5;
        for (int i = 0; i < curtainDrops; i++)
        {
            float t = (float)i / curtainDrops;
            float xOffset = Mathf.Sin(t * Mathf.PI * 2 + driftTime * 2) * 2f;
            Vector2 spawnPos = (Vector2)transform.position + new Vector2(xOffset, 0);

            // Dirección ligeramente diagonal
            Vector2 direction = new Vector2(Mathf.Sin(driftTime + t), -1f).normalized;
            CreateDroplet(spawnPos, direction);
        }
    }

    private void CreateDroplet(Vector2 position, Vector2 direction)
    {
        if (dropletPrefab == null) return;

        GameObject droplet = Instantiate(dropletPrefab, position, Quaternion.identity);
        HazardProjectile proj = droplet.GetComponent<HazardProjectile>();
        if (proj != null)
        {
            proj.Initialize(direction, dropSpeed, MovementType.Linear);
        }
    }
}