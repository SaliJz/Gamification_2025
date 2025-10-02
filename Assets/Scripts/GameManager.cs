using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public event Action<int> OnGameOver;

    [Header("Configuración del Juego")]
    [SerializeField] private float initialScrollSpeed = 5f;
    [SerializeField] private float speedIncreaseRate = 0.1f; // Cuánto aumenta la velocidad por segundo
    public float CurrentScrollSpeed { get; private set; }

    [Header("Sistema de Bandada")]
    [SerializeField] private int maxFlockSize = 5;
    [SerializeField] private float scoreMultiplierPerBird = 0.2f; // +20% de puntos por cada ave extra
    [SerializeField] private int flockSize = 1;

    [Header("Sistema de Puntuación")]
    [SerializeField] private float score = 0f;
    [SerializeField] private float distanceTraveled = 0f;

    [Header("Aparición de Aves")]
    [SerializeField] private GameObject birdToSpawnPrefab; // El prefab del ave coleccionable
    [SerializeField] private float distanceForGuaranteedSpawn = 200f;
    private float distanceTrackerForSpawn = 0f;

    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI flockSizeText;

    public float DistanceTraveled => distanceTraveled;
    public int Score => Mathf.FloorToInt(score);

    public int FlockSize
    {
        get {  return flockSize; }
        set {  flockSize = value; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        CurrentScrollSpeed = initialScrollSpeed;
        flockSize = 1;
        screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        UpdateUI();
    }

    private void Update()
    {
        // 1. Aumenta la velocidad del juego progresivamente
        CurrentScrollSpeed += speedIncreaseRate * Time.deltaTime;

        // 2. Calcula distancia y puntaje
        float distanceThisFrame = CurrentScrollSpeed * Time.deltaTime;
        distanceTraveled += distanceThisFrame;

        // El puntaje aumenta más por cada ave adicional en la bandada
        float currentScoreMultiplier = 1 + (flockSize - 1) * scoreMultiplierPerBird;
        score += distanceThisFrame * currentScoreMultiplier;

        // 3. Gestiona la aparición de la próxima ave
        if (flockSize < maxFlockSize)
        {
            distanceTrackerForSpawn += distanceThisFrame;
            // La probabilidad aumenta linealmente con la distancia recorrida
            if (distanceTrackerForSpawn >= distanceForGuaranteedSpawn)
            {
                SpawnBird();
                distanceTrackerForSpawn = 0; // Reinicia el contador
            }
        }

        // 4. Actualizar la UI
        UpdateUI();
    }

    public void PlayerTookDamage()
    {
        if (flockSize > 1)
        {
            flockSize--;
            // Añadir un efecto visual/sonido de impacto
        }

        if (flockSize <= 0)
        {
            TriggerGameOver();
        }
    }

    public void AddBirdToFlock()
    {
        if (flockSize < maxFlockSize)
        {
            flockSize++;
            // Por si se añade un efecto visual/sonido de recolección
            Debug.Log("Ave añadida a la bandada. Tamaño actual: " + flockSize);
        }
    }

    private void SpawnBird()
    {
        // Calcula una posición aleatoria en el ancho de la pantalla, un poco por encima de la vista actual
        float spawnX = Random.Range(-screenWidthInWorldUnits, screenWidthInWorldUnits);
        float spawnY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y + 5f; // 5 unidades por encima

        Instantiate(birdToSpawnPrefab, new Vector3(spawnX, spawnY, 0), Quaternion.identity);
    }

    private void UpdateUI()
    {
        scoreText.text = "Puntaje: " + Mathf.FloorToInt(score);
        flockSizeText.text = "Bandada: " + flockSize;
    }

    private void TriggerGameOver()
    {
        Debug.Log("GAME OVER. Puntaje final: " + Mathf.FloorToInt(Score));
        Time.timeScale = 0;
        OnGameOver?.Invoke(Score);
    }

    public float screenWidthInWorldUnits { get; private set; }
}