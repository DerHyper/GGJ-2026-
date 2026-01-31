using UnityEngine;
using UnityEngine.Events;

public class RoomManager : MonoBehaviour
{
    Room CurrentRoom;
    [SerializeField] Room startingRoom;
    public static RoomManager Instance;
    public UnityEvent CurrentRoomChanged;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetCurrentRoom(startingRoom);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    public Room GetCurrentRoom()
    {
        return CurrentRoom;
    }

    public void SetCurrentRoom(Room room)
    {
        CurrentRoom = room;
        CurrentRoomChanged.Invoke();
    }
}
