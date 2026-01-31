using Rooms;
using UnityEngine;

public class CameraTargetLogic : MonoBehaviour
{
    private const float WallHeight = 2f;

    void Update()
    {
        var player = Finder.FindPlayer();
        if (player == null) return;

        Vector2 playerPosition = player.transform.position;

        Room currentRoom = RoomManager.Instance?.GetCurrentRoom();
        if (currentRoom == null) return;

        float cameraTop = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)).y;
        float cameraRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)).x;
        float cameraLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        float cameraBottom = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;

        Bounds roomBounds = currentRoom.WorldBounds;
        Vector3 min = roomBounds.min;
        Vector3 max = roomBounds.max;

        bool overshootTop = cameraTop > max.y + WallHeight;
        bool overshootBottom = cameraBottom < min.y;
        bool overshootLeft = cameraLeft < min.x;
        bool overshootRight = cameraRight > max.x;

        Vector3 newPosition = playerPosition;
        if (overshootTop)
        {
            newPosition.y = max.y - 0.001f;
        }
        if (overshootBottom)
        {
            newPosition.y = min.y + 0.001f;
        }
        if (overshootLeft)
        {
            newPosition.x = min.x + 0.001f;
        }
        if (overshootRight)
        {
            newPosition.x = max.x - 0.001f;
        }
        transform.position = newPosition;
    }
}
