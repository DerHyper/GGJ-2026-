using System.Collections.Generic;
using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Rooms
{
    /// <summary>
    /// Procedurally generates rooms by stamping templates into a master tilemap.
    /// Supports linear vertical progression with dynamic loading/unloading.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class ProceduralMapGenerator : MonoBehaviour
    {
        public static ProceduralMapGenerator Instance { get; private set; }

        [Header("Templates")]
        [SerializeField] private List<RoomTemplate> templates = new List<RoomTemplate>();

        [Header("Generation Settings")]
        [Tooltip("Number of rooms to generate ahead of the player")]
        [SerializeField] private int generateAhead = 2;

        [Tooltip("Number of rooms to keep below the player before unloading")]
        [SerializeField] private int keepBehind = 1;

        [Header("Debug")]
        [SerializeField] private bool debugMode;

        private List<GeneratedRoom> generatedRooms = new List<GeneratedRoom>();
        private int currentTopFloor = -1;
        private int currentBottomFloor = 0;
        private Tilemap masterTilemap;

        public List<GeneratedRoom> GeneratedRooms => generatedRooms;
        public Tilemap MasterTilemap => masterTilemap;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                masterTilemap = GetComponent<Tilemap>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Generates the initial set of rooms (starting room + rooms ahead).
        /// Called by RoomManager before room detection.
        /// </summary>
        public void GenerateInitialRooms()
        {
            if (masterTilemap == null)
            {
                Debug.LogError("ProceduralMapGenerator: No master tilemap assigned!");
                return;
            }

            // Clear any existing tiles
            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Clearing tilemap. Current bounds: {masterTilemap.cellBounds}, tile count estimate: {masterTilemap.GetUsedTilesCount()}");
            }
            masterTilemap.ClearAllTiles();
            generatedRooms.Clear();
            currentTopFloor = -1;
            currentBottomFloor = 0;

            // Generate starting room at floor 0
            GenerateRoom(0, GetTemplateForFloor(0));

            // Generate rooms ahead
            for (int i = 1; i <= generateAhead; i++)
            {
                GenerateNextRoom();
            }

            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Generated {generatedRooms.Count} initial rooms (floors 0-{currentTopFloor})");
            }
        }

        /// <summary>
        /// Generates a room at the specified floor number.
        /// </summary>
        private GeneratedRoom GenerateRoom(int floorNumber, RoomTemplate template)
        {
            if (template == null)
            {
                Debug.LogError($"ProceduralMapGenerator: No template provided for floor {floorNumber}");
                return null;
            }

            // Get actual size and bounds from prefab
            var prefabTilemap = template.prefab.GetComponentInChildren<Tilemap>(true);
            if (prefabTilemap == null)
            {
                Debug.LogError($"ProceduralMapGenerator: No template provided for floor {floorNumber}");
                return null;
            }

            var templateBounds = prefabTilemap.cellBounds;
            Vector2Int actualSize = new Vector2Int(templateBounds.size.x, templateBounds.size.y);

            // Calculate tight bounds (actual content, excluding empty space)
            int stackHeight;
            if (template.stackingHeight > 0)
            {
                stackHeight = template.stackingHeight;
            }
            else
            {
                stackHeight = GetTightHeight(prefabTilemap);
            }

            // Calculate offset: rooms stack vertically
            Vector3Int offset = new Vector3Int(0, floorNumber * stackHeight, 0);

            // Calculate actual tile bounds in master tilemap (normalized to start at offset)
            // For rooms above floor 0, extend bounds down by 1 row to include shared boundary doors
            var boundsOffset = offset;
            var boundsSize = templateBounds.size;
            if (floorNumber > 0)
            {
                boundsOffset = new Vector3Int(offset.x, offset.y - 1, offset.z);
                boundsSize = new Vector3Int(boundsSize.x, boundsSize.y + 1, boundsSize.z);
            }
            var tileBounds = new BoundsInt(boundsOffset, boundsSize);

            var genRoom = new GeneratedRoom(floorNumber, template, offset, actualSize, tileBounds);
            genRoom.RoomInstance = StampRoomToTilemap(template, offset);
            genRoom.IsLoaded = true;

            // Collect spawn positions from SpawnPoint components in the instantiated prefab
            if (genRoom.RoomInstance != null)
            {
                var spawnPoints = genRoom.RoomInstance.GetComponentsInChildren<SpawnPoint>();
                foreach (var sp in spawnPoints)
                {
                    genRoom.SpawnPositions.Add(sp.transform.position);
                }
            }

            generatedRooms.Add(genRoom);

            if (floorNumber > currentTopFloor)
            {
                currentTopFloor = floorNumber;
            }

            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Generated room at floor {floorNumber}, offset {offset}");
            }

            return genRoom;
        }

        /// <summary>
        /// Generates the next room above the current top floor.
        /// </summary>
        public GeneratedRoom GenerateNextRoom()
        {
            int nextFloor = currentTopFloor + 1;
            var template = GetTemplateForFloor(nextFloor);
            return GenerateRoom(nextFloor, template);
        }

        /// <summary>
        /// Instantiates room prefab and stamps tiles into the master tilemap.
        /// </summary>
        private GameObject StampRoomToTilemap(RoomTemplate template, Vector3Int offset)
        {
            if (template.prefab == null)
            {
                Debug.LogError($"ProceduralMapGenerator: Template {template.name} has no prefab assigned!");
                return null;
            }

            // Get template tilemap (from prefab asset)
            var prefabTilemap = template.prefab.GetComponentInChildren<Tilemap>(true);
            if (prefabTilemap == null)
            {
                Debug.LogError($"ProceduralMapGenerator: Template prefab {template.prefab.name} has no Tilemap component!");
                return null;
            }

            var templateBounds = prefabTilemap.cellBounds;

            // Calculate world position for the prefab - subtract template bounds position to match normalized tile positions
            Vector3 worldOffset = masterTilemap.CellToWorld(offset - templateBounds.position);

            // Instantiate the prefab for visual sprites
            var roomInstance = Instantiate(template.prefab, worldOffset, Quaternion.identity, transform);
            roomInstance.name = $"Room_Floor{currentTopFloor + 1}";

            // Get the tilemap from the instantiated prefab
            var instanceTilemap = roomInstance.GetComponentInChildren<Tilemap>(true);
            int tileCount = 0;

            // Copy tiles to master tilemap - normalize positions relative to template bounds origin
            foreach (var pos in templateBounds.allPositionsWithin)
            {
                var tile = prefabTilemap.GetTile(pos);
                if (tile != null)
                {
                    // Subtract templateBounds.position to normalize tiles to start at (0,0) + offset
                    Vector3Int targetPos = pos - templateBounds.position + offset;

                    // Don't overwrite existing door tiles - preserves doors at room boundaries
                    var existingTile = masterTilemap.GetTile<GameTile>(targetPos);
                    if (existingTile != null &&
                        (existingTile.tileType == GameTile.TileType.Door ||
                         existingTile.tileType == GameTile.TileType.DoorClosed))
                    {
                        continue; // Preserve the door
                    }

                    masterTilemap.SetTile(targetPos, tile);
                    tileCount++;
                }
            }

            // Disable the instance's tilemap renderer (we use master tilemap for logic)
            var instanceTilemapRenderer = instanceTilemap.GetComponent<TilemapRenderer>();
            if (instanceTilemapRenderer != null)
            {
                instanceTilemapRenderer.enabled = false;
            }

            // Also disable tilemap collider if present (master tilemap handles collision)
            var instanceCollider = instanceTilemap.GetComponent<TilemapCollider2D>();
            if (instanceCollider != null)
            {
                instanceCollider.enabled = false;
            }

            // Refresh the master tilemap
            masterTilemap.CompressBounds();
            masterTilemap.RefreshAllTiles();

            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Instantiated {template.name} at {worldOffset}, stamped {tileCount} tiles at offset {offset}");
            }

            return roomInstance;
        }

        /// <summary>
        /// Selects an appropriate template for the given floor number.
        /// </summary>
        private RoomTemplate GetTemplateForFloor(int floorNumber)
        {
            // Filter templates that are valid for this floor
            var validTemplates = new List<RoomTemplate>();
            int totalWeight = 0;

            foreach (var template in templates)
            {
                // Check floor range
                if (floorNumber < template.minFloor) continue;
                if (template.maxFloor >= 0 && floorNumber > template.maxFloor) continue;

                validTemplates.Add(template);
                totalWeight += template.selectionWeight;
            }

            if (validTemplates.Count == 0)
            {
                if (debugMode)
                {
                    Debug.Log($"ProceduralMapGenerator: No templates configured for floor {floorNumber}, using fallback");
                }
                return templates.Count > 0 ? templates[0] : null;
            }

            // Weighted random selection
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var template in validTemplates)
            {
                currentWeight += template.selectionWeight;
                if (randomValue < currentWeight)
                {
                    return template;
                }
            }

            return validTemplates[0];
        }

        /// <summary>
        /// Called when player Y position changes. Triggers generation/unloading as needed.
        /// </summary>
        public void OnPlayerPositionChanged(float playerY)
        {
            int playerFloor = GetFloorAtY(playerY);

            // Generate rooms ahead if needed
            int targetTopFloor = playerFloor + generateAhead;
            while (currentTopFloor < targetTopFloor)
            {
                var newRoom = GenerateNextRoom();
                if (newRoom != null)
                {
                    OnRoomGenerated?.Invoke(newRoom);
                }
            }

            // Unload old rooms if needed
            int targetBottomFloor = Mathf.Max(0, playerFloor - keepBehind);
            if (targetBottomFloor > currentBottomFloor)
            {
                UnloadRoomsBelowFloor(targetBottomFloor);
            }
        }

        /// <summary>
        /// Unloads rooms below the specified floor number.
        /// </summary>
        private void UnloadRoomsBelowFloor(int floorNumber)
        {
            var roomsToUnload = new List<GeneratedRoom>();

            foreach (var room in generatedRooms)
            {
                if (room.FloorNumber < floorNumber && room.IsLoaded)
                {
                    roomsToUnload.Add(room);
                }
            }

            foreach (var room in roomsToUnload)
            {
                UnloadRoom(room);
            }

            currentBottomFloor = floorNumber;

            if (debugMode && roomsToUnload.Count > 0)
            {
                Debug.Log($"ProceduralMapGenerator: Unloaded {roomsToUnload.Count} rooms below floor {floorNumber}");
            }
        }

        /// <summary>
        /// Unloads a specific room, clearing its tiles from the master tilemap.
        /// </summary>
        private void UnloadRoom(GeneratedRoom room)
        {
            if (!room.IsLoaded) return;

            // Clear tiles in this room's bounds
            foreach (var pos in room.TileBounds.allPositionsWithin)
            {
                masterTilemap.SetTile(pos, null);
            }

            // Destroy the visual instance
            if (room.RoomInstance != null)
            {
                Destroy(room.RoomInstance);
                room.RoomInstance = null;
            }

            room.IsLoaded = false;

            OnRoomUnloaded?.Invoke(room);

            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Unloaded room at floor {room.FloorNumber}");
            }
        }

        /// <summary>
        /// Gets the floor number for a given Y world position.
        /// </summary>
        public int GetFloorAtY(float worldY)
        {
            if (generatedRooms.Count == 0) return 0;

            float cellHeight = masterTilemap.cellSize.y;

            // Get room height from first template
            int roomHeight = 12;
            if (templates.Count > 0)
            {
                var template = templates[0];
                if (template.stackingHeight > 0)
                {
                    roomHeight = template.stackingHeight;
                }
                else if (template.prefab != null)
                {
                    var tilemap = template.prefab.GetComponentInChildren<Tilemap>(true);
                    if (tilemap != null)
                    {
                        roomHeight = GetTightHeight(tilemap);
                    }
                }
            }

            return Mathf.FloorToInt(worldY / (roomHeight * cellHeight));
        }

        /// <summary>
        /// Gets the GeneratedRoom at the specified floor number.
        /// </summary>
        public GeneratedRoom GetRoomAtFloor(int floorNumber)
        {
            foreach (var room in generatedRooms)
            {
                if (room.FloorNumber == floorNumber)
                {
                    return room;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the GeneratedRoom containing the specified world Y position.
        /// </summary>
        public GeneratedRoom GetRoomAtY(float worldY)
        {
            return GetRoomAtFloor(GetFloorAtY(worldY));
        }

        /// <summary>
        /// Gets the bounds for a specific floor.
        /// </summary>
        public BoundsInt GetBoundsForFloor(int floorNumber)
        {
            var room = GetRoomAtFloor(floorNumber);
            return room?.TileBounds ?? new BoundsInt();
        }

        /// <summary>
        /// Calculates the tight height of actual tile content (excluding empty space).
        /// </summary>
        private int GetTightHeight(Tilemap tilemap)
        {
            var bounds = tilemap.cellBounds;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            int tileCount = 0;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.GetTile(pos) != null)
                {
                    minY = Mathf.Min(minY, pos.y);
                    maxY = Mathf.Max(maxY, pos.y);
                    tileCount++;
                }
            }

            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: GetTightHeight - bounds={bounds}, tilesFound={tileCount}, minY={minY}, maxY={maxY}");
            }

            if (minY == int.MaxValue)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"ProceduralMapGenerator: No tiles found in tilemap, using bounds height {bounds.size.y}");
                }
                return bounds.size.y; // Fallback to full bounds if no tiles found
            }

            int height = maxY - minY + 1;
            if (debugMode)
            {
                Debug.Log($"ProceduralMapGenerator: Tight height = {height}");
            }
            return height;
        }

        // Events for external systems to respond to generation
        public event System.Action<GeneratedRoom> OnRoomGenerated;
        public event System.Action<GeneratedRoom> OnRoomUnloaded;
    }
}
