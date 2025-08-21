using UnityEngine;
using UnityEngine.Tilemaps;

namespace Promorph.Data
{
    [CreateAssetMenu(fileName = "BoardData", menuName = "ScriptableObjects/BoardData")]
    public class BoardData : ScriptableObject
    {
        public Sprite BoardSprite; // Use as lower layer
        public Tile LightTile;
        public Tile DarkTile;
        public Tile MoveHighlightTile;
        public Tile CaptureHighlightTile;
        public Tile SpecialHighlightTile;
    }
}
