using System;
using UnityEngine;
using UnityEngine.Events;

public class RoomManager : MonoBehaviour
{
    Room CurrentRoom;
    public static RoomManager Instance;
    public UnityEvent CurrentRoomChanged;
    [SerializeField] PlayerController playerPrefab;

    [SerializeField] Room[] possibleRooms;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitiateRandomRoom();
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // void Start()
    // {
        
    // }

    void InitiateRandomRoom()
    {
        int rand = UnityEngine.Random.Range(0, possibleRooms.Length);

        Room newRoom = Instantiate(possibleRooms[rand], Vector3.zero, Quaternion.identity);
        SetCurrentRoom(newRoom);
        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer()
    {
        PlayerController player = Instantiate(playerPrefab, CurrentRoom.playerSpawn.position, Quaternion.identity);
    }

    void SpawnEnemies()
    {
        FindFirstObjectByType<RandomizedEnemySpawner>(FindObjectsInactive.Exclude).SpawnEnemies();
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
