using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Estados posibles del juego
/// </summary>
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Tutorial
}

/// <summary>
/// Gestor principal del juego. Maneja estados, UI, tutorial y progreso.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Events
    public event Action<int> OnGameOver;
    public event Action OnGameStart;
    public event Action OnPause;
    public event Action OnResume;
    #endregion

    #region Game State
    [Header("Estado del Juego")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    public GameState CurrentState => currentState;
    #endregion

    #region Configuracion del Juego
    [Header("Configuracion del Juego")]
    [SerializeField] private float initialScrollSpeed = 5f;
    [SerializeField] private float speedIncreaseRate = 0.1f;
    public float CurrentScrollSpeed { get; private set; }
    #endregion

    #region Sistema de Bandada
    [Header("Sistema de Bandada")]
    [SerializeField] private int maxFlockSize = 5;
    [SerializeField] private float scoreMultiplierPerBird = 0.2f;
    [SerializeField] private int flockSize = 1;
    public int FlockSize
    {
        get { return flockSize; }
        set { flockSize = Mathf.Clamp(value, 1, maxFlockSize); }
    }
    public int MaxFlockSize => maxFlockSize;
    #endregion

    #region Sistema de Puntuacion
    [Header("Sistema de Puntuacion")]
    [SerializeField] private float score = 0f;
    [SerializeField] private float distanceTraveled = 0f;
    public float DistanceTraveled => distanceTraveled;
    public int Score => Mathf.FloorToInt(score);
    #endregion

    #region UI Panels
    [Header("Paneles de UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject tutorialPanel;
    #endregion

    #region UI Elements - Main Menu
    [Header("Main Menu UI")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject franklinBirdMain;
    [SerializeField] private DialogueBox mainMenuDialogue;
    #endregion

    #region UI Elements - Pause
    [Header("Pause Menu UI")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button pauseExitButton;
    #endregion

    #region UI Elements - Game Over
    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button gameOverMainMenuButton;
    [SerializeField] private Button gameOverExitButton;
    [SerializeField] private GameObject franklinBirdGameOver;
    [SerializeField] private DialogueBox gameOverDialogue;
    #endregion

    #region UI Elements - Gameplay HUD
    [Header("Gameplay HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI flockSizeText;
    #endregion

    #region Tutorial System
    [Header("Tutorial System")]
    [SerializeField] private bool enableTutorial = true;
    [SerializeField] private TutorialManager tutorialManager;
    private bool tutorialCompleted = false;
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";
    public bool TutorialCompleted => tutorialCompleted;
    #endregion

    #region Screen Transition
    [Header("Screen Transition")]
    [SerializeField] private float transitionDuration = 0.5f; // Duracion de la transicion en segundos
    [SerializeField] private float slideDownOffset = -1080f; // Asumiendo resolucion Full HD
    #endregion

    #region Managers References
    [Header("Managers")]
    [SerializeField] private ImprovedHazardSpawner hazardSpawner;
    [SerializeField] private LevelGenerator levelGenerator;
    #endregion

    #region Screen Dimensions
    public float screenWidthInWorldUnits { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeButtons();
        LoadTutorialStatus();

        // Encontrar managers si no estan asignados
        if (hazardSpawner == null)
            hazardSpawner = FindAnyObjectByType<ImprovedHazardSpawner>();
        if (levelGenerator == null)
            levelGenerator = FindAnyObjectByType<LevelGenerator>();
    }

    private void Start()
    {
        screenWidthInWorldUnits = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;

        if (enableTutorial && tutorialManager != null)
        {
            ResetTutorial();
        }

        currentState = GameState.MainMenu;


        InitializeUIState();
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameplay();
        }
    }
    #endregion

    #region State Management
    private void InitializeUIState()
    {
        // Desactivar todos los paneles primero
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        // Luego activar el estado inicial
        EnterState(currentState);
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        ExitState(currentState);
        currentState = newState;
        EnterState(newState);
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                ShowMainMenu();
                break;
            case GameState.Playing:
                StartGameplay();
                break;
            case GameState.Paused:
                ShowPauseMenu();
                break;
            case GameState.GameOver:
                ShowGameOver();
                break;
            case GameState.Tutorial:
                StartTutorial();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                HideMainMenu();
                break;
            case GameState.Playing:
                StopGameplay();
                break;
            case GameState.Paused:
                HidePauseMenu();
                break;
            case GameState.GameOver:
                HideGameOver();
                break;
            case GameState.Tutorial:
                // Gestionado por TutorialManager
                break;
        }
    }
    #endregion

    #region Main Menu
    private void ShowMainMenu()
    {
        Time.timeScale = 1f;

        // Activar panel 
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            // Asegurar que esta en posicion visible
            RectTransform rect = mainMenuPanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
            }
        }

        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Pausar managers
        PauseAllManagers();

        ResetGameValues();
    }

    private void HideMainMenu()
    {
        if (mainMenuPanel != null)
        {
            StartCoroutine(SlideOutPanel(mainMenuPanel, () => {
                mainMenuPanel.SetActive(false);
            }));
        }
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("[GameManager] Play button clicked!");

        if (!tutorialCompleted)
        {
            ChangeState(GameState.Tutorial);
        }
        else
        {
            ChangeState(GameState.Playing);
        }
    }

    public void OnMainMenuExitClicked()
    {
        Debug.Log("[GameManager] Exit clicked");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Gameplay
    private void StartGameplay()
    {
        CurrentScrollSpeed = initialScrollSpeed;

        if (gameplayHUD != null) gameplayHUD.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);

        Time.timeScale = 1f;

        // Reanudar managers
        ResumeAllManagers();

        OnGameStart?.Invoke();
        UpdateUI();
    }

    private void StopGameplay()
    {
        // Pausar managers cuando se sale del gameplay
        PauseAllManagers();
    }

    private void UpdateGameplay()
    {
        CurrentScrollSpeed += speedIncreaseRate * Time.deltaTime;

        float distanceThisFrame = CurrentScrollSpeed * Time.deltaTime;
        distanceTraveled += distanceThisFrame;

        float currentScoreMultiplier = 1 + (flockSize - 1) * scoreMultiplierPerBird;
        score += distanceThisFrame * currentScoreMultiplier;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Puntaje: {Score}";

        if (flockSizeText != null)
            flockSizeText.text = $"Bandada: {flockSize}";
    }

    private void ResetGameValues()
    {
        score = 0f;
        distanceTraveled = 0f;
        flockSize = 1;
        CurrentScrollSpeed = initialScrollSpeed;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.ResetPlayer();
        }
    }
    #endregion

    #region Pause System
    public void OnPauseButtonClicked()
    {
        Debug.Log("[GameManager] Pause button clicked!");
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    private void ShowPauseMenu()
    {
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // NO pausar managers aqui, solo congelar tiempo
        OnPause?.Invoke();
    }

    private void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void OnResumeButtonClicked()
    {
        Debug.Log("[GameManager] Resume button clicked!");
        Time.timeScale = 1f;
        ChangeState(GameState.Playing);
        OnResume?.Invoke();
    }

    public void OnPauseMainMenuClicked()
    {
        Debug.Log("[GameManager] Pause => Main Menu clicked!");
        Time.timeScale = 1f;
        CleanupGameObjects();
        ChangeState(GameState.MainMenu);
    }

    public void OnPauseExitClicked()
    {
        Debug.Log("[GameManager] Pause => Exit clicked!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Game Over
    private void ShowGameOver()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Puntaje Final: {Score}";
        }

        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);

        // Pausar managers
        PauseAllManagers();

        OnGameOver?.Invoke(Score);
    }

    private void HideGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TriggerGameOver()
    {
        Debug.Log($"[GameManager] GAME OVER. Puntaje final: {Score}");
        ChangeState(GameState.GameOver);
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("[GameManager] Restart button clicked!");
        Time.timeScale = 1f;

        if (levelGenerator != null)
        {
            levelGenerator.ResetGenerator();
        }

        ResetGameValues();
        CleanupGameObjects();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }

        ChangeState(GameState.Playing);
    }

    public void OnGameOverMainMenuClicked()
    {
        Debug.Log("[GameManager] Game Over => Main Menu clicked!");
        Time.timeScale = 1f;
        CleanupGameObjects();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResetDifficulty();
        }

        ChangeState(GameState.MainMenu);
    }

    public void OnGameOverExitClicked()
    {
        Debug.Log("[GameManager] Game Over => Exit clicked!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion

    #region Tutorial System
    private void LoadTutorialStatus()
    {
        tutorialCompleted = PlayerPrefs.GetInt(TUTORIAL_COMPLETED_KEY, 0) == 1;
        Debug.Log($"[GameManager] Tutorial status: {(tutorialCompleted ? "Completado" : "No completado")}");
    }

    private void StartTutorial()
    {
        if (tutorialManager != null)
        {
            if (gameplayHUD != null) gameplayHUD.SetActive(false);
            if (pauseButton != null) pauseButton.gameObject.SetActive(false);

            PauseAllManagers();

            if (levelGenerator != null)
            {
                levelGenerator.enabled = true;
            }

            tutorialManager.StartTutorial();
        }
        else
        {
            Debug.LogWarning("[GameManager] TutorialManager no asignado!");
            ChangeState(GameState.Playing);
        }
    }

    public void OnTutorialCompleted()
    {
        tutorialCompleted = true;
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();

        Debug.Log("[GameManager] Tutorial completado y guardado!");

        if (gameplayHUD != null) gameplayHUD.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);

        ChangeState(GameState.Playing);
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        tutorialCompleted = false;
        Debug.Log("[GameManager] Tutorial reiniciado");
    }
    #endregion

    #region Damage & Collectibles
    public void PlayerTookDamage()
    {
        if (currentState != GameState.Playing && currentState != GameState.Tutorial) return;

        if (flockSize > 1)
        {
            flockSize--;
        }
        else
        {
            TriggerGameOver();
        }
    }

    public void AddBirdToFlock()
    {
        if (flockSize < maxFlockSize)
        {
            flockSize++;
            Debug.Log($"[GameManager] Ave aniadida a la bandada. Tamanio actual: {flockSize}");
        }
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }
    #endregion

    #region Managers Control
    private void PauseAllManagers()
    {
        // Pausar HazardSpawner
        if (hazardSpawner != null)
        {
            hazardSpawner.PauseSpawning();
        }

        // Pausar DifficultyManager
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.PauseDifficulty();
        }

        // Pausar LevelGenerator
        if (levelGenerator != null)
        {
            levelGenerator.enabled = false;
        }
    }

    private void ResumeAllManagers()
    {
        // Reanudar HazardSpawner
        if (hazardSpawner != null)
        {
            hazardSpawner.ResumeSpawning();
        }

        // Reanudar DifficultyManager
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ResumeDifficulty();
        }

        // Reanudar LevelGenerator
        if (levelGenerator != null)
        {
            levelGenerator.enabled = true;
        }
    }
    #endregion

    #region Screen Transitions
    private IEnumerator SlideOutPanel(GameObject panel, Action onComplete = null)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, slideDownOffset);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        rect.anchoredPosition = endPos;
        onComplete?.Invoke();
    }

    private IEnumerator SlideInPanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector2 targetPos = Vector2.zero;
        Vector2 startPos = new Vector2(targetPos.x, slideDownOffset);

        rect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }
    #endregion

    #region Button Initialization
    private void InitializeButtons()
    {
        // Main Menu
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButtonClicked);
            Debug.Log("[GameManager] Play button initialized");
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnMainMenuExitClicked);
        }

        // Pause Menu
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
            Debug.Log("[GameManager] Pause button initialized");
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
        if (pauseMainMenuButton != null)
        {
            pauseMainMenuButton.onClick.RemoveAllListeners();
            pauseMainMenuButton.onClick.AddListener(OnPauseMainMenuClicked);
        }
        if (pauseExitButton != null)
        {
            pauseExitButton.onClick.RemoveAllListeners();
            pauseExitButton.onClick.AddListener(OnPauseExitClicked);
        }

        // Game Over
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        if (gameOverMainMenuButton != null)
        {
            gameOverMainMenuButton.onClick.RemoveAllListeners();
            gameOverMainMenuButton.onClick.AddListener(OnGameOverMainMenuClicked);
        }
        if (gameOverExitButton != null)
        {
            gameOverExitButton.onClick.RemoveAllListeners();
            gameOverExitButton.onClick.AddListener(OnGameOverExitClicked);
        }
    }
    #endregion

    #region Cleanup
    private void CleanupGameObjects()
    {
        foreach (var hazard in FindObjectsByType<BaseHazard>(FindObjectsSortMode.None))
        {
            Destroy(hazard.gameObject);
        }

        foreach (var projectile in FindObjectsByType<HazardProjectile>(FindObjectsSortMode.None))
        {
            Destroy(projectile.gameObject);
        }

        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("CollectibleBird");
        foreach (var obj in collectibles)
        {
            Destroy(obj);
        }

        GameObject[] insects = GameObject.FindGameObjectsWithTag("Insect");
        foreach (var obj in insects)
        {
            Destroy(obj);
        }

        foreach (var magnetBird in FindObjectsByType<MagnetBird>(FindObjectsSortMode.None))
        {
            Destroy(magnetBird.gameObject);
        }
    }
    #endregion
}