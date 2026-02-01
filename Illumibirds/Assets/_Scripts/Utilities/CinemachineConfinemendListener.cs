using System;
using Rooms;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineConfinemendListener : MonoBehaviour
{
    private CinemachineConfiner2D confiner;
    private bool isSubscribed;

    private CinemachineConfiner2D ConfinerRequired => confiner
        ? confiner
        : throw new InvalidOperationException($"{nameof(CinemachineConfiner2D)} not found on {gameObject.name}");

    void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();
        // Validate immediately
        _ = ConfinerRequired;
    }

    void Update()
    {
        if (!isSubscribed && RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentRoomChanged.AddListener(OnRoomChanged);
            isSubscribed = true;
            OnRoomChanged();
        }
    }

    void OnRoomChanged()
    {
        var currentRoom = RoomManager.Required.GetCurrentRoom();
        if (currentRoom == null)
        {
            throw new InvalidOperationException("CurrentRoom is null when OnRoomChanged was called");
        }

        var cameraBounds = currentRoom.CameraBounds;
        if (cameraBounds == null)
        {
            throw new InvalidOperationException($"Room {currentRoom.Id} has no CameraBounds. WorldBounds={currentRoom.WorldBounds}");
        }

        ConfinerRequired.BoundingShape2D = cameraBounds;
        ConfinerRequired.InvalidateBoundingShapeCache();
        Debug.Log($"CinemachineConfiner: Set bounds to room {currentRoom.Id}");
    }

    void OnDestroy()
    {
        if (isSubscribed && RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentRoomChanged.RemoveListener(OnRoomChanged);
        }
    }
}
