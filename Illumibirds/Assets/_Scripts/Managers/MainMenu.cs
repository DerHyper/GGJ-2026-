using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;

    public MusicTrack music;
    public AudioClip startGameSound;
    public float startGameSoundVolume = 0.3f;

    [SerializeField] UIButton[] menuItems;
    UIButton activeItem;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AudioManager.Instance.PlayMusic(music);

        Debug.Log($"[MainMenu] Start - menuItems count: {(menuItems != null ? menuItems.Length : 0)}");
        if (menuItems != null && menuItems.Length > 0)
        {
            // Initialize all buttons to inactive scale first
            foreach (var item in menuItems)
            {
                item.SetActive(false, instant: true);
            }
            // Then set the first one as active
            SetActiveItem(menuItems[0], instant: true);
        }
    }

    public void SetActiveItem(UIButton item, bool instant = false)
    {
        Debug.Log($"[MainMenu] SetActiveItem({item?.name}) - current activeItem: {activeItem?.name}");
        if (item == activeItem) return;

        if (activeItem != null)
            activeItem.SetActive(false);

        activeItem = item;

        if (activeItem != null)
            activeItem.SetActive(true, instant);
    }

    public void ClearActiveItem(UIButton item)
    {
        if (item != activeItem) return;

        activeItem.SetActive(false);
        activeItem = null;
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
