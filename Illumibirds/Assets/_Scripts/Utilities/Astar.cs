using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
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
    [SerializeField] private  Tilemap WalkableTiles;
    private Node[,] grid;
    private int gridWidth;
    private int gridHeight;

    public AStarPathfinding()
    {
        // Init with room size
        OnCurrentRoomChanged();
        RoomManager.Instance.CurrentRoomChanged.AddListener(OnCurrentRoomChanged);
        BoundsInt bounds = WalkableTiles.cellBounds;
        int width = bounds.size.x * WalkPointsPerTile;
        int height = bounds.size.y * WalkPointsPerTile;
        InitializeGrid(width, height);
        SetWalkableForTilemap();
        DilateObstacles();
    }

    /// <summary>
    /// Get path from start to target in grid coordinates
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public List<Vector2Int> GetPathGrid(Vector2 start, Vector2 target)
    {
        // Test Player
        List<Vector2Int> path = FindPath(WorldToTileIndex(start), WorldToTileIndex(target));
        
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
        WalkableTiles = RoomManager.Instance.GetCurrentRoom().WalkableTiles;
        SetWalkableForTilemap();
    }

    private void SetWalkableForTilemap()
    {
        BoundsInt bounds = WalkableTiles.cellBounds;
        TileBase[] allTiles = WalkableTiles.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile == null)
                {
                    continue;
                }

                SetWalkableSubpoints(x, y);
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
    for (int iter = 0; iter < iterations; iter++)
    {
        // Erstelle temporäres Grid für aktuelle Iteration
        bool[,] tempWalkable = new bool[gridWidth, gridHeight];
        
        // Kopiere aktuellen Zustand
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tempWalkable[x, y] = grid[x, y].walkable;
            }
        }
        
        // Dilatiere
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Wenn aktuelles Feld nicht begehbar ist
                if (!grid[x, y].walkable)
                {
                    // Setze alle Nachbarn auf nicht begehbar
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
        
        // Übernehme Änderungen
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
        // Konvertiere zu Cell-Position
        Vector3Int cellPosition = WalkableTiles.WorldToCell(worldPosition);
        BoundsInt bounds = WalkableTiles.cellBounds;
        
        // Relative Tile-Position
        int tileX = cellPosition.x - bounds.xMin;
        int tileY = cellPosition.y - bounds.yMin;
        
        // Berechne Sub-Tile-Position innerhalb der Zelle
        Vector3 cellWorldPos = WalkableTiles.CellToWorld(cellPosition);
        Vector3 cellSize = WalkableTiles.cellSize;
        
        float relativeX = (worldPosition.x - cellWorldPos.x) / cellSize.x;
        float relativeY = (worldPosition.y - cellWorldPos.y) / cellSize.y;
        
        int subX = Mathf.FloorToInt(relativeX * WalkPointsPerTile);
        int subY = Mathf.FloorToInt(relativeY * WalkPointsPerTile);
        
        // Clamp Sub-Positionen
        subX = Mathf.Clamp(subX, 0, WalkPointsPerTile - 1);
        subY = Mathf.Clamp(subY, 0, WalkPointsPerTile - 1);
        
        // Kombiniere zu Grid-Index
        int gridX = tileX * WalkPointsPerTile + subX;
        int gridY = tileY * WalkPointsPerTile + subY;
        
        // Clamp zu Grid-Grenzen
        gridX = Mathf.Clamp(gridX, 0, gridWidth - 1);
        gridY = Mathf.Clamp(gridY, 0, gridHeight - 1);
        
        return new Vector2Int(gridX, gridY);
    }

    public Vector2 TileIndexToWorld(Vector2Int gridIndex)
    {
        BoundsInt bounds = WalkableTiles.cellBounds;
    
        // Berechne Tile-Index und Sub-Position
        int tileX = gridIndex.x / WalkPointsPerTile;
        int tileY = gridIndex.y / WalkPointsPerTile;
        int subX = gridIndex.x % WalkPointsPerTile;
        int subY = gridIndex.y % WalkPointsPerTile;
        
        // Cell-Position in der Tilemap
        Vector3Int cellPosition = new Vector3Int(
            tileX + bounds.xMin,
            tileY + bounds.yMin,
            0
        );
        
        // World-Position der Zelle (linke untere Ecke)
        Vector3 cellWorldPos = WalkableTiles.CellToWorld(cellPosition);
        Vector3 cellSize = WalkableTiles.cellSize;
        
        // Berechne Sub-Position innerhalb der Zelle
        float subPointSizeX = cellSize.x / WalkPointsPerTile;
        float subPointSizeY = cellSize.y / WalkPointsPerTile;
        
        // Zentriere den Sub-Punkt
        float worldX = cellWorldPos.x + (subX + 0.5f) * subPointSizeX;
        float worldY = cellWorldPos.y + (subY + 0.5f) * subPointSizeY;
        
        return new Vector2(worldX, worldY);
    }

    // Grid initialisieren
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

    // Hindernisse setzen
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
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns>List of Vector2Int positions representing the path in grid coordinates</returns>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        Node startNode = grid[start.x, start.y];
        Node targetNode = grid[target.x, target.y];

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // Finde Node mit niedrigsten fCost
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

            // Ziel erreicht?
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // Nachbarn überprüfen
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

        return null; // Kein Pfad gefunden
    }

    // Nachbarn einer Node finden (8 Richtungen)
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

    // Distanz zwischen zwei Nodes berechnen
    private float GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.position.x - b.position.x);
        int distY = Mathf.Abs(a.position.y - b.position.y);

        // Diagonale Bewegung berücksichtigen
        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    // Pfad zurückverfolgen
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