using System;
using System.Collections.Generic;
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

        private Dictionary<Vector2Int, BoardTile> _tileDict = new();
        private List<Vector2Int> _highltightedPos = new();

        public UnityEvent<Vector2Int, EPieceAction> OnTileClicked;

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

        private void GenerateBoard()
        {
            _tileDict.Clear();
            _highltightedPos.Clear();
            _tilemap.ClearAllTiles();

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    int halfSize = BoardSize / 2;
                    var tilePosition = new Vector3Int(x - halfSize, y - halfSize, 0);
                    var tile = new BoardTile((x + y) % 2 == 0 ? _lightTile : _darkTile);

                    _tilemap.SetTile(tilePosition, tile.Tile);
                    _tileDict.Add(new Vector2Int(tilePosition.x, tilePosition.y), tile);
                }
            }
        }

        public Vector2Int GetPositionInBoard(Vector3 worldPosition)
        {
            return (Vector2Int)_tilemap.WorldToCell(worldPosition);
        }

        public BoardTile GetTileByPosition(Vector2Int position)
        {
            if (!_tileDict.ContainsKey(position))
            {
                Debug.LogError($"Position {position} is not valid on the board.");
                return null;
            }

            return _tileDict[position];
        }

        public Vector2 GetTilePosition(Vector2Int position)
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
            Debug.Log($"Checking if position {position} is valid on the board.");
            //TODO: Tiles that opcupied by ally pieces is not valid
            return _tilemap.HasTile(new(position.x, position.y, 0));
        }

        public bool IsTileOccupied(Vector2Int position, out ChessPiece occupyingPiece)
        {
            occupyingPiece = null;

            if (!_tileDict.ContainsKey(position))
            {
                Debug.LogError($"Position {position} is not valid on the board.");
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
            var tilePosition = new Vector3Int(position.x, position.y, 0);
            var tile = _tileDict[position];
            // _tilemap.SetTileFlags(tilePosition, TileFlags.None);
            Tile tileToUse = action switch
            {
                EPieceAction.Move => _moveHighlightTile,
                EPieceAction.Capture => _captureHighlightTile,
                EPieceAction.FirstMove => _firstMoveHighlightTile,
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };
            _tilemap.SetTile(tilePosition, tileToUse);
            tile.SetTile(tileToUse);

            _highltightedPos.Add(position);
        }

        public void ClearHighlights()
        {
            OnTileClicked.RemoveAllListeners();
            if (_highltightedPos.Count == 0) return;

            foreach (var pos in _highltightedPos)
            {
                var tile = _tileDict[pos];
                var tileSprite = (pos.x + pos.y) % 2 == 0 ? _lightTile : _darkTile;
                tile.SetTile(tileSprite);
                _tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tileSprite);
            }

            _highltightedPos.Clear();
        }

        void Start()
        {
            GenerateBoard();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var tilePos = GetPositionInBoard(mouseWorldPos);
                Debug.Log($"Mouse clicked at tile position: {tilePos}");

                //TODO: Make sure check if tile is valid
                if (_highltightedPos.Contains(tilePos))
                {
                    var tile = _tileDict[tilePos];
                    if (tile.Tile == _captureHighlightTile)
                    {
                        OnTileClicked?.Invoke(tilePos, EPieceAction.Capture);
                        Debug.Log($"Capture at tile {tilePos}.");
                    }
                    else if (tile.Tile == _moveHighlightTile || tile.Tile == _firstMoveHighlightTile)
                    {
                        OnTileClicked?.Invoke(tilePos, EPieceAction.Move);
                        Debug.Log($"Move to tile {tilePos}.");
                    }
                }
                else
                {
                    // Debug.Log($"Tile at {tilePos} is not highlighted.");
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
        }
    }
}
