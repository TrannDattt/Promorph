using System;
using Promorph.Board;
using Promorph.Data;
using Promorph.Utils;
using UnityEngine;

namespace Promorph.Factories
{
    public class ChessPieceFactory : Singleton<ChessPieceFactory>
    {
        [SerializeField] private ChessPiece _chessPiecePrefab;

        public ChessPiece CreateChessPiece(EFaction faction, PieceData data)
        {
            var chessPiece = Instantiate(_chessPiecePrefab, default, Quaternion.identity);
            chessPiece.SetData(data);
            chessPiece.ChangeFaction(faction);
            return chessPiece;
        }
    }
}