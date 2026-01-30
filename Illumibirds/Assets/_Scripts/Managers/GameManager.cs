using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameState gamestate = GameState.mainMenu;

    public static GameManager Instance;
    [SerializeField] string GAMESCENE = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        gamestate = newState;
    }

    public bool GameIsPaused()
    {
        return gamestate == GameState.paused;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GAMESCENE);
    }

}
