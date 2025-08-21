using System.Collections.Generic;
using System.Linq;
using Promorph.Data;
using Promorph.Factories;
using Promorph.Utils;
using UnityEngine;

namespace Promorph.Board
{
    public class BoardManager : PersistentSingleton<BoardManager>
    {
        private ChessBoard _chessBoard;
        private List<ChessPiece> _blackPieces = new();
        private List<ChessPiece> _whitePieces = new();
        private ChessPiece _selectedPiece;

        // Test ////////////////////
        [SerializeField] List<PieceData> whitePieces;
        [SerializeField] List<PieceData> blackPieces;
        ///////////////////////////

        private void SpawnPieces(List<PieceData> pieces, EFaction faction, out List<ChessPiece> pieceList)
        {
            pieceList = new();

            if (pieces == null || pieces.Count == 0)
            {
                Debug.LogWarning($"No pieces found for faction {faction}. Please check the PieceData configuration.");
                return;
            }

            foreach (var pieceData in pieces)
            {
                var piece = ChessPieceFactory.Instance.CreateChessPiece(faction, pieceData);
                piece.OnClicked.AddListener(() =>
                {
                    SelectPiece(piece);
                });
                pieceList.Add(piece);
            }
        }

        public void Initialize(List<PieceData> whitePieces, List<PieceData> blackPieces)
        {
            SpawnPieces(whitePieces, EFaction.White, out _whitePieces);
            SpawnPieces(blackPieces, EFaction.Black, out _blackPieces);

            _chessBoard.GenerateBoard();
            _chessBoard.SetupPieces(_whitePieces, _blackPieces);
        }

        private List<Vector2Int> GetTilesBeingGuarded(EFaction factionToCheck)
        {
            var guardedTiles = new List<Vector2Int>();
            var _pieces = factionToCheck == EFaction.Black ? _whitePieces : _blackPieces;
            foreach (var piece in _pieces.Where(p => p.Faction != factionToCheck))
            {
                var moves = piece.GetCaptures(true);
                foreach (var move in moves)
                {
                    var targetPosition = _chessBoard.WorldToBoardPosition(piece.transform.position) + move;
                    if (_chessBoard.IsValidPosition(targetPosition) && !guardedTiles.Contains(targetPosition))
                    {
                        guardedTiles.Add(targetPosition);
                    }
                }
            }
            return guardedTiles;
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

            var dangerTiles = GetTilesBeingGuarded(piece.Faction);

            // Logic to show available moves for the piece
            Vector2Int piecePos = _chessBoard.WorldToBoardPosition(piece.transform.position);
            Vector2Int[] availableMoves = piece.MoveSet;
            if (piece.Type == EChessPiece.King)
            {
                // For King, we need to check if the move puts the king in danger
                availableMoves = availableMoves.Where(move => !dangerTiles.Contains(piecePos + move)).ToArray();
                Debug.Log("Checking available moves for King piece.");
            }

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
            if (piece.Type == EChessPiece.King)
            {
                // For King, we need to check if the move puts the king in danger
                availableCaptures = availableCaptures.Where(move => !dangerTiles.Contains(piecePos + move)).ToArray();
            }
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

            Initialize(whitePieces, blackPieces);
        }

        void Update()
        {
            // Test //////////////////
            if (Input.GetKeyDown(KeyCode.R))
            {
                Initialize(whitePieces, blackPieces);
                Debug.Log("Board reset with new pieces.");
            }
        }
    }
}
