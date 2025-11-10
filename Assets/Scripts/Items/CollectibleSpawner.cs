using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la aparición de coleccionables
/// durante el estado de juego 'Playing'.
/// </summary>
public class CollectibleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleSpawnConfig
    {
        public string name;
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance = 0.5f;
        public float minTimeBetweenSpawns = 5f;
        public float maxTimeBetweenSpawns = 10f;
        [HideInInspector] public float nextSpawnTime = 0f;
    }

    [Header("Configuración de Spawneo")]
    [SerializeField] private List<CollectibleSpawnConfig> collectibles;

    private Camera mainCamera;
    private Vector2 screenBounds;
    private bool isSpawning = false;

    private void Start()
    {
        mainCamera = Camera.main;
        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
        {
            isSpawning = false;
            return;
        }

        if (!isSpawning)
        {
            isSpawning = true;
            ResetSpawnTimers();
        }

        HandleSpawning();
    }

    private void ResetSpawnTimers()
    {
        // Reiniciar contadores al empezar a jugar
        foreach (var config in collectibles)
        {
            config.nextSpawnTime = Time.time + Random.Range(config.minTimeBetweenSpawns, config.maxTimeBetweenSpawns);
        }
    }

    private void HandleSpawning()
    {
        float difficulty = (DifficultyManager.Instance != null) ? DifficultyManager.Instance.CurrentDifficulty : 1f;

        foreach (var config in collectibles)
        {
            if (Time.time >= config.nextSpawnTime)
            {
                // Calcular próximo spawn (afectado por dificultad)
                float baseWait = Random.Range(config.minTimeBetweenSpawns, config.maxTimeBetweenSpawns);
                config.nextSpawnTime = Time.time + (baseWait / difficulty);

                // Intentar spawnear
                if (Random.value <= config.spawnChance)
                {
                    SpawnCollectible(config.prefab);
                }
            }
        }
    }

    private void SpawnCollectible(GameObject prefab)
    {
        if (prefab == null) return;

        // Posición de spawn: Aleatoria en X, justo encima de la pantalla
        float randomX = Random.Range(-screenBounds.x * 0.9f, screenBounds.x * 0.9f);
        Vector2 spawnPosition = new Vector2(randomX, screenBounds.y + 2f);

        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}