using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryChoiceScreen : MonoBehaviour
{
    [SerializeField] GameObject panel;

    void Start()
    {
        WaveCounterUI.OnMilestoneReached += OnMilestoneReached;
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        WaveCounterUI.OnMilestoneReached -= OnMilestoneReached;
    }

    void OnMilestoneReached(int wave)
    {
        ShowVictoryScreen();
    }

    void ShowVictoryScreen()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void OnContinueClicked()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void OnExitClicked()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameState.mainMenu);
        SceneManager.LoadScene(0);
    }
}
