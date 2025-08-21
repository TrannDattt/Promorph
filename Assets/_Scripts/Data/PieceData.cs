using System;
using Promorph.Board;
using UnityEngine;

namespace Promorph.Data
{
    [CreateAssetMenu(fileName = "BoardData", menuName = "ScriptableObjects/PieceData")]
    public class PieceData : ScriptableObject
    {
        public EChessPiece Type;
        public int Value;
        public Sprite WhiteIcon;
        public Sprite BlackIcon;
        public MovePattern MoveSet;
        public MovePattern CaptureSet;

        // Double square for pawns, castling, etc.
        public MovePattern SpecialMoveSet;
        // Special moves like en passant, etc.
        public MovePattern SpecialCaptureSet;
    }

    [Serializable]
    public class MovePattern
    {
        public Vector2Int[] Steps;
        public Vector2Int[] Directions;
        public int MaxDistance;
    }
}
