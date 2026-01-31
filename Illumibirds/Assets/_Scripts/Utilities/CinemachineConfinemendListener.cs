using Rooms;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineConfinemendListener : MonoBehaviour
{
    private CinemachineConfiner2D confiner;

    void Start()
    {
        confiner = GetComponent<CinemachineConfiner2D>();

        if (RoomManager.Instance != null)
        {
            OnRoomChanged();
            RoomManager.Instance.CurrentRoomChanged.AddListener(OnRoomChanged);
        }
    }

    void OnRoomChanged()
    {
        var currentRoom = RoomManager.Instance?.GetCurrentRoom();
        if (currentRoom == null || confiner == null) return;

        if (currentRoom.CameraBounds != null)
        {
            confiner.BoundingShape2D = currentRoom.CameraBounds;
        }
    }
}
