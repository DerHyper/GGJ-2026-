using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public MusicTrack music;
    public AudioClip startGameSound;
    public float startGameSoundVolume = 0.3f;
    private void Start() 
    {
        AudioManager.Instance.PlayMusic(music);
    }
   public void StartGame()
    {
        GameManager.Instance.StartGame();
        AudioManager.Instance.PlayOnce(startGameSound, startGameSoundVolume);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
