using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameState gamestate = GameState.mainMenu;

    public static GameManager Instance;
    [SerializeField] string GAMESCENE = "";

    public Action<GameState> OnGameStateChanged;

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
        OnGameStateChanged?.Invoke(gamestate);
    }

    public bool GameIsPaused()
    {
        return gamestate == GameState.paused;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GAMESCENE);
        ChangeState(GameState.inGame);
    }

}
