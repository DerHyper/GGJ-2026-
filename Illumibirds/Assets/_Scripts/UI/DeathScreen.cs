using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += ShowDeathScreen;
    }

    void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= ShowDeathScreen;
    }

    void ShowDeathScreen(GameState gameState)
    {
        if (gameState == GameState.gameOver) ToggleDeathScreen(true);
    }

    public void ToggleDeathScreen(bool active)
    {
        panel.SetActive(active);
    }

    public void GoBackToMainMenu()
    {
        GameManager.Instance.ChangeState(GameState.mainMenu);
        SceneManager.LoadScene(0);

    }
}