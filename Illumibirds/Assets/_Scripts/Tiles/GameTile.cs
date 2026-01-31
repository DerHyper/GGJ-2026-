using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles
{
    [CreateAssetMenu(menuName = "Tiles/Game Tile", fileName = "Tile_")]
    public class GameTile : Tile
    {
        public enum TileType
        {
            Floor,
            Wall,
            Door,
            DoorClosed,
            Spawn,
            HalfWall
        }

        [Tooltip("The functional type of this tile")]
        public TileType tileType;

        [Tooltip("Can entities pass through this tile?")]
        public bool isWalkable;

        [Tooltip("For Door tiles: the closed version of this door")]
        public GameTile closedDoorTile;

        [Tooltip("For DoorClosed tiles: the open version of this door")]
        public GameTile openDoorTile;
    }
}
