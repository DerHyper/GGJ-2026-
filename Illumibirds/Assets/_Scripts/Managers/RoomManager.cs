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
            // DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetCurrentRoom(startingRoom);
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
