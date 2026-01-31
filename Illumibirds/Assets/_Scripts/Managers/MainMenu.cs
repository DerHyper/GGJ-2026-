using UnityEngine;

public class MainMenu : MonoBehaviour
{
   public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
