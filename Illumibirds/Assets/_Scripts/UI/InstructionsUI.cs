using UnityEngine;

public class InstructionsUI : MonoBehaviour
{

    void Start()
    {
        Invoke(nameof(Deactivate), 10);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
