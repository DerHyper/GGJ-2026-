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
    }

    const int WalkPointsPerTile = 4;

    private Tilemap tilemap;
    private TilemapScanner scanner;
    private BoundsInt currentBounds;

    private Node[,] grid;
    private int gridWidth;
    private int gridHeight;

    public AStarPathfinding()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogWarning("AStarPathfinding: RoomManager.Instance is null");
            return;
        }

        tilemap = RoomManager.Instance.Tilemap;
        scanner = RoomManager.Instance.Scanner;

        if (tilemap == null || scanner == null)
        {
            Debug.LogWarning("AStarPathfinding: Tilemap or Scanner is null");
            return;
        }

        RoomManager.Instance.CurrentRoomChanged.AddListener(OnCurrentRoomChanged);
        OnCurrentRoomChanged();
    }

    public void DesubscribeFromEvent()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.CurrentRoomChanged.RemoveListener(OnCurrentRoomChanged);
        }
        Debug.Log("DESUB");
    }

    /// <summary>
    /// Get path from start to target in grid coordinates
    /// </summary>
    public List<Vector2Int> GetPathGrid(Vector2 start, Vector2 target)
    {
        List<Vector2Int> path = FindPath(WorldToTileIndex(start), WorldToTileIndex(target));

        if (path == null || path.Count == 0)
            return path;

        // Debug Path
        var lastItem = path[0];
        foreach (Vector2Int item in path)
        {
            Debug.DrawLine(TileIndexToWorld(lastItem), TileIndexToWorld(item), Color.red);
            lastItem = item;
        }

        return path;
    }

    public Vector2 GetNextPointWorld(Vector2 start, Vector2 target)
    {
        List<Vector2Int> path = FindPath(WorldToTileIndex(start), WorldToTileIndex(target));
        if (path == null || path.Count == 0)
            return start; // No path found or already at target

        Vector2Int nextIndex = path[0];
        return TileIndexToWorld(nextIndex);
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

        Node startNode = grid[start.x, start.y];
        Node targetNode = grid[target.x, target.y];

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Find node with lowest fCost
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            // Goal reached?
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Check neighbors
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedList.Contains(neighbor))
                    continue;

                float newGCost = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newGCost < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    // Find neighbors of a node (8 directions)
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

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
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
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
}
