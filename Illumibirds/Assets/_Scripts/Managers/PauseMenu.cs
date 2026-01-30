using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    [SerializeField] GameObject pauseCanvas;

    void Awake()
    {
        inputActions = new();
    }

    void OnEnable()
    {
        Debug.Log("A");
        inputActions.Enable();
        inputActions.Player.PauseGame.performed += OnPause;
    }

    void OnDisable()
    {
        Debug.Log("B");
        inputActions.Player.PauseGame.performed -= OnPause;
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        Debug.Log("PRESS PAUSE");
        if (GameManager.Instance.gamestate == GameState.inGame) GameManager.Instance.ChangeState(GameState.paused);
        else if (GameManager.Instance.gamestate == GameState.paused) GameManager.Instance.ChangeState(GameState.inGame);

        TogglePauseMenu(GameManager.Instance.GameIsPaused());
    }

    public void TogglePauseMenu(bool onOff)
    {
        pauseCanvas.SetActive(onOff);
    }


}