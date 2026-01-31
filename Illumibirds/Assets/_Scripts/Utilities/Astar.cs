using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinding : MonoBehaviour
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

    public Tilemap WalkableTiles;
    private Node[,] grid;
    private int gridWidth;
    private int gridHeight;
    public Transform TestEnemy;
    public Transform TestPlayer;

    private void Start() {

        // Init with room size
        BoundsInt bounds = WalkableTiles.cellBounds;
        int width =  bounds.size.x;
        int height = bounds.size.y;
        InitializeGrid(width,height);

        // Set Walkable
        TileBase[] allTiles = WalkableTiles.GetTilesBlock(bounds);
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if (tile != null) {
                    SetWalkable(x,y,true);
                }
            }
        }

        // Test Player
        var enemyPos = new Vector2Int(Mathf.RoundToInt(TestEnemy.position.x), Mathf.RoundToInt(TestEnemy.position.y));
        var playerPos = new Vector2Int(Mathf.RoundToInt(TestPlayer.position.x), Mathf.RoundToInt(TestPlayer.position.y));
        Debug.Log(FindPath(enemyPos, playerPos));
    }

    // public Vector2Int WorldToTileIndex(Vector2 worldPosition)
    // {
        
    // }

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

    // A* Pathfinding
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