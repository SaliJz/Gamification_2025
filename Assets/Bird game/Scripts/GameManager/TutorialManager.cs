using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el tutorial completo del juego, incluyendo hazards y coleccionables.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Configuration")]
    [SerializeField] private float delayBetweenSteps = 1.5f;

    [Header("Dialogue System")]
    [SerializeField] private DialogueBox dialogueBox;

    [Header("Hazard Prefabs")]
    [SerializeField] private GameObject toxicCloudPrefab;
    [SerializeField] private GameObject wildfirePrefab;
    [SerializeField] private GameObject acidRainPrefab;

    [Header("Collectible Prefabs")]
    [SerializeField] private GameObject flockBirdPrefab;
    [SerializeField] private GameObject insectPrefab;
    [SerializeField] private GameObject magnetBirdPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private float hazardLifetime = 10f;
    [SerializeField] private Vector2 topSpawnPosition = new Vector2(0, 10f);
    [SerializeField] private Vector2 leftSpawnPosition = new Vector2(-8f, 5f);

    private Camera mainCamera;
    private Transform playerTransform;
    private BaseHazard currentTutorialHazard;
    private GameObject currentTutorialCollectible;
    private bool tutorialActive = false;

    #region Tutorial Dialogues
    private readonly Dictionary<string, string> tutorialDialogues = new Dictionary<string, string>()
    {
        // Introducción
        {"inicio", "¡Hola! Soy la gaviota Franklin, y necesito tu ayuda para sobrevivir."},

        // Movimiento
        {"movement", "Desliza tu dedo en la dirección que quieras que vuele. Aparecerá un círculo en la pantalla para movernos."},
        
        // Hazards
        {"toxiccloud_intro", "¡Cuidado! Esta es una Nube Tóxica. Se mueve lentamente de izquierda o derecha pero causa daño continuo."},
        {"toxiccloud_advice", "Consejo: Mantengamonos alejados de las nubes. No pueden destruirse, solo esquivarse."},

        {"wildfire_intro", "¡Atención! Esta es una Bola de Fuego. ¡Explota en múltiples direcciones!"},
        {"wildfire_advice", "Consejo: Mantengamonos a distancia cuando se vea una. Explota después de un breve tiempo."},

        {"acidrain_intro", "¡Alerta! Una nube de tormenta producto de la contaminación lumínica aparece desde arriba."},
        {"acidrain_advice", "Consejo: Observemos dónde caen los rayos y movamonos hacia los espacios seguros."},
        
        // Collectibles
        {"flockbird_intro", "¡Mira! Otras aves vienen a ayudarnos. Juntas formamos una bandada protectora."},
        {"flockbird_advice", "Cada ave en nuestra bandada puede recibir daño por nosotros. ¡Recojamos todas las que podamos!"},

        {"insect_intro", "¡Insectos! Son tu alimento y te dan puntos extras."},
        {"insect_advice", "Cada insecto vale +10 puntos. ¡Recoge todos los que puedas!"},

        {"magnetbird_intro", "¡Ave Imán! Nos ayudará a recolectar insectos automáticamente."},
        {"magnetbird_advice", "Cuando la recojas, orbitará a tu alrededor y recogerá insectos cercanos."},
        
        // Final
        {"tutorial_complete", "¡Ya aprendiste lo necesario! Tu misión es ayudarme a resistir y cuidar los humedales. ¡Vamos a volar!"}
    };
    #endregion

    private void Awake()
    {
        mainCamera = Camera.main;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    #region Tutorial Flow
    public void StartTutorial()
    {
        if (tutorialActive) return;

        tutorialActive = true;
        StartCoroutine(TutorialSequence());
    }

    private IEnumerator TutorialSequence()
    {
        // Por la nueva recomendada:
        ImprovedHazardSpawner spawner = Object.FindFirstObjectByType<ImprovedHazardSpawner>();
        if (spawner != null)
        {
            spawner.PauseSpawning();
        }

        // 1. Introducción al movimiento
        yield return StartCoroutine(ShowMovementTutorial());

        // 2. Tutorial de Hazards
        yield return StartCoroutine(ShowHazardTutorial("ToxicCloud", toxicCloudPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowHazardTutorial("Wildfire", wildfirePrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowHazardTutorial("AcidRain", acidRainPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        // 3. Tutorial de Coleccionables
        yield return StartCoroutine(ShowCollectibleTutorial("FlockBird", flockBirdPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowCollectibleTutorial("Insect", insectPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowCollectibleTutorial("MagnetBird", magnetBirdPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        // 4. Finalizar tutorial
        yield return StartCoroutine(ShowCompletionMessage());

        // Reactivar spawner
        if (spawner != null)
        {
            spawner.ResumeSpawning();
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResumeDifficulty();
        }

        tutorialActive = false;
        GameManager.Instance?.OnTutorialCompleted();
    }
    #endregion

    #region Movement Tutorial
    private IEnumerator ShowMovementTutorial()
    {
        PauseGame();
        yield return StartCoroutine(ShowDialogue("inicio"));
        yield return StartCoroutine(ShowDialogue("movement"));
        ResumeGame();

        // Esperar 3 segundos para que el jugador practique
        yield return new WaitForSeconds(3f);
    }
    #endregion

    #region Hazard Tutorials
    private IEnumerator ShowHazardTutorial(string hazardType, GameObject hazardPrefab)
    {
        if (hazardPrefab == null)
        {
            Debug.LogWarning($"Prefab de {hazardType} no asignado!");
            yield break;
        }

        // Spawner el hazard
        Vector2 spawnPos = GetSpawnPositionForHazard(hazardType);
        GameObject hazardObj = Instantiate(hazardPrefab, spawnPos, Quaternion.identity);

        BaseHazard hazard = hazardObj.GetComponent<BaseHazard>();
        currentTutorialHazard = hazard;

        // Pausar y mostrar intro
        PauseGame();
        yield return StartCoroutine(ShowDialogue($"{hazardType.ToLower()}_intro"));
        yield return StartCoroutine(ShowDialogue($"{hazardType.ToLower()}_advice"));
        ResumeGame();

        // Inicializar hazard si existe
        if (hazard != null)
        {
            SpawnSide spawnSide = GetSpawnSideForHazard(hazardType);
            hazard.Initialize(spawnPos, spawnSide, playerTransform);
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.PauseDifficulty();
            DifficultyManager.Instance.SetDifficulty(1f);
        }

        // Esperar a que el hazard termine o sea destruido
        float timeout = hazardLifetime;
        while (hazardObj != null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Limpiar si aún existe
        if (hazardObj != null)
        {
            Destroy(hazardObj);
        }

        currentTutorialHazard = null;
    }

    private Vector2 GetSpawnPositionForHazard(string hazardType)
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        switch (hazardType)
        {
            case "ToxicCloud":
                return new Vector2(-screenBounds.x - 1f, 0f); // Desde la izquierda

            case "Wildfire":
                return new Vector2(0f, screenBounds.y + 1f); // Desde arriba centro

            case "AcidRain":
                return new Vector2(0f, screenBounds.y + 2f); // Desde arriba

            default:
                return topSpawnPosition;
        }
    }

    private SpawnSide GetSpawnSideForHazard(string hazardType)
    {
        switch (hazardType)
        {
            case "ToxicCloud":
                return SpawnSide.Left;
            case "Wildfire":
                return SpawnSide.Top;
            case "AcidRain":
                return SpawnSide.Top;
            default:
                return SpawnSide.Top;
        }
    }
    #endregion

    #region Collectible Tutorials
    private IEnumerator ShowCollectibleTutorial(string collectibleType, GameObject collectiblePrefab)
    {
        if (collectiblePrefab == null)
        {
            Debug.LogWarning($"Prefab de {collectibleType} no asignado!");
            yield break;
        }

        // Spawner el coleccionable
        Vector2 spawnPos = GetSpawnPositionForCollectible();
        GameObject collectibleObj = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
        currentTutorialCollectible = collectibleObj;

        // Pausar y mostrar intro
        PauseGame();
        yield return StartCoroutine(ShowDialogue($"{collectibleType.ToLower()}_intro"));
        yield return StartCoroutine(ShowDialogue($"{collectibleType.ToLower()}_advice"));
        ResumeGame();

        if (collectibleType.Equals("MagnetBird"))
        {
            StartCoroutine(SpawnTutorialInsects());
        }

        // Esperar a que sea recogido o salga de pantalla
        float timeout = 15f;
        while (collectibleObj != null && timeout > 0)
        {
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToViewportPoint(collectibleObj.transform.position);
                if (screenPos.y < -0.1f) // Solo revisa si salió por abajo
                {
                    break;
                }
            }
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Limpiar si aún existe
        if (collectibleObj != null)
        {
            Destroy(collectibleObj);
        }

        currentTutorialCollectible = null;
    }

    /// <summary>
    /// Genera 3 insectos de demostración para el tutorial del Ave Imán.
    /// </summary>
    private IEnumerator SpawnTutorialInsects()
    {
        if (insectPrefab == null)
        {
            Debug.LogWarning("Insect Prefab no asignado en TutorialManager.");
            yield break;
        }

        // Esperar un par de segundos para dar tiempo a recoger el imán
        yield return new WaitForSeconds(2.0f);

        for (int i = 0; i < 3; i++)
        {
            // No continuar si el tutorial se saltó o terminó
            if (!tutorialActive) yield break;

            // Spawnear un insecto
            Vector2 spawnPos = GetSpawnPositionForCollectible(); // Usa la misma lógica de spawn
            Instantiate(insectPrefab, spawnPos, Quaternion.identity);

            // Esperar 3 segundos
            yield return new WaitForSeconds(3.0f);
        }
    }

    private Vector2 GetSpawnPositionForCollectible()
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float randomX = Random.Range(-screenBounds.x * 0.5f, screenBounds.x * 0.5f);
        return new Vector2(randomX, screenBounds.y + 2f);
    }

    private bool IsOffScreen(Vector3 position)
    {
        Vector3 screenPos = mainCamera.WorldToViewportPoint(position);
        return screenPos.y < -0.1f || screenPos.y > 1.1f || screenPos.x < -0.1f || screenPos.x > 1.1f;
    }
    #endregion

    #region Completion
    private IEnumerator ShowCompletionMessage()
    {
        PauseGame();
        yield return StartCoroutine(ShowDialogue("tutorial_complete"));
        ResumeGame();
    }
    #endregion

    #region Dialogue System
    private IEnumerator ShowDialogue(string dialogueKey)
    {
        if (dialogueBox == null)
        {
            Debug.LogWarning("DialogueBox no asignado!");
            yield return new WaitForSecondsRealtime(2f);
            yield break;
        }

        if (tutorialDialogues.TryGetValue(dialogueKey, out string text))
        {
            dialogueBox.ShowDialogue(text);

            // Esperar a que el jugador presione continuar
            yield return new WaitUntil(() => dialogueBox.IsWaitingForInput == false);
        }
        else
        {
            Debug.LogWarning($"Diálogo no encontrado: {dialogueKey}");
            yield return new WaitForSecondsRealtime(2f);
        }
    }
    #endregion

    #region Pause Management
    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }
    #endregion

    #region Public Methods
    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

    public void SkipTutorial()
    {
        if (!tutorialActive) return;

        StopAllCoroutines();

        // Limpiar objetos del tutorial
        if (currentTutorialHazard != null)
        {
            Destroy(currentTutorialHazard.gameObject);
        }

        if (currentTutorialCollectible != null)
        {
            Destroy(currentTutorialCollectible);
        }

        tutorialActive = false;
        ResumeGame();

        GameManager.Instance?.OnTutorialCompleted();
    }
    #endregion
}