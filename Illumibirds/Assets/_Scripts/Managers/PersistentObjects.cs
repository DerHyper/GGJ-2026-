using UnityEngine;

public class PersistentObjects : MonoBehaviour
{
    [SerializeField] GameManager gameManagerPrefab;

    public static PersistentObjects Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {

        Instantiate(gameManagerPrefab, transform);
    }
}
