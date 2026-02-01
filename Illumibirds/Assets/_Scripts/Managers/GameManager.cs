using System;
using System.Collections;
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
            DontDestroyOnLoad(gameObject);
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
        Finder.ClearCache();
        SceneManager.LoadScene(GAMESCENE);
        ChangeState(GameState.inGame);
    }

    public void FreezeFrame()
    {
        StartCoroutine(FreezeFrameCoroutine(0.1f));
    }

    public IEnumerator FreezeFrameCoroutine(float duration)
    {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1;
    }

}
