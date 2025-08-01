using Promorph.Data;
using Promorph.Utils;
using UnityEngine;

namespace Promorph.Board
{
    public class BoardManager : Singleton<BoardManager>
    {
        private ChessBoard _chessBoard;

        public void ShowAvailableMoves(ChessPiece piece)
        {
            if (_chessBoard == null)
            {
                Debug.LogError("ChessBoard instance is not set.");
                return;
            }

            _chessBoard.ClearHighlights();
            _chessBoard.OnTileClicked.AddListener((position, action) =>
            {
                if (action == EPieceAction.Move || action == EPieceAction.Capture)
                {
                    PerformPieceAction(piece, position, action);
                }
            });

            // Logic to show available moves for the piece
            Vector2Int piecePos = _chessBoard.GetPositionInBoard(piece.transform.position);
            Vector2Int[] availableMoves = piece.MoveSet;

            //TODO: Highlight tile if has tile and not blocked by other pieces
            foreach (var move in availableMoves)
            {
                var targetPosition = piecePos + move;
                if (_chessBoard.IsValidPosition(targetPosition) && !_chessBoard.IsTileOccupied(targetPosition, out _))
                {
                    // Highlight or indicate the available move on the board
                    _chessBoard.HighlightTile(targetPosition, EPieceAction.Move);
                }
            }

            Vector2Int[] availableCaptures = piece.CaptureSet;
            //TODO: Highlight tile if has tile and is occupied by an enemy piece
            foreach (var capture in availableCaptures)
            {
                var targetPosition = piecePos + capture;
                Debug.Log($"{_chessBoard.IsValidPosition(targetPosition)}, {_chessBoard.IsTileOccupied(targetPosition, out var occupyingPiece2)}, {!piece.CheckIsAllyPiece(occupyingPiece2)}");
                if (_chessBoard.IsValidPosition(targetPosition) && _chessBoard.IsTileOccupied(targetPosition, out var occupyingPiece) && !piece.CheckIsAllyPiece(occupyingPiece))
                {
                    // Highlight or indicate the available capture on the board
                    _chessBoard.HighlightTile(targetPosition, EPieceAction.Capture);
                }
            }
        }

        private void PerformPieceAction(ChessPiece piece, Vector2Int targetPosition, EPieceAction action)
        {
            if (_chessBoard == null)
            {
                Debug.LogError("ChessBoard instance is not set.");
                return;
            }

            var curPos = _chessBoard.GetPositionInBoard(piece.transform.position);
            var curTile = _chessBoard.GetTileByPosition(curPos);
            curTile.SetOccupyingPiece(null);

            var worldPos = _chessBoard.GetTilePosition(targetPosition);
            var tile = _chessBoard.GetTileByPosition(targetPosition);

            _chessBoard.ClearHighlights();

            if (tile == null)
            {
                return;
            }

            if (tile.OccupyingPiece)
            {
                tile.OccupyingPiece.BeCaptured();
                Debug.Log($"Captured piece {tile.OccupyingPiece.Type} at position {targetPosition}");
            }

            piece.Move(worldPos);
            tile.SetOccupyingPiece(piece);
        }

        public void HideAvailableMoves()
        {
            _chessBoard.ClearHighlights();
        }

        protected override void Awake()
        {
            base.Awake();

            _chessBoard = FindAnyObjectByType<ChessBoard>();
            if (_chessBoard == null)
            {
                Debug.LogError("ChessBoard instance not found in the scene. Please ensure it is present.");
            }
        }

        // public void SetBoardData(BoardData boardData)
        // {
        //     _boardData = boardData;
        //     ChessBoard.Instance.SetBoardData(boardData);
        // }
    }
}
