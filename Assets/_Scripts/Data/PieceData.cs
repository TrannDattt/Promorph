using Promorph.Board;
using UnityEngine;

namespace Promorph.Data
{
    [CreateAssetMenu(fileName = "BoardData", menuName = "ScriptableObjects/PieceData")]
    public class PieceData : ScriptableObject
    {
        public EChessPiece Type;
        public Sprite Icon;
        public Vector2Int[] MoveSet;
        public Vector2Int[] CaptureSet;
        
        // Double square for pawns, castling, etc.
        public Vector2Int[] FirstMoveSet;
        // Special moves like en passant, etc.
        public Vector2Int[] SpecialCaptureSet;
    }
}
