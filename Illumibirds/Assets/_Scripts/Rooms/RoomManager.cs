using System;
using System.Collections.Generic;
using Tiles;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace Rooms
{
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapScanner))]
    [RequireComponent(typeof(RoomDetector))]
    [RequireComponent(typeof(DoorController))]
    [RequireComponent(typeof(DoorTriggerSpawner))]
    [RequireComponent(typeof(ProceduralMapGenerator))]
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }
        public static RoomManager Required => Instance
            ? Instance
            : throw new InvalidOperationException($"{nameof(RoomManager)} instance not found. Ensure it exists in the scene.");

        [Header("Player")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerController playerPrefab;

        [Header("Overlay Settings")]
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private int blackTileSortingOrder = 10;

        [Header("Procedural Generation")]
        [SerializeField] private bool useProceduralGeneration;
        [SerializeField] private float generationCheckInterval = 0.5f;

        // Auto-wired components
        private RoomDetector detector;
        private TilemapScanner scanner;
        private Tilemap tilemap;
        private ProceduralMapGenerator generator;
        private DoorTriggerSpawner doorTriggerSpawner;

        public event Action<Room> OnRoomRevealed;
        public event Action<Room> OnRoomEntered;
        public UnityEvent CurrentRoomChanged;

        private List<Room> rooms = new List<Room>();
        private Room currentRoom;
        private float lastGenerationCheckTime;
        private float lastPlayerY;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Auto-wire all components on this GameObject
                scanner = GetComponent<TilemapScanner>();
                tilemap = GetComponent<Tilemap>();
                detector = GetComponent<RoomDetector>();
                generator = GetComponent<ProceduralMapGenerator>();
                doorTriggerSpawner = GetComponent<DoorTriggerSpawner>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (useProceduralGeneration && generator != null)
            {
                InitializeProceduralGeneration();
            }
            else
            {
                InitializeRooms();
            }

            // Register enemies after rooms are detected
            RoomCombatManager.Required.RegisterEnemiesInRooms();
        }

        private void Update()
        {
            if (!useProceduralGeneration || generator == null) return;
            if (playerTransform == null) return;

            // Check for generation at intervals
            if (Time.time - lastGenerationCheckTime >= generationCheckInterval)
            {
                lastGenerationCheckTime = Time.time;
                float playerY = playerTransform.position.y;

                if (Mathf.Abs(playerY - lastPlayerY) > 0.5f)
                {
                    lastPlayerY = playerY;
                    generator.OnPlayerPositionChanged(playerY);
                }
            }
        }

        private void InitializeProceduralGeneration()
        {
            // Reset detection state
            detector.ResetDetectionState();

            // Generate initial rooms
            generator.GenerateInitialRooms();

            // Subscribe to generation events
            generator.OnRoomGenerated += OnRoomGenerated;
            generator.OnRoomUnloaded += OnRoomUnloaded;

            // Detect and register all initially generated rooms
            foreach (var genRoom in generator.GeneratedRooms)
            {
                DetectAndRegisterRoom(genRoom);
            }

            Debug.Log($"RoomManager: Procedural generation initialized with {rooms.Count} rooms");

            // Move player to starting room
            MovePlayerToStartingRoom();

            // Enter starting room
            EnterStartingRoom();
        }

        private void MovePlayerToStartingRoom()
        {
            if (rooms.Count == 0)
            {
                Debug.LogWarning("RoomManager: No rooms detected, cannot spawn player");
                return;
            }

            // Calculate spawn position
            var startingRoom = rooms[0];
            var bounds = startingRoom.WorldBounds;
            var spawnPos = new Vector3(bounds.center.x, bounds.min.y + 1f, 0f);

            // Try to find existing player
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    Debug.Log("RoomManager: Found existing player in scene");
                }
            }

            // Instantiate player if not found and prefab is assigned
            if (playerTransform == null && playerPrefab != null)
            {
                var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                playerTransform = player.transform;
                Debug.Log($"RoomManager: Spawned player at {spawnPos}");

                // Find CameraTarget child and assign to Cinemachine camera
                SetupCinemachineTarget(player.transform);
                return;
            }

            if (playerTransform == null)
            {
                Debug.LogWarning("RoomManager: No player found and no playerPrefab assigned!");
                return;
            }

            // Move existing player to starting room
            playerTransform.position = spawnPos;
            Debug.Log($"RoomManager: Moved player to starting room at {spawnPos}");
        }

        private void SetupCinemachineTarget(Transform player)
        {
            // Find CameraTarget child in player
            var cameraTarget = player.Find("CameraTarget");
            if (cameraTarget == null)
            {
                Debug.LogWarning("RoomManager: No CameraTarget child found in player, using player transform");
                cameraTarget = player;
            }

            // Find Cinemachine camera and set tracking target
            var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Target.TrackingTarget = cameraTarget;
                Debug.Log($"RoomManager: Set Cinemachine tracking target to {cameraTarget.name}");
            }
            else
            {
                Debug.LogWarning("RoomManager: No CinemachineCamera found in scene");
            }
        }

        private void OnRoomGenerated(GeneratedRoom genRoom)
        {
            DetectAndRegisterRoom(genRoom);
        }

        private void OnRoomUnloaded(GeneratedRoom genRoom)
        {
            if (genRoom.DetectedRoom != null)
            {
                UnregisterRoom(genRoom.DetectedRoom, genRoom.TileBounds);
            }
        }

        private void DetectAndRegisterRoom(GeneratedRoom genRoom)
        {
            // Detect room in the generated bounds
            var detectedRoom = detector.DetectRoomInBounds(genRoom.TileBounds);

            if (detectedRoom != null)
            {
                genRoom.DetectedRoom = detectedRoom;
                detectedRoom.SpawnPositions = genRoom.SpawnPositions;
                rooms.Add(detectedRoom);
                CreateRoomOverlay(detectedRoom);

                // Register doors in this area
                DoorController.Required.RegisterDoorsInBounds(genRoom.TileBounds);

                // Spawn door triggers in this area
                doorTriggerSpawner?.SpawnTriggersInBounds(genRoom.TileBounds);

                Debug.Log($"RoomManager: Registered procedural room {detectedRoom.Id} at floor {genRoom.FloorNumber}");
            }
            else
            {
                Debug.LogWarning($"RoomManager: Failed to detect room at floor {genRoom.FloorNumber}");
            }
        }

        public void RegisterGeneratedRoom(Room room, int floorNumber)
        {
            if (!rooms.Contains(room))
            {
                rooms.Add(room);
                CreateRoomOverlay(room);
                Debug.Log($"RoomManager: Manually registered room {room.Id} at floor {floorNumber}");
            }
        }

        public void UnregisterRoom(Room room, BoundsInt bounds)
        {
            if (room == null) return;

            // Destroy overlay tiles
            foreach (var tile in room.BlackOverlayTiles)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
            room.BlackOverlayTiles.Clear();

            // Destroy enemies in the room
            foreach (var enemy in room.Enemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }
            room.Enemies.Clear();

            // Clear detection state for these bounds
            detector.ClearVisitedInBounds(bounds);

            // Unregister doors
            DoorController.Required.UnregisterDoorsInBounds(bounds);

            // Destroy door triggers
            doorTriggerSpawner?.DestroyTriggersInBounds(bounds);

            rooms.Remove(room);

            Debug.Log($"RoomManager: Unregistered room {room.Id}");
        }

        private void OnDestroy()
        {
            if (generator != null)
            {
                generator.OnRoomGenerated -= OnRoomGenerated;
                generator.OnRoomUnloaded -= OnRoomUnloaded;
            }
        }

        private void InitializeRooms()
        {
            rooms = detector.DetectRooms();
            Debug.Log($"RoomManager: Detected {rooms.Count} rooms");

            // Debug: Log each room's bounds
            foreach (var room in rooms)
            {
                Debug.Log($"Room {room.Id}: TileCount={room.TilePositions.Count}, Bounds={room.WorldBounds.center} size={room.WorldBounds.size}");
            }

            // Create overlays for all rooms first
            foreach (var room in rooms)
            {
                CreateRoomOverlay(room);
            }

            // Spawn/move player to starting room
            MovePlayerToStartingRoom();

            // Reveal and enter starting room
            EnterStartingRoom();
        }

        private void CreateRoomOverlay(Room room)
        {
            room.BlackOverlayTiles = new List<GameObject>();
            var cellSize = scanner.CellSize;

            foreach (var tilePos in room.TilePositions)
            {
                var worldPos = scanner.CellToWorld(tilePos);
                var blackTile = CreateBlackTile(worldPos, cellSize, $"BlackTile_Room{room.Id}");
                room.BlackOverlayTiles.Add(blackTile);
            }
        }

        private GameObject CreateBlackTile(Vector3 position, Vector3 size, string name)
        {
            var tile = new GameObject(name);
            tile.transform.position = position;
            tile.transform.parent = transform;

            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f),
                4f
            );
            sr.color = Color.black;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = blackTileSortingOrder;
            tile.transform.localScale = new Vector3(size.x, size.y, 1f);

            return tile;
        }

        private void EnterStartingRoom()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            Debug.Log($"EnterStartingRoom: playerTransform={(playerTransform != null ? playerTransform.position.ToString() : "NULL")}");

            Room startingRoom = null;

            if (playerTransform != null)
            {
                startingRoom = GetRoomAtPosition(playerTransform.position);
                Debug.Log($"EnterStartingRoom: GetRoomAtPosition returned {(startingRoom != null ? $"Room {startingRoom.Id}" : "NULL")}");
            }

            // Also check spawn points
            if (startingRoom == null && scanner != null)
            {
                var spawnPositions = scanner.GetSpawnPositions();
                Debug.Log($"EnterStartingRoom: Checking {spawnPositions.Count} spawn positions");
                foreach (var spawnPos in spawnPositions)
                {
                    var worldPos = scanner.CellToWorld(spawnPos);
                    Debug.Log($"EnterStartingRoom: SpawnPos cell={spawnPos}, world={worldPos}");
                    var room = GetRoomAtPosition(worldPos);
                    if (room != null)
                    {
                        startingRoom = room;
                        Debug.Log($"EnterStartingRoom: Found room {room.Id} at spawn");
                        break;
                    }
                }
            }

            if (startingRoom != null)
            {
                Debug.Log($"EnterStartingRoom: Revealing room {startingRoom.Id}");
                // Directly reveal and enter starting room without combat trigger
                RevealRoom(startingRoom);
                currentRoom = startingRoom;
                startingRoom.DoorsOpen = true;

                // Open doors to adjacent rooms (starting room is already clear)
                var adjacentRooms = GetAdjacentRooms(startingRoom);
                foreach (var adjRoom in adjacentRooms)
                {
                    DoorController.Required.OpenDoorsForRoom(adjRoom);
                }

                CurrentRoomChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning("EnterStartingRoom: No starting room found!");
            }
        }

        public void EnterRoom(Room room)
        {
            if (room == null || room == currentRoom) return;

            Debug.Log($"RoomManager: Player entering room {room.Id}");

            // Close doors behind (from previous room)
            if (currentRoom != null)
            {
                DoorController.Required.CloseDoorsForRoom(currentRoom);
            }

            // Reveal the new room
            RevealRoom(room);

            // Update current room
            currentRoom = room;
            CurrentRoomChanged?.Invoke();
            OnRoomEntered?.Invoke(room);

            // Start combat in this room
            RoomCombatManager.Required.StartCombat(room);
        }

        public void RevealRoom(Room room)
        {
            if (room == null || room.IsRevealed) return;

            room.IsRevealed = true;

            // Destroy all black overlay tiles for this room
            foreach (var tile in room.BlackOverlayTiles)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }
            room.BlackOverlayTiles.Clear();

            OnRoomRevealed?.Invoke(room);
            Debug.Log($"RoomManager: Revealed room {room.Id}");
        }

        public void SetCurrentRoom(Room room)
        {
            if (room == null || currentRoom == room) return;
            currentRoom = room;
            CurrentRoomChanged?.Invoke();
        }

        public Room GetCurrentRoom() => currentRoom;

        public Room GetRoomAtPosition(Vector3 worldPos)
        {
            foreach (var room in rooms)
            {
                bool contains = room.ContainsWorldPosition(worldPos);
                Debug.Log($"GetRoomAtPosition: pos={worldPos}, Room {room.Id} bounds={room.WorldBounds}, contains={contains}");
                if (contains)
                {
                    return room;
                }
            }
            return null;
        }

        public Room GetRoomContainingCell(Vector3Int cellPos)
        {
            foreach (var room in rooms)
            {
                if (room.TilePositions.Contains(cellPos) || room.DoorPositions.Contains(cellPos))
                {
                    return room;
                }
            }
            return null;
        }

        public List<Room> GetAdjacentRooms(Room room)
        {
            var adjacent = new List<Room>();

            foreach (var doorPos in room.DoorPositions)
            {
                foreach (var otherRoom in rooms)
                {
                    if (otherRoom != room && otherRoom.DoorPositions.Contains(doorPos))
                    {
                        if (!adjacent.Contains(otherRoom))
                        {
                            adjacent.Add(otherRoom);
                        }
                    }
                }
            }

            return adjacent;
        }

        public Room CurrentRoom => currentRoom;
        public List<Room> AllRooms => rooms;
        public Tilemap Tilemap => tilemap;
        public TilemapScanner Scanner => scanner;
        public bool UseProceduralGeneration => useProceduralGeneration;
    }
}
