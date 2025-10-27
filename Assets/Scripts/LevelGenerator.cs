using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración de Terreno")]
    [Tooltip("Prefabs de terreno (usa prefabs, no el initialGround de la escena)")]
    [SerializeField] private List<GameObject> groundPrefabs;
    [Tooltip("GameObject ya colocado en la escena que sirve como terreno inicial (no será destruido).")]
    [SerializeField] private GameObject initialGround;
    [Tooltip("Distancia mínima entre el borde superior de la cámara y el último terreno")]
    [SerializeField] private float spawnDistanceAhead = 30f;
    [Tooltip("Espacio vertical entre terrenos consecutivos")]
    [SerializeField] private float verticalSpacing = 0f;
    [Tooltip("Margen por debajo de la cámara para destruir terrenos antiguos")]
    [SerializeField] private float destroyMargin = 10f;

    private List<GameObject> activeGrounds = new List<GameObject>();
    private float nextSpawnY;
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

        ScrollableObject scrollComponent = initialGround.GetComponent<ScrollableObject>();
        if (scrollComponent != null)
        {
            scrollComponent.enabled = false;
        }

        initialGround.SetActive(true);
        activeGrounds.Add(initialGround);
        spawnX = initialGround.transform.position.x;

        float initialHeight = GetHeight(initialGround);
        nextSpawnY = initialGround.transform.position.y + (initialHeight / 2f) + verticalSpacing;

        GenerateTerrainAhead();
    }

    private void Update()
    {
        // Mueve todos los terrenos activos hacia abajo
        MoveAllTerrain();

        // Genera nuevo terreno si es necesario
        GenerateTerrainAhead();

        // Limpia terrenos antiguos que ya no son visibles
        CleanupOldTerrain();
    }

    private void MoveAllTerrain()
    {
        float speed = GameManager.Instance.CurrentScrollSpeed;
        float movement = speed * Time.deltaTime;

        // Mueve todos los terrenos activos
        foreach (GameObject ground in activeGrounds)
        {
            if (ground != null)
            {
                ground.transform.Translate(Vector3.down * movement);
            }
        }

        // Ajusta también la posición del próximo spawn
        nextSpawnY -= movement;
    }

    private void GenerateTerrainAhead()
    {
        float cameraTopY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;

        // Genera terreno mientras el próximo punto de spawn esté dentro del rango
        while (nextSpawnY < cameraTopY + spawnDistanceAhead)
        {
            SpawnGround();
        }
    }

    private void CleanupOldTerrain()
    {
        if (activeGrounds.Count == 0) return;

        float cameraBottomY = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

        // Revisa todos los terrenos desde el más antiguo
        for (int i = activeGrounds.Count - 1; i >= 0; i--)
        {
            GameObject ground = activeGrounds[i];
            if (ground == null) continue;

            float groundHeight = GetHeight(ground);
            float groundTopEdge = ground.transform.position.y + (groundHeight / 2f);

            // Si el borde superior está por debajo del límite, lo destruye
            if (groundTopEdge < cameraBottomY - destroyMargin)
            {
                activeGrounds.RemoveAt(i);

                if (ground == initialGround)
                {
                    ground.SetActive(false);
                    Debug.Log("[LevelGenerator] initialGround desactivado.");
                }
                else
                {
                    Destroy(ground);
                }
            }
        }
    }

    private void SpawnGround()
    {
        // Elige prefab aleatorio
        int randomIndex = Random.Range(0, groundPrefabs.Count);
        GameObject prefab = groundPrefabs[randomIndex];

        // Calcula altura del nuevo prefab
        float newHeight = GetPrefabHeight(prefab);

        // Posiciona en nextSpawnY
        Vector3 spawnPosition = new Vector3(spawnX, nextSpawnY + (newHeight / 2f), 0f);

        GameObject newGround = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Desactiva ScrollableObject para evitar movimiento doble
        ScrollableObject scrollComponent = newGround.GetComponent<ScrollableObject>();
        if (scrollComponent != null)
        {
            scrollComponent.enabled = false;
        }

        activeGrounds.Add(newGround);

        // Actualiza la posición del siguiente spawn
        nextSpawnY += newHeight + verticalSpacing;
    }

    private float GetHeight(GameObject gameObject)
    {
        if (gameObject == null) return 1f;

        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            return sr.sprite.bounds.size.y * gameObject.transform.localScale.y;

        Renderer r = gameObject.GetComponent<Renderer>();
        if (r != null) return r.bounds.size.y;

        return 1f;
    }

    private float GetPrefabHeight(GameObject prefab)
    {
        if (prefab == null) return 1f;

        SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            return sr.sprite.bounds.size.y * prefab.transform.localScale.y;

        Renderer r = prefab.GetComponent<Renderer>();
        if (r != null) return r.bounds.size.y;

        return 1f;
    }
}