using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialManagerV2 : MonoBehaviour
{
    [Header("Tutorial Configuration")]
    [SerializeField] private float delayBetweenSteps = 1.5f;

    [Header("Dialogue System")]
    [SerializeField] private DialogueBox dialogueBox;

    [Header("Hazard Prefabs (Espaciales)")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private GameObject explosiveMeteorPrefab;
    [SerializeField] private GameObject alienShipPrefab;

    [Header("Collectible Prefabs (Espaciales)")]
    [SerializeField] private GameObject satellitePrefab;
    [SerializeField] private GameObject fuelPrefab;
    [SerializeField] private GameObject supportShipPrefab;
    [SerializeField] private GameObject spaceGunPrefab;

    [Header("Spawn Configuration")]
    [SerializeField] private float hazardLifetime = 10f;
    [SerializeField] private Vector2 topSpawnPosition = new Vector2(0, 10f);
    [SerializeField] private Vector2 leftSpawnPosition = new Vector2(-8f, 5f);

    private Camera mainCamera;
    private Transform playerTransform;
    private BaseHazard currentTutorialHazard;
    private GameObject currentTutorialCollectible;
    private bool tutorialActive = false;

    #region Tutorial Dialogues V2
    private readonly Dictionary<string, string> tutorialDialogues = new Dictionary<string, string>()
    {
        // Introducción
        {"inicio", "Comandante, aquí Control de Misión. Debes pilotear la nave y llegar a salvo a la base en la Luna."},

        // Movimiento
        {"movement", "Usa los controles para navegar. El espacio es traicionero, tu prioridad es esquivar con precisión."},
        
        // Hazards (Adaptados)
        {"toxiccloud_intro", "¡Alerta de impacto! Un Meteorito Gigante en ruta de colisión."},
        {"toxiccloud_advice", "Esquivarlo es la única opción. Su masa es demasiado densa para atravesarla."},

        {"wildfire_intro", "¡Peligro! Meteorito inestable detectado. Va a fragmentarse."},
        {"wildfire_advice", "Mantén la distancia. Al explotar liberará escombros letales en todas direcciones."},

        {"acidrain_intro", "¡Enemigo detectado! Una Nave Alienígena desciende disparando."},
        {"acidrain_advice", "Analiza sus patrones de disparo y busca los huecos seguros para sobrevivir."},
        
        // Collectibles (Adaptados)
        {"flockbird_intro", "¡Señal aliada! Un Satélite de defensa orbital."},
        {"flockbird_advice", "Acóplalo a tu nave. Servirá como escudo contra un impacto directo."},

        {"insect_intro", "¡Suministros! Contenedores de Fuel flotando en el vacío."},
        {"insect_advice", "Recógelos para obtener puntos de misión extra. ¡Los necesitamos!"},

        {"magnetbird_intro", "Refuerzos: Nave de Apoyo Logístico."},
        {"magnetbird_advice", "Esta unidad atraerá contenedores de Fuel cercanos automáticamente."},
        
        // Nuevo Coleccionable (Arma)
        {"gun_intro", "¡Tecnología ofensiva detectada! Un Módulo de Disparo de Plasma."},
        {"gun_advice", "Te permite disparar tocando la pantalla. Tienes 4 disparos para destruir obstáculos. ¡Úsalos sabiamente!"},

        // Final
        {"tutorial_complete", "Entrenamiento finalizado, Comandante. Esquiva, sobrevive y llega a la Luna. ¡Despegue!"}
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
        // Pausar spawners externos si existen
        ImprovedHazardSpawner spawner = Object.FindFirstObjectByType<ImprovedHazardSpawner>();
        if (spawner != null)
        {
            spawner.PauseSpawning();
        }

        // 1. Introducción al movimiento
        yield return StartCoroutine(ShowMovementTutorial());

        // 2. Tutorial de Hazards
        // Usamos las claves originales del diccionario pero cargamos los nuevos prefabs
        yield return StartCoroutine(ShowHazardTutorial("ToxicCloud", meteorPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowHazardTutorial("Wildfire", explosiveMeteorPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowHazardTutorial("AcidRain", alienShipPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        // 3. Tutorial de Coleccionables
        yield return StartCoroutine(ShowCollectibleTutorial("FlockBird", satellitePrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowCollectibleTutorial("Insect", fuelPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        yield return StartCoroutine(ShowCollectibleTutorial("MagnetBird", supportShipPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        // 4. Tutorial del Nuevo Arma (Exclusivo V2)
        yield return StartCoroutine(ShowCollectibleTutorial("Gun", spaceGunPrefab));
        yield return new WaitForSeconds(delayBetweenSteps);

        // 5. Finalizar tutorial
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

        // Esperar 3 segundos para práctica
        yield return new WaitForSeconds(3f);
    }
    #endregion

    #region Hazard Tutorials
    private IEnumerator ShowHazardTutorial(string hazardTypeKey, GameObject hazardPrefab)
    {
        if (hazardPrefab == null)
        {
            Debug.LogWarning($"Prefab para {hazardTypeKey} no asignado en TutorialManagerV2!");
            yield break;
        }

        Vector2 spawnPos = GetSpawnPositionForHazard(hazardTypeKey);
        GameObject hazardObj = Instantiate(hazardPrefab, spawnPos, Quaternion.identity);

        BaseHazard hazard = hazardObj.GetComponent<BaseHazard>();
        currentTutorialHazard = hazard;

        // Pausar y mostrar intro
        PauseGame();
        // Usamos ToLower() para coincidir con las claves del diccionario (toxiccloud_intro, etc.)
        yield return StartCoroutine(ShowDialogue($"{hazardTypeKey.ToLower()}_intro"));
        yield return StartCoroutine(ShowDialogue($"{hazardTypeKey.ToLower()}_advice"));
        ResumeGame();

        // Inicializar hazard
        if (hazard != null)
        {
            SpawnSide spawnSide = GetSpawnSideForHazard(hazardTypeKey);
            hazard.Initialize(spawnPos, spawnSide, playerTransform);
        }

        // Control de dificultad temporal
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.PauseDifficulty();
            DifficultyManager.Instance.SetDifficulty(1f);
        }

        // Esperar destrucción o tiempo
        float timeout = hazardLifetime;
        while (hazardObj != null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (hazardObj != null) Destroy(hazardObj);
        currentTutorialHazard = null;
    }

    private Vector2 GetSpawnPositionForHazard(string hazardType)
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));

        switch (hazardType)
        {
            case "ToxicCloud": // Meteorito
                return new Vector2(-screenBounds.x - 1f, 0f);
            case "Wildfire":   // Meteorito Explosivo
                return new Vector2(0f, screenBounds.y + 1f);
            case "AcidRain":   // Nave Alienígena
                return new Vector2(0f, screenBounds.y + 2f);
            default:
                return topSpawnPosition;
        }
    }

    private SpawnSide GetSpawnSideForHazard(string hazardType)
    {
        // Ajustar según el comportamiento deseado de los scripts originales
        if (hazardType == "ToxicCloud") return SpawnSide.Left;
        return SpawnSide.Top;
    }
    #endregion

    #region Collectible Tutorials
    private IEnumerator ShowCollectibleTutorial(string collectibleTypeKey, GameObject collectiblePrefab)
    {
        if (collectiblePrefab == null)
        {
            Debug.LogWarning($"Prefab de {collectibleTypeKey} no asignado en TutorialManagerV2!");
            yield break;
        }

        Vector2 spawnPos = GetSpawnPositionForCollectible();
        GameObject collectibleObj = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
        currentTutorialCollectible = collectibleObj;

        PauseGame();
        yield return StartCoroutine(ShowDialogue($"{collectibleTypeKey.ToLower()}_intro"));
        yield return StartCoroutine(ShowDialogue($"{collectibleTypeKey.ToLower()}_advice"));
        ResumeGame();

        // Si es la Nave de Apoyo (Magnet), lanzamos Fuel de prueba
        if (collectibleTypeKey.Equals("MagnetBird"))
        {
            StartCoroutine(SpawnTutorialInsects());
        }

        // Si es el Arma, podríamos lanzar un obstáculo de prueba para disparar
        if (collectibleTypeKey.Equals("Gun"))
        {
            StartCoroutine(SpawnTargetDummy());
        }

        // Esperar a ser recogido o salir de pantalla
        float timeout = 15f;
        while (collectibleObj != null && timeout > 0)
        {
            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToViewportPoint(collectibleObj.transform.position);
                if (screenPos.y < -0.1f) break;
            }
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (collectibleObj != null) Destroy(collectibleObj);
        currentTutorialCollectible = null;
    }

    private IEnumerator SpawnTutorialInsects()
    {
        if (fuelPrefab == null) yield break;
        yield return new WaitForSeconds(2.0f);

        for (int i = 0; i < 3; i++)
        {
            if (!tutorialActive) yield break;
            Instantiate(fuelPrefab, GetSpawnPositionForCollectible(), Quaternion.identity);
            yield return new WaitForSeconds(3.0f);
        }
    }

    // Opcional: Generar un blanco para probar el disparo
    private IEnumerator SpawnTargetDummy()
    {
        yield return new WaitForSeconds(3.0f);
        // Usamos el meteorito como blanco de práctica si está disponible
        if (meteorPrefab != null && tutorialActive)
        {
            GameObject dummy = Instantiate(meteorPrefab, topSpawnPosition, Quaternion.identity);
            // Configurar comportamiento básico si es necesario
            BaseHazard hz = dummy.GetComponent<BaseHazard>();
            if (hz != null) hz.Initialize(topSpawnPosition, SpawnSide.Top, playerTransform);
        }
    }

    private Vector2 GetSpawnPositionForCollectible()
    {
        Vector2 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        float randomX = Random.Range(-screenBounds.x * 0.5f, screenBounds.x * 0.5f);
        return new Vector2(randomX, screenBounds.y + 2f);
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
            Debug.LogWarning("DialogueBox no asignado en V2!");
            yield return new WaitForSecondsRealtime(2f);
            yield break;
        }

        if (tutorialDialogues.TryGetValue(dialogueKey, out string text))
        {
            dialogueBox.ShowDialogue(text);
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
    private void PauseGame() { Time.timeScale = 0f; }
    private void ResumeGame() { Time.timeScale = 1f; }
    #endregion

    #region Public Methods
    public bool IsTutorialActive() { return tutorialActive; }

    public void SkipTutorial()
    {
        if (!tutorialActive) return;
        StopAllCoroutines();
        if (currentTutorialHazard != null) Destroy(currentTutorialHazard.gameObject);
        if (currentTutorialCollectible != null) Destroy(currentTutorialCollectible);
        tutorialActive = false;
        ResumeGame();
        GameManager.Instance?.OnTutorialCompleted();
    }
    #endregion
}