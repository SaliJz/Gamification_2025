using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuracion de Terreno")]
    [Tooltip("Prefabs de terreno (usa prefabs, no el initialGround de la escena)")]
    [SerializeField] private List<GameObject> groundPrefabs;
    [Tooltip("GameObject ya colocado en la escena que sirve como terreno inicial (no sera destruido).")]
    [SerializeField] private GameObject initialGround;
    [Tooltip("Distancia minima entre el borde superior de la camara y el ultimo terreno")]
    [SerializeField] private float spawnDistanceAhead = 30f;
    [Tooltip("Espacio vertical entre terrenos consecutivos")]
    [SerializeField] private float verticalSpacing = 0f;
    [Tooltip("Margen por debajo de la camara para destruir terrenos antiguos")]
    [SerializeField] private float destroyMargin = 10f;

    private List<GameObject> activeGrounds = new List<GameObject>();
    private float nextSpawnY;
    private float spawnX;
    private bool isInitialized = false;

    private void Start()
    {
        if (groundPrefabs.Count == 0)
        {
            Debug.LogError("[LevelGenerator] No hay prefabs de terreno asignados.");
            return;
        }

        if (initialGround == null)
        {
            Debug.LogError("[LevelGenerator] Debes asignar el terreno inicial.");
            return;
        }

        InitializeGenerator();
    }

    private void InitializeGenerator()
    {
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
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        if (GameManager.Instance == null) return;

        GameState currentState = GameManager.Instance.CurrentState;
        if (currentState != GameState.Playing && currentState != GameState.Tutorial)
        {
            return;
        }

        MoveAllTerrain();
        GenerateTerrainAhead();
        CleanupOldTerrain();
    }

    private void MoveAllTerrain()
    {
        if (GameManager.Instance == null) return;

        float speed = GameManager.Instance.CurrentScrollSpeed;
        float movement = speed * Time.deltaTime;

        foreach (GameObject ground in activeGrounds)
        {
            if (ground != null)
            {
                ground.transform.Translate(Vector3.down * movement);
            }
        }

        nextSpawnY -= movement;
    }

    private void GenerateTerrainAhead()
    {
        float cameraTopY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;

        while (nextSpawnY < cameraTopY + spawnDistanceAhead)
        {
            SpawnGround();
        }
    }

    private void CleanupOldTerrain()
    {
        if (activeGrounds.Count == 0) return;

        float cameraBottomY = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

        for (int i = activeGrounds.Count - 1; i >= 0; i--)
        {
            GameObject ground = activeGrounds[i];
            if (ground == null) continue;

            float groundHeight = GetHeight(ground);
            float groundTopEdge = ground.transform.position.y + (groundHeight / 2f);

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
        int randomIndex = Random.Range(0, groundPrefabs.Count);
        GameObject prefab = groundPrefabs[randomIndex];

        float newHeight = GetPrefabHeight(prefab);
        Vector3 spawnPosition = new Vector3(spawnX, nextSpawnY + (newHeight / 2f), 0f);

        GameObject newGround = Instantiate(prefab, spawnPosition, Quaternion.identity);

        ScrollableObject scrollComponent = newGround.GetComponent<ScrollableObject>();
        if (scrollComponent != null)
        {
            scrollComponent.enabled = false;
        }

        activeGrounds.Add(newGround);
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

    #region Public Methods
    public void ResetGenerator()
    {
        // Limpiar terrenos activos excepto el inicial
        for (int i = activeGrounds.Count - 1; i >= 0; i--)
        {
            GameObject ground = activeGrounds[i];
            if (ground != null && ground != initialGround)
            {
                Destroy(ground);
            }
        }

        activeGrounds.Clear();

        if (initialGround != null)
        {
            InitializeGenerator();
        }
    }
    #endregion
}