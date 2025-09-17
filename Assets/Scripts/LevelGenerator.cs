using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración de Terreno")]
    [Tooltip("Prefabs de terreno (usa prefabs, no el initialGround de la escena)")]
    [SerializeField] private List<GameObject> groundPrefabs;
    [Tooltip("GameObject ya colocado en la escena que sirve como terreno inicial (no será destruido).")]
    [SerializeField] private GameObject initialGround;
    [Tooltip("Número máximo de prefabs activos en escena")]
    [SerializeField] private int maxActiveGrounds = 6;
    [Tooltip("Margen por encima de la cámara para spawnear nuevos terrenos")]
    [SerializeField] private float spawnOffset = 0.25f;
    [Tooltip("Espacio vertical entre terrenos consecutivos")]
    [SerializeField] private float verticalSpacing = 0f;
    [Tooltip("Margen por debajo de la cámara para destruir terrenos antiguos")]
    [SerializeField] private float destroyMargin = 0.5f;

    private Queue<GameObject> activeGrounds = new Queue<GameObject>();
    private GameObject lastSpawnedGround;
    private float lastGroundHeight;
    private float spawnX;

    private void Start()
    {
        if (groundPrefabs.Count == 0)
        {
            Debug.LogError("No hay prefabs de terreno asignados en el LevelGenerator.");
            return;
        }

        if (initialGround == null)
        {
            Debug.LogError("Debes asignar el terreno inicial en LevelGenerator.");
            return;
        }

        initialGround.SetActive(true);
        activeGrounds.Enqueue(initialGround);
        lastSpawnedGround = initialGround;
        lastGroundHeight = GetHeight(initialGround);
        spawnX = initialGround.transform.position.x;

        while (activeGrounds.Count < maxActiveGrounds)
        {
            SpawnGround();
        }
    }

    private void Update()
    {
        while (activeGrounds.Count < maxActiveGrounds)
        {
            SpawnGround();
        }

        if (activeGrounds.Count > 0)
        {
            float cameraBottomEdge = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
            GameObject oldest = activeGrounds.Peek();

            float oldestTopEdge = oldest.transform.position.y + (GetHeight(oldest) / 2f);
            if (oldestTopEdge < cameraBottomEdge - destroyMargin)
            {
                GameObject removed = activeGrounds.Dequeue();
                if (removed == initialGround)
                {
                    removed.SetActive(false);
                    Debug.Log("[LevelGenerator] initialGround desactivado en vez de destruido.");
                }
                else
                {
                    Destroy(removed);
                }
            }
        }
    }

    private void SpawnGround()
    {
        // Elege prefab aleatorio
        int randomIndex = Random.Range(0, groundPrefabs.Count);
        GameObject prefab = groundPrefabs[randomIndex];

        // Calcula altura del prefab (a partir del prefab o su SpriteRenderer)
        float newHeight = GetPrefabHeight(prefab);

        Vector3 spawnPosition;

        if (lastSpawnedGround != null)
        {
            float lastTopEdge = lastSpawnedGround.transform.position.y + (lastGroundHeight / 2f);
            float newY = lastTopEdge + (newHeight / 2f) + verticalSpacing;
            spawnPosition = new Vector3(spawnX, newY, 0f);
        }
        else
        {
            float cameraTop = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
            float newY = cameraTop + (newHeight / 2f) + spawnOffset;
            spawnPosition = new Vector3(spawnX, newY, 0f);
        }

        GameObject newGround = Instantiate(prefab, spawnPosition, Quaternion.identity);

        activeGrounds.Enqueue(newGround);

        lastSpawnedGround = newGround;
        lastGroundHeight = GetHeight(newGround);
    }

    private float GetHeight(GameObject gameObject)
    {
        if (gameObject == null) return 0.0f;

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null) return spriteRenderer.sprite.bounds.size.y * gameObject.transform.localScale.y;

        Renderer r = gameObject.GetComponent<Renderer>();
        if (r != null) return r.bounds.size.y;

        return 1f;
    }

    private float GetPrefabHeight(GameObject prefab)
    {
        if (prefab == null) return 1f;
        SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null) return spriteRenderer.sprite.bounds.size.y * prefab.transform.localScale.y;

        Renderer renderer = prefab.GetComponent<Renderer>();
        if (renderer != null) return renderer.bounds.size.y;

        return 1f;
    }
}