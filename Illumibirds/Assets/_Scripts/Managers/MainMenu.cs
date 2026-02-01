using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public MusicTrack music;
    private void Start() 
    {
        AudioManager.Instance.PlayMusic(music);
    }
   public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
