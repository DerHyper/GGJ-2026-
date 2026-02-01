using UnityEngine;

public class PanelOptions : MonoBehaviour
{
    public void ToggleVisibility()
    {
        if (gameObject.activeSelf)
        {
            DoInvisible();
        }
        else
        {
            DoVisible();
        }
    }

    public void DoInvisible()
    {
        gameObject.SetActive(false);
    }

    public void DoVisible()
    {
        gameObject.SetActive(true);
    }
}
