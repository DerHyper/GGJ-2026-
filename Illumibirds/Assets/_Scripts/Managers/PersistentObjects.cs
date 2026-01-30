using UnityEngine;

public class PersistentObjects : MonoBehaviour
{
    [SerializeField] GameManager gameManagerPrefab;

    void Start()
    {
        DontDestroyOnLoad(this);
        Instantiate(gameManagerPrefab, transform);
    }
}
