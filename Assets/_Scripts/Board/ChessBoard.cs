using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Promorph.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace Promorph.Board
{
    public class ChessBoard : MonoBehaviour
    {
        [SerializeField] private Tile _lightTile;
        [SerializeField] private Tile _darkTile;
        [SerializeField] private Tile _moveHighlightTile;
        [SerializeField] private Tile _captureHighlightTile;
        [SerializeField] private Tile _firstMoveHighlightTile;

        [SerializeField] private Tilemap _tilemap;

        private const int BoardSize = 8;
        private const int HalfBoardSize = BoardSize / 2;

        private Dictionary<Vector2Int, BoardTile> _tileDict = new();
        private List<Vector2Int> _highltightedPos = new();

        public UnityEvent<Vector2Int> OnTileClicked;

        public void SetBoardData(BoardData boardData, bool generateNew = false)
        {
            _lightTile = boardData.LightTile;
            _darkTile = boardData.DarkTile;
            _moveHighlightTile = boardData.MoveHighlightTile;
            _captureHighlightTile = boardData.CaptureHighlightTile;
            _firstMoveHighlightTile = boardData.SpecialHighlightTile;

            if (generateNew)
            {
                GenerateBoard();
            }
        }

        public void GenerateBoard()
        {
            _tileDict.Clear();
            _highltightedPos.Clear();
            _tilemap.ClearAllTiles();

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    var tilePosition = new Vector3Int(x - HalfBoardSize, y - HalfBoardSize, 0);
                    var tile = new BoardTile((x + y) % 2 == 0 ? _darkTile : _lightTile);

                    _tilemap.SetTile(tilePosition, tile.Tile);
                    _tileDict.Add(new Vector2Int(tilePosition.x, tilePosition.y), tile);
                }
            }
        }

        public void SetupPieces(List<ChessPiece> whitePieces, List<ChessPiece> blackPieces)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    var pieceIndex = y * BoardSize + x;
                    if( pieceIndex >= whitePieces.Count && pieceIndex >= blackPieces.Count)
                    {
                        return;
                    }

                    var whitePiece = pieceIndex >= whitePieces.Count ? null : whitePieces[pieceIndex];
                    var whitePosition = new Vector2Int(x - HalfBoardSize, y - HalfBoardSize);
                    if (whitePiece != null)
                    {
                        whitePiece.ChangeFaction(EFaction.White);
                        var whiteTile = _tileDict[whitePosition];
                        whiteTile.SetOccupyingPiece(whitePiece);
                        whitePiece.transform.position = BoardToWorldPosition(whitePosition);
                    }

                    var blackPiece = pieceIndex >= blackPieces.Count ? null : blackPieces[pieceIndex];
                    var blackPosition = new Vector2Int(-whitePosition.x - 1, -whitePosition.y - 1);
                    if (blackPiece != null)
                    {
                        blackPiece.ChangeFaction(EFaction.Black);
                        var blackTile = _tileDict[blackPosition];
                        blackTile.SetOccupyingPiece(blackPiece);
                        blackPiece.transform.position = BoardToWorldPosition(blackPosition);
                    }
                }
            }
        }

        public Vector2Int WorldToBoardPosition(Vector3 worldPosition)
        {
            return (Vector2Int)_tilemap.WorldToCell(worldPosition);
        }

        public BoardTile BoardPositionToTile(Vector2Int position)
        {
            if (!_tileDict.ContainsKey(position))
            {
                Debug.LogError($"Position {position} is not valid on the board.");
                return null;
            }

            return _tileDict[position];
        }

        public Vector2 TileToWorldPosition(BoardTile tile)
        {
            if (!_tileDict.ContainsValue(tile))
            {
                Debug.LogError($"Tile {tile} is not valid on the board.");
                return Vector2.zero;
            }

            var position = _tileDict.FirstOrDefault(x => x.Value == tile).Key;
            return _tilemap.GetCellCenterWorld(new(position.x, position.y, 0));
        }

        public Vector2 BoardToWorldPosition(Vector2Int position)
        {
            if (!_tileDict.ContainsKey(position))
            {
                Debug.LogError($"Position {position} is not valid on the board.");
                return Vector2.zero;
            }

            return _tilemap.GetCellCenterWorld(new(position.x, position.y, 0));
        }

        public bool IsValidPosition(Vector2Int position)
        {
            // Debug.Log($"Checking if position {position} is valid on the board.");
            return _tilemap.HasTile(new(position.x, position.y, 0));
        }

        public bool IsTileOccupied(Vector2Int position, out ChessPiece occupyingPiece)
        {
            occupyingPiece = null;

            if (!_tileDict.ContainsKey(position))
            {
                Debug.LogWarning($"Position {position} is not valid on the board.");
                return false;
            }

            var tile = _tileDict[position];
            if (tile.OccupyingPiece != null)
            {
                occupyingPiece = tile.OccupyingPiece;
                return true;
            }

            return false;
        }

        public void HighlightTile(Vector2Int position, EPieceAction action)
        {
            Debug.Log($"Highlighting tile at {position} with action {action}.");
            var tilePosition = new Vector3Int(position.x, position.y, 0);
            var tile = _tileDict[position];
            // _tilemap.SetTileFlags(tilePosition, TileFlags.None);
            Tile tileToUse = action switch
            {
                EPieceAction.Move => _moveHighlightTile,
                EPieceAction.Capture => _captureHighlightTile,
                _ => _firstMoveHighlightTile
            };
            _tilemap.SetTile(tilePosition, tileToUse);
            tile.SetTile(tileToUse);
            // Debug.Log($"Setting tile {tile} to {tile.Tile}.");

            _highltightedPos.Add(position);
        }

        public void ClearHighlights()
        {
            OnTileClicked.RemoveAllListeners();
            if (_highltightedPos.Count == 0) return;

            foreach (var pos in _highltightedPos)
            {
                var tile = _tileDict[pos];
                var tileSprite = (pos.x + pos.y) % 2 == 0 ? _darkTile : _lightTile;
                tile.SetTile(tileSprite);
                _tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tileSprite);
            }

            _highltightedPos.Clear();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var tilePos = WorldToBoardPosition(mouseWorldPos);
                if (!_tileDict.ContainsKey(tilePos))
                {
                    return;
                }

                var tile = _tileDict[tilePos];

                //TODO: Make sure check if tile is valid
                if (_highltightedPos.Contains(tilePos))
                {
                    OnTileClicked?.Invoke(tilePos);
                    // if (tile.Tile == _captureHighlightTile)
                    // {
                    //     Debug.Log($"Capture at tile {tilePos}.");
                    // }
                    // else if (tile.Tile == _moveHighlightTile || tile.Tile == _firstMoveHighlightTile)
                    // {
                    //     OnTileClicked?.Invoke(tilePos, EPieceAction.Move);
                    //     Debug.Log($"Move to tile {tilePos}.");
                    // }
                }
                else
                {
                    if (tile.OccupyingPiece != null)
                    {
                        tile.OccupyingPiece.OnPieceClicked();
                        // OnTileClicked?.Invoke(tilePos, EPieceAction.FirstMove);
                        // Debug.Log($"First move at tile {tilePos}.");
                    }
                }
            }
        }
    }

    public class BoardTile
    {
        public Tile Tile { get; private set; }
        public ChessPiece OccupyingPiece { get; private set; }

        public BoardTile(Tile tile)
        {
            Tile = tile;
            OccupyingPiece = null;
        }

        public void SetTile(Tile tile)
        {
            Tile = tile;
        }

        public void SetOccupyingPiece(ChessPiece piece)
        {
            OccupyingPiece = piece;
            // Debug.Log($"Set occupying piece: {(piece != null ? piece.Type : "null")}");
        }
    }
}
