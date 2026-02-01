using System.Collections.Generic;
using Rooms;
using Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinding
{
    public class Node
    {
        public Vector2Int position;
        public bool walkable;
        public float gCost; // Distance from start
        public float hCost; // Heuristic Distance to goal
        public float fCost => gCost + hCost; // total goal
        public Node parent;

        public Node(Vector2Int pos, bool isWalkable)
        {
            position = pos;
            walkable = isWalkable;
        }

        public void Reset()
        {
            gCost = 0;
            hCost = 0;
            parent = null;
        }
    }

    // Singleton instance
    private static AStarPathfinding _instance;
    public static AStarPathfinding Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AStarPathfinding();
            }
            return _instance;
        }
    }

    const int WalkPointsPerTile = 4;

    private Tilemap tilemap;
    private TilemapScanner scanner;
    private BoundsInt currentBounds;

    private Node[,] grid;
    private int gridWidth;
    private int gridHeight;
    private bool isInitialized;

    // Path caching per requester
    private Dictionary<int, CachedPath> _pathCache = new Dictionary<int, CachedPath>();
    private const float PATH_CACHE_DURATION = 0.3f; // Recalculate path every 0.3 seconds
    private const float PATH_RECALC_DISTANCE = 1.5f; // Recalculate if target moved more than this

    private class CachedPath
    {
        public List<Vector2Int> path;
        public float timestamp;
        public Vector2 lastTargetPos;
        public int currentIndex;
    }

    public AStarPathfinding()
    {
        // Only initialize if this is the singleton instance being created
    }

    private void EnsureInitialized()
    {
        if (isInitialized) return;

        var roomManager = RoomManager.Instance;
        if (roomManager == null) return;

        tilemap = roomManager.Tilemap;
        scanner = roomManager.Scanner;

        if (tilemap == null || scanner == null)
        {
            Debug.LogWarning("AStarPathfinding: Tilemap or Scanner is null");
            return;
        }

        roomManager.CurrentRoomChanged.AddListener(OnCurrentRoomChanged);
        OnCurrentRoomChanged();
        isInitialized = true;
    }

    public void DesubscribeFromEvent()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentRoomChanged.RemoveListener(OnCurrentRoomChanged);
        }
    }

    /// <summary>
    /// Get next point for an entity to move towards (with caching)
    /// </summary>
    public Vector2 GetNextPointWorld(Vector2 start, Vector2 target, int requesterId)
    {
        EnsureInitialized();
        if (grid == null) return start;

        float currentTime = Time.time;
        bool needsRecalc = true;
        CachedPath cached = null;

        if (_pathCache.TryGetValue(requesterId, out cached))
        {
            // Check if we can reuse the cached path
            float timeSinceCalc = currentTime - cached.timestamp;
            float targetMovement = Vector2.Distance(target, cached.lastTargetPos);

            if (timeSinceCalc < PATH_CACHE_DURATION && targetMovement < PATH_RECALC_DISTANCE)
            {
                needsRecalc = false;
            }
        }

        if (needsRecalc)
        {
            // Calculate new path
            List<Vector2Int> path = FindPath(WorldToTileIndex(start), WorldToTileIndex(target));

            if (cached == null)
            {
                cached = new CachedPath();
                _pathCache[requesterId] = cached;
            }

            cached.path = path;
            cached.timestamp = currentTime;
            cached.lastTargetPos = target;
            cached.currentIndex = 0;
        }

        // Return next point from cached path
        if (cached == null || cached.path == null || cached.path.Count == 0)
            return start;

        // Advance index if we're close to current waypoint
        while (cached.currentIndex < cached.path.Count)
        {
            Vector2 waypoint = TileIndexToWorld(cached.path[cached.currentIndex]);
            if (Vector2.Distance(start, waypoint) < 0.3f)
            {
                cached.currentIndex++;
            }
            else
            {
                break;
            }
        }

        if (cached.currentIndex >= cached.path.Count)
            return target; // Reached end of path

        return TileIndexToWorld(cached.path[cached.currentIndex]);
    }

    /// <summary>
    /// Legacy method - Get path from start to target in grid coordinates
    /// </summary>
    public List<Vector2Int> GetPathGrid(Vector2 start, Vector2 target)
    {
        EnsureInitialized();
        List<Vector2Int> path = FindPath(WorldToTileIndex(start), WorldToTileIndex(target));
        return path;
    }

    public Vector2 GetNextPointWorld(Vector2 start, Vector2 target)
    {
        // Legacy method without caching - use instance ID 0
        return GetNextPointWorld(start, target, 0);
    }

    private void OnCurrentRoomChanged()
    {
        if (tilemap == null) return;

        currentBounds = tilemap.cellBounds;
        int width = currentBounds.size.x * WalkPointsPerTile;
        int height = currentBounds.size.y * WalkPointsPerTile;

        InitializeGrid(width, height);
        SetWalkableForTilemap();
        DilateObstacles();

        // Clear path cache when room changes
        _pathCache.Clear();
    }

    private void SetWalkableForTilemap()
    {
        if (scanner == null) return;

        for (int x = 0; x < currentBounds.size.x; x++)
        {
            for (int y = 0; y < currentBounds.size.y; y++)
            {
                Vector3Int cellPos = new Vector3Int(
                    x + currentBounds.xMin,
                    y + currentBounds.yMin,
                    0
                );

                // Use TilemapScanner to check walkability via GameTile.isWalkable
                if (scanner.IsWalkable(cellPos))
                {
                    SetWalkableSubpoints(x, y);
                }
            }
        }
    }

    private void SetWalkableSubpoints(int x, int y)
    {
        for (int subX = 0; subX < WalkPointsPerTile; subX++)
        {
            for (int subY = 0; subY < WalkPointsPerTile; subY++)
            {
                int gridX = x * WalkPointsPerTile + subX;
                int gridY = y * WalkPointsPerTile + subY;
                SetWalkable(gridX, gridY, true);
            }
        }
    }

    public void DilateObstacles(int iterations = 1)
    {
        if (grid == null) return;

        for (int iter = 0; iter < iterations; iter++)
        {
            bool[,] tempWalkable = new bool[gridWidth, gridHeight];

            // Copy current state
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    tempWalkable[x, y] = grid[x, y].walkable;
                }
            }

            // Dilate
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (!grid[x, y].walkable)
                    {
                        // Set all neighbors to non-walkable
                        for (int nx = -1; nx <= 1; nx++)
                        {
                            for (int ny = -1; ny <= 1; ny++)
                            {
                                int checkX = x + nx;
                                int checkY = y + ny;

                                if (checkX >= 0 && checkX < gridWidth &&
                                    checkY >= 0 && checkY < gridHeight)
                                {
                                    tempWalkable[checkX, checkY] = false;
                                }
                            }
                        }
                    }
                }
            }

            // Apply changes
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y].walkable = tempWalkable[x, y];
                }
            }
        }
    }

    public Vector2Int WorldToTileIndex(Vector2 worldPosition)
    {
        if (tilemap == null) return Vector2Int.zero;

        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        // Relative Tile-Position
        int tileX = cellPosition.x - currentBounds.xMin;
        int tileY = cellPosition.y - currentBounds.yMin;

        // Calculate sub-tile position within cell
        Vector3 cellWorldPos = tilemap.CellToWorld(cellPosition);
        Vector3 cellSize = tilemap.cellSize;

        float relativeX = (worldPosition.x - cellWorldPos.x) / cellSize.x;
        float relativeY = (worldPosition.y - cellWorldPos.y) / cellSize.y;

        int subX = Mathf.FloorToInt(relativeX * WalkPointsPerTile);
        int subY = Mathf.FloorToInt(relativeY * WalkPointsPerTile);

        // Clamp sub-positions
        subX = Mathf.Clamp(subX, 0, WalkPointsPerTile - 1);
        subY = Mathf.Clamp(subY, 0, WalkPointsPerTile - 1);

        // Combine to grid index
        int gridX = tileX * WalkPointsPerTile + subX;
        int gridY = tileY * WalkPointsPerTile + subY;

        // Clamp to grid bounds
        gridX = Mathf.Clamp(gridX, 0, gridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, gridHeight - 1);

        return new Vector2Int(gridX, gridY);
    }

    public Vector2 TileIndexToWorld(Vector2Int gridIndex)
    {
        if (tilemap == null) return Vector2.zero;

        // Calculate tile index and sub-position
        int tileX = gridIndex.x / WalkPointsPerTile;
        int tileY = gridIndex.y / WalkPointsPerTile;
        int subX = gridIndex.x % WalkPointsPerTile;
        int subY = gridIndex.y % WalkPointsPerTile;

        // Cell position in tilemap
        Vector3Int cellPosition = new Vector3Int(
            tileX + currentBounds.xMin,
            tileY + currentBounds.yMin,
            0
        );

        // World position of cell (bottom-left corner)
        Vector3 cellWorldPos = tilemap.CellToWorld(cellPosition);
        Vector3 cellSize = tilemap.cellSize;

        // Calculate sub-position within cell
        float subPointSizeX = cellSize.x / WalkPointsPerTile;
        float subPointSizeY = cellSize.y / WalkPointsPerTile;

        // Center the sub-point
        float worldX = cellWorldPos.x + (subX + 0.5f) * subPointSizeX;
        float worldY = cellWorldPos.y + (subY + 0.5f) * subPointSizeY;

        return new Vector2(worldX, worldY);
    }

    public void InitializeGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Node(new Vector2Int(x, y), false);
            }
        }
    }

    public void SetWalkable(int x, int y, bool walkable)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            grid[x, y].walkable = walkable;
        }
    }

    /// <summary>
    /// A* Pathfinding
    /// </summary>
    /// <returns>List of Vector2Int positions representing the path in grid coordinates</returns>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        if (grid == null) return null;
        if (start.x < 0 || start.x >= gridWidth || start.y < 0 || start.y >= gridHeight) return null;
        if (target.x < 0 || target.x >= gridWidth || target.y < 0 || target.y >= gridHeight) return null;

        // Reset nodes used in previous search
        ResetGridCosts();

        Node startNode = grid[start.x, start.y];
        Node targetNode = grid[target.x, target.y];

        // Use a simple priority approach - sorted insert
        List<Node> openList = new List<Node>(256);
        HashSet<Node> closedSet = new HashSet<Node>();
        HashSet<Node> openSet = new HashSet<Node>();

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);
        openList.Add(startNode);
        openSet.Add(startNode);

        int maxIterations = 5000; // Prevent infinite loops
        int iterations = 0;

        while (openList.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            // Find node with lowest fCost (from end is faster for sorted list)
            Node currentNode = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Goal reached?
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Check neighbors
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                float newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        // Insert sorted (highest fCost first, so lowest is at end)
                        InsertSorted(openList, neighbor);
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null; // No path found
    }

    private void InsertSorted(List<Node> list, Node node)
    {
        float fCost = node.fCost;
        // Insert in descending order (highest first, lowest at end)
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].fCost < fCost)
            {
                list.Insert(i, node);
                return;
            }
        }
        list.Add(node);
    }

    private void ResetGridCosts()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y].gCost = float.MaxValue;
                grid[x, y].hCost = 0;
                grid[x, y].parent = null;
            }
        }
    }

    // Reusable list for neighbors to reduce allocations
    private List<Node> _neighborCache = new List<Node>(8);

    // Find neighbors of a node (8 directions)
    private List<Node> GetNeighbors(Node node)
    {
        _neighborCache.Clear();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.position.x + x;
                int checkY = node.position.y + y;

                if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
                {
                    _neighborCache.Add(grid[checkX, checkY]);
                }
            }
        }

        return _neighborCache;
    }

    // Calculate distance between two nodes
    private float GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.position.x - b.position.x);
        int distY = Mathf.Abs(a.position.y - b.position.y);

        // Consider diagonal movement
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    // Retrace path
    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.position);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    public void RemoveRequester(int requesterId)
    {
        _pathCache.Remove(requesterId);
    }
}
