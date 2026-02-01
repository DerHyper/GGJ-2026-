using UnityEngine;

public class MainMenu : MonoBehaviour
{
   public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
