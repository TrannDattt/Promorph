using System.Collections.Generic;
using System.Linq;
using Promorph.Data;
using Promorph.Utils;
using UnityEngine;

namespace Promorph.Board
{
    public class BoardManager : Singleton<BoardManager>
    {
        private ChessBoard _chessBoard;
        private List<ChessPiece> _pieces = new();
        private ChessPiece _selectedPiece;

        public void Initialize()
        {
            _chessBoard.GenerateBoard();
            _chessBoard.SetupPieces(new(){ _pieces[0], _pieces[1], _pieces[2] }, new(){ _pieces[3], _pieces[4], _pieces[5] });
        }

        public void ShowAvailableMoves(ChessPiece piece)
        {
            _chessBoard.ClearHighlights();
            _chessBoard.OnTileClicked.AddListener((position) =>
            {
                // if (action == EPieceAction.Move || action == EPieceAction.Capture)
                // {
                    PerformPieceAction(piece, position);
                // }
            });

            // Logic to show available moves for the piece
            Vector2Int piecePos = _chessBoard.WorldToBoardPosition(piece.transform.position);
            Vector2Int[] availableMoves = piece.MoveSet;

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
            foreach (var capture in availableCaptures)
            {
                var targetPosition = piecePos + capture;
                if (_chessBoard.IsValidPosition(targetPosition) && _chessBoard.IsTileOccupied(targetPosition, out var occupyingPiece) && !piece.CheckIsAllyPiece(occupyingPiece))
                {
                    // Highlight or indicate the available capture on the board
                    _chessBoard.HighlightTile(targetPosition, EPieceAction.Capture);
                }
            }
        }

        private void PerformPieceAction(ChessPiece piece, Vector2Int targetPosition)
        {
            var curPos = _chessBoard.WorldToBoardPosition(piece.transform.position);
            var curTile = _chessBoard.BoardPositionToTile(curPos);
            curTile.SetOccupyingPiece(null);
            _chessBoard.ClearHighlights();

            var targetTile = _chessBoard.BoardPositionToTile(targetPosition);

            if (targetTile.OccupyingPiece != null)
            {
                CaptureChessPiece(targetTile);
            }

            MoveChessToTile(piece, targetTile);

            DeselectPiece(_selectedPiece);
        }

        public bool CheckTileBlock(ChessPiece piece, Vector2Int relativePosition, out ChessPiece blockPiece)
        {
            Vector2Int piecePos = _chessBoard.WorldToBoardPosition(piece.transform.position);
            var absolutePosition = piecePos + relativePosition;
            return _chessBoard.IsTileOccupied(absolutePosition, out blockPiece);
        }

        private void MoveChessToTile(ChessPiece piece, BoardTile tile)
        {
            var targetPosition = _chessBoard.TileToWorldPosition(tile);
            piece.Move(targetPosition);
            tile.SetOccupyingPiece(piece);
        }

        private void CaptureChessPiece(BoardTile tile)
        {
            var piece = tile.OccupyingPiece;
            piece.BeCaptured();
            tile.SetOccupyingPiece(null);
        }

        public void HideAvailableMoves()
        {
            _chessBoard.ClearHighlights();
        }

        private void SelectPiece(ChessPiece piece)
        {
            if (_selectedPiece != null && !_selectedPiece.CheckIsAllyPiece(piece))
            {
                return;
            }

            if (piece == null || _selectedPiece == piece)
            {
                DeselectPiece(_selectedPiece);
                return;
            }

            Debug.Log($"Piece clicked: {piece.Type}");
            DeselectPiece(_selectedPiece);
            _selectedPiece = piece;
            ShowAvailableMoves(_selectedPiece);
        }

        private void DeselectPiece(ChessPiece piece)
        {
            if (_selectedPiece != null && _selectedPiece == piece)
            {
                _selectedPiece = null;
                HideAvailableMoves();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // _chessBoard = FindAnyObjectByType<ChessBoard>();
            // if (_chessBoard == null)
            // {
            //     Debug.LogError("ChessBoard instance not found in the scene. Please ensure it is present.");
            // }

            // _pieces.Clear();
            // _pieces = FindObjectsByType<ChessPiece>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            // Debug.Log($"{_pieces.Count} pieces found.");
            // foreach (var piece in _pieces)
            // {
            //     piece.OnClicked.AddListener(() =>
            //     {
            //         SelectPiece(piece);
            //     });
            // }

            // _selectedPiece = null;
        }

        void Start()
        {
            _chessBoard = FindAnyObjectByType<ChessBoard>();
            if (_chessBoard == null)
            {
                Debug.LogError("ChessBoard instance not found in the scene. Please ensure it is present.");
            }

            _pieces.Clear();
            _pieces = FindObjectsByType<ChessPiece>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            Debug.Log($"{_pieces.Count} pieces found.");
            foreach (var piece in _pieces)
            {
                piece.OnClicked.AddListener(() =>
                {
                    SelectPiece(piece);
                });
            }

            _selectedPiece = null;

            Initialize();
        }

        // public void SetBoardData(BoardData boardData)
        // {
        //     _boardData = boardData;
        //     ChessBoard.Instance.SetBoardData(boardData);
        // }
    }
}
