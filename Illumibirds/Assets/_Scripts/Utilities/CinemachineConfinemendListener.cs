using UnityEngine;
using Unity;
using Unity.Cinemachine;

public class CinemachineConfinemendListener : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnRoomChanged();
        RoomManager.Instance.CurrentRoomChanged.AddListener(OnRoomChanged);
    }

    void OnRoomChanged()
    {
        var cameraBoundary = RoomManager.Instance.GetCurrentRoom().cameraBounds;
        gameObject.GetComponent<CinemachineConfiner2D>().BoundingShape2D = cameraBoundary;
    }
}
