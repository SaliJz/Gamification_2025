using UnityEngine;
using System.Collections;

/// <summary>
/// Configuración de escalado de dificultad para hazards.
/// Define los valores mínimos y máximos para cada tipo de hazard.
/// </summary>
[System.Serializable]
public class DifficultyScaling
{
    [Header("Wildfire Scaling")]
    [Tooltip("Proyectiles mínimos en explosión")]
    public int wildfireMinProjectiles = 4;
    [Tooltip("Proyectiles máximos en explosión")]
    public int wildfireMaxProjectiles = 16;
    [Tooltip("Velocidad mínima de proyectiles")]
    public float wildfireMinSpeed = 3f;
    [Tooltip("Velocidad máxima de proyectiles")]
    public float wildfireMaxSpeed = 12f;

    [Header("Acid Rain Scaling")]
    [Tooltip("Gotas mínimas por ciclo")]
    public int acidRainMinDroplets = 4;
    [Tooltip("Gotas máximas por ciclo")]
    public int acidRainMaxDroplets = 32;
    [Tooltip("Frecuencia mínima (segundos entre spawns)")]
    public float acidRainMinFrequency = 1f;
    [Tooltip("Frecuencia máxima (menor = más rápido)")]
    public float acidRainMaxFrequency = 0.5f;

    [Header("Toxic Cloud Scaling")]
    [Tooltip("Velocidad mínima de movimiento")]
    public float toxicCloudMinSpeed = 3f;
    [Tooltip("Velocidad máxima de movimiento")]
    public float toxicCloudMaxSpeed = 16f;

    /// <summary>
    /// Interpola entre min y max basado en el multiplicador de dificultad (1.0 a 3.0)
    /// </summary>
    public static float Lerp(float min, float max, float difficultyMultiplier)
    {
        float t = Mathf.InverseLerp(1f, 3f, difficultyMultiplier);
        return Mathf.Lerp(min, max, t);
    }

    /// <summary>
    /// Versión entera de Lerp
    /// </summary>
    public static int LerpInt(int min, int max, float difficultyMultiplier)
    {
        return Mathf.RoundToInt(Lerp(min, max, difficultyMultiplier));
    }
}

/// <summary>
/// Gestor global de dificultad. Singleton que escala la dificultad con el tiempo.
/// Todos los hazards consultan este manager para obtener sus valores escalados.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    private static DifficultyManager instance;

    public static DifficultyManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("DifficultyManager");
                instance = go.AddComponent<DifficultyManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Difficulty Progression")]
    [Tooltip("Incremento de dificultad por minuto de juego")]
    [SerializeField] private float difficultyIncreaseRate = 0.1f;

    [Tooltip("Intervalo de actualización en segundos")]
    [SerializeField] private float updateInterval = 10f;

    [Tooltip("Dificultad mínima (inicio del juego)")]
    [SerializeField] private float minDifficulty = 1f;

    [Tooltip("Dificultad máxima (cap)")]
    [SerializeField] private float maxDifficulty = 3f;

    [Header("Scaling Configuration")]
    public DifficultyScaling scaling = new DifficultyScaling();

    private float currentDifficulty = 1f;
    private float gameTime = 0f;
    private bool isPaused = false;

    /// <summary>
    /// Multiplicador de dificultad actual (1.0 a 3.0)
    /// </summary>
    public float CurrentDifficulty => currentDifficulty;

    /// <summary>
    /// Tiempo de juego en segundos
    /// </summary>
    public float GameTime => gameTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(DifficultyUpdateRoutine());
    }

    private IEnumerator DifficultyUpdateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            if (!isPaused)
            {
                gameTime += updateInterval;

                // Fórmula: dificultad = min + (tiempo * rate / 60)
                currentDifficulty = minDifficulty + (gameTime * difficultyIncreaseRate / 60f);
                currentDifficulty = Mathf.Clamp(currentDifficulty, minDifficulty, maxDifficulty);

                Debug.Log($"[DifficultyManager] Dificultad: {currentDifficulty:F2}x | Tiempo: {FormatTime(gameTime)}");
            }
        }
    }

    #region Public Methods

    /// <summary>
    /// Establece manualmente la dificultad
    /// </summary>
    public void SetDifficulty(float difficulty)
    {
        currentDifficulty = Mathf.Clamp(difficulty, minDifficulty, maxDifficulty);
        Debug.Log($"[DifficultyManager] Dificultad establecida manualmente a: {currentDifficulty:F2}x");
    }

    /// <summary>
    /// Reinicia la dificultad y el tiempo a valores iniciales
    /// </summary>
    public void ResetDifficulty()
    {
        currentDifficulty = minDifficulty;
        gameTime = 0f;
        Debug.Log("[DifficultyManager] Dificultad reiniciada");
    }

    /// <summary>
    /// Pausa el incremento de dificultad
    /// </summary>
    public void PauseDifficulty()
    {
        isPaused = true;
    }

    /// <summary>
    /// Reanuda el incremento de dificultad
    /// </summary>
    public void ResumeDifficulty()
    {
        isPaused = false;
    }

    /// <summary>
    /// Obtiene el progreso de dificultad como porcentaje (0 a 1)
    /// </summary>
    public float GetDifficultyProgress()
    {
        return Mathf.InverseLerp(minDifficulty, maxDifficulty, currentDifficulty);
    }

    #endregion

    #region Helper Methods

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{minutes:00}:{secs:00}";
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, 10, 300, 30),
                $"Dificultad: {currentDifficulty:F2}x ({GetDifficultyProgress() * 100:F0}%)", style);
            GUI.Label(new Rect(10, 35, 300, 30),
                $"Tiempo: {FormatTime(gameTime)}", style);
        }
    }

    #endregion
}