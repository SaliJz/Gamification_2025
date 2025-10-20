using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private string retrySceneName = "Nivel";
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void OnRetryButtonClicked()
    {
        Debug.Log("Reintentando el juego...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(retrySceneName);
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}